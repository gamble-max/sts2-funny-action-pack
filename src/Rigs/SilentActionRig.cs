using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

internal sealed class SilentActionRig : ICharacterActionRig
{
    public static readonly string[] TrackedBones =
    {
        "cog", "hips", "body_1", "body_2", "body_3", "head",
        "leg_upper_r", "leg_lower_r", "foot_r", "toe_r", "leg_r_ik",
        "leg_upper_l", "leg_ik_l", "foot_l", "toe_l",
        "shoulder_r", "arm_upper_r", "arm_lower_r", "hand_r", "weapon_r",
        "shoulder_l", "arm_upper_l", "arm_lower_l", "hand_l", "weapon_l",
        "crotch_cloth", "crotch_cloth2", "leg_flap", "leg_flap2",
        "arm_flap_l", "arm_flap_l2", "arm_flap_l3",
        "arm_flap_r", "arm_flap_r2", "arm_flap_r3",
        "hair1", "hair2", "hair3"
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

        bones.AddPosition("cog", s * 10f, bounce * 38f, weight);
        bones.AddRotation("hips", -s * 12f, weight);
        bones.AddRotation("body_1", s * 8f, weight);
        bones.AddRotation("body_2", 8f + s * 10f, weight);
        bones.AddRotation("body_3", 6f + c * 6f, weight);
        bones.AddRotation("head", -10f - s * 8f, weight);

        // High-knee IK lift + upper/lower leg bend
        bones.AddPosition("leg_ik_l", liftL * 48f, liftL * 172f, weight);
        bones.AddRotation("leg_upper_l", liftL * 74f, weight);
        bones.AddRotation("foot_l", -liftL * 34f, weight);
        bones.AddRotation("toe_l", -liftL * 20f, weight);

        bones.AddPosition("leg_r_ik", liftR * 48f, liftR * 172f, weight);
        bones.AddRotation("leg_upper_r", liftR * 74f, weight);
        bones.AddRotation("leg_lower_r", -liftR * 96f, weight);
        bones.AddRotation("foot_r", -liftR * 34f, weight);
        bones.AddRotation("toe_r", -liftR * 20f, weight);

        // Opposite-arm pump & dagger counter-swing
        bones.AddRotation("shoulder_r", liftL * 22f - liftR * 14f, weight);
        bones.AddRotation("arm_upper_r", 44f * liftL - 32f * liftR, weight);
        bones.AddRotation("arm_lower_r", 62f * liftL + 18f * liftR, weight);
        bones.AddRotation("hand_r", s * 16f, weight);
        bones.AddRotation("weapon_r", -24f - s * 32f, weight);

        bones.AddRotation("shoulder_l", liftR * 22f - liftL * 14f, weight);
        bones.AddRotation("arm_upper_l", 44f * liftR - 32f * liftL, weight);
        bones.AddRotation("arm_lower_l", 62f * liftR + 18f * liftL, weight);
        bones.AddRotation("hand_l", -s * 16f, weight);
        bones.AddRotation("weapon_l", -24f + s * 32f, weight);

        // Cloth, leg flaps, sleeve flaps & hair billow
        bones.AddRotation("crotch_cloth", s * 28f + bounce * 18f, weight);
        bones.AddRotation("crotch_cloth2", -s * 24f + bounce * 22f, weight);
        bones.AddRotation("leg_flap", 24f + liftR * 46f, weight);
        bones.AddRotation("leg_flap2", 28f + liftR * 52f, weight);
        AddSleeveWave(bones, "arm_flap_l", 18f + bounce * 20f, c * 16f, weight);
        AddSleeveWave(bones, "arm_flap_r", -18f - bounce * 20f, -c * 16f, weight);
        bones.AddRotation("hair1", -16f - bounce * 18f, weight);
        bones.AddRotation("hair2", -20f - bounce * 24f - c * 14f, weight);
        bones.AddRotation("hair3", -24f - bounce * 28f + c * 16f, weight);
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
            bones.AddPosition("cog", jiggle * 6f, -12f + MathF.Abs(jiggle) * 10f, bw);
            bones.AddScale("body_1", jiggle * 0.06f, -jiggle * 0.04f, bw);
            bones.AddRotation("hips", -14f + jiggle * 6f, bw);
            bones.AddRotation("body_1", -16f + jiggle * 6f, bw);
            bones.AddRotation("body_2", -24f + jiggle * 10f, bw);
            bones.AddRotation("body_3", -16f + jiggle * 8f, bw);
            bones.AddRotation("head", -22f + shake * 12f, bw);

            bones.AddRotation("arm_upper_l", 36f + jiggle * 8f, bw);
            bones.AddRotation("arm_lower_l", 76f + jiggle * 10f, bw);
            bones.AddRotation("arm_upper_r", -36f - jiggle * 8f, bw);
            bones.AddRotation("arm_lower_r", -76f - jiggle * 10f, bw);
            bones.AddRotation("weapon_l", -30f, bw);
            bones.AddRotation("weapon_r", 30f, bw);

            AddSleeveWave(bones, "arm_flap_l", jiggle * 14f, shake * 10f, bw);
            AddSleeveWave(bones, "arm_flap_r", -jiggle * 14f, -shake * 10f, bw);
            bones.AddRotation("hair1", -20f + shake * 12f, bw);
            bones.AddRotation("hair2", -24f - shake * 14f, bw);
            bones.AddRotation("hair3", -28f + shake * 16f, bw);
        }

