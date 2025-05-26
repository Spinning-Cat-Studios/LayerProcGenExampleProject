using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DData_Test
{
    [TestCase]
    public void Terrain3DData_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Property_RegionLocations()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        var value = instance.RegionLocations;
        instance.RegionLocations = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Property_HeightMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.HeightMaps = default;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Property_ControlMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.ControlMaps = default;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Property_ColorMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.ColorMaps = default;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionCount()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionCount();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionsActive()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionsActive(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionsAll()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionsAll();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionMap()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionMap();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionMapIndex()
    {
        Terrain3DData.GetRegionMapIndex(default);
    }
    [TestCase]
    public void Terrain3DData_Method_DoForRegions()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.DoForRegions(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_ChangeRegionSize()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.ChangeRegionSize(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionLocation()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionLocation(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionId()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionId(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionIdp()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionIdp(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_HasRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.HasRegion(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_HasRegionp()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.HasRegionp(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegion(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRegionp()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRegionp(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetRegionModified()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetRegionModified(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_IsRegionModified()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.IsRegionModified(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetRegionDeleted()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetRegionDeleted(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_IsRegionDeleted()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.IsRegionDeleted(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_AddRegionBlankp()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.AddRegionBlankp(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_AddRegionBlank()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.AddRegionBlank(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_AddRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.AddRegion(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_RemoveRegionp()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.RemoveRegionp(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_RemoveRegionl()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.RemoveRegionl(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_RemoveRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.RemoveRegion(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SaveDirectory()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SaveDirectory(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SaveRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SaveRegion(default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_LoadDirectory()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.LoadDirectory(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_LoadRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.LoadRegion(default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetMaps(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_ForceUpdateMaps()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.ForceUpdateMaps(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetHeightMapsRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetHeightMapsRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlMapsRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlMapsRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetColorMapsRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetColorMapsRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetPixel()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetPixel(default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetPixel()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetPixel(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetHeight()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetHeight(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetHeight()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetHeight(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetColor()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetColor(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetColor()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetColor(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControl()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControl(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControl()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControl(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetRoughness()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetRoughness(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetRoughness()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetRoughness(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlBaseId()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlBaseId(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlBaseId()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlBaseId(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlOverlayId()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlOverlayId(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlOverlayId()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlOverlayId(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlBlend()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlBlend(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlBlend()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlBlend(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlAngle()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlAngle(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlAngle()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlAngle(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlScale()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlScale(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlScale()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlScale(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlHole()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlHole(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlHole()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlHole(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlNavigation()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlNavigation(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlNavigation()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlNavigation(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_SetControlAuto()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.SetControlAuto(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetControlAuto()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetControlAuto(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetNormal()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetNormal(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_IsInSlope()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.IsInSlope(default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetTextureId()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetTextureId(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetMeshVertex()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetMeshVertex(default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_GetHeightRange()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.GetHeightRange();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_CalcHeightRange()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.CalcHeightRange(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_ImportImages()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.ImportImages(default, default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_ExportImage()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.ExportImage(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DData_Method_LayeredToImage()
    {
        var instance = GDExtension.Wrappers.Terrain3DData.Instantiate();
        instance.LayeredToImage(default);
        instance.Free();
    }
}
