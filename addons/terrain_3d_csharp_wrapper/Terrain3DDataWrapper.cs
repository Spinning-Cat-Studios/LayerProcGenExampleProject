using System;
using System.Linq;
using Godot;
using Godot.Collections;
using TokisanGames;
using Array = Godot.Collections.Array;

namespace Terrain3DWrapper;

public class Terrain3DDataWrapper : _Terrain3DInstanceWrapper_
{
	private static readonly StringName set_control_name = "set_control";
	private static readonly StringName force_update_maps_name = "force_update_maps";
    
    private Resource AsResource => Instance as Resource;

	public void SetControl(Vector3 globalPosition, uint control)
    {
        AsResource.Call(set_control_name, globalPosition, control);
    }

    public void ForceUpdateMaps(Terrain3DRegion.MapType mapType = Terrain3DRegion.MapType.Max)
	{
		AsResource.Call(force_update_maps_name, (int)mapType);
	}

    public Terrain3DDataWrapper(GodotObject instance) : base(instance)
    {
    }
}
