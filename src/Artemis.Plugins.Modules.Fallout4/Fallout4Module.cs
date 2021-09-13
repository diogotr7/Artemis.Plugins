using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.Fallout4.DataModels;
using Artemis.Plugins.Modules.Fallout4.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Timer = System.Timers.Timer;

namespace Artemis.Plugins.Modules.Fallout4
{
    [PluginFeature(AlwaysEnabled = true, Name ="Fallout 4", Icon ="Radioactive")]
    public class Fallout4Module : Module<Fallout4DataModel>
    {
        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new() { new ProcessActivationRequirement("Fallout4") };
        private readonly byte[] _heartbeatPacket = new byte[5];
        private readonly Dictionary<uint, (FalloutDataType DataType, object Data)> _database = new Dictionary<uint, (FalloutDataType DataType, object Data)>();
        private readonly Timer heartbeatTimer = new Timer(5000);
        private TcpClient tcpClient;
        private NetworkStream stream;
        private bool first;
        private int dictCount;

        public Fallout4Module()
        {
            UpdateDuringActivationOverride = false;
        }

        public override void Enable()
        {
            heartbeatTimer.Elapsed += SendHeartbeat;
        }

        public override void Disable()
        {
        }

        public override void ModuleActivated(bool isOverride)
        {
            tcpClient = new TcpClient("127.0.0.1", 27000);
            stream = tcpClient.GetStream();
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            heartbeatTimer?.Stop();
            stream?.Dispose();
            tcpClient?.Dispose();
            _database.Clear();
            ClearAccessors();
            first = true;
            dictCount = 0;
        }

        public override void Update(double deltaTime)
        {
            if (stream is null)
                return;
            if (!stream.DataAvailable)
                return;

            byte[] header = new byte[5];
            if (stream.Read(header, 0, 5) != 5)
                return;

            BinaryReader headerReader = new BinaryReader(new MemoryStream(header), Encoding.UTF8);

            var expectedSize = headerReader.ReadUInt32();
            var commandType = (FalloutPacketType)headerReader.ReadByte();

            byte[] data = stream.FullRead(expectedSize);

            BinaryReader buffer = new BinaryReader(new MemoryStream(data));

            switch (commandType)
            {
                case FalloutPacketType.Heartbeat:
                    SendHeartbeat(null, null);
                    break;
                case FalloutPacketType.NewConnection:
                    string gameInfo = buffer.ReadNullTerminatedString();
                    heartbeatTimer.Start();
                    break;
                case FalloutPacketType.DataUpdate:
                    while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                    {
                        FalloutDataType updateDataType = (FalloutDataType)buffer.ReadByte();
                        uint updateId = buffer.ReadUInt32();
                        object value = null;
                        uint[] removeList = null;
                        if (updateDataType == FalloutDataType.Map)
                        {
                            Dictionary<uint, string> addList = Enumerable
                                .Range(0, buffer.ReadUInt16())
                                .Select(_ => (buffer.ReadUInt32(), buffer.ReadNullTerminatedString()))
                                .ToDictionary(a => a.Item1, b => b.Item2);

                            removeList = Enumerable
                                .Range(0, buffer.ReadUInt16())
                                .Select(_ => buffer.ReadUInt32())
                                .ToArray();

                            value = addList;
                        }
                        else
                        {
                            value = updateDataType switch
                            {
                                FalloutDataType.Boolean => buffer.ReadBoolean(),
                                FalloutDataType.SByte => buffer.ReadSByte(),
                                FalloutDataType.Byte => buffer.ReadByte(),
                                FalloutDataType.Int => buffer.ReadInt32(),
                                FalloutDataType.UInt => buffer.ReadUInt32(),
                                FalloutDataType.Float => buffer.ReadSingle(),
                                FalloutDataType.String => buffer.ReadNullTerminatedString(),
                                FalloutDataType.Array => Enumerable.Range(0, buffer.ReadUInt16()).Select(_ => buffer.ReadUInt32()).ToArray(),
                                _ => throw new ArgumentException()
                            };
                        }

                        _database[updateId] = (updateDataType, value);
                        if (removeList != null)
                        {
                            foreach (uint removeId in removeList)
                            {
                                _database.Remove(removeId);
                            }
                        }
                    }
                    if (!first || dictCount < _database.Count)
                    {
                        dictCount = _database.Count;
                        MapNode root = new MapNode(0);
                        root.Fill(_database);

                        FillAccessors(root);
                        first = true;
                    }

                    break;
            }
        }

        private void SendHeartbeat(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                stream?.Write(_heartbeatPacket);
            }
            catch
            {
            }

        }

