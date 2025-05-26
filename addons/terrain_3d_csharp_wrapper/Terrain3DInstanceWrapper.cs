using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Terrain3DWrapper;

public class _Terrain3DInstanceWrapper_ : IDisposable
{
    protected virtual string ExpectedGodotClassName => GetType().Name.Replace("Wrapper", "");

    public _Terrain3DInstanceWrapper_(GodotObject instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance), "GodotObject instance passed to wrapper is null.");
        if (!ClassDB.IsParentClass(instance.GetClass(), ExpectedGodotClassName))
            throw new ArgumentException("\"_instance\" has the wrong type.");
        Instance = instance;
    }

    public GodotObject Instance { get; protected set; }

    public void Dispose()
    {
        Instance?.Dispose();
        Instance = null!;
    }

    public void ClearNativePointer()
    {
        Instance = null!;
    }
}
