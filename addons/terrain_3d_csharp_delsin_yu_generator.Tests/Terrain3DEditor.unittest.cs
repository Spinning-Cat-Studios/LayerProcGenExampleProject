using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DEditor_Test
{
    [TestCase]
    public void Terrain3DEditor_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_SetTerrain()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.SetTerrain(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_GetTerrain()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.GetTerrain();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_SetBrushData()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.SetBrushData(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_SetTool()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.SetTool(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_GetTool()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.GetTool();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_SetOperation()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.SetOperation(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_GetOperation()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.GetOperation();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_StartOperation()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.StartOperation(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_IsOperating()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.IsOperating();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_Operate()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.Operate(default, default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_BackupRegion()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.BackupRegion(default);
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_StopOperation()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.StopOperation();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DEditor_Method_ApplyUndo()
    {
        var instance = GDExtension.Wrappers.Terrain3DEditor.Instantiate();
        instance.ApplyUndo(default);
        instance.Free();
    }
}
