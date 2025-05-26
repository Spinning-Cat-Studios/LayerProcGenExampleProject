using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DInstancer_Test
{
    [TestCase]
    public void Terrain3DInstancer_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_ClearByMesh()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.ClearByMesh(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_ClearByLocation()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.ClearByLocation(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_ClearByRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.ClearByRegion(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_AddInstances()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.AddInstances(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_RemoveInstances()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.RemoveInstances(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_AddMultimesh()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.AddMultimesh(default, default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_AddTransforms()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.AddTransforms(default, default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_AppendLocation()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.AppendLocation(default, default, default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_AppendRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.AppendRegion(default, default, default, default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_UpdateTransforms()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.UpdateTransforms(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_ForceUpdateMmis()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.ForceUpdateMmis();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_SwapIds()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.SwapIds(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_DumpData()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.DumpData();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DInstancer_Method_DumpMmis()
    {
        var instance = GDExtension.Wrappers.Terrain3DInstancer.Instantiate();
        instance.DumpMmis();
        instance.Free();
    }
}
