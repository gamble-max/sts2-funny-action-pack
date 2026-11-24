using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mods.Shared;

namespace Sts2ActionPack.Core;

[ModInitializer(nameof(Init))]
public static class ActionPackModEntry
{
    public static readonly ModLog Log = new("Sts2ActionPack");
    private static CombatManager? _subscribedCombatMgr;
    private static double _bridgeCheckAccum;

    public static void Init()
    {
        ActionPackConfig.Load();
        var harmony = new Harmony("sts2mods.sts2actionpack");

        int ok = 0;
        int failed = 0;
        foreach (var type in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()))
        {
            if (type.GetCustomAttribute<HarmonyPatch>() == null)
                continue;

            try
            {
                harmony.CreateClassProcessor(type).Patch();
                ok++;
            }
            catch (Exception ex)
            {
                failed++;
                Log.Error($"Harmony patch class {type.FullName} failed: {ex.Message}");
            }
        }

        CoopQolBridge.EnsureRegistered();
        Log.Info($"Sts2ActionPack v1.0.0 loaded (patches ok={ok}, failed={failed}, CoopQolSuite={CoopQolBridge.IsAvailable}).");
    }

    private static void EnsureCombatEvents()
    {
        var cm = CombatManager.Instance;
        if (cm == null || ReferenceEquals(cm, _subscribedCombatMgr)) return;

        if (_subscribedCombatMgr != null)
        {
            _subscribedCombatMgr.CombatSetUp -= OnCombatSetUp;
            _subscribedCombatMgr.CombatEnded -= OnCombatEnded;
        }

        _subscribedCombatMgr = cm;
        cm.CombatSetUp += OnCombatSetUp;
        cm.CombatEnded += OnCombatEnded;
    }

    private static void OnCombatSetUp(CombatState _) => ActionStageDirector.ClearAll();
    private static void OnCombatEnded(CombatRoom _) => ActionStageDirector.ClearAll();

    [HarmonyPatch(typeof(NGame), nameof(NGame._Input))]
    private static class NGameInputPatch
    {
        private static void Postfix(InputEvent inputEvent)
        {
            if (inputEvent is not InputEventKey keyEvt || !keyEvt.Pressed || keyEvt.Echo)
                return;
            if (keyEvt.CtrlPressed || keyEvt.AltPressed || keyEvt.MetaPressed)
                return;

            var kind = ActionPackConfig.MatchHotkey(keyEvt.Keycode);
            if (kind == ActionKind.None)
                return;

            if (NDevConsole.IsConsoleVisible) return;
            if (NCapstoneContainer.Instance?.InUse == true) return;

            var vp = NGame.Instance?.GetViewport();
            var focusOwner = vp?.GuiGetFocusOwner();
            if (focusOwner is LineEdit or TextEdit)
                return;

            var runState = RunManager.Instance?.DebugOnlyGetState();
            if (runState == null) return;
            var local = LocalContext.GetMe(runState) ?? (runState.Players.Count > 0 ? runState.Players[0] : null);
            if (local == null)
                return;

            if (!CoopQolBridge.TryTriggerNetworked(local, kind))
                ActionStageDirector.Toggle(local, kind);

            vp?.SetInputAsHandled();
        }
    }

    [HarmonyPatch(typeof(NGame), nameof(NGame.Relocalize))]
    private static class NGameRelocalizePatch
    {
        private static void Postfix()
        {
            CoopQolBridge.RegisterOrRefreshAll();
        }
    }

    [HarmonyPatch(typeof(NRun), nameof(NRun._Ready))]
    private static class NRunReadyPatch
    {
        private static void Postfix()
        {
            EnsureCombatEvents();
            CoopQolBridge.EnsureRegistered();
        }
    }

    [HarmonyPatch(typeof(NRun), nameof(NRun._Process))]
    private static class NRunProcessPatch
    {
        private static void Postfix(double delta)
        {
            _bridgeCheckAccum += delta;
            if (_bridgeCheckAccum >= 0.5)
            {
                _bridgeCheckAccum = 0;
                CoopQolBridge.EnsureRegistered();
            }
            ActionStageDirector.Tick(delta);
        }
    }

    [HarmonyPatch(typeof(NRun), nameof(NRun._Notification))]
    private static class NRunNotificationPatch
    {
        private static void Postfix(int what)
        {
            if (what == (int)Node.NotificationExitTree)
            {
                ActionStageDirector.ClearAll();
            }
        }
    }
}
