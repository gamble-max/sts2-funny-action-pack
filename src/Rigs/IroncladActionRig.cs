using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

internal sealed class IroncladActionRig : ICharacterActionRig
{
    public static readonly string[] TrackedBones =
    {
        "cog", "bod_lower", "bod_upper", "neck", "head", "hips",
        "leg_upper_r", "leg_lower_r", "leg_ik_r", "foot_r1", "foot_r2",
        "leg_upper_l", "leg_lower_l", "leg_ik_l", "foot_l1", "foot_l2",
        "shoulder_r", "arm_upper_r", "arm_lower_r", "land_r",
        "shoulder_l", "arm_upper_l", "arm_lower_l", "hand_l",
        "shoulder_plate_r", "shoulder_plate_l",
        "skirt_r", "skirt_l", "belt_middle",
        "hair_1a", "hair_1b", "sword"
    };

    public void ApplyPose(
        SpineBoneCache bones,
        Node2D visuals,
        Creature? ownerCreature,
        ActionKind kind,
        float phase,
        float elapsedSec,
        float weight)
    {
        if (weight <= 0.001f)
            return;

        switch (kind)
        {
            case ActionKind.HighKnees:
                ApplyHighKnees(bones, phase, weight);
                break;
            case ActionKind.BellyLaugh:
                ApplyBellyLaugh(bones, elapsedSec, weight);
                break;
            case ActionKind.JadeRabbit:
                ApplyJadeRabbit(bones, phase, weight);
                break;
            case ActionKind.Taunt:
                ApplyTaunt(bones, phase, elapsedSec, weight);
                break;
        }
    }

    private static void ApplyHighKnees(SpineBoneCache bones, float phase, float weight)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float liftL = MathF.Max(0f, s);
        float liftR = MathF.Max(0f, -s);
        float bounce = MathF.Abs(s);

        bones.AddPosition("cog", s * 10f, bounce * 36f, weight);
        bones.AddRotation("hips", -s * 12f, weight);
        bones.AddRotation("bod_lower", s * 8f, weight);
        bones.AddRotation("bod_upper", 10f + s * 10f, weight);
        bones.AddRotation("neck", -4f + c * 6f, weight);
        bones.AddRotation("head", -8f - s * 8f, weight);

        // High-knee IK lift + upper/lower leg bend
        bones.AddPosition("leg_ik_l", liftL * 48f, liftL * 168f, weight);
        bones.AddRotation("leg_upper_l", liftL * 72f, weight);
        bones.AddRotation("leg_lower_l", -liftL * 98f, weight);
        bones.AddRotation("foot_l1", -liftL * 32f, weight);
        bones.AddRotation("foot_l2", -liftL * 18f, weight);

        bones.AddPosition("leg_ik_r", liftR * 48f, liftR * 168f, weight);
        bones.AddRotation("leg_upper_r", liftR * 72f, weight);
        bones.AddRotation("leg_lower_r", -liftR * 98f, weight);
        bones.AddRotation("foot_r1", -liftR * 32f, weight);
        bones.AddRotation("foot_r2", -liftR * 18f, weight);

        // Opposite-arm pump & sword counter-swing
        bones.AddRotation("shoulder_r", liftL * 24f - liftR * 16f, weight);
        bones.AddRotation("arm_upper_r", 42f * liftL - 30f * liftR, weight);
        bones.AddRotation("arm_lower_r", 58f * liftL + 18f * liftR, weight);
        bones.AddRotation("land_r", s * 16f, weight);
        bones.AddRotation("sword", -32f - s * 28f, weight);

        bones.AddRotation("shoulder_l", liftR * 24f - liftL * 16f, weight);
        bones.AddRotation("arm_upper_l", 45f * liftR - 32f * liftL, weight);
        bones.AddRotation("arm_lower_l", 62f * liftR + 20f * liftL, weight);
        bones.AddRotation("hand_l", -s * 18f, weight);

