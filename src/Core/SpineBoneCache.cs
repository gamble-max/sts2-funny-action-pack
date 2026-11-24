using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace Sts2ActionPack.Core;

/// <summary>
/// Caches MegaBone / SpineBone GodotObject references on the main thread for the
/// lifetime of the creature visuals, and deterministically disposes them on the
/// main thread in TreeExiting (PRG-6985 safe).
/// Tracks applied per-frame deltas so unkeyed bones never accumulate across frames.
/// </summary>
internal sealed class SpineBoneCache
{
    private struct BoneDelta
    {
        public float Dx;
        public float Dy;
        public float DRot;
        public float DSx;
        public float DSy;
    }

    private readonly List<MegaBone> _ownedWrappers = new();
    private readonly Dictionary<string, GodotObject> _bones = new(StringComparer.Ordinal);
    private readonly Dictionary<string, BoneDelta> _appliedDeltas = new(StringComparer.Ordinal);

    public SpineBoneCache(MegaSprite sprite, IEnumerable<string> boneNames)
    {
        if (sprite?.BoundObject == null || !GodotObject.IsInstanceValid(sprite.BoundObject))
            return;

        using var skel = sprite.GetSkeleton();
        if (skel?.BoundObject == null || !GodotObject.IsInstanceValid(skel.BoundObject))
            return;

        foreach (var name in boneNames)
        {
            if (string.IsNullOrEmpty(name) || _bones.ContainsKey(name))
                continue;

            var bone = skel.FindBone(name);
            var obj = bone?.BoundObject;
            if (bone != null && obj != null && GodotObject.IsInstanceValid(obj))
            {
                _ownedWrappers.Add(bone);
                _bones[name] = obj;
            }
            else
            {
                bone?.Dispose();
            }
        }
    }

    public bool HasBone(string name) => TryGetBone(name, out _);

    public bool TryGetBone(string name, out GodotObject bone)
    {
        if (_bones.TryGetValue(name, out var found) && GodotObject.IsInstanceValid(found))
        {
            bone = found;
            return true;
        }
        bone = null!;
        return false;
    }

    public void UndoAppliedDeltas()
    {
        if (_appliedDeltas.Count == 0)
            return;

        foreach (var kv in _appliedDeltas)
        {
            if (!TryGetBone(kv.Key, out var bone))
                continue;

            var d = kv.Value;
            if (d.Dx != 0f)
                bone.Call("set_x", (float)bone.Call("get_x").AsDouble() - d.Dx);
            if (d.Dy != 0f)
                bone.Call("set_y", (float)bone.Call("get_y").AsDouble() - d.Dy);
            if (d.DRot != 0f)
                bone.Call("set_rotation", (float)bone.Call("get_rotation").AsDouble() - d.DRot);
            if (d.DSx != 0f)
                bone.Call("set_scale_x", (float)bone.Call("get_scale_x").AsDouble() - d.DSx);
            if (d.DSy != 0f)
                bone.Call("set_scale_y", (float)bone.Call("get_scale_y").AsDouble() - d.DSy);
        }
        _appliedDeltas.Clear();
    }

    public void AddRotation(string name, float deltaDeg, float weight = 1f)
    {
        float d = deltaDeg * weight;
        if (MathF.Abs(d) <= 0.0001f) return;
        if (!TryGetBone(name, out var bone)) return;
        float cur = (float)bone.Call("get_rotation").AsDouble();
        bone.Call("set_rotation", cur + d);

        _appliedDeltas.TryGetValue(name, out var acc);
        acc.DRot += d;
        _appliedDeltas[name] = acc;
    }

    public void AddPosition(string name, float dx, float dy, float weight = 1f)
    {
        float adx = dx * weight;
        float ady = dy * weight;
        if (MathF.Abs(adx) <= 0.0001f && MathF.Abs(ady) <= 0.0001f) return;
        if (!TryGetBone(name, out var bone)) return;

        _appliedDeltas.TryGetValue(name, out var acc);
        if (MathF.Abs(adx) > 0.0001f)
        {
            float cx = (float)bone.Call("get_x").AsDouble();
            bone.Call("set_x", cx + adx);
            acc.Dx += adx;
        }
        if (MathF.Abs(ady) > 0.0001f)
        {
            float cy = (float)bone.Call("get_y").AsDouble();
            bone.Call("set_y", cy + ady);
            acc.Dy += ady;
        }
        _appliedDeltas[name] = acc;
    }

    public void AddScale(string name, float sxDelta, float syDelta, float weight = 1f)
    {
        float asx = sxDelta * weight;
        float asy = syDelta * weight;
        if (MathF.Abs(asx) <= 0.0001f && MathF.Abs(asy) <= 0.0001f) return;
        if (!TryGetBone(name, out var bone)) return;

        _appliedDeltas.TryGetValue(name, out var acc);
        if (MathF.Abs(asx) > 0.0001f)
        {
            float sx = (float)bone.Call("get_scale_x").AsDouble();
            bone.Call("set_scale_x", sx + asx);
            acc.DSx += asx;
        }
        if (MathF.Abs(asy) > 0.0001f)
        {
            float sy = (float)bone.Call("get_scale_y").AsDouble();
            bone.Call("set_scale_y", sy + asy);
            acc.DSy += asy;
        }
        _appliedDeltas[name] = acc;
    }

    public void Clear()
    {
        _appliedDeltas.Clear();
        _bones.Clear();
        foreach (var w in _ownedWrappers)
        {
            try { w.Dispose(); } catch { }
        }
        _ownedWrappers.Clear();
    }
}
