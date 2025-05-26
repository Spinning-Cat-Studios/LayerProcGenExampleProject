using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DUtil_Test
{
    [TestCase]
    public void Terrain3DUtil_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DUtil.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DUtil_Method_AsFloat()
    {
        Terrain3DUtil.AsFloat(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_AsUint()
    {
        Terrain3DUtil.AsUint(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetBase()
    {
        Terrain3DUtil.GetBase(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncBase()
    {
        Terrain3DUtil.EncBase(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetOverlay()
    {
        Terrain3DUtil.GetOverlay(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncOverlay()
    {
        Terrain3DUtil.EncOverlay(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetBlend()
    {
        Terrain3DUtil.GetBlend(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncBlend()
    {
        Terrain3DUtil.EncBlend(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetUvRotation()
    {
        Terrain3DUtil.GetUvRotation(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncUvRotation()
    {
        Terrain3DUtil.EncUvRotation(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetUvScale()
    {
        Terrain3DUtil.GetUvScale(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncUvScale()
    {
        Terrain3DUtil.EncUvScale(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_IsHole()
    {
        Terrain3DUtil.IsHole(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncHole()
    {
        Terrain3DUtil.EncHole(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_IsNav()
    {
        Terrain3DUtil.IsNav(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncNav()
    {
        Terrain3DUtil.EncNav(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_IsAuto()
    {
        Terrain3DUtil.IsAuto(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_EncAuto()
    {
        Terrain3DUtil.EncAuto(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_FilenameToLocation()
    {
        Terrain3DUtil.FilenameToLocation(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_LocationToFilename()
    {
        Terrain3DUtil.LocationToFilename(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_BlackToAlpha()
    {
        Terrain3DUtil.BlackToAlpha(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetMinMax()
    {
        Terrain3DUtil.GetMinMax(default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetThumbnail()
    {
        Terrain3DUtil.GetThumbnail(default, default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_GetFilledImage()
    {
        Terrain3DUtil.GetFilledImage(default, default, default, default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_LoadImage()
    {
        Terrain3DUtil.LoadImage(default, default, default, default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_PackImage()
    {
        Terrain3DUtil.PackImage(default, default, default, default, default);
    }
    [TestCase]
    public void Terrain3DUtil_Method_LuminanceToHeight()
    {
        Terrain3DUtil.LuminanceToHeight(default);
    }
}
