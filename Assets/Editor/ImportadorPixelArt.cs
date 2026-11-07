using UnityEditor;

/// <summary>
/// Configura AUTOMATICAMENTE toda imagen PNG que entre al proyecto con las reglas de la Guia visual:
/// Sprite, 32 pixeles por unidad, Filter Mode Point, sin compresion y sin mipmaps.
/// Asi los artistas solo arrastran o reemplazan el PNG y no tienen que tocar nada en Unity.
/// </summary>
public class ImportadorPixelArt : AssetPostprocessor
{
    public const int PixelesPorUnidad = 32;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/") || !assetPath.EndsWith(".png")) return;
        if (assetPath.StartsWith("Assets/Welcome/")) return;
        var imp = (TextureImporter)assetImporter;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = PixelesPorUnidad;
        imp.filterMode = UnityEngine.FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.wrapMode = UnityEngine.TextureWrapMode.Clamp;
    }
}
