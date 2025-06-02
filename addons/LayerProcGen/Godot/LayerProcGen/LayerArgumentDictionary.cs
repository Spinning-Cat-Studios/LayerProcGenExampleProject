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
        public Godot.Collections.Dictionary<string, Godot.Collections.Dictionary<string, Variant>> parameters { get; set; } = new();

        public LayerArgumentDictionary Clone()
        {
            var clone = new LayerArgumentDictionary();
            foreach (var kvp in parameters)
            {
                clone.parameters[kvp.Key] = new Godot.Collections.Dictionary<string, Variant>(kvp.Value);
            }
            return clone;
        }

        public LayerArgumentDictionary Merge(LayerArgumentDictionary other)
        {
            var merged = Clone();
            foreach (var kvp in other.parameters)
            {
                if (merged.parameters.ContainsKey(kvp.Key))
                {
                    // Merge dictionaries for the same key
                    foreach (var innerKvp in kvp.Value)
                    {
                        merged.parameters[kvp.Key][innerKvp.Key] = innerKvp.Value;
                    }
                }
                else
                {
                    merged.parameters[kvp.Key] = new Godot.Collections.Dictionary<string, Variant>(kvp.Value);
                }
            }
            return merged;
        }

        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("LayerArgumentDictionary:");
            foreach (var kvp in parameters)
            {
                result.AppendLine($"  {kvp.Key}:");
                foreach (var innerKvp in kvp.Value)
                {
                    result.AppendLine($"    {innerKvp.Key}: {innerKvp.Value}");
                }
            }
            return result.ToString();
        }
    }
}
