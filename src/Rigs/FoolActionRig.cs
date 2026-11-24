using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

internal sealed class FoolActionRig : ICharacterActionRig
{
    public static readonly string[] TrackedBones =
    {
        "cog",
        "body_lower",
        "body_mid",
        "body_upper",
        "hood_base",
        "head",
        "ear_l1",
        "ear_l2",
        "ear_l3",
        "bell_l",
        "ear_r1",
        "ear_r2",
        "ear_r3",
        "bell_r",
        "hips",
        "leg_upper_r",
        "leg_lower_r",
        "foot_1_r",
        "foot_2_r",
        "leg_r_ik_const",
        "leg_upper_l",
        "leg_lower_l",
        "foot_1_l",
        "foot_2_l",
        "leg_l_ik_const",
        "arm_base_r",
        "arm_upper_r",
        "arm_lower_r",
        "wrist_r",
        "hand_r",
        "arm_base_l",
        "arm_upper_l",
        "arm_lower_l",
        "wrist_l",
        "hand_l",
        "yellow_skirt_r1",
        "yellow_skirt_r2",
        "yellow_skirt_l1",
        "yellow_skirt_l2",
        "front_flap1",
        "front_flap2",
        "cape_l1",
        "cape_l2",
        "cape_r1",
        "cape_r2",
        "bow_attach",
        "bow",
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
                ApplyBellyLaugh(bones, phase, weight);
                break;
            case ActionKind.JadeRabbit:
                ApplyJadeRabbit(bones, phase, weight);
                break;
            case ActionKind.Taunt:
                ApplyTaunt(bones, phase, weight);
                break;
        }
    }

    private static void ApplyHighKnees(SpineBoneCache bones, float phase, float weight)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float bounce = MathF.Abs(s);
        float leftLift = MathF.Max(0f, s);
        float rightLift = MathF.Max(0f, -s);

        // Bouncy jester cog & torso twist
        bones.AddPosition("cog", 0f, bounce * 42f, weight);
        bones.AddRotation("cog", s * 6f, weight);
        bones.AddRotation("hips", s * 10f, weight);
        bones.AddRotation("body_lower", -s * 5f, weight);
        bones.AddRotation("body_mid", -s * 5f, weight);
        bones.AddRotation("body_upper", -s * 6f, weight);
        bones.AddRotation("head", bounce * 10f - 4f, weight);

        // High-knee IK lifts (+130..+160 range) + leg angle reinforcement
        bones.AddPosition("leg_r_ik_const", 0f, rightLift * 148f, weight);
        bones.AddPosition("leg_l_ik_const", 0f, leftLift * 148f, weight);
        bones.AddRotation("leg_upper_r", rightLift * 34f, weight);
        bones.AddRotation("leg_lower_r", -rightLift * 42f, weight);
        bones.AddRotation("foot_1_r", rightLift * 24f, weight);
        bones.AddRotation("foot_2_r", rightLift * 18f, weight);
        bones.AddRotation("leg_upper_l", leftLift * 34f, weight);
        bones.AddRotation("leg_lower_l", -leftLift * 42f, weight);
        bones.AddRotation("foot_1_l", leftLift * 24f, weight);
        bones.AddRotation("foot_2_l", leftLift * 18f, weight);

        // Opposite-phase arm pumping
        bones.AddRotation("arm_base_l", -s * 12f, weight);
        bones.AddRotation("arm_upper_l", -s * 48f + 14f, weight);
        bones.AddRotation("arm_lower_l", -54f - rightLift * 28f, weight);
        bones.AddRotation("wrist_l", -s * 18f, weight);
        bones.AddRotation("hand_l", -s * 14f, weight);

        bones.AddRotation("arm_base_r", s * 12f, weight);
        bones.AddRotation("arm_upper_r", s * 48f + 14f, weight);
        bones.AddRotation("arm_lower_r", -54f - leftLift * 28f, weight);
        bones.AddRotation("wrist_r", s * 18f, weight);
        bones.AddRotation("hand_r", s * 14f, weight);

        // Whipping twin ears & jingling bells
        ApplyEars(bones, -10f + c * 26f, -14f - s * 28f, -12f + c * 24f, s * 22f, weight);

        // Billowing skirt, flaps, cape & bow
        ApplyCloth(bones, bounce * 34f, s * 22f, c * 26f, weight);
    }

    private static void ApplyBellyLaugh(SpineBoneCache bones, float phase, float weight)
    {
        float cycle = PositiveMod(phase / (MathF.PI * 2f), 1f);
        float shake = MathF.Sin(phase * 6f);
        float kick = MathF.Sin(phase * 5f);

        float beat1 = BandWeight(cycle, 0.00f, 0.33f); // Lean way back clutching belly
        float beat2 = BandWeight(cycle, 0.33f, 0.66f); // Hunch forward holding head/ears
        float beat3 = BandWeight(cycle, 0.66f, 1.00f); // ROFL tip-back kicking legs

        float cogY = beat1 * 10f + beat2 * -10f + beat3 * 26f + MathF.Abs(shake) * 8f;
        float cogRot = beat1 * -18f + beat2 * 16f + beat3 * -28f;
        float torsoRot = beat1 * -9f + beat2 * 11f + beat3 * -8f + shake * 2.5f;

        bones.AddPosition("cog", 0f, cogY, weight);
        bones.AddRotation("cog", cogRot, weight);
        bones.AddRotation("hips", torsoRot, weight);
        bones.AddRotation("body_lower", torsoRot, weight);
        bones.AddRotation("body_mid", torsoRot, weight);
        bones.AddRotation("body_upper", torsoRot * 1.2f, weight);
        bones.AddRotation("hood_base", beat1 * 10f - beat2 * 8f, weight);
        bones.AddRotation("head", beat1 * 24f + beat2 * 18f + beat3 * 24f + shake * 8f, weight);

        // Arms: beat1 clutch belly, beat2 hold ears/head, beat3 flail wildly
        bones.AddRotation("arm_upper_l", beat1 * 42f + beat2 * -82f + beat3 * (-48f + kick * 26f), weight);
        bones.AddRotation("arm_lower_l", beat1 * -98f + beat2 * -108f + beat3 * (-54f - kick * 22f), weight);
        bones.AddRotation("arm_upper_r", beat1 * 42f + beat2 * -82f + beat3 * (-48f - kick * 26f), weight);
        bones.AddRotation("arm_lower_r", beat1 * -98f + beat2 * -108f + beat3 * (-54f + kick * 22f), weight);

        // Beat 3 ROFL kicking legs via IK (+130..+160 range)
        float kickR = MathF.Max(0f, kick);
        float kickL = MathF.Max(0f, -kick);
        bones.AddPosition("leg_r_ik_const", 0f, beat3 * (40f + kickR * 115f), weight);
        bones.AddPosition("leg_l_ik_const", 0f, beat3 * (40f + kickL * 115f), weight);
        bones.AddRotation("leg_upper_r", beat3 * kickR * 36f, weight);
        bones.AddRotation("leg_upper_l", beat3 * kickL * 36f, weight);

        // Ears flopping & flying in laughter
        ApplyEars(
            bones,
            beat1 * -24f + beat2 * 28f + beat3 * (-34f + shake * 22f),
            beat1 * -20f + beat2 * 24f + beat3 * (-28f - shake * 26f),
            shake * 28f,
            shake * 24f,
            weight);

        ApplyCloth(bones, beat3 * 46f + MathF.Abs(shake) * 18f, shake * 24f, beat3 * 36f, weight);
    }

    private static void ApplyJadeRabbit(SpineBoneCache bones, float phase, float weight)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float s2 = MathF.Sin(phase - 0.55f);
        float leftLift = MathF.Max(0f, s);
        float rightLift = MathF.Max(0f, -s);

        // Sinuous S-curve princess sway
        bones.AddPosition("cog", s * 26f, MathF.Abs(c) * 20f, weight);
        bones.AddRotation("cog", s * 8f, weight);
        bones.AddRotation("hips", s * 14f, weight);
        bones.AddRotation("body_lower", s * 7f, weight);
        bones.AddRotation("body_mid", -s * 6f, weight);
        bones.AddRotation("body_upper", -s * 8f, weight);
        bones.AddRotation("head", s2 * 12f, weight);

        // Dainty alternating leg lift
        bones.AddPosition("leg_r_ik_const", 0f, rightLift * 84f, weight);
        bones.AddPosition("leg_l_ik_const", 0f, leftLift * 84f, weight);
        bones.AddRotation("foot_1_r", rightLift * 26f, weight);
        bones.AddRotation("foot_1_l", leftLift * 26f, weight);

        // Lotus hands & graceful sleeves
        bones.AddRotation("arm_base_l", -14f + s * 10f, weight);
        bones.AddRotation("arm_upper_l", -66f + s * 18f, weight);
        bones.AddRotation("arm_lower_l", -68f - c * 18f, weight);
        bones.AddRotation("wrist_l", 26f + s2 * 24f, weight);
        bones.AddRotation("hand_l", 18f + s2 * 18f, weight);

        bones.AddRotation("arm_base_r", 14f + s * 10f, weight);
        bones.AddRotation("arm_upper_r", -60f - s * 18f, weight);
        bones.AddRotation("arm_lower_r", -62f + c * 18f, weight);
        bones.AddRotation("wrist_r", -26f - s2 * 24f, weight);
        bones.AddRotation("hand_r", -18f - s2 * 18f, weight);

        // Tall, perky Jade Rabbit ears swaying gracefully!
        bones.AddRotation("hood_base", -8f, weight);
        bones.AddRotation("ear_l1", -38f + s * 12f, weight);
        bones.AddRotation("ear_l2", -24f + s2 * 18f, weight);
        bones.AddRotation("ear_l3", -16f + c * 22f, weight);
        bones.AddRotation("bell_l", s2 * 28f, weight);

        bones.AddRotation("ear_r1", 38f + s * 12f, weight);
        bones.AddRotation("ear_r2", 24f + s2 * 18f, weight);
        bones.AddRotation("ear_r3", 16f + c * 22f, weight);
        bones.AddRotation("bell_r", -s2 * 28f, weight);

        ApplyCloth(bones, 22f + MathF.Abs(c) * 18f, s * 20f, c * 18f, weight);
    }

    private static void ApplyTaunt(SpineBoneCache bones, float phase, float weight)
    {
        float cycle = PositiveMod(phase / (MathF.PI * 2f), 1f);
        float patWeight = SmoothStep01(1f - MathF.Abs(cycle - 0.25f) * 4f);
        float beckonWeight = SmoothStep01(1f - MathF.Abs(cycle - 0.75f) * 4f);
        float wave = MathF.Sin(phase * 4f);
        float slap = MathF.Abs(wave);

        // Lean forward slapping/patting hip, then cocky beckoning lean-back
        bones.AddPosition("cog", patWeight * -16f + beckonWeight * 12f, slap * 10f, weight);
        bones.AddRotation("cog", patWeight * 8f - beckonWeight * 8f, weight);
        bones.AddRotation("hips", patWeight * (-18f + wave * 14f) + beckonWeight * 8f, weight);
        bones.AddRotation("body_lower", patWeight * 10f - beckonWeight * 5f, weight);
        bones.AddRotation("body_mid", patWeight * 10f - beckonWeight * 5f, weight);
        bones.AddRotation("body_upper", patWeight * 12f - beckonWeight * 10f, weight);
        bones.AddRotation("head", patWeight * -10f + beckonWeight * (wave * 16f), weight);

        // Right arm: rhythmic hip/butt patting
        bones.AddRotation("arm_upper_r", patWeight * (36f + slap * 24f), weight);
        bones.AddRotation("arm_lower_r", patWeight * (-74f - slap * 26f), weight);
        bones.AddRotation("wrist_r", patWeight * (wave * 26f), weight);
        bones.AddRotation("hand_r", patWeight * (wave * 20f), weight);

        // Left arm: cocky beckoning wave
        bones.AddRotation("arm_base_l", beckonWeight * -16f, weight);
        bones.AddRotation("arm_upper_l", beckonWeight * -74f, weight);
        bones.AddRotation("arm_lower_l", beckonWeight * (-52f + wave * 30f), weight);
        bones.AddRotation("wrist_l", beckonWeight * (20f + wave * 34f), weight);
        bones.AddRotation("hand_l", beckonWeight * (wave * 24f), weight);

        // Cheeky ear waggle & skirt bounce on pat
        ApplyEars(bones, wave * 18f, -wave * 22f, wave * 20f, wave * 24f, weight);
        ApplyCloth(bones, patWeight * slap * 36f + 16f, wave * 20f, slap * 24f, weight);
    }

    private static void ApplyEars(
        SpineBoneCache bones,
        float baseAngle,
        float midAngle,
        float tipAngle,
        float bellAngle,
        float weight)
    {
        bones.AddRotation("ear_l1", baseAngle, weight);
        bones.AddRotation("ear_l2", midAngle, weight);
        bones.AddRotation("ear_l3", tipAngle, weight);
        bones.AddRotation("bell_l", bellAngle, weight);

        bones.AddRotation("ear_r1", -baseAngle, weight);
        bones.AddRotation("ear_r2", -midAngle, weight);
        bones.AddRotation("ear_r3", -tipAngle, weight);
        bones.AddRotation("bell_r", -bellAngle, weight);
    }

    private static void ApplyCloth(
        SpineBoneCache bones,
        float flare,
        float sway,
        float flutter,
        float weight)
    {
        bones.AddRotation("yellow_skirt_r1", flare + sway * 0.4f, weight);
        bones.AddRotation("yellow_skirt_r2", flare * 0.7f + flutter, weight);
        bones.AddRotation("yellow_skirt_l1", -flare + sway * 0.4f, weight);
        bones.AddRotation("yellow_skirt_l2", -flare * 0.7f - flutter, weight);

        bones.AddRotation("front_flap1", flare * 0.8f + sway * 0.5f, weight);
        bones.AddRotation("front_flap2", flare * 0.6f + flutter, weight);

        bones.AddRotation("cape_l1", -flare * 0.85f + sway * 0.5f, weight);
        bones.AddRotation("cape_l2", -flare * 0.7f - flutter, weight);
        bones.AddRotation("cape_r1", flare * 0.85f + sway * 0.5f, weight);
        bones.AddRotation("cape_r2", flare * 0.7f + flutter, weight);

        bones.AddRotation("bow_attach", sway * 0.5f, weight);
        bones.AddRotation("bow", flutter * 0.8f, weight);
    }

    private static float PositiveMod(float value, float modulus)
    {
        float r = value % modulus;
        return r < 0f ? r + modulus : r;
    }

    private static float BandWeight(float t, float start, float end)
    {
        float mid = (start + end) * 0.5f;
        float half = (end - start) * 0.5f;
        float dist = MathF.Abs(t - mid);
        return SmoothStep01(1f - dist / MathF.Max(0.001f, half));
    }

    private static float SmoothStep01(float x)
    {
        float t = Math.Clamp(x, 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    public void Dispose() { }
}
