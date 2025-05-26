using GDExtension.Wrappers;
using GdUnit4;

namespace GDExtensionAPIGenerator.Tests;

[TestSuite]
public class Terrain3DTextureList_Test
{
    [TestCase]
    public void Terrain3DTextureList_Construction()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureList.Instantiate();
        instance.Free();
    }
    [TestCase]
    public void Terrain3DTextureList_Property_Textures()
    {
        var instance = GDExtension.Wrappers.Terrain3DTextureList.Instantiate();
        var value = instance.Textures;
        instance.Textures = value;
        instance.Free();
    }
}
