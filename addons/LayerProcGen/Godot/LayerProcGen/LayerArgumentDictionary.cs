using Godot;
using System;
using Godot.Collections;

namespace Runevision.LayerProcGen
{
    public partial class LayerArgument : Resource
    {
        public Dictionary<string, Dictionary<string, Variant>> parameters = new();
    }

    public class LayerArgumentDictionary
    {
        // now you can click “Add Element” in the inspector
        // and pick “LayerArgument” resources
        [Export(PropertyHint.ResourceType, nameof(LayerArgument))]
        public Array<LayerArgument> Arguments { get; set; } = new();

        public LayerArgumentDictionary()
        {
            // Initialize the Arguments array with a default LayerArgument instance
            Arguments.Add(new LayerArgument());
        }
    }
}
