using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DAssets_Test
{
    [TestCase]
    public void Terrain3DAssets_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Property_MeshList()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        var value = instance.MeshList;
        instance.MeshList = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Property_TextureList()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        var value = instance.TextureList;
        instance.TextureList = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_SetTexture()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.SetTexture(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetTexture()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetTexture(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetTextureCount()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetTextureCount();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetAlbedoArrayRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetAlbedoArrayRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetNormalArrayRid()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetNormalArrayRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetTextureColors()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetTextureColors();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetTextureUvScales()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetTextureUvScales();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetTextureDetiles()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetTextureDetiles();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_UpdateTextureList()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.UpdateTextureList();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_SetMeshAsset()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.SetMeshAsset(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetMeshAsset()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetMeshAsset(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_GetMeshCount()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.GetMeshCount();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_CreateMeshThumbnails()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.CreateMeshThumbnails(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_UpdateMeshList()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.UpdateMeshList();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DAssets_Method_Save()
    {
        var instance = GDExtension.Wrappers.Terrain3DAssets.Instantiate();
        instance.Save(default);
        instance.Free();
    }
}