        private void FillAccessors(MapNode root)
        {
            #region Status
            MapNode status = root.Data["Status"] as MapNode;
            ArrayNode effectColor = status.Data["EffectColor"] as ArrayNode;
            DataModel.Status.effectColorR = new DictAccessor<float>(_database, (effectColor.Data[0] as Node<float>).Id);
            DataModel.Status.effectColorG = new DictAccessor<float>(_database, (effectColor.Data[1] as Node<float>).Id);
            DataModel.Status.effectColorB = new DictAccessor<float>(_database, (effectColor.Data[2] as Node<float>).Id);

            DataModel.Status.isDataUnavailable = new DictAccessor<bool>(_database, (status.Data["IsDataUnavailable"] as Node<bool>).Id);
            DataModel.Status.isPlayerDead = new DictAccessor<bool>(_database, (status.Data["IsPlayerDead"] as Node<bool>).Id);
            DataModel.Status.isInVatsPlayback = new DictAccessor<bool>(_database, (status.Data["IsInVatsPlayback"] as Node<bool>).Id);
            DataModel.Status.isLoading = new DictAccessor<bool>(_database, (status.Data["IsLoading"] as Node<bool>).Id);
            DataModel.Status.isInAnimation = new DictAccessor<bool>(_database, (status.Data["IsInAnimation"] as Node<bool>).Id);
            DataModel.Status.isPipboyNotEquipped = new DictAccessor<bool>(_database, (status.Data["IsPipboyNotEquipped"] as Node<bool>).Id);
            DataModel.Status.isInAutoVanity = new DictAccessor<bool>(_database, (status.Data["IsInAutoVanity"] as Node<bool>).Id);
            DataModel.Status.isInVats = new DictAccessor<bool>(_database, (status.Data["IsInVats"] as Node<bool>).Id);
            DataModel.Status.isPlayerMovementLocked = new DictAccessor<bool>(_database, (status.Data["IsPlayerMovementLocked"] as Node<bool>).Id);
            DataModel.Status.isPlayerPipboyLocked = new DictAccessor<bool>(_database, (status.Data["IsPlayerPipboyLocked"] as Node<bool>).Id);
            DataModel.Status.isMenuOpen = new DictAccessor<bool>(_database, (status.Data["IsMenuOpen"] as Node<bool>).Id);
            DataModel.Status.isPlayerInDialogue = new DictAccessor<bool>(_database, (status.Data["IsPlayerInDialogue"] as Node<bool>).Id);
            #endregion

            #region Special
            ArrayNode special = root.Data["Special"] as ArrayNode;

            DataModel.Special.strength = new DictAccessor<int>(_database, ((special.Data[0] as MapNode).Data["Value"] as Node<int>).Id);
            DataModel.Special.perception = new DictAccessor<int>(_database, ((special.Data[1] as MapNode).Data["Value"] as Node<int>).Id);
            DataModel.Special.endurance = new DictAccessor<int>(_database, ((special.Data[2] as MapNode).Data["Value"] as Node<int>).Id);
            DataModel.Special.charisma = new DictAccessor<int>(_database, ((special.Data[3] as MapNode).Data["Value"] as Node<int>).Id);
            DataModel.Special.intelligence = new DictAccessor<int>(_database, ((special.Data[4] as MapNode).Data["Value"] as Node<int>).Id);
            DataModel.Special.agility = new DictAccessor<int>(_database, ((special.Data[5] as MapNode).Data["Value"] as Node<int>).Id);
            DataModel.Special.luck = new DictAccessor<int>(_database, ((special.Data[6] as MapNode).Data["Value"] as Node<int>).Id);
            #endregion

            #region Stats
            MapNode stats = root.Data["Stats"] as MapNode;

            DataModel.Stats.lLegCondition = new DictAccessor<float>(_database, (stats.Data["LLegCondition"] as Node<float>).Id);
            DataModel.Stats.radawayCount = new DictAccessor<uint>(_database, (stats.Data["RadawayCount"] as Node<uint>).Id);
            DataModel.Stats.headCondition = new DictAccessor<float>(_database, (stats.Data["HeadCondition"] as Node<float>).Id);
            DataModel.Stats.stimpakCount = new DictAccessor<uint>(_database, (stats.Data["StimpakCount"] as Node<uint>).Id);
            DataModel.Stats.rLegCondition = new DictAccessor<float>(_database, (stats.Data["RLegCondition"] as Node<float>).Id);
            DataModel.Stats.torsoCondition = new DictAccessor<float>(_database, (stats.Data["TorsoCondition"] as Node<float>).Id);
            DataModel.Stats.rArmCondition = new DictAccessor<float>(_database, (stats.Data["RArmCondition"] as Node<float>).Id);
            DataModel.Stats.bodyFlags = new DictAccessor<uint>(_database, (stats.Data["BodyFlags"] as Node<uint>).Id);
            DataModel.Stats.headFlags = new DictAccessor<uint>(_database, (stats.Data["HeadFlags"] as Node<uint>).Id);
            DataModel.Stats.lArmCondition = new DictAccessor<float>(_database, (stats.Data["LArmCondition"] as Node<float>).Id);
            #endregion

            #region PlayerInfo

            MapNode playerInfo = root.Data["PlayerInfo"] as MapNode;

            DataModel.Player.maxWeight = new DictAccessor<float>(_database, (playerInfo.Data["MaxWeight"] as Node<float>).Id);
            DataModel.Player.xPLevel = new DictAccessor<int>(_database, (playerInfo.Data["XPLevel"] as Node<int>).Id);
            DataModel.Player.dateYear = new DictAccessor<uint>(_database, (playerInfo.Data["DateYear"] as Node<uint>).Id);
            DataModel.Player.maxHP = new DictAccessor<float>(_database, (playerInfo.Data["MaxHP"] as Node<float>).Id);
            DataModel.Player.currWeight = new DictAccessor<float>(_database, (playerInfo.Data["CurrWeight"] as Node<float>).Id);
            DataModel.Player.perkPoints = new DictAccessor<uint>(_database, (playerInfo.Data["PerkPoints"] as Node<uint>).Id);
            DataModel.Player.maxAP = new DictAccessor<float>(_database, (playerInfo.Data["MaxAP"] as Node<float>).Id);
            DataModel.Player.currHP = new DictAccessor<float>(_database, (playerInfo.Data["CurrHP"] as Node<float>).Id);
            DataModel.Player.playerName = new DictAccessor<string>(_database, (playerInfo.Data["PlayerName"] as Node<string>).Id);
            DataModel.Player.xPProgressPct = new DictAccessor<float>(_database, (playerInfo.Data["XPProgressPct"] as Node<float>).Id);
            DataModel.Player.dateDay = new DictAccessor<sbyte>(_database, (playerInfo.Data["DateDay"] as Node<sbyte>).Id);
            DataModel.Player.timeHour = new DictAccessor<float>(_database, (playerInfo.Data["TimeHour"] as Node<float>).Id);
            DataModel.Player.caps = new DictAccessor<int>(_database, (playerInfo.Data["Caps"] as Node<int>).Id);
            DataModel.Player.currentHPGain = new DictAccessor<float>(_database, (playerInfo.Data["CurrentHPGain"] as Node<float>).Id);
            DataModel.Player.dateMonth = new DictAccessor<uint>(_database, (playerInfo.Data["DateMonth"] as Node<uint>).Id);
            DataModel.Player.currAP = new DictAccessor<float>(_database, (playerInfo.Data["CurrAP"] as Node<float>).Id);
            #endregion
        }

