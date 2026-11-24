using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2ActionPack.Rigs;

namespace Sts2ActionPack.Core;

internal static class ActionStageDirector
{
    private sealed class PlayerActionState
    {
        public ActionKind DesiredKind;
        public ActionKind RenderingKind;
        public float Weight;
        public float Phase;
        public float ElapsedSec;
    }

    private sealed class OstyActionRigAdapter : ICharacterActionRig
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
            NecrobinderActionRig.ApplyOstyPose(bones, kind, phase, elapsedSec, weight);
        }

        public void Dispose() { }
    }

    private sealed class StandaloneEntry
    {
        public required NCreatureVisuals Visuals;
        public CharacterModel? Character;
        public bool IsOsty;
        public float Phase;
        public float ElapsedSec;
    }

    private sealed class RigSession : IDisposable
    {
        private readonly NCreatureVisuals _visuals;
        private readonly MegaSprite _spineSprite;
        private readonly SpineBoneCache _bones;
        private readonly ICharacterActionRig _rig;
        private readonly ActionVfxBundle _vfx;
        private readonly Callable _beforeApplyCallable;
        private readonly Callable _transformCallable;
        private bool _disposed;

        public ActionKind CurrentKind { get; private set; }
        public float CurrentPhase { get; private set; }
        public float CurrentElapsed { get; private set; }
        public float CurrentWeight { get; private set; }
        public Creature? OwnerCreature { get; private set; }

        public RigSession(NCreatureVisuals visuals, ICharacterActionRig rig, IEnumerable<string> boneNames)
        {
            _visuals = visuals;
            _spineSprite = visuals.SpineBody!;
            _rig = rig;
            _bones = new SpineBoneCache(_spineSprite, boneNames);
            _vfx = new ActionVfxBundle(visuals);
            _beforeApplyCallable = Callable.From<GodotObject>(_ => _bones.UndoAppliedDeltas());
            _transformCallable = Callable.From<GodotObject>(_ => OnBeforeWorldTransformsChange());

            _spineSprite.ConnectBeforeAnimationStateApply(_beforeApplyCallable);
            _spineSprite.ConnectBeforeWorldTransformsChange(_transformCallable);
            _visuals.TreeExiting += Dispose;
        }

        public float GetStepFrequency(ActionKind kind) => _rig.GetStepFrequency(kind);

        public void UpdateState(Creature? owner, ActionKind kind, float phase, float elapsedSec, float weight)
        {
            OwnerCreature = owner;
            CurrentKind = kind;
            CurrentPhase = phase;
            CurrentElapsed = elapsedSec;
            CurrentWeight = weight;
            _vfx.Update(kind, weight);
        }

        private void OnBeforeWorldTransformsChange()
        {
            if (_disposed || CurrentWeight <= 0.001f || CurrentKind == ActionKind.None)
                return;
            if (_visuals == null || !GodotObject.IsInstanceValid(_visuals))
                return;

            _rig.ApplyPose(_bones, _visuals, OwnerCreature, CurrentKind, CurrentPhase, CurrentElapsed, CurrentWeight);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_spineSprite?.BoundObject != null && GodotObject.IsInstanceValid(_spineSprite.BoundObject))
                {
                    _spineSprite.DisconnectBeforeAnimationStateApply(_beforeApplyCallable);
                    _spineSprite.DisconnectBeforeWorldTransformsChange(_transformCallable);
                }
            }
            catch { }

            _vfx.Dispose();
            _rig.Dispose();
            _bones.Clear();
        }
    }

    private static readonly Dictionary<ulong, PlayerActionState> PlayerStates = new();
    private static readonly ConditionalWeakTable<NCreatureVisuals, RigSession> Sessions = new();
    private static readonly List<StandaloneEntry> StandaloneVisuals = new();
    private static ActionKind _standaloneAction = ActionKind.None;
    private static float _standaloneTimeScale = 1.0f;

    public static void Toggle(Player player, ActionKind kind, bool? explicitState = null)
    {
        if (player == null || kind == ActionKind.None)
            return;

        if (!PlayerStates.TryGetValue(player.NetId, out var st))
        {
            st = new PlayerActionState();
            PlayerStates[player.NetId] = st;
        }

        bool turnOn = explicitState ?? (st.DesiredKind != kind);
        if (turnOn)
        {
            if (st.DesiredKind != kind)
            {
                st.DesiredKind = kind;
                st.RenderingKind = kind;
                st.Phase = 0f;
                st.ElapsedSec = 0f;
            }
        }
        else
        {
            st.DesiredKind = ActionKind.None;
        }
    }

    public static void ClearAll()
    {
        foreach (var st in PlayerStates.Values)
        {
            st.DesiredKind = ActionKind.None;
            st.RenderingKind = ActionKind.None;
            st.Weight = 0f;
        }
        PlayerStates.Clear();
        StandaloneVisuals.Clear();
        _standaloneAction = ActionKind.None;
        _standaloneTimeScale = 1.0f;
    }

    public static void AttachStandaloneVisuals(NCreatureVisuals visuals, CharacterModel character)
    {
        if (visuals == null || !GodotObject.IsInstanceValid(visuals) || character == null)
            return;

        GetOrCreateSession(visuals, character);
        StandaloneVisuals.Add(new StandaloneEntry
        {
            Visuals = visuals,
            Character = character,
        });
    }

    public static void AttachStandaloneOsty(NCreatureVisuals visuals)
    {
        if (visuals == null || !GodotObject.IsInstanceValid(visuals))
            return;

        GetOrCreateOstySession(visuals);
        StandaloneVisuals.Add(new StandaloneEntry
        {
            Visuals = visuals,
            IsOsty = true,
        });
    }

    public static void SetStandaloneAction(ActionKind kind, float timeScale = 1.0f)
    {
        _standaloneAction = kind;
        _standaloneTimeScale = Math.Max(0.1f, timeScale);
        foreach (var entry in StandaloneVisuals)
        {
            entry.Phase = 0f;
            entry.ElapsedSec = 0f;
        }
    }

    public static void Tick(double delta)
    {
        float dt = (float)delta;

        // Tick standalone chorus-line visuals (used for GIF verification)
        if (StandaloneVisuals.Count > 0)
        {
            for (int i = StandaloneVisuals.Count - 1; i >= 0; i--)
            {
                var entry = StandaloneVisuals[i];
                if (entry.Visuals == null || !GodotObject.IsInstanceValid(entry.Visuals))
                {
                    StandaloneVisuals.RemoveAt(i);
                    continue;
                }
                var session = entry.IsOsty
                    ? GetOrCreateOstySession(entry.Visuals)
                    : GetOrCreateSession(entry.Visuals, entry.Character);
                if (session == null) continue;

                float scaledDt = dt * _standaloneTimeScale;
                float freq = session.GetStepFrequency(_standaloneAction);
                entry.Phase += MathF.PI * freq * scaledDt;
                entry.ElapsedSec += scaledDt;
                session.UpdateState(null, _standaloneAction, entry.Phase, entry.ElapsedSec, _standaloneAction == ActionKind.None ? 0f : 1f);
            }
        }

        var combatRoom = NCombatRoom.Instance;
        if (combatRoom == null || !GodotObject.IsInstanceValid(combatRoom))
            return;

        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null)
            return;

        foreach (var player in runState.Players)
        {
            var creature = player.Creature;
            if (creature == null || !creature.IsAlive)
            {
                if (PlayerStates.TryGetValue(player.NetId, out var deadSt))
                {
                    deadSt.DesiredKind = ActionKind.None;
                    deadSt.Weight = 0f;
                }
                continue;
            }

            if (!PlayerStates.TryGetValue(player.NetId, out var st))
            {
                st = new PlayerActionState();
                PlayerStates[player.NetId] = st;
            }

            var creatureNode = combatRoom.GetCreatureNode(creature);
            var visuals = creatureNode?.Visuals;
            if (visuals == null || !GodotObject.IsInstanceValid(visuals) || visuals.SpineBody == null || !visuals.HasSpineAnimation)
                continue;

            var session = GetOrCreateSession(visuals, player.Character);
            if (session == null)
                continue;

            bool duckForCombatAnim = IsCombatAnimPlaying(visuals.SpineBody, out bool isDead);
            if (isDead)
                st.DesiredKind = ActionKind.None;

            float targetWeight = (st.DesiredKind != ActionKind.None && !duckForCombatAnim) ? 1f : 0f;
            float rate = targetWeight > st.Weight ? 6.5f : 5.5f;
            st.Weight = MoveToward(st.Weight, targetWeight, dt * rate);

            if (st.DesiredKind != ActionKind.None)
                st.RenderingKind = st.DesiredKind;
            else if (st.Weight <= 0.001f)
                st.RenderingKind = ActionKind.None;

            if (st.Weight > 0.001f && st.RenderingKind != ActionKind.None)
            {
                float freq = session.GetStepFrequency(st.RenderingKind);
                st.Phase += MathF.PI * freq * dt;
                st.ElapsedSec += dt;
            }

            session.UpdateState(creature, st.RenderingKind, st.Phase, st.ElapsedSec, st.Weight);

            if (player.Character is Necrobinder)
                UpdateOstyForOwner(combatRoom, player, st.RenderingKind, st.Phase, st.ElapsedSec, st.Weight);
        }
    }

    private static void UpdateOstyForOwner(
        NCombatRoom combatRoom,
        Player owner,
        ActionKind kind,
        float phase,
        float elapsedSec,
        float weight)
    {
        var combat = owner.Creature?.CombatState;
        if (combat == null) return;

        foreach (var ally in combat.Allies)
        {
            if (ally == null || !ally.IsAlive) continue;
            if (!ally.IsPet || ally.PetOwner != owner) continue;
            if (ally.Monster is not Osty) continue;

            var ostyNode = combatRoom.GetCreatureNode(ally);
            var visuals = ostyNode?.Visuals;
            if (visuals == null || !GodotObject.IsInstanceValid(visuals) || visuals.SpineBody == null || !visuals.HasSpineAnimation)
                continue;

            var session = GetOrCreateOstySession(visuals);
            session?.UpdateState(ally, kind, phase, elapsedSec, weight);
        }
    }

    private static RigSession? GetOrCreateOstySession(NCreatureVisuals visuals)
    {
        if (visuals.SpineBody?.BoundObject == null || !GodotObject.IsInstanceValid(visuals.SpineBody.BoundObject))
            return null;

        if (Sessions.TryGetValue(visuals, out var existing))
            return existing;

        var created = new RigSession(visuals, new OstyActionRigAdapter(), NecrobinderActionRig.OstyTrackedBones);
        Sessions.Add(visuals, created);
        return created;
    }

    private static RigSession? GetOrCreateSession(NCreatureVisuals visuals, CharacterModel? character)
    {
        if (visuals.SpineBody?.BoundObject == null || !GodotObject.IsInstanceValid(visuals.SpineBody.BoundObject))
            return null;

        if (Sessions.TryGetValue(visuals, out var existing))
            return existing;

        var (rig, bones) = CreateRigForCharacter(character);
        var created = new RigSession(visuals, rig, bones);
        Sessions.Add(visuals, created);
        return created;
    }

    private static (ICharacterActionRig Rig, string[] Bones) CreateRigForCharacter(CharacterModel? character)
    {
        return character switch
        {
            Ironclad => (new IroncladActionRig(), IroncladActionRig.TrackedBones),
            Silent => (new SilentActionRig(), SilentActionRig.TrackedBones),
            Defect => (new DefectActionRig(), DefectActionRig.TrackedBones),
            Necrobinder => (new NecrobinderActionRig(), NecrobinderActionRig.TrackedBones),
            Regent => (new RegentActionRig(), RegentActionRig.TrackedBones),
            Fool => (new FoolActionRig(), FoolActionRig.TrackedBones),
            _ => (new FallbackNodeRig(), Array.Empty<string>()),
        };
    }

    private static bool IsCombatAnimPlaying(MegaSprite sprite, out bool isDead)
    {
        isDead = false;
        try
        {
            if (!sprite.IsAnimationStateReady())
                return false;

            using var animState = sprite.GetAnimationState();
            var name = animState.GetCurrentAnimationName(0);
            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Contains("die", StringComparison.OrdinalIgnoreCase))
            {
                isDead = true;
                return true;
            }

            if (name.Contains("idle", StringComparison.OrdinalIgnoreCase))
                return false;

            if (name.Contains("attack", StringComparison.OrdinalIgnoreCase)
                || name.Contains("shoot", StringComparison.OrdinalIgnoreCase)
                || name.Contains("cast", StringComparison.OrdinalIgnoreCase)
                || name.Contains("hurt", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch { }

        return false;
    }

    private static float MoveToward(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }
}
