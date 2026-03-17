#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ProjectSetup
{
    [MenuItem("Setup/Build & Update System")]
    public static void BuildAndUpdateSystem()
    {
        Debug.Log("Starting Build & Update System...");
        
        // 1. Sprite Settings
        FixSprite("Assets/dirt_sprite.png");
        FixSprite("Assets/player_sprite.png");

        // 2. Tile Assets
        Tile dirtTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/DirtTile.asset");
        if (dirtTile == null) {
            dirtTile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(dirtTile, "Assets/DirtTile.asset");
        }
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/dirt_sprite.png");
        dirtTile.sprite = s;
        dirtTile.colliderType = Tile.ColliderType.Sprite;
        EditorUtility.SetDirty(dirtTile);

        // 3. Find missing scripts and re-attach WorldGenerator if possible
        GameObject wgObj = GameObject.Find("WorldGenerator");
        if (wgObj != null) {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(wgObj);
            if (wgObj.GetComponent<WorldGenerator>() == null) {
                Debug.Log("Re-attaching WorldGenerator script...");
                wgObj.AddComponent<WorldGenerator>();
            }
        }

        // 4. World Generator & Tilemap Setup
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        Tilemap ground = GameObject.Find("Ground")?.GetComponent<Tilemap>();
        if (wg != null && ground != null)
        {
            wg.groundTilemap = ground;
            wg.groundTile = dirtTile;
            wg.GenerateWorld();
            
            ground.RefreshAllTiles();
            ground.CompressBounds();
            
            var col = ground.GetComponent<TilemapCollider2D>();
            if (col) col.ProcessTilemapChanges();

            Debug.Log($"Final Bounds: {ground.localBounds}, ShapeCount: {col?.shapeCount}");
            EditorUtility.SetDirty(wg);
        }

        // 5. Player Setup
        GameObject player = GameObject.Find("Player");
        if (player != null) {
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr) sr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            
            if (wg != null) {
                Vector3 baseSpawn = wg.GetSpawnPosition();
                // Add an additional offset to ensure the player spawns in the air and falls down
                player.transform.position = new Vector3(baseSpawn.x, baseSpawn.y + 1.5f, baseSpawn.z);
            }
            EditorUtility.SetDirty(player);

            // 6. Create Safety Platform
            CreateSafetyPlatform(player.transform.position);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log("Build & Update System Completed and Scene Saved!");
    }

    static void CreateSafetyPlatform(Vector3 spawnPos)
    {
        GameObject platformParent = GameObject.Find("SpawnSafetyPlatform");
        if (platformParent != null) Object.DestroyImmediate(platformParent);
        
        platformParent = new GameObject("SpawnSafetyPlatform");
        
        // Brown color for the blocks
        Color brown = new Color(0.45f, 0.25f, 0.1f);
        // Lower by 1 additional unit as requested (total -2.5 from spawnPos)
        Vector3 platformStart = spawnPos + Vector3.down * 2.5f;

        // Expanded to 15x6 (X: -7 to 7, Y: 0 to -5)
        for (int y = 0; y > -6; y--)
        {
            for (int x = -7; x <= 7; x++)
            {
                GameObject block = new GameObject($"SafetyBlock_{x}_{Mathf.Abs(y)}");
                block.transform.parent = platformParent.transform;
                block.transform.position = platformStart + new Vector3(x, y, 0);
                
                // Add BoxCollider2D (Static is default)
                var bc = block.AddComponent<BoxCollider2D>();
                bc.size = Vector2.one;
                
                // Visuals
                SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
                Sprite dirtSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/dirt_sprite.png");
                sr.sprite = dirtSprite;
                sr.color = brown;
                sr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            }
        }
        
        Debug.Log("Expanded 15x6 Safety Foundation created under the player.");
    }

    static void FixSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }
    }
}
#endif
