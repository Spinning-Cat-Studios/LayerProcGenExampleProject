using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Terrain3DWrapper;

public class Terrain3DDataWrapper : _Terrain3DInstanceWrapper_
{
    public Terrain3DDataWrapper(GodotObject instance) : base(instance)
    {
    }
}
