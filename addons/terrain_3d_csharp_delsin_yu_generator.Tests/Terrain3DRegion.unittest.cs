using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DRegion_Test
{
    [TestCase]
    public void Terrain3DRegion_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Version()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Version;
        instance.Version = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_RegionSize()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.RegionSize;
        instance.RegionSize = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_VertexSpacing()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.VertexSpacing;
        instance.VertexSpacing = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_HeightRange()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.HeightRange;
        instance.HeightRange = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_HeightMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.HeightMap;
        instance.HeightMap = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_ControlMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.ControlMap;
        instance.ControlMap = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_ColorMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.ColorMap;
        instance.ColorMap = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Instances()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Instances;
        instance.Instances = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Edited()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Edited;
        instance.Edited = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Deleted()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Deleted;
        instance.Deleted = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Modified()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Modified;
        instance.Modified = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Location()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Location;
        instance.Location = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Property_Multimeshes()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        var value = instance.Multimeshes;
        instance.Multimeshes = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_SetMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.SetMap(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_GetMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.GetMap(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_SetMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.SetMaps(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_GetMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.GetMaps();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_SanitizeMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.SanitizeMaps();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_SanitizeMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.SanitizeMap(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_ValidateMapSize()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.ValidateMapSize(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_UpdateHeight()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.UpdateHeight(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_UpdateHeights()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.UpdateHeights(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_CalcHeightRange()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.CalcHeightRange();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_Save()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.Save(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_SetData()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.SetData(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_GetData()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.GetData();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DRegion_Method_Duplicate()
    {
        var instance = GDExtension.Wrappers.Terrain3DRegion.Instantiate();
        instance.Duplicate(default);
        instance.Free();
    }
}
