using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

/// <summary>
/// Safe fallback rig for modded or non-Spine characters using lightweight Node2D motion.
/// </summary>
internal sealed class FallbackNodeRig : ICharacterActionRig
{
    public void ApplyPose(
        SpineBoneCache bones,
        Node2D visuals,
        Creature? ownerCreature,
        ActionKind kind,
        float phase,
        float elapsedSec,
        float weight)
    {
        if (visuals == null || !GodotObject.IsInstanceValid(visuals) || weight <= 0.001f)
            return;

        float s = MathF.Sin(phase);
        switch (kind)
        {
            case ActionKind.HighKnees:
                visuals.Position = new Vector2(0f, -MathF.Abs(s) * 24f * weight);
                visuals.RotationDegrees = s * 5f * weight;
                break;
            case ActionKind.BellyLaugh:
                visuals.Position = new Vector2(0f, -MathF.Abs(MathF.Sin(phase * 2f)) * 16f * weight);
                visuals.RotationDegrees = -8f * weight + MathF.Sin(phase * 3f) * 6f * weight;
                break;
            case ActionKind.JadeRabbit:
                visuals.Position = new Vector2(MathF.Sin(phase) * 20f * weight, -MathF.Abs(MathF.Cos(phase)) * 18f * weight);
                visuals.RotationDegrees = MathF.Sin(phase) * 10f * weight;
                break;
            case ActionKind.Taunt:
                visuals.Position = new Vector2(MathF.Sin(phase * 2f) * 14f * weight, 0f);
                visuals.RotationDegrees = 10f * weight + MathF.Sin(phase * 2f) * 8f * weight;
                break;
        }
    }

    public void Dispose() { }
}
