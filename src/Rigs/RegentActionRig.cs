using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

internal sealed class RegentActionRig : ICharacterActionRig
{
    public static readonly string[] TrackedBones =
    {
        "throne_adjust",
        "throne_bone",
        "minion_r_cog",
        "minion_r_arm_1",
        "minion_r_arm_2",
        "minion_l_cog",
        "minion_l_leg_1",
        "minion_l_leg_2",
        "minion_l_arm_1",
        "minion_r_leg_ik1_const",
        "minion_r_leg_ik2_const",
        "minion_l_leg_ik1_const",
        "minion_l_leg_ik2_const",
        "butt_attach",
        "regent_cog",
        "body",
        "neck",
        "head",
        "shoulder_r",
        "arm_upper_r",
        "arm_lower_r",
        "hand_r",
        "elbow_r_ik",
        "shoulder_l",
        "arm_upper_l",
        "arm_lower_l",
        "hand",
        "index_l",
        "index_l2",
        "pinky_l1",
        "thumb_l",
        "leg_upper_l",
        "leg_lower_l",
        "foot_l",
        "leg_r_upper",
        "leg_r_lower",
        "regent_cog4",
        "weapons_base",
        "weapol_large",
        "weapon_med",
        "weapon_small",
        "weapol_large2",
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

        // Floating royal bounce
        bones.AddPosition("regent_cog", 0f, bounce * 40f, weight);
        bones.AddRotation("regent_cog", -s * 6f, weight);

        bones.AddRotation("body", s * 7f, weight);
        bones.AddRotation("neck", -s * 4f, weight);
        bones.AddRotation("head", bounce * 8f - 4f, weight);

        // Alternating high knees
        bones.AddRotation("leg_upper_l", leftLift * 92f - rightLift * 18f, weight);
        bones.AddRotation("leg_lower_l", -leftLift * 108f, weight);
        bones.AddRotation("foot_l", leftLift * 24f, weight);

        bones.AddRotation("leg_r_upper", rightLift * 92f - leftLift * 18f, weight);
        bones.AddRotation("leg_r_lower", -rightLift * 108f, weight);

        // Opposite marching arms
        bones.AddRotation("shoulder_l", -s * 14f, weight);
        bones.AddRotation("arm_upper_l", -s * 46f + 16f, weight);
        bones.AddRotation("arm_lower_l", -52f - rightLift * 26f, weight);
        bones.AddRotation("hand", -s * 18f, weight);

        bones.AddRotation("shoulder_r", s * 14f, weight);
        bones.AddRotation("arm_upper_r", s * 46f + 16f, weight);
        bones.AddRotation("arm_lower_r", -52f - leftLift * 26f, weight);
        bones.AddRotation("hand_r", s * 18f, weight);
        bones.AddPosition("elbow_r_ik", s * 22f, leftLift * 26f, weight);

        // Minions marching enthusiastically alongside Regent
        bones.AddPosition("minion_r_cog", 0f, rightLift * 36f, weight);
        bones.AddRotation("minion_r_cog", c * 10f, weight);
        bones.AddRotation("minion_r_arm_1", -34f + s * 32f, weight);
        bones.AddRotation("minion_r_arm_2", -28f - s * 24f, weight);
        bones.AddPosition("minion_r_leg_ik1_const", 0f, leftLift * 28f, weight);
        bones.AddPosition("minion_r_leg_ik2_const", 0f, rightLift * 28f, weight);

        bones.AddPosition("minion_l_cog", 0f, leftLift * 36f, weight);
        bones.AddRotation("minion_l_cog", -c * 10f, weight);
        bones.AddRotation("minion_l_arm_1", -34f - s * 32f, weight);
        bones.AddRotation("minion_l_leg_1", rightLift * 36f, weight);
        bones.AddRotation("minion_l_leg_2", -rightLift * 44f, weight);
        bones.AddPosition("minion_l_leg_ik1_const", 0f, rightLift * 28f, weight);
        bones.AddPosition("minion_l_leg_ik2_const", 0f, leftLift * 28f, weight);

        // Cosmic weapons bouncing on beat
        bones.AddPosition("weapons_base", 0f, bounce * 24f, weight);
        bones.AddRotation("weapons_base", s * 8f, weight);
        bones.AddRotation("weapol_large", -s * 18f, weight);
        bones.AddRotation("weapon_med", s * 22f, weight);
        bones.AddRotation("weapon_small", -c * 24f, weight);
        bones.AddRotation("weapol_large2", c * 18f, weight);
    }

