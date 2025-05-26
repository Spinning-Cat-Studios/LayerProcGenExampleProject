using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using Terrain3DWrapper;
using TokisanGames;

namespace Terrain3DWrapper;

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

    public Terrain3DWrapper() : base(ClassDB.Instantiate(nameof(Terrain3D)).AsGodotObject())
	{
	}

    public float MeshVertexSpacing
    {
        get => Instance.Get(mesh_vertex_spacing_name).AsSingle();
        set => Instance.Set(mesh_vertex_spacing_name, value);
    }

    public Terrain3DStorageWrapper Storage
    {
        get
        {
            var storageObj = Instance.Get(storage_name).AsGodotObject();
            if (storageObj == null)
            {
                GD.PrintErr("Terrain3D 'storage' property is null!");
                return null;
            }
            storage ??= new Terrain3DStorageWrapper(storageObj);
            return storage;
        }
        set => Instance.Set(storage_name, value?.Instance);
    }

    public Terrain3DDataWrapper Data
    {
        get
        {
            var dataObj = Instance.Get(data_name).AsGodotObject();
            if (dataObj == null)
            {
                GD.PrintErr("Terrain3D 'data' property is null!");
                return null;
            }
            data ??= new Terrain3DDataWrapper(dataObj);
            return data;
        }
        set => Instance.Set(data_name, value?.Instance);
    }
}
