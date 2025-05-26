using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DTextureAsset_Test
{
    [TestCase]
    public void Terrain3DTextureAsset_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_Name()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.Name;
        instance.Name = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_Id()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.Id;
        instance.Id = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_AlbedoColor()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.AlbedoColor;
        instance.AlbedoColor = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_AlbedoTexture()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.AlbedoTexture;
        instance.AlbedoTexture = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_NormalTexture()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.NormalTexture;
        instance.NormalTexture = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_UvScale()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.UvScale;
        instance.UvScale = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Property_Detiling()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        var value = instance.Detiling;
        instance.Detiling = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureAsset_Method_Clear()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureAsset.Instantiate();
        instance.Clear();
        instance.Free();
    }
}