    private static void ApplyBellyLaugh(SpineBoneCache bones, float phase, float weight)
    {
        float cycle = PositiveMod(phase / (MathF.PI * 2f), 1f);
        float shake = MathF.Sin(phase * 6f);
        float kick = MathF.Sin(phase * 5f);

        float beat1 = BandWeight(cycle, 0.00f, 0.33f); // Lean way back clutching belly
        float beat2 = BandWeight(cycle, 0.33f, 0.66f); // Hunch forward holding head
        float beat3 = BandWeight(cycle, 0.66f, 1.00f); // ROFL tip-back kicking legs

        float cogY = beat1 * 10f + beat2 * -8f + beat3 * 28f + MathF.Abs(shake) * 8f;
        float cogRot = beat1 * -16f + beat2 * 14f + beat3 * -26f;
        float bodyRot = beat1 * -22f + beat2 * 26f + beat3 * -24f + shake * 5f;
        float headRot = beat1 * 24f + beat2 * 16f + beat3 * 22f + shake * 8f;

        bones.AddPosition("regent_cog", 0f, cogY, weight);
        bones.AddRotation("regent_cog", cogRot, weight);

        bones.AddRotation("body", bodyRot, weight);
        bones.AddRotation("neck", beat2 * 12f - beat1 * 8f, weight);
        bones.AddRotation("head", headRot, weight);

        // Arms: beat1 clutch belly, beat2 hold head/crown, beat3 flail in ROFL
        float armUpperL = beat1 * 38f + beat2 * -78f + beat3 * (-45f + kick * 24f);
        float armLowerL = beat1 * -96f + beat2 * -105f + beat3 * (-55f - kick * 20f);
        float armUpperR = beat1 * 38f + beat2 * -78f + beat3 * (-45f - kick * 24f);
        float armLowerR = beat1 * -96f + beat2 * -105f + beat3 * (-55f + kick * 20f);

        bones.AddRotation("arm_upper_l", armUpperL, weight);
        bones.AddRotation("arm_lower_l", armLowerL, weight);
        bones.AddRotation("arm_upper_r", armUpperR, weight);
        bones.AddRotation("arm_lower_r", armLowerR, weight);
        bones.AddPosition("elbow_r_ik", beat1 * -24f, beat2 * 45f, weight);

        // Legs: beat3 wild ROFL kicks
        float kickL = MathF.Max(0f, kick);
        float kickR = MathF.Max(0f, -kick);
        bones.AddRotation("leg_upper_l", beat3 * (68f + kickL * 42f), weight);
        bones.AddRotation("leg_lower_l", beat3 * (-42f - kickL * 48f), weight);
        bones.AddRotation("leg_r_upper", beat3 * (68f + kickR * 42f), weight);
        bones.AddRotation("leg_r_lower", beat3 * (-42f - kickR * 48f), weight);

        // Minions roaring with laughter alongside Regent
        bones.AddPosition("minion_r_cog", 0f, MathF.Abs(shake) * 24f + beat3 * 16f, weight);
        bones.AddRotation("minion_r_cog", beat1 * -18f + beat2 * 22f + beat3 * -24f, weight);
        bones.AddRotation("minion_r_arm_1", -55f + shake * 22f, weight);
        bones.AddRotation("minion_r_arm_2", -45f - shake * 18f, weight);

        bones.AddPosition("minion_l_cog", 0f, MathF.Abs(shake) * 24f + beat3 * 16f, weight);
        bones.AddRotation("minion_l_cog", beat1 * -18f + beat2 * 22f + beat3 * -24f, weight);
        bones.AddRotation("minion_l_arm_1", -55f - shake * 22f, weight);
        bones.AddRotation("minion_l_leg_1", beat3 * kickR * 45f, weight);

        // Cosmic swords wobble in amusement
        bones.AddRotation("weapol_large", shake * 18f + beat3 * 22f, weight);
        bones.AddRotation("weapon_med", -shake * 22f - beat3 * 18f, weight);
        bones.AddRotation("weapon_small", shake * 26f, weight);
        bones.AddRotation("weapol_large2", -shake * 16f, weight);
    }

    private static void ApplyJadeRabbit(SpineBoneCache bones, float phase, float weight)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float s2 = MathF.Sin(phase - 0.6f);
        float leftLift = MathF.Max(0f, s);
        float rightLift = MathF.Max(0f, -s);

        // Sinuous S-curve royal sway
        bones.AddPosition("regent_cog", s * 28f, MathF.Abs(c) * 22f, weight);
        bones.AddRotation("regent_cog", s * 8f, weight);

        bones.AddRotation("body", s * 14f, weight);
        bones.AddRotation("neck", -s * 10f, weight);
        bones.AddRotation("head", s2 * 12f, weight);

        // Lotus hands & graceful fingers
        bones.AddRotation("shoulder_l", -14f + s * 10f, weight);
        bones.AddRotation("arm_upper_l", -64f + s * 18f, weight);
        bones.AddRotation("arm_lower_l", -68f - c * 18f, weight);
        bones.AddRotation("hand", 28f + s2 * 22f, weight);
        bones.AddRotation("index_l", -18f, weight);
        bones.AddRotation("index_l2", -14f, weight);
        bones.AddRotation("pinky_l1", 22f, weight);
        bones.AddRotation("thumb_l", -20f, weight);

