using System;
using Godot;
using Sts2Mods.Shared;

namespace Sts2ActionPack.Core;

internal sealed class ActionPackConfigData
{
    public string HighKneesKey { get; set; } = "J";
    public string BellyLaughKey { get; set; } = "K";
    public string JadeRabbitKey { get; set; } = "L";
    public string TauntKey { get; set; } = "U";
}

internal static class ActionPackConfig
{
    private const string ModId = "Sts2ActionPack";
    public static ActionPackConfigData Current { get; private set; } = new();

    public static void Load()
    {
        Current = ModConfigBridge.LoadJsonConfig(ModId, new ActionPackConfigData());
    }

    public static void Save()
    {
        ModConfigBridge.SaveJsonConfigDeferred(ModId, Current);
    }

    public static Key ParseKey(string name, Key fallback)
    {
        if (!string.IsNullOrWhiteSpace(name) && Enum.TryParse<Key>(name.Trim(), true, out var parsed) && parsed != Key.None)
            return parsed;
        return fallback;
    }

    public static ActionKind MatchHotkey(Key key)
    {
        if (key == ParseKey(Current.HighKneesKey, Key.J)) return ActionKind.HighKnees;
        if (key == ParseKey(Current.BellyLaughKey, Key.K)) return ActionKind.BellyLaugh;
        if (key == ParseKey(Current.JadeRabbitKey, Key.L)) return ActionKind.JadeRabbit;
        if (key == ParseKey(Current.TauntKey, Key.U)) return ActionKind.Taunt;
        return ActionKind.None;
    }
}
