#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class ProjectSetup
{
    [MenuItem("Tools/Execute System Setup")]
    public static void ExecuteSystemSetup()
    {
        Debug.Log("=== Execute System Setup: Starting... ===");

        // 0. Core setup via Helper
        SetupHelper.RunSetup();

        // 1. Grid & Tilemap Setup
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj == null) gridObj = new GameObject("Grid");
        Grid grid = gridObj.GetComponent<Grid>() ?? gridObj.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        GameObject groundObj = GameObject.Find("GroundTilemap");
        if (groundObj == null)
        {
            groundObj = new GameObject("GroundTilemap");
            groundObj.transform.parent = gridObj.transform;
        }
        
        Tilemap tilemap = groundObj.GetComponent<Tilemap>() ?? groundObj.AddComponent<Tilemap>();
        TilemapRenderer tr = groundObj.GetComponent<TilemapRenderer>() ?? groundObj.AddComponent<TilemapRenderer>();
        
        // Layer Setup
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0) groundObj.layer = groundLayer;

        // Physics Setup on Ground
        Rigidbody2D rb = groundObj.GetComponent<Rigidbody2D>() ?? groundObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        
        CompositeCollider2D composite = groundObj.GetComponent<CompositeCollider2D>() ?? groundObj.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
        
        TilemapCollider2D tc = groundObj.GetComponent<TilemapCollider2D>() ?? groundObj.AddComponent<TilemapCollider2D>();
        tc.usedByComposite = true;

        // 2. Player Setup
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            // Disable SpriteRenderer, Enable TextMeshPro (己)
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            TMPro.TextMeshPro textMesh = player.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh == null)
            {
                GameObject textObj = new GameObject("Text (己)");
                textObj.transform.SetParent(player.transform);
                textObj.transform.localPosition = Vector3.zero;
                textMesh = textObj.AddComponent<TMPro.TextMeshPro>();
            }
            textMesh.text = "己";
            textMesh.fontSize = 5;
            textMesh.color = Color.green;
            textMesh.alignment = TMPro.TextAlignmentOptions.Center;
            
            var fontAsset = AssetDatabase.FindAssets("NotoSansJP-Bold SDF").Select(guid => AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid))).FirstOrDefault();
            if (fontAsset != null) textMesh.font = fontAsset;
        }

        // 3. Tile Assets & WorldGenerator
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        if (wg != null)
        {
            wg.groundTilemap = tilemap;
            wg.backgroundTilemap = GameObject.Find("Background")?.GetComponent<Tilemap>();
            wg.groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/DirtTile.asset");
            wg.wallTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/WallTile.asset");

            // KiTile
            string kiPath = "Assets/Tiles/KiTile.asset";
            if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");
            Tile kiTile = AssetDatabase.LoadAssetAtPath<Tile>(kiPath);
            if (kiTile == null)
            {
                kiTile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(kiTile, kiPath);
            }
            kiTile.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Images/Ki.png").OfType<Sprite>().FirstOrDefault();
            EditorUtility.SetDirty(kiTile);
            wg.kiTile = kiTile;

            // KinTile
            string kinPath = "Assets/Tiles/KinTile.asset";
            Tile kinTile = AssetDatabase.LoadAssetAtPath<Tile>(kinPath);
            if (kinTile == null)
            {
                kinTile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(kinTile, kinPath);
            }
            kinTile.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Images/Kin.png").OfType<Sprite>().FirstOrDefault();
            EditorUtility.SetDirty(kinTile);
            wg.kinTile = kinTile;

            EditorUtility.SetDirty(wg);
        }

        // 4. BlockInteraction Sync
        BlockInteraction bi = player != null ? player.GetComponent<BlockInteraction>() : null;
        if (bi != null && wg != null)
        {
            bi.groundTilemap = tilemap;
            bi.kiTile = wg.kiTile;
            bi.kinTile = wg.kinTile;
            bi.groundTile = wg.groundTile;
            EditorUtility.SetDirty(bi);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("=== Execute System Setup: Completed! ===");
    }
}
#endif