        // Beat 2: Hunch forward, hands raised near head/ears shaking with laughter
        if (w2 > 0.001f)
        {
            float bw = weight * w2;
            bones.AddPosition("cog", shake * 8f, -24f, bw);
            bones.AddRotation("body_1", 20f + shake * 5f, bw);
            bones.AddRotation("body_2", 28f + shake * 7f, bw);
            bones.AddRotation("body_3", 18f, bw);
            bones.AddRotation("head", 20f + shake * 14f, bw);

            bones.AddRotation("shoulder_l", 24f, bw);
            bones.AddRotation("arm_upper_l", 112f + shake * 10f, bw);
            bones.AddRotation("arm_lower_l", 92f + shake * 12f, bw);
            bones.AddRotation("shoulder_r", -24f, bw);
            bones.AddRotation("arm_upper_r", -112f - shake * 10f, bw);
            bones.AddRotation("arm_lower_r", -92f - shake * 12f, bw);

            bones.AddRotation("hair1", 20f + shake * 14f, bw);
            bones.AddRotation("hair2", 26f - shake * 16f, bw);
        }

        // Beat 3: ROFL tip-back! Drop cog -125, tilt body -65 deg, kick both legs up!
        if (w3 > 0.001f)
        {
            float bw = weight * w3;
            float kick = MathF.Sin(elapsedSec * 18f);
            float kickL = MathF.Max(0f, kick);
            float kickR = MathF.Max(0f, -kick);

            bones.AddPosition("cog", -18f, -125f + MathF.Abs(jiggle) * 8f, bw);
            bones.AddRotation("hips", -20f, bw);
            bones.AddRotation("body_1", -24f + jiggle * 6f, bw);
            bones.AddRotation("body_2", -24f + jiggle * 8f, bw);
            bones.AddRotation("body_3", -18f, bw);
            bones.AddRotation("head", -16f + shake * 12f, bw);

            bones.AddPosition("leg_ik_l", 52f + kickL * 24f, 134f + kickL * 32f, bw);
            bones.AddRotation("leg_upper_l", 64f + kickL * 22f, bw);

            bones.AddPosition("leg_r_ik", 52f + kickR * 24f, 134f + kickR * 32f, bw);
            bones.AddRotation("leg_upper_r", 64f + kickR * 22f, bw);
            bones.AddRotation("leg_lower_r", -54f - kickR * 28f, bw);

            bones.AddRotation("arm_upper_l", 44f + jiggle * 12f, bw);
            bones.AddRotation("arm_lower_l", 68f, bw);
            bones.AddRotation("arm_upper_r", -44f - jiggle * 12f, bw);
            bones.AddRotation("arm_lower_r", -68f, bw);
            bones.AddRotation("crotch_cloth", -42f, bw);
            bones.AddRotation("leg_flap", 42f, bw);
            AddSleeveWave(bones, "arm_flap_l", -34f, shake * 14f, bw);
            AddSleeveWave(bones, "arm_flap_r", 34f, -shake * 14f, bw);
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
        bones.AddRotation("body_1", -s * 10f, weight);
        bones.AddRotation("body_2", -s * 16f, weight);
        bones.AddRotation("body_3", s * 8f, weight);
        bones.AddRotation("head", s * 16f + c * 6f, weight);

        // Raised lotus arms & flowing sleeves
        bones.AddRotation("shoulder_l", 20f + c * 10f, weight);
        bones.AddRotation("arm_upper_l", 120f + s * 18f, weight);
        bones.AddRotation("arm_lower_l", 50f + flow * 22f, weight);
        bones.AddRotation("hand_l", -36f + flow * 18f, weight);
        bones.AddRotation("weapon_l", 18f + flow * 14f, weight);

        bones.AddRotation("shoulder_r", -20f - c * 10f, weight);
        bones.AddRotation("arm_upper_r", -120f + s * 18f, weight);
        bones.AddRotation("arm_lower_r", -50f + flow * 22f, weight);
        bones.AddRotation("hand_r", 36f + flow * 18f, weight);
        bones.AddRotation("weapon_r", -18f + flow * 14f, weight);

        AddSleeveWave(bones, "arm_flap_l", 26f, flow * 28f, weight);
        AddSleeveWave(bones, "arm_flap_r", -26f, flow * 28f, weight);
        bones.AddRotation("crotch_cloth", flow * 28f, weight);
        bones.AddRotation("crotch_cloth2", flow * 34f, weight);
        bones.AddRotation("leg_flap", -flow * 26f, weight);
        bones.AddRotation("leg_flap2", -flow * 32f, weight);
        bones.AddRotation("hair1", -flow * 18f, weight);
        bones.AddRotation("hair2", -flow * 24f, weight);
        bones.AddRotation("hair3", -flow * 28f, weight);

        // Alternating dainty leg lift
        bones.AddPosition("leg_ik_l", liftL * 36f, liftL * 145f, weight);
        bones.AddRotation("leg_upper_l", liftL * 54f, weight);
        bones.AddRotation("foot_l", -liftL * 38f, weight);
        bones.AddRotation("toe_l", -liftL * 24f, weight);

        bones.AddPosition("leg_r_ik", liftR * 36f, liftR * 145f, weight);
        bones.AddRotation("leg_upper_r", liftR * 54f, weight);
        bones.AddRotation("leg_lower_r", -liftR * 78f, weight);
        bones.AddRotation("foot_r", -liftR * 38f, weight);
        bones.AddRotation("toe_r", -liftR * 24f, weight);
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
            bones.AddRotation("body_1", -12f, bw);
            bones.AddRotation("body_2", -22f, bw);
            bones.AddRotation("body_3", 10f, bw);
            bones.AddRotation("head", 24f + MathF.Sin(phase * 2f) * 10f, bw);

            bones.AddRotation("arm_upper_l", -38f + slap * 28f, bw);
            bones.AddRotation("arm_lower_l", 64f + slap * 34f, bw);
            bones.AddRotation("hand_l", slap * 24f, bw);

            bones.AddRotation("arm_upper_r", -24f, bw);
            bones.AddRotation("arm_lower_r", -42f, bw);
            bones.AddRotation("crotch_cloth", slap * 26f, bw);
            bones.AddRotation("leg_flap", 22f + slap * 20f, bw);
        }

