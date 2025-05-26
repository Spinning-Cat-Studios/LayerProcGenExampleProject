using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DStorage_Test
{
    [TestCase]
    public void Terrain3DStorage_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_Version()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.Version;
        instance.Version = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_RegionSize()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.RegionSize;
        instance.RegionSize = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_Save16Bit()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.Save16Bit;
        instance.Save16Bit = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_HeightRange()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.HeightRange;
        instance.HeightRange = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_RegionOffsets()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.RegionOffsets;
        instance.RegionOffsets = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_HeightMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.HeightMaps;
        instance.HeightMaps = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_ControlMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.ControlMaps;
        instance.ControlMaps = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_ColorMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.ColorMaps;
        instance.ColorMaps = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Property_Multimeshes()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        var value = instance.Multimeshes;
        instance.Multimeshes = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Method_SetMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        instance.SetMaps(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DStorage_Method_GetMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DStorage.Instantiate();
        instance.GetMaps(default);
        instance.Free();
    }
}
