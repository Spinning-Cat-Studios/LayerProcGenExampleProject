#pragma warning disable CS0109
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

namespace TokisanGames;

public partial class Terrain3DStorage : Resource
{
    private new static readonly StringName NativeName = new StringName("Terrain3DStorage");

    [Obsolete("Wrapper types cannot be constructed with constructors (it only instantiate the underlying Terrain3DStorage object), please use the Instantiate() method instead.")]
    protected Terrain3DStorage() { }

    private static CSharpScript _wrapperScriptAsset;

    /// <summary>
    /// Try to cast the script on the supplied <paramref name="godotObject"/> to the <see cref="Terrain3DStorage"/> wrapper type,
    /// if no script has attached to the type, or the script attached to the type does not inherit the <see cref="Terrain3DStorage"/> wrapper type,
    /// a new instance of the <see cref="Terrain3DStorage"/> wrapper script will get attaches to the <paramref name="godotObject"/>.
    /// </summary>
    /// <remarks>The developer should only supply the <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</remarks>
    /// <param name="godotObject">The <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</param>
    /// <returns>The existing or a new instance of the <see cref="Terrain3DStorage"/> wrapper script attached to the supplied <paramref name="godotObject"/>.</returns>
    public new static Terrain3DStorage Bind(GodotObject godotObject)
    {
#if DEBUG
        if (!IsInstanceValid(godotObject))
            throw new InvalidOperationException("The supplied GodotObject instance is not valid.");
#endif
        if (godotObject is Terrain3DStorage wrapperScriptInstance)
            return wrapperScriptInstance;

#if DEBUG
        var expectedType = typeof(Terrain3DStorage);
        var currentObjectClassName = godotObject.GetClass();
        if (!ClassDB.IsParentClass(expectedType.Name, currentObjectClassName))
            throw new InvalidOperationException($"The supplied GodotObject ({currentObjectClassName}) is not the {expectedType.Name} type.");
#endif

        if (_wrapperScriptAsset is null)
        {
            var scriptPathAttribute = typeof(Terrain3DStorage).GetCustomAttributes<ScriptPathAttribute>().FirstOrDefault();
            if (scriptPathAttribute is null) throw new UnreachableException();
            _wrapperScriptAsset = ResourceLoader.Load<CSharpScript>(scriptPathAttribute.Path);
        }

        var instanceId = godotObject.GetInstanceId();
        godotObject.SetScript(_wrapperScriptAsset);
        return (Terrain3DStorage)InstanceFromId(instanceId);
    }

    /// <summary>
    /// Creates an instance of the GDExtension <see cref="Terrain3DStorage"/> type, and attaches a wrapper script instance to it.
    /// </summary>
    /// <returns>The wrapper instance linked to the underlying GDExtension "Terrain3DStorage" type.</returns>
    public new static Terrain3DStorage Instantiate() => Bind(ClassDB.Instantiate(NativeName).As<GodotObject>());

    public enum MapType
    {
        Height = 0,
        Control = 1,
        Color = 2,
        Max = 3,
    }

    public enum RegionSizeEnum
    {
        Size1024 = 1024,
    }

    public new static class GDExtensionPropertyName
    {
        public new static readonly StringName Version = "version";
        public new static readonly StringName RegionSize = "region_size";
        public new static readonly StringName Save16Bit = "save_16_bit";
        public new static readonly StringName HeightRange = "height_range";
        public new static readonly StringName RegionOffsets = "region_offsets";
        public new static readonly StringName HeightMaps = "height_maps";
        public new static readonly StringName ControlMaps = "control_maps";
        public new static readonly StringName ColorMaps = "color_maps";
        public new static readonly StringName Multimeshes = "multimeshes";
    }

    public new double Version
    {
        get => Get(GDExtensionPropertyName.Version).As<double>();
        set => Set(GDExtensionPropertyName.Version, value);
    }

    public new long/* "1024:1024" */ RegionSize
    {
        get => Get(GDExtensionPropertyName.RegionSize).As<long/* "1024:1024" */>();
        set => Set(GDExtensionPropertyName.RegionSize, value);
    }

    public new bool Save16Bit
    {
        get => Get(GDExtensionPropertyName.Save16Bit).As<bool>();
        set => Set(GDExtensionPropertyName.Save16Bit, value);
    }

    public new Vector2 HeightRange
    {
        get => Get(GDExtensionPropertyName.HeightRange).As<Vector2>();
        set => Set(GDExtensionPropertyName.HeightRange, value);
    }

    public new Godot.Collections.Array RegionOffsets
    {
        get => Get(GDExtensionPropertyName.RegionOffsets).As<Godot.Collections.Array>();
        set => Set(GDExtensionPropertyName.RegionOffsets, value);
    }

    public new Godot.Collections.Array HeightMaps
    {
        get => Get(GDExtensionPropertyName.HeightMaps).As<Godot.Collections.Array>();
        set => Set(GDExtensionPropertyName.HeightMaps, value);
    }

    public new Godot.Collections.Array ControlMaps
    {
        get => Get(GDExtensionPropertyName.ControlMaps).As<Godot.Collections.Array>();
        set => Set(GDExtensionPropertyName.ControlMaps, value);
    }

    public new Godot.Collections.Array ColorMaps
    {
        get => Get(GDExtensionPropertyName.ColorMaps).As<Godot.Collections.Array>();
        set => Set(GDExtensionPropertyName.ColorMaps, value);
    }

    public new Godot.Collections.Dictionary Multimeshes
    {
        get => Get(GDExtensionPropertyName.Multimeshes).As<Godot.Collections.Dictionary>();
        set => Set(GDExtensionPropertyName.Multimeshes, value);
    }

    public new static class GDExtensionMethodName
    {
        public new static readonly StringName SetMaps = "set_maps";
        public new static readonly StringName GetMaps = "get_maps";
    }

    public new void SetMaps(Terrain3DStorage.MapType mapType, Godot.Collections.Array maps) => 
        Call(GDExtensionMethodName.SetMaps, [Variant.From(mapType), maps]);

    public new Godot.Collections.Array GetMaps(Terrain3DStorage.MapType mapType) => 
        Call(GDExtensionMethodName.GetMaps, [Variant.From(mapType)]).As<Godot.Collections.Array>();

}