        // Beat 2: Stand up cockily, beckon/wave right hand provocatively, head cocked & bobbing
        if (w2 > 0.001f)
        {
            float bw = weight * w2;
            float beckon = MathF.Sin(phase * 2.2f);

            bones.AddPosition("cog", 12f, 14f, bw);
            bones.AddRotation("hips", -10f, bw);
            bones.AddRotation("body_2", 14f, bw);
            bones.AddRotation("head", -14f + beckon * 10f, bw);

            bones.AddRotation("shoulder_r", -18f, bw);
            bones.AddRotation("arm_upper_r", -88f + beckon * 18f, bw);
            bones.AddRotation("arm_lower_r", -54f + beckon * 38f, bw);
            bones.AddRotation("hand_r", beckon * 28f, bw);
            bones.AddRotation("weapon_r", -20f + beckon * 16f, bw);

            bones.AddRotation("arm_upper_l", -28f, bw);
            bones.AddRotation("arm_lower_l", 76f, bw);
            AddSleeveWave(bones, "arm_flap_r", -18f, beckon * 20f, bw);
            bones.AddRotation("hair1", beckon * 12f, bw);
        }
    }

    private static void AddSleeveWave(SpineBoneCache bones, string prefix, float baseDeg, float waveDeg, float weight)
    {
        bones.AddRotation(prefix, baseDeg + waveDeg * 0.7f, weight);
        bones.AddRotation(prefix + "2", baseDeg * 0.8f + waveDeg, weight);
        bones.AddRotation(prefix + "3", baseDeg * 0.6f + waveDeg * 1.25f, weight);
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
