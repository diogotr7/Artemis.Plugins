using Artemis.Core.LayerBrushes;
using Artemis.Plugins.LayerBrushes.Particle.PropertyGroups;
using SkiaSharp;
using System;
using System.Collections.Generic;

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

            if (Properties.SpawnEnabled.CurrentValue)
                SpawnParticles(deltaTime);
        }

        public override void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            if (rect != path.Bounds)
                rect = path.Bounds;

            foreach (Particle particle in _particles)
            {
                paint.Shader = SKShader.CreateColor(Properties.Gradient.CurrentValue.GetColor((float)(particle.Lifetime / particle.MaxLifetime)));
                canvas.DrawCircle(particle.Position, particle.Radius, paint);
            }
        }

        private void SpawnParticles(double deltaTime)
        {
            if (deltaTime < 0)
                return;

            nextSpawnInterval -= deltaTime;
            if (nextSpawnInterval <= 0)
            {
                float spawnCount = _random.RandomBetween(
                    Properties.SpawnAmount.CurrentValue.X,
                    Properties.SpawnAmount.CurrentValue.Y
                );

                for (int i = 0; i < spawnCount; i++)
                {
                    _particles.Add(new Particle(Properties, rect));
                }

                nextSpawnInterval = _random.RandomBetween(
                    Properties.SpawnTime.CurrentValue.X,
                    Properties.SpawnTime.CurrentValue.Y
                );
            }
        }

        private void DespawnParticles()
        {
            _particles.RemoveAll(p => !p.IsAlive || !p.InBounds(rect));
        }
    }

    public class Particle
    {
        private static readonly Random rnd = new Random();

        public SKPoint Position { get; set; }
        public SKPoint Velocity { get; set; }

        public float Radius { get; set; }
        public double Lifetime { get; set; }
        public double MaxLifetime { get; set; }

        public bool IsAlive => Lifetime < MaxLifetime;
        public float LeftEdge => Position.X - (Radius / 2f);
        public float RightEdge => Position.X + (Radius / 2f);
        public float TopEdge => Position.Y - (Radius / 2f);
        public float BottomEdge => Position.Y + (Radius / 2f);

        public Particle(ParticlePropertyGroup properties, SKRect rect)
        {
            Position = properties.SpawnPosition.CurrentValue switch
            {
                SpawnPosition.Top => new SKPoint(rnd.RandomBetween(0, rect.Width), 0),
                SpawnPosition.Right => new SKPoint(rect.Width, rnd.RandomBetween(0, rect.Height)),
                SpawnPosition.Bottom => new SKPoint(rnd.RandomBetween(0, rect.Width), rect.Height),
                SpawnPosition.Left => new SKPoint(0, rnd.RandomBetween(0, rect.Height)),
                _ => new SKPoint(rnd.RandomBetween(0, rect.Width), rnd.RandomBetween(0, rect.Height)),
            };
            Velocity = new SKPoint(
                rnd.RandomBetween(properties.InitialXVelocity.CurrentValue.X, properties.InitialXVelocity.CurrentValue.Y),
                rnd.RandomBetween(properties.InitialYVelocity.CurrentValue.X, properties.InitialYVelocity.CurrentValue.Y)
            );
            Radius = rnd.RandomBetween(properties.Size.CurrentValue.X, properties.Size.CurrentValue.Y);
            Lifetime = 0;
            MaxLifetime = rnd.RandomBetween(properties.MaxLifetime.CurrentValue.X, properties.MaxLifetime.CurrentValue.Y);
        }

        public void Update(ParticlePropertyGroup properties, double deltaTime)
        {
            if (deltaTime < 0)
                return;//TODO

            Lifetime += deltaTime;
            Velocity = new SKPoint(
                Velocity.X + (float)(properties.Acceleration.CurrentValue.X * deltaTime),
                Velocity.Y + (float)(properties.Acceleration.CurrentValue.Y * deltaTime)
            );
            Velocity = new SKPoint(
                Velocity.X * (float)Math.Pow(1 - properties.Drag.CurrentValue.X, deltaTime),
                Velocity.Y * (float)Math.Pow(1 - properties.Drag.CurrentValue.Y, deltaTime)
            );
            Position = new SKPoint(
                Position.X + Velocity.X,
                Position.Y + Velocity.Y
            );
            Radius += (float)(properties.DeltaSize.CurrentValue * deltaTime);
        }

        internal bool InBounds(SKRect rect) =>
                    BottomEdge > rect.Top &&
                    TopEdge < rect.Bottom &&
                    LeftEdge < rect.Right &&
                    RightEdge > rect.Left;
    }
}