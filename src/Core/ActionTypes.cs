using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Sts2ActionPack.Core;

public enum ActionKind
{
    None = 0,
    HighKnees = 1,
    BellyLaugh = 2,
    JadeRabbit = 3,
    Taunt = 4,
}

internal interface ICharacterActionRig : IDisposable
{
    void ApplyPose(
        SpineBoneCache bones,
        Node2D visuals,
        Creature? ownerCreature,
        ActionKind kind,
        float phase,
        float elapsedSec,
        float weight);

    float GetStepFrequency(ActionKind kind) => kind switch
    {
        ActionKind.HighKnees => 4.4f,
        ActionKind.BellyLaugh => 3.2f,
        ActionKind.JadeRabbit => 1.8f,
        ActionKind.Taunt => 3.0f,
        _ => 2.0f,
    };
}
