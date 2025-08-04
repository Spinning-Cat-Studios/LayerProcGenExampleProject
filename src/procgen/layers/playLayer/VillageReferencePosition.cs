using Godot;

namespace LayerProcGenExampleProject.ProcGen.Layers.PlayLayerComponents
{
    /// <summary>
    /// Simple static class to store the current reference position for village chunk generation.
    /// This allows chunks to determine whether they should render based on distance from the reference position.
    /// </summary>
    public static class VillageReferencePosition
    {
        private static Vector3 _currentReferencePosition = Vector3.Zero;
        
        public static Vector3 Current => _currentReferencePosition;
        
        public static void Update(Vector3 newReferencePosition)
        {
            _currentReferencePosition = newReferencePosition;
        }
    }
}