        // Armor plates, skirt, belt & hair follow-through
        bones.AddRotation("shoulder_plate_r", bounce * 18f + s * 12f, weight);
        bones.AddRotation("shoulder_plate_l", bounce * 18f - s * 12f, weight);
        bones.AddRotation("skirt_r", 26f + liftR * 48f + bounce * 14f, weight);
        bones.AddRotation("skirt_l", -26f - liftL * 48f - bounce * 14f, weight);
        bones.AddRotation("belt_middle", s * 26f + bounce * 16f, weight);
        bones.AddRotation("hair_1a", -18f - bounce * 22f - c * 14f, weight);
        bones.AddRotation("hair_1b", -22f - bounce * 26f + c * 18f, weight);
    }

    private static void ApplyBellyLaugh(SpineBoneCache bones, float elapsedSec, float weight)
    {
        float cycle = elapsedSec % 2.4f;
        float w1 = SmoothWindow(cycle, 0.0f, 0.9f, 0.14f);
        float w2 = SmoothWindow(cycle, 0.9f, 1.5f, 0.14f);
        float w3 = SmoothWindow(cycle, 1.5f, 2.4f, 0.16f);

        float jiggle = MathF.Sin(elapsedSec * 22f);
        float shake = MathF.Sin(elapsedSec * 26f);

        // Beat 1: Lean way back clutching belly with rapid belly-jiggle
        if (w1 > 0.001f)
        {
            float bw = weight * w1;
            bones.AddPosition("cog", jiggle * 6f, -14f + MathF.Abs(jiggle) * 10f, bw);
            bones.AddScale("bod_lower", jiggle * 0.06f, -jiggle * 0.04f, bw);
            bones.AddRotation("hips", -14f + jiggle * 6f, bw);
            bones.AddRotation("bod_lower", -18f + jiggle * 7f, bw);
            bones.AddRotation("bod_upper", -28f + jiggle * 10f, bw);
            bones.AddRotation("neck", -14f + jiggle * 8f, bw);
            bones.AddRotation("head", -24f + shake * 12f, bw);

            bones.AddRotation("arm_upper_l", 34f + jiggle * 8f, bw);
            bones.AddRotation("arm_lower_l", 78f + jiggle * 10f, bw);
            bones.AddRotation("arm_upper_r", -32f - jiggle * 8f, bw);
            bones.AddRotation("arm_lower_r", -74f - jiggle * 10f, bw);
            bones.AddRotation("sword", -40f, bw);

            bones.AddRotation("shoulder_plate_l", -16f + jiggle * 10f, bw);
            bones.AddRotation("shoulder_plate_r", 16f - jiggle * 10f, bw);
            bones.AddRotation("belt_middle", jiggle * 20f, bw);
            bones.AddRotation("hair_1a", -24f + shake * 14f, bw);
            bones.AddRotation("hair_1b", -28f - shake * 16f, bw);
        }

        // Beat 2: Hunch forward, hands raised near head/ears shaking with laughter
        if (w2 > 0.001f)
        {
            float bw = weight * w2;
            bones.AddPosition("cog", shake * 8f, -26f, bw);
            bones.AddRotation("bod_lower", 24f + shake * 5f, bw);
            bones.AddRotation("bod_upper", 32f + shake * 7f, bw);
            bones.AddRotation("neck", 16f, bw);
            bones.AddRotation("head", 20f + shake * 14f, bw);

            bones.AddRotation("shoulder_l", 26f, bw);
            bones.AddRotation("arm_upper_l", 112f + shake * 10f, bw);
            bones.AddRotation("arm_lower_l", 92f + shake * 12f, bw);
            bones.AddRotation("shoulder_r", -26f, bw);
            bones.AddRotation("arm_upper_r", -108f - shake * 10f, bw);
            bones.AddRotation("arm_lower_r", -88f - shake * 12f, bw);
            bones.AddRotation("sword", -65f, bw);

            bones.AddRotation("hair_1a", 22f + shake * 16f, bw);
            bones.AddRotation("hair_1b", 28f - shake * 18f, bw);
        }

        // Beat 3: ROFL tip-back! Drop cog -125, tilt body -65 deg, kick both legs up!
        if (w3 > 0.001f)
        {
            float bw = weight * w3;
            float kick = MathF.Sin(elapsedSec * 18f);
            float kickL = MathF.Max(0f, kick);
            float kickR = MathF.Max(0f, -kick);

            bones.AddPosition("cog", -20f, -125f + MathF.Abs(jiggle) * 8f, bw);
            bones.AddRotation("hips", -22f, bw);
            bones.AddRotation("bod_lower", -34f + jiggle * 6f, bw);
            bones.AddRotation("bod_upper", -32f + jiggle * 8f, bw);
            bones.AddRotation("neck", -10f, bw);
            bones.AddRotation("head", -18f + shake * 12f, bw);

            bones.AddPosition("leg_ik_l", 52f + kickL * 24f, 134f + kickL * 32f, bw);
            bones.AddRotation("leg_upper_l", 64f + kickL * 22f, bw);
            bones.AddRotation("leg_lower_l", -54f - kickL * 28f, bw);

            bones.AddPosition("leg_ik_r", 52f + kickR * 24f, 134f + kickR * 32f, bw);
            bones.AddRotation("leg_upper_r", 64f + kickR * 22f, bw);
            bones.AddRotation("leg_lower_r", -54f - kickR * 28f, bw);

            bones.AddRotation("arm_upper_l", 42f + jiggle * 12f, bw);
            bones.AddRotation("arm_lower_l", 70f, bw);
            bones.AddRotation("arm_upper_r", -42f - jiggle * 12f, bw);
            bones.AddRotation("arm_lower_r", -70f, bw);
            bones.AddRotation("skirt_l", -48f, bw);
            bones.AddRotation("skirt_r", 48f, bw);
            bones.AddRotation("hair_1a", -32f + shake * 12f, bw);
            bones.AddRotation("hair_1b", -36f - shake * 12f, bw);
        }
    }

    private static void ApplyJadeRabbit(SpineBoneCache bones, float phase, float weight)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float flow = MathF.Sin(phase - 0.55f);
        float liftL = MathF.Max(0f, s);
        float liftR = MathF.Max(0f, -s);

        // Sinuous S-curve hip/torso sway
        bones.AddPosition("cog", s * 28f, MathF.Abs(c) * 20f, weight);
        bones.AddRotation("hips", s * 16f, weight);
        bones.AddRotation("bod_lower", -s * 12f, weight);
        bones.AddRotation("bod_upper", -s * 18f, weight);
        bones.AddRotation("neck", s * 10f, weight);
        bones.AddRotation("head", s * 16f + c * 6f, weight);

        // Raised lotus arms & flowing shoulder plates
        bones.AddRotation("shoulder_l", 22f + c * 10f, weight);
        bones.AddRotation("arm_upper_l", 118f + s * 18f, weight);
        bones.AddRotation("arm_lower_l", 52f + flow * 22f, weight);
        bones.AddRotation("hand_l", -34f + flow * 18f, weight);

        bones.AddRotation("shoulder_r", -22f - c * 10f, weight);
        bones.AddRotation("arm_upper_r", -118f + s * 18f, weight);
        bones.AddRotation("arm_lower_r", -52f + flow * 22f, weight);
        bones.AddRotation("land_r", 34f + flow * 18f, weight);
        bones.AddRotation("sword", -18f + flow * 16f, weight);

        bones.AddRotation("shoulder_plate_l", 26f + flow * 18f, weight);
        bones.AddRotation("shoulder_plate_r", -26f + flow * 18f, weight);
        bones.AddRotation("skirt_l", -18f - flow * 28f, weight);
        bones.AddRotation("skirt_r", 18f - flow * 28f, weight);
        bones.AddRotation("belt_middle", flow * 32f, weight);
        bones.AddRotation("hair_1a", -flow * 20f, weight);
        bones.AddRotation("hair_1b", -flow * 26f, weight);

        // Alternating dainty leg lift
        bones.AddPosition("leg_ik_l", liftL * 36f, liftL * 145f, weight);
        bones.AddRotation("leg_upper_l", liftL * 54f, weight);
        bones.AddRotation("leg_lower_l", -liftL * 78f, weight);
        bones.AddRotation("foot_l1", -liftL * 38f, weight);

        bones.AddPosition("leg_ik_r", liftR * 36f, liftR * 145f, weight);
        bones.AddRotation("leg_upper_r", liftR * 54f, weight);
        bones.AddRotation("leg_lower_r", -liftR * 78f, weight);
        bones.AddRotation("foot_r1", -liftR * 38f, weight);
    }

    private static void ApplyTaunt(SpineBoneCache bones, float phase, float elapsedSec, float weight)
    {
        float cycle = elapsedSec % 2.2f;
        float w1 = SmoothWindow(cycle, 0.0f, 1.1f, 0.15f);
        float w2 = SmoothWindow(cycle, 1.1f, 2.2f, 0.15f);

        // Beat 1: Lean forward (-22 deg), stick hips back, slap/pat hip with left arm
        if (w1 > 0.001f)
        {
            float bw = weight * w1;
            float slap = MathF.Sin(phase * 2.5f);

            bones.AddPosition("cog", -18f, -12f + MathF.Abs(slap) * 8f, bw);
            bones.AddRotation("hips", 22f + slap * 8f, bw);
            bones.AddRotation("bod_lower", -14f, bw);
            bones.AddRotation("bod_upper", -22f, bw);
            bones.AddRotation("neck", 16f, bw);
            bones.AddRotation("head", 24f + MathF.Sin(phase * 2f) * 10f, bw);

            bones.AddRotation("arm_upper_l", -38f + slap * 28f, bw);
            bones.AddRotation("arm_lower_l", 64f + slap * 34f, bw);
            bones.AddRotation("hand_l", slap * 24f, bw);

            bones.AddRotation("arm_upper_r", -24f, bw);
            bones.AddRotation("arm_lower_r", -42f, bw);
            bones.AddRotation("skirt_l", -26f - slap * 18f, bw);
            bones.AddRotation("skirt_r", 26f + slap * 18f, bw);
            bones.AddRotation("belt_middle", slap * 22f, bw);
        }

        // Beat 2: Stand up cockily, beckon/wave right hand provocatively, head cocked & bobbing
        if (w2 > 0.001f)
        {
            float bw = weight * w2;
            float beckon = MathF.Sin(phase * 2.2f);

            bones.AddPosition("cog", 12f, 14f, bw);
            bones.AddRotation("hips", -10f, bw);
            bones.AddRotation("bod_upper", 14f, bw);
            bones.AddRotation("head", -14f + beckon * 10f, bw);

            bones.AddRotation("shoulder_r", -18f, bw);
            bones.AddRotation("arm_upper_r", -88f + beckon * 18f, bw);
            bones.AddRotation("arm_lower_r", -54f + beckon * 38f, bw);
            bones.AddRotation("land_r", beckon * 26f, bw);
            bones.AddRotation("sword", -28f + beckon * 14f, bw);

            bones.AddRotation("arm_upper_l", -28f, bw);
            bones.AddRotation("arm_lower_l", 76f, bw);
            bones.AddRotation("shoulder_plate_r", -18f + beckon * 10f, bw);
            bones.AddRotation("hair_1a", beckon * 12f, bw);
        }
    }

    private static float SmoothWindow(float t, float start, float end, float fade)
    {
        if (t < start - fade || t > end + fade)
            return 0f;
        float enter = SmoothStep(start - fade, start + fade, t);
        float exit = 1f - SmoothStep(end - fade, end + fade, t);
        return MathF.Min(enter, exit);
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Math.Clamp((x - edge0) / MathF.Max(0.0001f, edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    public void Dispose() { }
}
