#pragma warning disable CS0109
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Godot;
using Godot.Collections;

namespace TokisanGames;

public partial class Terrain3DTextureList : Resource
{

    private new static readonly StringName NativeName = new StringName("Terrain3DTextureList");

    [Obsolete("Wrapper types cannot be constructed with constructors (it only instantiate the underlying Terrain3DTextureList object), please use the Instantiate() method instead.")]
    protected Terrain3DTextureList() { }

    private static CSharpScript _wrapperScriptAsset;

    /// <summary>
    /// Try to cast the script on the supplied <paramref name="godotObject"/> to the <see cref="Terrain3DTextureList"/> wrapper type,
    /// if no script has attached to the type, or the script attached to the type does not inherit the <see cref="Terrain3DTextureList"/> wrapper type,
    /// a new instance of the <see cref="Terrain3DTextureList"/> wrapper script will get attaches to the <paramref name="godotObject"/>.
    /// </summary>
    /// <remarks>The developer should only supply the <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</remarks>
    /// <param name="godotObject">The <paramref name="godotObject"/> that represents the correct underlying GDExtension type.</param>
    /// <returns>The existing or a new instance of the <see cref="Terrain3DTextureList"/> wrapper script attached to the supplied <paramref name="godotObject"/>.</returns>
    public new static Terrain3DTextureList Bind(GodotObject godotObject)
    {
#if DEBUG
        if (!IsInstanceValid(godotObject))
            throw new InvalidOperationException("The supplied GodotObject instance is not valid.");
#endif
        if (godotObject is Terrain3DTextureList wrapperScriptInstance)
            return wrapperScriptInstance;

#if DEBUG
        var expectedType = typeof(Terrain3DTextureList);
        var currentObjectClassName = godotObject.GetClass();
        if (!ClassDB.IsParentClass(expectedType.Name, currentObjectClassName))
            throw new InvalidOperationException($"The supplied GodotObject ({currentObjectClassName}) is not the {expectedType.Name} type.");
#endif

        if (_wrapperScriptAsset is null)
        {
            var scriptPathAttribute = typeof(Terrain3DTextureList).GetCustomAttributes<ScriptPathAttribute>().FirstOrDefault();
            if (scriptPathAttribute is null) throw new UnreachableException();
            _wrapperScriptAsset = ResourceLoader.Load<CSharpScript>(scriptPathAttribute.Path);
        }

        var instanceId = godotObject.GetInstanceId();
        godotObject.SetScript(_wrapperScriptAsset);
        return (Terrain3DTextureList)InstanceFromId(instanceId);
    }

    /// <summary>
    /// Creates an instance of the GDExtension <see cref="Terrain3DTextureList"/> type, and attaches a wrapper script instance to it.
    /// </summary>
    /// <returns>The wrapper instance linked to the underlying GDExtension "Terrain3DTextureList" type.</returns>
    public new static Terrain3DTextureList Instantiate() => Bind(ClassDB.Instantiate(NativeName).As<GodotObject>());

    public new static class GDExtensionPropertyName
    {
        public new static readonly StringName Textures = "textures";
    }

    public new Godot.Collections.Array Textures
    {
        get => Get(GDExtensionPropertyName.Textures).As<Godot.Collections.Array>();
        set => Set(GDExtensionPropertyName.Textures, value);
    }

}
