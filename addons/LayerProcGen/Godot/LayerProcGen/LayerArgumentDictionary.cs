using Godot;
using System;
using Godot.Collections;
using System.Linq;

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

        public LayerArgumentDictionary Remove(string key)
        {
            var modified = Clone();
            if (modified.parameters.ContainsKey(key))
            {
                modified.parameters.Remove(key);
            }
            return modified;
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

        public override bool Equals(object obj)
        {
            if (obj is not LayerArgumentDictionary other)
                return false;

            // Compare parameters deeply
            if (parameters.Count != other.parameters.Count)
                return false;

            foreach (var key in parameters.Keys)
            {
                if (!other.parameters.ContainsKey(key))
                    return false;

                var dictA = parameters[key];
                var dictB = other.parameters[key];

                if (dictA.Count != dictB.Count)
                    return false;

                foreach (var innerKey in dictA.Keys)
                {
                    if (!dictB.ContainsKey(innerKey) || !Equals(dictA[innerKey], dictB[innerKey]))
                        return false;
                }
            }
            return true;
        }
        
        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var key in parameters.Keys.OrderBy(k => k))
            {
                hash = hash * 31 + key.GetHashCode();
                var dict = parameters[key];
                foreach (var innerKey in dict.Keys.OrderBy(k => k))
                {
                    hash = hash * 31 + innerKey.GetHashCode();
                    hash = hash * 31 + dict[innerKey].GetHashCode();
                }
            }
            return hash;
        }
    }
}
