using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Particle.PropertyGroups;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace Artemis.Plugins.LayerBrushes.Particle
{
    public class ParticleLayerBrush : LayerBrush<ParticlePropertyGroup>
    {
        private readonly List<Particle> _particles = new List<Particle>();
        private readonly Random _random = new Random();
        private double nextSpawnInterval;
        private SKRect rect;

        public override void EnableLayerBrush()
        {
        }

        public override void DisableLayerBrush()
        {
            _particles.Clear();
            nextSpawnInterval = 0;
        }

        public override void Update(double deltaTime)
        {
            foreach (Particle particle in _particles)
            {
                particle.Update(Properties, deltaTime);
            }

            DespawnParticles();

            if (Properties.Spawn.SpawnEnabled.CurrentValue)
                SpawnParticles(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
            if (rect != bounds)
                rect = bounds;

            foreach (Particle particle in _particles)
            {
                //handle color modes here.
                switch (Properties.Color.ColorMode.CurrentValue)
                {
                    case ParticleColorMode.Lifetime:
                        paint.Shader = SKShader.CreateColor(Properties.Color.Gradient.CurrentValue.GetColor((float)particle.LifetimePercent));
                        break;
                    case ParticleColorMode.Static:
                        paint.Shader = SKShader.CreateColor(Properties.Color.Color);
                        break;
                    case ParticleColorMode.Sweep:
                        paint.Shader = SKShader.CreateSweepGradient(particle.Position, Properties.Color.Gradient.CurrentValue.Colors);
                        break;
                    case ParticleColorMode.Radial:
                        paint.Shader = SKShader.CreateRadialGradient(particle.Position, particle.Radius, Properties.Color.Gradient.CurrentValue.Colors, SKShaderTileMode.Clamp);
                        break;
                    default:
                        break;
                }
                canvas.DrawCircle(particle.Position, particle.Radius, paint);

                if (Properties.Trail.DrawTrail.CurrentValue)
                {
                    SKPoint normalized = SKPoint.Normalize(particle.Velocity);
                    float mult = Properties.Trail.TrailLength.CurrentValue.GetValueAtPercent((float)particle.LifetimePercent);
                    SKPoint trailEndPosition = new SKPoint(
                        particle.Position.X - (normalized.X * mult),
                        particle.Position.Y - (normalized.Y * mult)
                    );
                    paint.StrokeWidth = Properties.Trail.TrailWidth.CurrentValue.GetValueAtPercent((float)particle.LifetimePercent);
                    canvas.DrawLine(particle.Position, trailEndPosition, paint);
                }
            }
        }

        private void SpawnParticles(double deltaTime)
        {
            if (deltaTime < 0)
                return;

            nextSpawnInterval -= deltaTime;
            if (nextSpawnInterval <= 0)
            {
                for (int i = 0; i < Properties.Spawn.SpawnAmountRange.CurrentValue.GetRandomValue(); i++)
                {
                    _particles.Add(new Particle(Properties, rect, _random));
                }

                nextSpawnInterval = Properties.Spawn.SpawnTimeRange.CurrentValue.GetRandomValue();
            }
        }

        private void DespawnParticles()
        {
            _particles.RemoveAll(p => !p.IsAlive ||
                (Properties.Spawn.DespawnOutOfBounds.CurrentValue && !p.InBounds(rect)));
        }
    }

    public class Particle
    {
        public SKPoint Position { get; set; }
        public SKPoint Velocity { get; set; }

        public float Radius { get; set; }
        public double Lifetime { get; set; }
        public double MaxLifetime { get; set; }
        public double LifetimePercent => Lifetime / MaxLifetime;

        public bool IsAlive => Lifetime < MaxLifetime;
        public float LeftEdge => Position.X - (Radius / 2f);
        public float RightEdge => Position.X + (Radius / 2f);
        public float TopEdge => Position.Y - (Radius / 2f);
        public float BottomEdge => Position.Y + (Radius / 2f);

        public Particle(ParticlePropertyGroup properties, SKRect rect, Random rnd)
        {
            Position = properties.Spawn.SpawnPosition.CurrentValue switch
            {
                SpawnPosition.TopEdge => new SKPoint(rnd.RandomBetween(0, rect.Width), 0),
                SpawnPosition.RightEdge => new SKPoint(rect.Width, rnd.RandomBetween(0, rect.Height)),
                SpawnPosition.BottomEdge => new SKPoint(rnd.RandomBetween(0, rect.Width), rect.Height),
                SpawnPosition.LeftEdge => new SKPoint(0, rnd.RandomBetween(0, rect.Height)),
                SpawnPosition.Custom => new SKPoint(rect.Width * properties.Spawn.SpawnPositionPercent.CurrentValue.X,
                                                    rect.Height * properties.Spawn.SpawnPositionPercent.CurrentValue.Y),
                _ => new SKPoint(rnd.RandomBetween(0, rect.Width), rnd.RandomBetween(0, rect.Height)),
            };
            Velocity = new SKPoint(
                properties.Spawn.InitialXVelocityRange.CurrentValue.GetRandomValue(),
                properties.Spawn.InitialYVelocityRange.CurrentValue.GetRandomValue()
            );
            Radius = properties.Spawn.SizeRange.CurrentValue.GetRandomValue();
            Lifetime = 0;
            MaxLifetime = properties.Spawn.MaxLifetimeRange.CurrentValue.GetRandomValue();
        }

        public void Update(ParticlePropertyGroup properties, double deltaTime)
        {
            if (deltaTime < 0)
                return;//TODO

            Lifetime += deltaTime;
            Velocity = new SKPoint(
                Velocity.X + (float)(properties.Physics.Acceleration.CurrentValue.X * deltaTime),
                Velocity.Y + (float)(properties.Physics.Acceleration.CurrentValue.Y * deltaTime)
            );
            Velocity = new SKPoint(
                Velocity.X * (float)Math.Pow(1 - properties.Physics.Drag.CurrentValue.X, deltaTime),
                Velocity.Y * (float)Math.Pow(1 - properties.Physics.Drag.CurrentValue.Y, deltaTime)
            );
            Position = new SKPoint(
                Position.X + Velocity.X,
                Position.Y + Velocity.Y
            );
            Radius += (float)(properties.Physics.DeltaSize.CurrentValue * deltaTime);
        }

        public bool InBounds(SKRect rect) =>
                    BottomEdge > rect.Top &&
                    TopEdge < rect.Bottom &&
                    LeftEdge < rect.Right &&
                    RightEdge > rect.Left;
    }
}