using Godot;
using System;
using Godot.Collections;

namespace Runevision.LayerProcGen
{
    [Tool]
    [GlobalClass]
    public partial class LayerArgumentDictionary : Resource
    {
        [Export]
        public Dictionary<string, Dictionary<string, Variant>> parameters { get; set; } = new();
    }
}
