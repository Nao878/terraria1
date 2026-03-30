#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class ProjectSetup
{
    [MenuItem("Tools/Execute System Setup")]
    public static void ExecuteSystemSetup()
    {
        Debug.Log("=== Execute System Setup: Starting... ===");

        // 1. Run UI & Scene Setup
        SetupHelper.RunSetup();

        // 2. Run Tile & Texture Optimizations (Consolidated)
        TilePerformanceFixer.RunOptimizations();

        // 3. Grid & Layers
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj != null)
        {
            gridObj.GetComponent<Grid>().cellSize = Vector3.one;
        }

        // 4. World Generator & Player Sync
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        GameObject player = GameObject.Find("Player");
        
        if (wg != null && player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            BlockInteraction bi = player.GetComponent<BlockInteraction>();
            
            // Sync tiles
            wg.groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/DirtTile.asset");
            wg.wallTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/WallTile.asset");
            wg.kiTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/KiTile.asset");
            wg.kinTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Tiles/KinTile.asset");

            if (bi != null)
            {
                bi.groundTile = wg.groundTile;
                bi.kiTile = wg.kiTile;
                bi.kinTile = wg.kinTile;
                EditorUtility.SetDirty(bi);
            }

            EditorUtility.SetDirty(wg);
        }

        // 5. Finalize Scene
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log("=== Execute System Setup: Completed Successfully! ===");
    }
}
#endif
