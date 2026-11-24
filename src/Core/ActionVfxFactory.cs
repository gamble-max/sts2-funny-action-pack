using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Sts2ActionPack.Core;

internal sealed class ActionVfxBundle : IDisposable
{
    private static ImageTexture? _softDotTex;
    private static ImageTexture? _sparkTex;

    private readonly CpuParticles2D _dust;
    private readonly CpuParticles2D _sweat;
    private readonly CpuParticles2D _laughTears;
    private readonly CpuParticles2D _jadePetals;
    private readonly CpuParticles2D _tauntSparks;
    private bool _disposed;

    public ActionVfxBundle(NCreatureVisuals visuals)
    {
        EnsureTextures();

        _dust = new CpuParticles2D
        {
            Name = "ActionPackDust",
            Position = new Vector2(0f, -12f),
            Amount = 14,
            Lifetime = 0.45f,
            Emitting = false,
            Texture = _softDotTex,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
            EmissionRectExtents = new Vector2(56f, 8f),
            Direction = new Vector2(0f, -1f),
            Spread = 70f,
            Gravity = new Vector2(0f, 38f),
            InitialVelocityMin = 24f,
            InitialVelocityMax = 68f,
            ScaleAmountMin = 0.55f,
            ScaleAmountMax = 1.25f,
            Color = new Color(0.84f, 0.86f, 0.89f, 0.55f),
            ZIndex = 2,
        };
        visuals.AddChild(_dust);

        _sweat = new CpuParticles2D
        {
            Name = "ActionPackSweat",
            Position = new Vector2(16f, -240f),
            Amount = 8,
            Lifetime = 0.6f,
            Emitting = false,
            Texture = _softDotTex,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere,
            EmissionSphereRadius = 32f,
            Direction = new Vector2(0.75f, -0.65f),
            Spread = 50f,
            Gravity = new Vector2(0f, 240f),
            InitialVelocityMin = 48f,
            InitialVelocityMax = 110f,
            ScaleAmountMin = 0.35f,
            ScaleAmountMax = 0.7f,
            Color = new Color(0.62f, 0.90f, 1.0f, 0.88f),
            ZIndex = 4,
        };
        visuals.AddChild(_sweat);

        _laughTears = new CpuParticles2D
        {
            Name = "ActionPackLaugh",
            Position = new Vector2(0f, -235f),
            Amount = 14,
            Lifetime = 0.65f,
            Emitting = false,
            Texture = _softDotTex,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere,
            EmissionSphereRadius = 34f,
            Direction = new Vector2(0f, -0.8f),
            Spread = 120f,
            Gravity = new Vector2(0f, 220f),
            InitialVelocityMin = 75f,
            InitialVelocityMax = 155f,
            ScaleAmountMin = 0.45f,
            ScaleAmountMax = 0.9f,
            Color = new Color(1.0f, 0.92f, 0.45f, 0.92f),
            ZIndex = 5,
        };
        visuals.AddChild(_laughTears);

        _jadePetals = new CpuParticles2D
        {
            Name = "ActionPackJadePetals",
            Position = new Vector2(0f, -180f),
            Amount = 16,
            Lifetime = 1.15f,
            Emitting = false,
            Texture = _sparkTex,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle,
            EmissionRectExtents = new Vector2(110f, 95f),
            Direction = new Vector2(0.25f, -1f),
            Spread = 65f,
            Gravity = new Vector2(-12f, -28f),
            InitialVelocityMin = 24f,
            InitialVelocityMax = 65f,
            ScaleAmountMin = 0.45f,
            ScaleAmountMax = 0.95f,
            Color = new Color(1.0f, 0.85f, 0.42f, 0.86f),
            ZIndex = 5,
        };
        visuals.AddChild(_jadePetals);

        _tauntSparks = new CpuParticles2D
        {
            Name = "ActionPackTauntSparks",
            Position = new Vector2(26f, -255f),
            Amount = 12,
            Lifetime = 0.55f,
            Emitting = false,
            Texture = _sparkTex,
            EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere,
            EmissionSphereRadius = 36f,
            Direction = new Vector2(0.4f, -1f),
            Spread = 85f,
            Gravity = new Vector2(0f, -55f),
            InitialVelocityMin = 65f,
            InitialVelocityMax = 135f,
            ScaleAmountMin = 0.5f,
            ScaleAmountMax = 1.0f,
            Color = new Color(1.0f, 0.42f, 0.25f, 0.92f),
            ZIndex = 5,
        };
        visuals.AddChild(_tauntSparks);
    }

    public void Update(ActionKind kind, float weight)
    {
        if (_disposed) return;
        bool active = weight > 0.35f && kind != ActionKind.None;
        SetEmit(_dust, active && kind == ActionKind.HighKnees);
        SetEmit(_sweat, active && kind == ActionKind.HighKnees);
        SetEmit(_laughTears, active && kind == ActionKind.BellyLaugh);
        SetEmit(_jadePetals, active && kind == ActionKind.JadeRabbit);
        SetEmit(_tauntSparks, active && kind == ActionKind.Taunt);
    }

    private static void SetEmit(CpuParticles2D? p, bool on)
    {
        if (p != null && GodotObject.IsInstanceValid(p) && p.Emitting != on)
            p.Emitting = on;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var p in new[] { _dust, _sweat, _laughTears, _jadePetals, _tauntSparks })
        {
            if (p != null && GodotObject.IsInstanceValid(p))
                p.Emitting = false;
        }
    }

    private static void EnsureTextures()
    {
        if (_softDotTex == null)
        {
            const int size = 16;
            var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float d = MathF.Sqrt(dx * dx + dy * dy);
                    float alpha = Math.Clamp(1f - d, 0f, 1f);
                    alpha *= alpha;
                    img.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            _softDotTex = ImageTexture.CreateFromImage(img);
        }

        if (_sparkTex == null)
        {
            const int size = 16;
            var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = MathF.Abs(x - center) / center;
                    float dy = MathF.Abs(y - center) / center;
                    float star = Math.Clamp(1f - (dx + dy) * 0.72f, 0f, 1f);
                    float cross = Math.Max(Math.Clamp(1f - dx * 2.2f, 0f, 1f) * Math.Clamp(1f - dy * 0.9f, 0f, 1f),
                                           Math.Clamp(1f - dy * 2.2f, 0f, 1f) * Math.Clamp(1f - dx * 0.9f, 0f, 1f));
                    float alpha = Math.Clamp(Math.Max(star * star, cross), 0f, 1f);
                    img.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            _sparkTex = ImageTexture.CreateFromImage(img);
        }
    }
}