        bones.AddRotation("shoulder_r", 14f + s * 10f, weight);
        bones.AddRotation("arm_upper_r", -58f - s * 18f, weight);
        bones.AddRotation("arm_lower_r", -62f + c * 18f, weight);
        bones.AddRotation("hand_r", -28f - s2 * 22f, weight);
        bones.AddPosition("elbow_r_ik", s * 24f, 34f + c * 16f, weight);

        // Dainty alternating leg lift
        bones.AddRotation("leg_upper_l", leftLift * 44f, weight);
        bones.AddRotation("leg_lower_l", -leftLift * 58f, weight);
        bones.AddRotation("foot_l", leftLift * 28f, weight);

        bones.AddRotation("leg_r_upper", rightLift * 44f, weight);
        bones.AddRotation("leg_r_lower", -rightLift * 58f, weight);

        // Orbiting cosmic weapons forming a floating halo
        bones.AddPosition("weapons_base", -s * 16f, 34f + c * 12f, weight);
        bones.AddRotation("weapons_base", s * 12f, weight);
        bones.AddRotation("weapol_large", -24f + s * 18f, weight);
        bones.AddRotation("weapon_med", 24f - s * 18f, weight);
        bones.AddRotation("weapon_small", 42f + c * 24f, weight);
        bones.AddRotation("weapol_large2", -42f - c * 24f, weight);

        // Minions swaying like synchronized backup dancers
        bones.AddPosition("minion_r_cog", s * 18f, MathF.Abs(c) * 14f, weight);
        bones.AddRotation("minion_r_cog", s * 12f, weight);
        bones.AddRotation("minion_r_arm_1", -48f + c * 20f, weight);
        bones.AddRotation("minion_r_arm_2", -26f + s * 18f, weight);

        bones.AddPosition("minion_l_cog", s * 18f, MathF.Abs(c) * 14f, weight);
        bones.AddRotation("minion_l_cog", s * 12f, weight);
        bones.AddRotation("minion_l_arm_1", -48f - c * 20f, weight);
    }

    private static void ApplyTaunt(SpineBoneCache bones, float phase, float weight)
    {
        float cycle = PositiveMod(phase / (MathF.PI * 2f), 1f);
        float patWeight = SmoothStep01(1f - MathF.Abs(cycle - 0.25f) * 4f);
        float beckonWeight = SmoothStep01(1f - MathF.Abs(cycle - 0.75f) * 4f);
        float patWave = MathF.Sin(phase * 4f);
        float slap = MathF.Abs(patWave);

        // Butt-patting phase -> cocky beckoning phase
        bones.AddPosition("regent_cog", patWeight * -18f + beckonWeight * 12f, slap * 10f, weight);
        bones.AddRotation("regent_cog", patWeight * 8f - beckonWeight * 8f, weight);

        bones.AddRotation("body", patWeight * 22f - beckonWeight * 12f, weight);
        bones.AddRotation("neck", patWeight * -10f + beckonWeight * 8f, weight);
        bones.AddRotation("head", patWeight * -8f + beckonWeight * (patWave * 14f), weight);

        // Right arm: rhythmic hip/butt patting
        bones.AddRotation("arm_upper_r", patWeight * (32f + slap * 24f), weight);
        bones.AddRotation("arm_lower_r", patWeight * (-72f - slap * 28f), weight);
        bones.AddRotation("hand_r", patWeight * (patWave * 28f), weight);
        bones.AddPosition("elbow_r_ik", patWeight * -28f, patWeight * slap * 20f, weight);

        // Left arm: cocky beckoning hand/finger wave
        bones.AddRotation("shoulder_l", beckonWeight * -18f, weight);
        bones.AddRotation("arm_upper_l", beckonWeight * -72f, weight);
        bones.AddRotation("arm_lower_l", beckonWeight * (-54f + patWave * 28f), weight);
        bones.AddRotation("hand", beckonWeight * (20f + patWave * 34f), weight);
        bones.AddRotation("index_l", beckonWeight * (-26f - slap * 24f), weight);
        bones.AddRotation("index_l2", beckonWeight * (-24f - slap * 26f), weight);
        bones.AddRotation("pinky_l1", beckonWeight * 18f, weight);

        // Minions mimicking the taunt & hopping cheekily
        bones.AddPosition("minion_r_cog", 0f, slap * 22f, weight);
        bones.AddRotation("minion_r_cog", patWave * 10f, weight);
        bones.AddRotation("minion_r_arm_1", -52f + patWave * 26f, weight);
        bones.AddRotation("minion_r_arm_2", -35f - patWave * 24f, weight);

        bones.AddPosition("minion_l_cog", 0f, slap * 22f, weight);
        bones.AddRotation("minion_l_cog", -patWave * 10f, weight);
        bones.AddRotation("minion_l_arm_1", -52f - patWave * 26f, weight);

        // Weapons tilt smugly
        bones.AddRotation("weapons_base", patWave * 10f, weight);
        bones.AddRotation("weapol_large", beckonWeight * 16f, weight);
        bones.AddRotation("weapon_med", -beckonWeight * 16f, weight);
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
