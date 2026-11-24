using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

/// <summary>
/// Spine bone rig for Defect across HighKnees, BellyLaugh, JadeRabbit, and Taunt.
/// </summary>
internal sealed class DefectActionRig : ICharacterActionRig
{
    public static readonly string[] TrackedBones =
    {
        "COG_bonus_control",
        "COG",
        "pelvis",
        "torso",
        "torso_upper",
        "hips",
        "neck",
        "head",
        "eye_orb",
        "orb",
        "leg_upper_r",
        "leg_lower_r",
        "foot_r",
        "leg_r_ik",
        "leg_upper_l",
        "leg_lower_l",
        "foot_l",
        "leg_l_ik",
        "shoulder_l",
        "arm_upper_l",
        "arm_lower_l",
        "hand_l",
        "hand_l2",
        "hand_l4",
        "hand_l6",
        "shoulder_r",
        "arm_upper_r",
        "arm_lower_r",
        "hand_r",
        "index_1r",
        "middle_1r",
        "thumb_1r",
        "cape_top_l",
        "cape_top_r",
        "cape_top_c",
        "cowl_l2",
        "cowl_r2",
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
        if (weight <= 0.001f || kind == ActionKind.None)
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

    private static void ApplyHighKnees(SpineBoneCache bones, float phase, float w)
    {
        float s = MathF.Sin(phase);
        float liftL = MathF.Pow(MathF.Max(0f, s), 0.72f);
        float liftR = MathF.Pow(MathF.Max(0f, -s), 0.72f);
        float bounce01 = MathF.Abs(s);

        bones.AddPosition("COG", 0f, bounce01 * 36f - 6f, w);
        bones.AddPosition("COG_bonus_control", 0f, bounce01 * 8f, w);
        bones.AddRotation("pelvis", (liftL - liftR) * 5f, w);
        bones.AddRotation("hips", (liftR - liftL) * 4f, w);
        bones.AddRotation("torso", -5f, w);
        bones.AddRotation("torso_upper", -4f + bounce01 * 4f, w);
        bones.AddRotation("neck", 4f, w);
        bones.AddRotation("head", 5f + bounce01 * 5f, w);

        // Chest orb & eye pulse on beat (keep scale untouched so unkeyed bones never accumulate)
        bones.AddRotation("torso_upper", bounce01 * 3f, w);

        // Alternating IK leg high-lifts
        bones.AddPosition("leg_r_ik", liftR * 18f, liftR * 150f, w);
        bones.AddPosition("leg_l_ik", liftL * 18f, liftL * 150f, w);
        bones.AddRotation("leg_upper_r", liftR * 20f, w);
        bones.AddRotation("leg_lower_r", -liftR * 18f, w);
        bones.AddRotation("foot_r", liftR * 22f, w);
        bones.AddRotation("leg_upper_l", liftL * 20f, w);
        bones.AddRotation("leg_lower_l", -liftL * 18f, w);
        bones.AddRotation("foot_l", liftL * 22f, w);

        // Opposite arms pumping
        bones.AddRotation("shoulder_l", (liftR - liftL) * 14f, w);
        bones.AddRotation("arm_upper_l", (liftR - liftL) * 48f - 12f, w);
        bones.AddRotation("arm_lower_l", 45f + liftR * 28f, w);
        bones.AddRotation("hand_l", 12f, w);

        bones.AddRotation("shoulder_r", (liftL - liftR) * 14f, w);
        bones.AddRotation("arm_upper_r", (liftL - liftR) * 48f - 12f, w);
        bones.AddRotation("arm_lower_r", 45f + liftL * 28f, w);
        bones.AddRotation("hand_r", 12f, w);

        // Cape & cowl flutter
        float flutter = MathF.Sin(phase * 2f) * 8f;
        bones.AddRotation("cape_top_l", -18f - bounce01 * 18f + flutter, w);
        bones.AddRotation("cape_top_r", 18f + bounce01 * 18f - flutter, w);
        bones.AddRotation("cape_top_c", -14f - bounce01 * 14f, w);
        bones.AddRotation("cowl_l2", -10f * bounce01, w);
        bones.AddRotation("cowl_r2", 10f * bounce01, w);
    }

    private static void ApplyBellyLaugh(SpineBoneCache bones, float phase, float w)
    {
        ComputeThreeBeatWeights(phase, out float b1, out float b2, out float b3);
        float chuckle = MathF.Sin(phase * 4f);
        float kickL = MathF.Max(0f, MathF.Sin(phase * 6f));
        float kickR = MathF.Max(0f, -MathF.Sin(phase * 6f));

        // 1) Lean back clutching core -> 2) Hunch holding head -> 3) ROFL tip-back kicking legs
        float cogY = b1 * (10f + MathF.Abs(chuckle) * 8f) - b2 * 24f - b3 * (36f - MathF.Abs(chuckle) * 10f);
        float cogX = -b1 * 10f + b2 * 12f - b3 * 22f;
        bones.AddPosition("COG", cogX, cogY, w);

        bones.AddRotation("pelvis", -b1 * 12f + b2 * 14f - b3 * 32f, w);
        bones.AddRotation("torso", -b1 * 18f + b2 * 26f - b3 * 28f, w);
        bones.AddRotation("torso_upper", -b1 * 14f + b2 * 22f - b3 * 18f + chuckle * 5f, w);
        bones.AddRotation("neck", -b1 * 8f + b2 * 10f - b3 * 12f, w);
        bones.AddRotation("head", -b1 * 22f + b2 * 18f - b3 * 24f + chuckle * 7f, w);

        // Chest shake with laughter
        bones.AddRotation("torso_upper", chuckle * 4f, w);

        // Arms: clutch belly (b1) -> hold head/cowl (b2) -> flail/clutch belly in ROFL (b3)
        float armUpL = b1 * 24f - b2 * 64f + b3 * (20f + chuckle * 14f);
        float armUpR = b1 * -24f + b2 * 64f - b3 * (20f - chuckle * 14f);
        float armLow = b1 * 68f + b2 * 82f + b3 * 52f;
        bones.AddRotation("arm_upper_l", armUpL, w);
        bones.AddRotation("arm_lower_l", armLow, w);
        bones.AddRotation("arm_upper_r", armUpR, w);
        bones.AddRotation("arm_lower_r", -armLow, w);
        bones.AddRotation("hand_l", chuckle * 14f, w);
        bones.AddRotation("hand_r", -chuckle * 14f, w);

        // Legs: planted in b1/b2, kicking high in ROFL b3
        bones.AddPosition("leg_r_ik", b3 * 35f, b3 * (115f + kickR * 55f), w);
        bones.AddPosition("leg_l_ik", b3 * 35f, b3 * (115f + kickL * 55f), w);
        bones.AddRotation("leg_upper_r", b3 * (28f + kickR * 22f), w);
        bones.AddRotation("leg_upper_l", b3 * (28f + kickL * 22f), w);
        bones.AddRotation("foot_r", b3 * (24f + kickR * 18f), w);
        bones.AddRotation("foot_l", b3 * (24f + kickL * 18f), w);

        // Cape & cowl reaction
        bones.AddRotation("cape_top_l", -14f - b3 * 24f + chuckle * 8f, w);
        bones.AddRotation("cape_top_r", 14f + b3 * 24f - chuckle * 8f, w);
        bones.AddRotation("cowl_l2", b2 * 14f + chuckle * 6f, w);
        bones.AddRotation("cowl_r2", -b2 * 14f - chuckle * 6f, w);
    }

    private static void ApplyJadeRabbit(SpineBoneCache bones, float phase, float w)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float liftL = MathF.Max(0f, s);
        float liftR = MathF.Max(0f, -s);

        // Graceful hip/torso sway
        bones.AddPosition("COG", s * 22f, MathF.Abs(c) * 20f, w);
        bones.AddRotation("pelvis", s * 12f, w);
        bones.AddRotation("hips", -s * 10f, w);
        bones.AddRotation("torso", -s * 10f, w);
        bones.AddRotation("torso_upper", s * 8f, w);
        bones.AddRotation("neck", -s * 6f, w);
        bones.AddRotation("head", s * 10f + MathF.Sin(phase * 0.5f) * 5f, w);

        bones.AddRotation("torso_upper", MathF.Sin(phase * 2f) * 4f, w);

        // Lotus hand & finger poses
        bones.AddRotation("shoulder_l", -14f + s * 8f, w);
        bones.AddRotation("arm_upper_l", -48f + c * 18f, w);
        bones.AddRotation("arm_lower_l", -38f + s * 16f, w);
        bones.AddRotation("hand_l", 24f + c * 14f, w);
        bones.AddRotation("hand_l2", 18f + s * 10f, w);
        bones.AddRotation("hand_l4", -14f - s * 8f, w);
        bones.AddRotation("hand_l6", 22f + c * 10f, w);

        bones.AddRotation("shoulder_r", 14f + s * 8f, w);
        bones.AddRotation("arm_upper_r", 52f - c * 18f, w);
        bones.AddRotation("arm_lower_r", 40f - s * 16f, w);
        bones.AddRotation("hand_r", -24f - c * 14f, w);
        bones.AddRotation("index_1r", 20f + s * 10f, w);
        bones.AddRotation("middle_1r", 14f + c * 8f, w);
        bones.AddRotation("thumb_1r", -16f, w);

        // Alternating dainty leg lift
        bones.AddPosition("leg_l_ik", 0f, liftL * 75f, w);
        bones.AddPosition("leg_r_ik", 0f, liftR * 75f, w);
        bones.AddRotation("foot_l", -liftL * 24f, w);
        bones.AddRotation("foot_r", -liftR * 24f, w);

        // Flowing cape
        float capeLag = MathF.Sin(phase - 0.5f);
        bones.AddRotation("cape_top_l", -26f + capeLag * 18f, w);
        bones.AddRotation("cape_top_r", 26f + capeLag * 18f, w);
        bones.AddRotation("cape_top_c", -16f + MathF.Cos(phase - 0.5f) * 12f, w);
    }

