using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Sts2ActionPack.Core;

namespace Sts2ActionPack.Rigs;

/// <summary>
/// Spine bone rig for Necrobinder (plus static Osty companion pose helper)
/// across HighKnees, BellyLaugh, JadeRabbit, and Taunt.
/// </summary>
internal sealed class NecrobinderActionRig : ICharacterActionRig
{
    public static readonly string[] TrackedBones =
    {
        "cog",
        "body_lower",
        "body_upper",
        "head",
        "leg_l_upper",
        "leg_l_lower",
        "foot_l",
        "toe_l",
        "leg_r_upper",
        "leg_r_lower",
        "foot_r",
        "toe_r",
        "skirt_r",
        "skirt_r2",
        "skirt_r3",
        "skirt_l",
        "skirt_l2",
        "skirt_center",
        "skirt_center2",
        "tie",
        "arm_r_ik",
        "arm_l_ik",
        "scythe_main",
    };

    public static readonly string[] OstyTrackedBones =
    {
        "cog",
        "cog_adjust",
        "wrist",
        "index",
        "index2",
        "middle",
        "middle2",
        "ring",
        "ring2",
        "pinky",
        "pinky2",
        "thumb",
        "thumb2",
        "index_ik",
        "middle_ik",
        "ring_ik",
        "thumb_ik",
        "pinki_ik",
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

    public static void ApplyOstyPose(
        SpineBoneCache ostyBones,
        ActionKind kind,
        float phase,
        float elapsedSec,
        float weight)
    {
        if (ostyBones == null || weight <= 0.001f || kind == ActionKind.None)
            return;

        switch (kind)
        {
            case ActionKind.HighKnees:
            {
                float wave = MathF.Sin(phase);
                float arcY = MathF.Abs(MathF.Cos(phase)) * 22f;
                float lagWave = MathF.Sin(phase - 0.45f);

                ostyBones.AddPosition("cog", wave * 44f, arcY, weight);
                ostyBones.AddRotation("cog_adjust", -wave * 22f, weight);
                ostyBones.AddRotation("wrist", wave * 10f, weight);

                float fingerDelta = -lagWave * 9f;
                ostyBones.AddRotation("index", fingerDelta, weight);
                ostyBones.AddRotation("middle", fingerDelta, weight);
                ostyBones.AddRotation("ring", fingerDelta, weight);
                ostyBones.AddRotation("pinky", fingerDelta, weight);
                ostyBones.AddRotation("thumb", -lagWave * 6f, weight);
                break;
            }
            case ActionKind.BellyLaugh:
            {
                ComputeThreeBeatWeights(phase, out float b1, out float b2, out float b3);
                float chuckle = MathF.Sin(phase * 4f);
                float bounceY = MathF.Abs(chuckle) * 28f - b2 * 10f - b3 * 18f;

                ostyBones.AddPosition("cog", MathF.Sin(phase * 2f) * 16f, bounceY, weight);
                ostyBones.AddRotation("cog_adjust", -b1 * 18f + b2 * 22f - b3 * 30f + chuckle * 8f, weight);
                ostyBones.AddRotation("wrist", chuckle * 14f, weight);

                float curl = (b1 + b2) * (22f + chuckle * 10f) + b3 * (-18f + MathF.Sin(phase * 6f) * 14f);
                ostyBones.AddRotation("index", curl, weight);
                ostyBones.AddRotation("index2", curl * 0.8f, weight);
                ostyBones.AddRotation("middle", curl, weight);
                ostyBones.AddRotation("middle2", curl * 0.8f, weight);
                ostyBones.AddRotation("ring", curl, weight);
                ostyBones.AddRotation("ring2", curl * 0.8f, weight);
                ostyBones.AddRotation("pinky", curl, weight);
                ostyBones.AddRotation("pinky2", curl * 0.8f, weight);
                ostyBones.AddRotation("thumb", curl * 0.6f, weight);
                ostyBones.AddRotation("thumb2", curl * 0.5f, weight);
                break;
            }
            case ActionKind.JadeRabbit:
            {
                float s = MathF.Sin(phase);
                float arcY = MathF.Abs(MathF.Cos(phase)) * 26f;

                ostyBones.AddPosition("cog", s * 32f, arcY, weight);
                ostyBones.AddRotation("cog_adjust", MathF.Sin(phase - 0.3f) * 18f, weight);
                ostyBones.AddRotation("wrist", MathF.Sin(phase - 0.6f) * 16f, weight);

                ostyBones.AddRotation("index", MathF.Sin(phase - 0.2f) * 14f, weight);
                ostyBones.AddRotation("index2", MathF.Sin(phase - 0.5f) * 16f, weight);
                ostyBones.AddRotation("middle", MathF.Sin(phase - 0.4f) * 14f, weight);
                ostyBones.AddRotation("middle2", MathF.Sin(phase - 0.7f) * 16f, weight);
                ostyBones.AddRotation("ring", MathF.Sin(phase - 0.6f) * 14f, weight);
                ostyBones.AddRotation("ring2", MathF.Sin(phase - 0.9f) * 16f, weight);
                ostyBones.AddRotation("pinky", MathF.Sin(phase - 0.8f) * 14f, weight);
                ostyBones.AddRotation("pinky2", MathF.Sin(phase - 1.1f) * 16f, weight);
                ostyBones.AddRotation("thumb", -12f + MathF.Sin(phase - 0.3f) * 10f, weight);
                ostyBones.AddRotation("thumb2", MathF.Sin(phase - 0.6f) * 12f, weight);
                break;
            }
            case ActionKind.Taunt:
            {
                float s2 = MathF.Sin(phase * 2f);
                float beckon = MathF.Sin(phase * 4f);
                ostyBones.AddPosition("cog", s2 * 20f, 16f + MathF.Abs(beckon) * 18f, weight);
                ostyBones.AddRotation("cog_adjust", s2 * 16f, weight);
                ostyBones.AddRotation("wrist", beckon * 18f, weight);

                float fingerBeckon = 24f + beckon * 26f;
                ostyBones.AddRotation("index", fingerBeckon, weight);
                ostyBones.AddRotation("index2", fingerBeckon * 0.9f, weight);
                ostyBones.AddRotation("middle", fingerBeckon * 0.85f, weight);
                ostyBones.AddRotation("middle2", fingerBeckon * 0.75f, weight);
                ostyBones.AddRotation("ring", 35f, weight);
                ostyBones.AddRotation("ring2", 25f, weight);
                ostyBones.AddRotation("pinky", 35f, weight);
                ostyBones.AddRotation("pinky2", 25f, weight);
                ostyBones.AddRotation("thumb", -18f + beckon * 10f, weight);
                ostyBones.AddRotation("thumb2", -12f, weight);
                break;
            }
        }
    }

    private static void ApplyHighKnees(SpineBoneCache bones, float phase, float w)
    {
        float s = MathF.Sin(phase);
        float liftL = MathF.Pow(MathF.Max(0f, s), 0.72f);
        float liftR = MathF.Pow(MathF.Max(0f, -s), 0.72f);
        float pushL = MathF.Max(0f, -s);
        float pushR = MathF.Max(0f, s);
        float bounce01 = MathF.Abs(s);

        // Center of gravity bounce & lively sprint lean
        bones.AddPosition("cog", 0f, bounce01 * 40f - 8f, w);
        bones.AddRotation("body_lower", -6f + (liftL - liftR) * 3.5f, w);
        bones.AddRotation("body_upper", -3f, w);
        bones.AddRotation("head", 5f + bounce01 * 5f, w);

        // Left leg: high knee lift + vertical shin fold, or stance push-off
        bones.AddRotation("leg_l_upper", liftL * 128f - pushL * 16f, w);
        bones.AddRotation("leg_l_lower", -liftL * 125f - pushL * 18f, w);
        bones.AddRotation("foot_l", liftL * 28f + pushL * 16f, w);
        bones.AddRotation("toe_l", -liftL * 12f, w);

        // Right leg: alternating high knee lift + vertical shin fold
        bones.AddRotation("leg_r_upper", liftR * 128f - pushR * 16f, w);
        bones.AddRotation("leg_r_lower", -liftR * 125f - pushR * 18f, w);
        bones.AddRotation("foot_r", liftR * 28f + pushR * 16f, w);
        bones.AddRotation("toe_r", -liftR * 12f, w);

        // Billow front robe & skirt hem upward so both pumping legs are clearly visible
        float skirtDrive = MathF.Max(liftR, liftL * 0.85f);
        float skirtRipple = MathF.Sin(phase * 2f) * 6f;
        bones.AddRotation("skirt_r", 82f * skirtDrive + 24f + skirtRipple, w);
        bones.AddRotation("skirt_r2", 30f * skirtDrive, w);
        bones.AddRotation("skirt_r3", 10f * skirtDrive, w);
        bones.AddRotation("skirt_center", 68f * skirtDrive + 20f, w);
        bones.AddRotation("skirt_center2", 18f * skirtDrive, w);
        bones.AddRotation("skirt_l", 48f * liftL + 16f - skirtRipple, w);
        bones.AddRotation("skirt_l2", 12f * liftL, w);
        bones.AddRotation("tie", 56f * skirtDrive + 14f, w);

        // Opposite-arm IK sprint pump & scythe counter-swing
        bones.AddPosition("arm_r_ik", (liftL - liftR) * 72f, bounce01 * 40f + (liftL - liftR) * 44f, w);
        bones.AddRotation("scythe_main", (liftR - liftL) * 16f, w);
        bones.AddPosition("scythe_main", (liftR - liftL) * 30f, bounce01 * 28f, w);
    }

    private static void ApplyBellyLaugh(SpineBoneCache bones, float phase, float w)
    {
        ComputeThreeBeatWeights(phase, out float b1, out float b2, out float b3);
        float chuckle = MathF.Sin(phase * 4f);
        float kickL = MathF.Max(0f, MathF.Sin(phase * 6f));
        float kickR = MathF.Max(0f, -MathF.Sin(phase * 6f));

        // 1) Lean back clutching belly -> 2) Hunch forward holding head -> 3) ROFL tip-back kicking legs
        float cogY = b1 * (12f + MathF.Abs(chuckle) * 8f) - b2 * 22f - b3 * (34f - MathF.Abs(chuckle) * 10f);
        float cogX = -b1 * 10f + b2 * 12f - b3 * 22f;
        bones.AddPosition("cog", cogX, cogY, w);

        bones.AddRotation("body_lower", -b1 * 18f + b2 * 24f - b3 * 34f, w);
        bones.AddRotation("body_upper", -b1 * 16f + b2 * 22f - b3 * 26f + chuckle * 5f, w);
        bones.AddRotation("head", -b1 * 24f + b2 * 16f - b3 * 20f + chuckle * 8f, w);

        // Arms: clutch belly -> hold head -> flail
        bones.AddPosition("arm_r_ik", -b1 * 26f - b2 * 12f + b3 * (chuckle * 24f), -b1 * 12f + b2 * 56f + b3 * 20f, w);
        bones.AddPosition("arm_l_ik", b1 * 26f + b2 * 12f - b3 * (chuckle * 24f), -b1 * 12f + b2 * 56f + b3 * 20f, w);
        bones.AddRotation("scythe_main", -b1 * 18f + b2 * 14f - b3 * 28f, w);

        // Legs: planted in b1/b2, kicking high in ROFL b3
        bones.AddRotation("leg_l_upper", b3 * (95f + kickL * 35f), w);
        bones.AddRotation("leg_l_lower", b3 * (-45f - kickL * 40f), w);
        bones.AddRotation("foot_l", b3 * (22f + kickL * 14f), w);
        bones.AddRotation("leg_r_upper", b3 * (95f + kickR * 35f), w);
        bones.AddRotation("leg_r_lower", b3 * (-45f - kickR * 40f), w);
        bones.AddRotation("foot_r", b3 * (22f + kickR * 14f), w);

        // Skirt & tie follow laughter
        float skirtFlare = b3 * 64f + b1 * 18f + MathF.Abs(chuckle) * 8f;
        bones.AddRotation("skirt_r", skirtFlare, w);
        bones.AddRotation("skirt_r2", skirtFlare * 0.4f, w);
        bones.AddRotation("skirt_center", skirtFlare * 0.85f, w);
        bones.AddRotation("skirt_l", skirtFlare * 0.7f, w);
        bones.AddRotation("tie", b2 * 38f + b3 * 45f + chuckle * 10f, w);
    }

    private static void ApplyJadeRabbit(SpineBoneCache bones, float phase, float w)
    {
        float s = MathF.Sin(phase);
        float c = MathF.Cos(phase);
        float liftL = MathF.Max(0f, s);
        float liftR = MathF.Max(0f, -s);

        // Graceful S-curve sway
        bones.AddPosition("cog", s * 24f, MathF.Abs(c) * 22f, w);
        bones.AddRotation("body_lower", s * 12f, w);
        bones.AddRotation("body_upper", -s * 11f, w);
        bones.AddRotation("head", s * 10f, w);

        // Graceful lotus/ribbon arm arcs
        bones.AddPosition("arm_l_ik", -35f + s * 32f, 55f + c * 28f, w);
        bones.AddPosition("arm_r_ik", 35f + s * 32f, 55f - c * 28f, w);
        bones.AddRotation("scythe_main", s * 10f - 12f, w);
        bones.AddPosition("scythe_main", s * 18f, MathF.Abs(c) * 16f, w);

        // Alternating dainty leg lift with pointed toes
        bones.AddRotation("leg_l_upper", liftL * 65f, w);
        bones.AddRotation("leg_l_lower", -liftL * 70f, w);
        bones.AddRotation("foot_l", liftL * 32f, w);
        bones.AddRotation("toe_l", liftL * 18f, w);

        bones.AddRotation("leg_r_upper", liftR * 65f, w);
        bones.AddRotation("leg_r_lower", -liftR * 70f, w);
        bones.AddRotation("foot_r", liftR * 32f, w);
        bones.AddRotation("toe_r", liftR * 18f, w);

        // Flowing silk skirt & tie cascade
        float lag1 = MathF.Sin(phase - 0.4f);
        float lag2 = MathF.Sin(phase - 0.8f);
        bones.AddRotation("skirt_l", 24f + lag1 * 22f, w);
        bones.AddRotation("skirt_l2", 16f + lag2 * 18f, w);
        bones.AddRotation("skirt_r", 28f - lag1 * 22f, w);
        bones.AddRotation("skirt_r2", 18f - lag2 * 18f, w);
        bones.AddRotation("skirt_r3", 12f - MathF.Sin(phase - 1.1f) * 14f, w);
        bones.AddRotation("skirt_center", 20f + MathF.Cos(phase - 0.4f) * 16f, w);
        bones.AddRotation("skirt_center2", 14f + MathF.Cos(phase - 0.8f) * 14f, w);
        bones.AddRotation("tie", 18f + MathF.Sin(phase - 0.3f) * 20f, w);
    }

    private static void ApplyTaunt(SpineBoneCache bones, float phase, float w)
    {
        ComputeTwoBeatWeights(phase, out float p1, out float p2);
        float slap = MathF.Sin(phase * 4f);

        // Part 1: Lean forward slapping/patting hip -> Part 2: Cocky beckon + head tilt
        bones.AddPosition("cog", -p1 * 12f + p2 * 10f, -p1 * 14f + p2 * 10f, w);
        bones.AddRotation("body_lower", p1 * 24f - p2 * 10f, w);
        bones.AddRotation("body_upper", p1 * 16f - p2 * 8f, w);
        bones.AddRotation("head", -p1 * (18f - slap * 6f) + p2 * (MathF.Sin(phase * 2f) * 14f), w);

        bones.AddPosition("arm_r_ik", p1 * (-48f + slap * 28f), p1 * (-18f + slap * 22f), w);
        bones.AddPosition("arm_l_ik", p2 * 48f, p2 * (45f + slap * 20f), w);
        bones.AddRotation("scythe_main", -p1 * 20f + p2 * 10f, w);

        bones.AddRotation("skirt_r", p1 * (28f + MathF.Max(0f, slap) * 10f), w);
        bones.AddRotation("tie", p1 * 22f + slap * 8f, w);
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
