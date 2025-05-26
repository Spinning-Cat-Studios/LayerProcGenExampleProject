using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DMaterial_Test
{
    [TestCase]
    public void Terrain3DMaterial_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShaderParameters()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShaderParameters;
        instance.ShaderParameters = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_WorldBackground()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.WorldBackground;
        instance.WorldBackground = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_TextureFiltering()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.TextureFiltering;
        instance.TextureFiltering = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_AutoShader()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.AutoShader;
        instance.AutoShader = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_DualScaling()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.DualScaling;
        instance.DualScaling = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShaderOverrideEnabled()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShaderOverrideEnabled;
        instance.ShaderOverrideEnabled = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShaderOverride()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShaderOverride;
        instance.ShaderOverride = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowCheckered()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowCheckered;
        instance.ShowCheckered = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowGrey()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowGrey;
        instance.ShowGrey = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowHeightmap()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowHeightmap;
        instance.ShowHeightmap = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowColormap()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowColormap;
        instance.ShowColormap = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowRoughmap()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowRoughmap;
        instance.ShowRoughmap = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowControlTexture()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowControlTexture;
        instance.ShowControlTexture = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowControlAngle()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowControlAngle;
        instance.ShowControlAngle = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowControlScale()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowControlScale;
        instance.ShowControlScale = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowControlBlend()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowControlBlend;
        instance.ShowControlBlend = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowAutoshader()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowAutoshader;
        instance.ShowAutoshader = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowNavigation()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowNavigation;
        instance.ShowNavigation = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowTextureHeight()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowTextureHeight;
        instance.ShowTextureHeight = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowTextureNormal()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowTextureNormal;
        instance.ShowTextureNormal = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowTextureRough()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowTextureRough;
        instance.ShowTextureRough = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowRegionGrid()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowRegionGrid;
        instance.ShowRegionGrid = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowVertexGrid()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowVertexGrid;
        instance.ShowVertexGrid = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Property_ShowInstancerGrid()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        var value = instance.ShowInstancerGrid;
        instance.ShowInstancerGrid = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Method_Update()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.Update();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Method_GetMaterialRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.GetMaterialRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Method_GetShaderRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.GetShaderRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Method_SetShaderParam()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.SetShaderParam(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Method_GetShaderParam()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.GetShaderParam(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMaterial_Method_Save()
    {
        var instance = GDExtension.Wrappers.Terrain3DMaterial.Instantiate();
        instance.Save(default);
        instance.Free();
    }
}