    private static void ApplyTaunt(SpineBoneCache bones, float phase, float w)
    {
        ComputeTwoBeatWeights(phase, out float p1, out float p2);
        float slap = MathF.Sin(phase * 4f);
        float beckon = MathF.Sin(phase * 4f - 0.3f);

        // Part 1: Lean forward slapping/patting hip -> Part 2: Cocky beckoning wave + head tilt
        bones.AddPosition("COG", -p1 * 12f + p2 * 10f, -p1 * 14f + p2 * 8f, w);
        bones.AddRotation("pelvis", -p1 * 18f + p2 * 8f, w);
        bones.AddRotation("hips", -p1 * 20f + p2 * 10f, w);
        bones.AddRotation("torso", p1 * 28f - p2 * 10f, w);
        bones.AddRotation("torso_upper", p1 * 14f - p2 * 6f, w);
        bones.AddRotation("head", -p1 * 16f + p2 * (MathF.Sin(phase * 2f) * 14f - 8f), w);

        // Right arm reaches back to pat hip in p1, rests on hip in p2
        bones.AddRotation("arm_upper_r", p1 * (-42f + slap * 24f) + p2 * 18f, w);
        bones.AddRotation("arm_lower_r", p1 * (38f - slap * 28f) + p2 * 24f, w);
        bones.AddRotation("hand_r", p1 * (slap * 20f) + p2 * (beckon * 12f), w);
        bones.AddRotation("index_1r", p2 * (beckon * 18f), w);
        bones.AddRotation("middle_1r", p2 * (beckon * 16f), w);

        // Left arm raised in cocky beckoning wave in p2
        bones.AddRotation("arm_upper_l", p1 * -14f + p2 * 52f, w);
        bones.AddRotation("arm_lower_l", p1 * 20f + p2 * 48f, w);
        bones.AddRotation("hand_l", p2 * (slap * 28f), w);
        bones.AddRotation("hand_l2", p2 * (beckon * 24f), w);
        bones.AddRotation("hand_l4", p2 * (beckon * 24f), w);
        bones.AddRotation("hand_l6", p2 * (beckon * 24f), w);

        bones.AddRotation("cape_top_l", -p1 * 18f + slap * 6f, w);
        bones.AddRotation("cape_top_r", p1 * 18f - slap * 6f, w);
    }

    private static void ComputeThreeBeatWeights(float phase, out float b1, out float b2, out float b3)
    {
        float u = ((phase % (MathF.PI * 2f)) + MathF.PI * 2f) % (MathF.PI * 2f) / (MathF.PI * 2f);
        float t12 = SmoothStep(0.28f, 0.38f, u);
        float t23 = SmoothStep(0.61f, 0.71f, u);
        float t31 = SmoothStep(0.94f, 1.00f, u);

        b1 = (1f - t12) + t31;
        b2 = t12 * (1f - t23);
        b3 = t23 * (1f - t31);
        float sum = b1 + b2 + b3;
        if (sum > 0.0001f)
        {
            b1 /= sum;
            b2 /= sum;
            b3 /= sum;
        }
    }

    private static void ComputeTwoBeatWeights(float phase, out float p1, out float p2)
    {
        float u = ((phase % (MathF.PI * 2f)) + MathF.PI * 2f) % (MathF.PI * 2f) / (MathF.PI * 2f);
        float t12 = SmoothStep(0.45f, 0.55f, u);
        float t21 = SmoothStep(0.95f, 1.00f, u);
        p1 = (1f - t12) + t21;
        p2 = 1f - p1;
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    public void Dispose() { }
}
