using SkiaSharp;

namespace Artemis.Plugins.Modules.Fallout4.DataModels
{
    public class StatusDataModel
    {
        #region helper accessors
        internal DictAccessor<float> effectColorR;
        internal DictAccessor<float> effectColorG;
        internal DictAccessor<float> effectColorB;
        internal DictAccessor<bool> isDataUnavailable;
        internal DictAccessor<bool> isPlayerDead;
        internal DictAccessor<bool> isInVatsPlayback;
        internal DictAccessor<bool> isLoading;
        internal DictAccessor<bool> isInAnimation;
        internal DictAccessor<bool> isPipboyNotEquipped;
        internal DictAccessor<bool> isInAutoVanity;
        internal DictAccessor<bool> isInVats;
        internal DictAccessor<bool> isPlayerMovementLocked;
        internal DictAccessor<bool> isPlayerPipboyLocked;
        internal DictAccessor<bool> isMenuOpen;
        internal DictAccessor<bool> isPlayerInDialogue;
        #endregion

        public SKColor EffectColor => new SKColor((byte)((effectColorR?.Value ?? default) * 255f),
                                                  (byte)((effectColorG?.Value ?? default) * 255f),
                                                  (byte)((effectColorB?.Value ?? default) * 255f));
        public bool IsDataUnavailable => isDataUnavailable?.Value ?? default;
        public bool IsPlayerDead => isPlayerDead?.Value ?? default;
        public bool IsInVatsPlayback => isInVatsPlayback?.Value ?? default;
        public bool IsLoading => isLoading?.Value ?? default;
        public bool IsInAnimation => isInAnimation?.Value ?? default;
        public bool IsPipboyNotEquipped => isPipboyNotEquipped?.Value ?? default;
        public bool IsInAutoVanity => isInAutoVanity?.Value ?? default;
        public bool IsInVats => isInVats?.Value ?? default;
        public bool IsPlayerMovementLocked => isPlayerMovementLocked?.Value ?? default;
        public bool IsPlayerPipboyLocked => isPlayerPipboyLocked?.Value ?? default;
        public bool IsMenuOpen => isMenuOpen?.Value ?? default;
        public bool IsPlayerInDialogue => isPlayerInDialogue?.Value ?? default;
    }
}