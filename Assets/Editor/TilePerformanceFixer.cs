#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TilePerformanceFixer
{
    [MenuItem("Tools/Optimize/Run Performance Fixes")]
    public static void RunOptimizations()
    {
        // 1. Optimize Tiles
        string[] tilePaths = { "Assets/Tiles/KiTile.asset", "Assets/Tiles/KinTile.asset" };
        foreach (var path in tilePaths)
        {
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile != null)
            {
                tile.colliderType = Tile.ColliderType.Grid;
                EditorUtility.SetDirty(tile);
                Debug.Log($"Tile {path} ColliderType set to Grid.");
            }
        }

        // 2. Optimize Textures
        string[] texPaths = { "Assets/Images/Ki.png", "Assets/Images/Kin.png" };
        foreach (var path in texPaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                
                // Disable fallback physics shape generation (Sprite mesh physics)
                settings.spriteGenerateFallbackPhysicsShape = false;
                
                importer.SetTextureSettings(settings);
                importer.maxTextureSize = 256;
                // Ensure compression is normal
                importer.textureCompression = TextureImporterCompression.Compressed;
                
                importer.SaveAndReimport();
                Debug.Log($"Texture {path} optimized: MaxSize=256, PhysicsShape=Disabled.");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("=== Performance Optimizations: Completed! ===");
    }
}
#endif