        private void ClearAccessors()
        {
            DataModel.Status.effectColorR = null;
            DataModel.Status.effectColorG = null;
            DataModel.Status.effectColorB = null;
            DataModel.Status.isDataUnavailable = null;
            DataModel.Status.isPlayerDead = null;
            DataModel.Status.isInVatsPlayback = null;
            DataModel.Status.isLoading = null;
            DataModel.Status.isInAnimation = null;
            DataModel.Status.isPipboyNotEquipped = null;
            DataModel.Status.isInAutoVanity = null;
            DataModel.Status.isInVats = null;
            DataModel.Status.isPlayerMovementLocked = null;
            DataModel.Status.isPlayerPipboyLocked = null;
            DataModel.Status.isMenuOpen = null;
            DataModel.Status.isPlayerInDialogue = null;
            DataModel.Special.strength = null;
            DataModel.Special.perception = null;
            DataModel.Special.endurance = null;
            DataModel.Special.charisma = null;
            DataModel.Special.intelligence = null;
            DataModel.Special.agility = null;
            DataModel.Special.luck = null;
            DataModel.Stats.lLegCondition = null;
            DataModel.Stats.radawayCount = null;
            DataModel.Stats.headCondition = null;
            DataModel.Stats.stimpakCount = null;
            DataModel.Stats.rLegCondition = null;
            DataModel.Stats.torsoCondition = null;
            DataModel.Stats.rArmCondition = null;
            DataModel.Stats.bodyFlags = null;
            DataModel.Stats.headFlags = null;
            DataModel.Stats.lArmCondition = null;
            DataModel.Player.maxWeight = null;
            DataModel.Player.xPLevel = null;
            DataModel.Player.dateYear = null;
            DataModel.Player.maxHP = null;
            DataModel.Player.currWeight = null;
            DataModel.Player.perkPoints = null;
            DataModel.Player.maxAP = null;
            DataModel.Player.currHP = null;
            DataModel.Player.playerName = null;
            DataModel.Player.xPProgressPct = null;
            DataModel.Player.dateDay = null;
            DataModel.Player.timeHour = null;
            DataModel.Player.caps = null;
            DataModel.Player.currentHPGain = null;
            DataModel.Player.dateMonth = null;
            DataModel.Player.currAP = null;
        }
    }
}