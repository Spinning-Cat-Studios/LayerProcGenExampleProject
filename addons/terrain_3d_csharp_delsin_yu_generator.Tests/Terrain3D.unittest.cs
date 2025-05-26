using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3D_Test
{
    [TestCase]
    public void Terrain3D_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Version()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.Version = default;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_DebugLevel()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.DebugLevel;
        instance.DebugLevel = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_DataDirectory()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.DataDirectory;
        instance.DataDirectory = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Data()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.Data = default;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Material()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.Material;
        instance.Material = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Assets()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.Assets;
        instance.Assets = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Instancer()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.Instancer = default;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_RegionSize()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.RegionSize;
        instance.RegionSize = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Save16Bit()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.Save16Bit;
        instance.Save16Bit = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_LabelDistance()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.LabelDistance;
        instance.LabelDistance = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_LabelSize()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.LabelSize;
        instance.LabelSize = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_ShowGrid()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.ShowGrid;
        instance.ShowGrid = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CollisionEnabled()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CollisionEnabled;
        instance.CollisionEnabled = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CollisionMode()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CollisionMode;
        instance.CollisionMode = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CollisionLayer()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CollisionLayer;
        instance.CollisionLayer = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CollisionMask()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CollisionMask;
        instance.CollisionMask = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CollisionPriority()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CollisionPriority;
        instance.CollisionPriority = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_MeshLods()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.MeshLods;
        instance.MeshLods = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_MeshSize()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.MeshSize;
        instance.MeshSize = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_VertexSpacing()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.VertexSpacing;
        instance.VertexSpacing = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_RenderLayers()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.RenderLayers;
        instance.RenderLayers = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_MouseLayer()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.MouseLayer;
        instance.MouseLayer = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CastShadows()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CastShadows;
        instance.CastShadows = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_GiMode()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.GiMode;
        instance.GiMode = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_CullMargin()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.CullMargin;
        instance.CullMargin = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_TextureList()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.TextureList;
        instance.TextureList = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Property_Storage()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        var value = instance.Storage;
        instance.Storage = value;
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_SetEditor()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.SetEditor(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_GetEditor()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.GetEditor();
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_SetPlugin()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.SetPlugin(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_GetPlugin()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.GetPlugin();
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_SetCamera()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.SetCamera(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_GetCamera()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.GetCamera();
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_GetCollisionRid()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.GetCollisionRid();
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_IsCompatibilityMode()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.IsCompatibilityMode();
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_GetIntersection()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.GetIntersection(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_BakeMesh()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.BakeMesh(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_GenerateNavMeshSourceGeometry()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.GenerateNavMeshSourceGeometry(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3D_Method_SplitStorage()
    {
        var instance = GDExtension.Wrappers.Terrain3D.Instantiate();
        instance.SplitStorage();
        instance.Free();
    }
}
