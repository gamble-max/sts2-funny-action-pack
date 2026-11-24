using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ActionPack.Core;

/// <summary>
/// Soft reflection bridge to CoopQolSuite.Actions.CharacterActionRegistry.
/// Never latches unavailability prematurely in case CoopQolSuite loads after Sts2ActionPack.
/// </summary>
internal static class CoopQolBridge
{
    public const string HighKneesId = "actionpack.high_knees";
    public const string BellyLaughId = "actionpack.belly_laugh";
    public const string JadeRabbitId = "actionpack.jade_rabbit";
    public const string TauntId = "actionpack.taunt";

    private static Type? _registryType;
    private static MethodInfo? _registerSimpleMethod;
    private static MethodInfo? _triggerMethod;
    private static bool _registeredOnce;

    public static bool IsAvailable
    {
        get
        {
            EnsureProbed();
            return _registryType != null && _registerSimpleMethod != null;
        }
    }

    private static void EnsureProbed()
    {
        if (_registryType != null && _registerSimpleMethod != null)
            return;

        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!string.Equals(asm.GetName().Name, "CoopQolSuite", StringComparison.OrdinalIgnoreCase))
                    continue;

                var reg = asm.GetType("CoopQolSuite.Actions.CharacterActionRegistry");
                if (reg != null)
                {
                    _registryType = reg;
                    _registerSimpleMethod = reg.GetMethod(
                        "RegisterSimple",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        new[] { typeof(string), typeof(string), typeof(Action<object, object?>), typeof(string) },
                        null);
                    _triggerMethod = reg.GetMethod("Trigger", BindingFlags.Public | BindingFlags.Static);
                }
                break;
            }
        }
        catch (Exception ex)
        {
            ActionPackModEntry.Log.Warn($"CoopQolBridge probe failed: {ex.Message}");
        }
    }

    public static void EnsureRegistered()
    {
        if (!IsAvailable)
            return;
        if (_registeredOnce)
            return;
        RegisterOrRefreshAll();
    }

    public static void RegisterOrRefreshAll()
    {
        if (!IsAvailable)
            return;

        try
        {
            RegisterOne(HighKneesId, ActionI18n.T("高抬腿", "High-Knees"), ActionKind.HighKnees);
            RegisterOne(BellyLaughId, ActionI18n.T("捧腹笑", "Belly Laugh"), ActionKind.BellyLaugh);
            RegisterOne(JadeRabbitId, ActionI18n.T("玉兔舞", "Jade Rabbit"), ActionKind.JadeRabbit);
            RegisterOne(TauntId, ActionI18n.T("挑衅", "Taunt"), ActionKind.Taunt);
            _registeredOnce = true;
        }
        catch (Exception ex)
        {
            ActionPackModEntry.Log.Warn($"CoopQolBridge register failed: {ex.Message}");
        }
    }

    private static void RegisterOne(string id, string title, ActionKind kind)
    {
        Action<object, object?> cb = (playerObj, _) =>
        {
            if (playerObj is Player p)
                ActionStageDirector.Toggle(p, kind);
        };
        _registerSimpleMethod!.Invoke(null, new object[] { id, title, cb, "" });
    }

    public static bool TryTriggerNetworked(Player localPlayer, ActionKind kind)
    {
        if (!IsAvailable || _triggerMethod == null)
            return false;

        var netType = RunManager.Instance?.NetService?.Type ?? NetGameType.Singleplayer;
        if (netType == NetGameType.Singleplayer)
            return false;

        string? actionId = kind switch
        {
            ActionKind.HighKnees => HighKneesId,
            ActionKind.BellyLaugh => BellyLaughId,
            ActionKind.JadeRabbit => JadeRabbitId,
            ActionKind.Taunt => TauntId,
            _ => null,
        };
        if (actionId == null)
            return false;

        try
        {
            EnsureRegistered();
            _triggerMethod.Invoke(null, new object[] { actionId, "" });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
