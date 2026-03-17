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
        Debug.Log("Starting Build & Update System (Troubleshooting Mode)...");
        
        // 1. Grid & Tilemap Infrastructure Fix (Phase 1)
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj == null) gridObj = new GameObject("Grid");
        
        // Reset Grid Transform
        gridObj.transform.position = Vector3.zero;
        gridObj.transform.rotation = Quaternion.identity;
        gridObj.transform.localScale = Vector3.one;

        // Force Grid Component
        Grid grid = gridObj.GetComponent<Grid>();
        if (grid == null) grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);

        // GroundTilemap Setup
        GameObject groundObj = GameObject.Find("GroundTilemap");
        if (groundObj == null)
        {
            groundObj = new GameObject("GroundTilemap");
            groundObj.transform.parent = gridObj.transform;
        }
        
        // Reset Tilemap Transform
        groundObj.transform.localPosition = Vector3.zero;
        groundObj.transform.localRotation = Quaternion.identity;
        groundObj.transform.localScale = Vector3.one;

        // Ensure Components
        Tilemap tilemap = groundObj.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = groundObj.AddComponent<Tilemap>();
        
        TilemapRenderer tr = groundObj.GetComponent<TilemapRenderer>();
        if (tr == null) tr = groundObj.AddComponent<TilemapRenderer>();
        tr.enabled = true;

        TilemapCollider2D tc = groundObj.GetComponent<TilemapCollider2D>();
        if (tc == null) tc = groundObj.AddComponent<TilemapCollider2D>();
        tc.enabled = true;

        // 2. Simple Spawning Logic (Phase 2)
        GameObject player = GameObject.Find("Player");
        Tile dirtTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/DirtTile.asset");

        if (player != null && tilemap != null && dirtTile != null)
        {
            // Clear current tiles to isolate the test
            tilemap.ClearAllTiles();

            // Place exactly 1 block under the player
            Vector3 playerPos = player.transform.position;
            // Target the cell directly below the player's center
            Vector3Int cellPos = tilemap.WorldToCell(playerPos + Vector3.down * 1.0f);
            
            tilemap.SetTile(cellPos, dirtTile);
            
            // Sync Collider and Display
            tilemap.RefreshAllTiles();
            tc.ProcessTilemapChanges();

            Debug.Log($"Verification tile placed at Cell: {cellPos} (World Pos mapped from player: {playerPos})");
        }
        else
        {
            Debug.LogError($"Missing references! Player: {player != null}, Tilemap: {tilemap != null}, Tile: {dirtTile != null}");
        }

        // 3. Cleanup & Save
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log("Build & Update System Completed (Phase 1 & 2 Fixes Applied).");
    }
}
#endif
