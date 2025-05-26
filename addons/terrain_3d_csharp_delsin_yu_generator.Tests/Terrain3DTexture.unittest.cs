using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DTexture_Test
{
    [TestCase]
    public void Terrain3DTexture_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DTexture.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTexture_Property_TextureId()
    {
        var instance = GDExtension.Wrappers.Terrain3DTexture.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTexture_Property_UvRotation()
    {
        var instance = GDExtension.Wrappers.Terrain3DTexture.Instantiate();
        var value = instance.UvRotation;
        instance.UvRotation = value;
        instance.Free();
    }
}
