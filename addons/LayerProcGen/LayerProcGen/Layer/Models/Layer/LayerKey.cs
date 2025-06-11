using System;
using System.Collections.Generic;
using Godot;

namespace Runevision.LayerProcGen
{

    public class LayerKey : IEquatable<LayerKey>
    {
        public Type LayerType { get; }
        public LayerArgumentDictionary Arguments { get; }
        public string Subtype { get; }

        public LayerKey(Type layerType, LayerArgumentDictionary arguments, string subtype = null)
        {
            LayerType = layerType;
            Arguments = arguments;
            Subtype = subtype ?? "";
        }

        public bool Equals(LayerKey other)
        {
            return other != null &&
                LayerType == other.LayerType &&
                Subtype == other.Subtype &&
                (Arguments?.Equals(other.Arguments) ?? other.Arguments == null);
        }

        public override bool Equals(object obj) => Equals(obj as LayerKey);

        public override int GetHashCode()
        {
            return HashCode.Combine(LayerType, Arguments, Subtype);
        }

        public override string ToString()
        {
            return $"({LayerType.Name}, {Subtype}, {Arguments})";
        }
    }

}
