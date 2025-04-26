using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;

public class LSystemVillageLayer : ChunkBasedDataLayer<LSystemVillageLayer, LSystemVillageChunk>, ILayerVisualization
{
    public override int chunkW => 360;
    public override int chunkH => 360;

    public static readonly Point requiredPadding = new Point(180, 180);

    public Node3D layerParent;

    public LSystemVillageLayer()
    {
        layerParent = new Node3D { Name = "LSystemVillageLayer" };
        AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
    }

    public Node LayerRoot() => layerParent;

    public void VisualizationUpdate() { /* implement debug visualization if desired */ }
}
