using Godot;
using System;

public class RoadPainterService
{
    private NodePath _terrainPath;

    public RoadPainterService(NodePath terrainPath)
    {
        _terrainPath = terrainPath;
    }

    public void SetTerrain(NodePath path)
    {
        // Set the terrain path for the road painter service
        _terrainPath = path;
    }
}
