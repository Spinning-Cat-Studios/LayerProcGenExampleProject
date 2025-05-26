using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DMeshAsset_Test
{
    [TestCase]
    public void Terrain3DMeshAsset_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_Name()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.Name;
        instance.Name = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_Id()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.Id;
        instance.Id = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_HeightOffset()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.HeightOffset;
        instance.HeightOffset = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_Density()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.Density;
        instance.Density = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_VisibilityRange()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.VisibilityRange;
        instance.VisibilityRange = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_CastShadows()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.CastShadows;
        instance.CastShadows = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_SceneFile()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.SceneFile;
        instance.SceneFile = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_MaterialOverride()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.MaterialOverride;
        instance.MaterialOverride = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_GeneratedType()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.GeneratedType;
        instance.GeneratedType = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_GeneratedFaces()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.GeneratedFaces;
        instance.GeneratedFaces = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Property_GeneratedSize()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        var value = instance.GeneratedSize;
        instance.GeneratedSize = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Method_Clear()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        instance.Clear();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Method_GetMesh()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        instance.GetMesh(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Method_GetMeshCount()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        instance.GetMeshCount();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DMeshAsset_Method_GetThumbnail()
    {
        var instance = GDExtension.Wrappers.Terrain3DMeshAsset.Instantiate();
        instance.GetThumbnail();
        instance.Free();
    }
}
