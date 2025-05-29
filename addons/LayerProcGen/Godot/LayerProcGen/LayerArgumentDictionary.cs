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

        public override bool Equals(object obj)
        {
            if (obj is not LayerArgumentDictionary other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (parameters.Count != other.parameters.Count)
                return false;

            foreach (var kvp in parameters)
            {
                if (!other.parameters.TryGetValue(kvp.Key, out var otherInnerDict))
                    return false;

                var innerDict = kvp.Value;
                if (innerDict.Count != otherInnerDict.Count)
                    return false;

                foreach (var innerKvp in innerDict)
                {
                    if (!otherInnerDict.TryGetValue(innerKvp.Key, out var otherValue))
                        return false;
                    if (!Equals(innerKvp.Value, otherValue))
                        return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var kvp in parameters)
            {
                hash = hash * 31 + kvp.Key.GetHashCode();
                foreach (var innerKvp in kvp.Value)
                {
                    hash = hash * 31 + innerKvp.Key.GetHashCode();
                    hash = hash * 31 + innerKvp.Value.GetHashCode();
                }
            }
            return hash;
        }
    }
}
