using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using Terrain3DWrapper;
using TokisanGames;

namespace Terrain3DWrapper;

public class _Terrain3DInstanceWrapper_ : IDisposable
{
    public _Terrain3DInstanceWrapper_(GodotObject instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        if (!ClassDB.IsParentClass(instance.GetClass(), GetType().Name)) throw new ArgumentException("\"_instance\" has the wrong type.");
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

public class Terrain3DWrapper : _Terrain3DInstanceWrapper_
{
    private static readonly StringName storage_name = "storage";
    private static readonly StringName data_name = "data";
    private static readonly StringName mesh_vertex_spacing_name = "mesh_vertex_spacing";

    private Terrain3DStorageWrapper? storage;
    private Terrain3DDataWrapper? data;

    public Terrain3DWrapper(GodotObject instance) : base(instance)
    {
    }

    public float MeshVertexSpacing
    {
        get => Instance.Get(mesh_vertex_spacing_name).AsSingle();
        set => Instance.Set(mesh_vertex_spacing_name, value);
    }

    public TokisanGames.Terrain3DStorage Storage
    {
        get
        {
            storage ??= new Terrain3DStorageWrapper(Instance.Get(storage_name).AsGodotObject());
            return (TokisanGames.Terrain3DStorage)storage.Instance;
        }
        set => Instance.Set(storage_name, value.Instance); //TODO: maybe cleanup the old one
    }

    public Terrain3DData Data
    {
        get
        {
            data ??= new Terrain3DDataWrapper(Instance.Get(data_name).AsGodotObject());
            return (TokisanGames.Terrain3DData)data.Instance;
        }
        set => Instance.Set(data_name, value);
    }
}
