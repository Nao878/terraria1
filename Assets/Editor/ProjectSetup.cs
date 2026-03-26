#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ProjectSetup
{
    [MenuItem("Setup/Execute System Setup")]
    public static void ExecuteSystemSetup()
    {
        Debug.Log("=== Execute System Setup: Starting... ===");

        // ========================================
        // Phase A: Run SetupHelper (core scene setup)
        // ========================================
        SetupHelper.RunSetup();

        // ========================================
        // Phase B: Grid & Tilemap Infrastructure
        // ========================================
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj == null) gridObj = new GameObject("Grid");

        gridObj.transform.position = Vector3.zero;
        gridObj.transform.rotation = Quaternion.identity;
        gridObj.transform.localScale = Vector3.one;

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

        groundObj.transform.localPosition = Vector3.zero;
        groundObj.transform.localRotation = Quaternion.identity;
        groundObj.transform.localScale = Vector3.one;

        // === CRITICAL FIX: Set GroundTilemap layer to "Ground" (Layer 8) ===
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex >= 0)
        {
            groundObj.layer = groundLayerIndex;
            Debug.Log($"GroundTilemap layer set to: {groundLayerIndex} (\"Ground\")");
        }
        else
        {
            Debug.LogError("Layer \"Ground\" not found in TagManager! Please add it.");
        }

        // Ensure Tilemap components
        Tilemap tilemap = groundObj.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = groundObj.AddComponent<Tilemap>();

        TilemapRenderer tr = groundObj.GetComponent<TilemapRenderer>();
        if (tr == null) tr = groundObj.AddComponent<TilemapRenderer>();
        tr.enabled = true;

        TilemapCollider2D tc = groundObj.GetComponent<TilemapCollider2D>();
        if (tc == null) tc = groundObj.AddComponent<TilemapCollider2D>();
        tc.enabled = true;

        // ========================================
        // Phase C: Physics Material Setup
        // ========================================
        PhysicsMaterial2D zeroFriction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/ZeroFrictionMaterial.physicsMaterial2D");
        if (zeroFriction == null)
        {
            zeroFriction = new PhysicsMaterial2D("ZeroFrictionMaterial");
            AssetDatabase.CreateAsset(zeroFriction, "Assets/ZeroFrictionMaterial.physicsMaterial2D");
        }
        zeroFriction.friction = 0f;
        zeroFriction.bounciness = 0f;
        EditorUtility.SetDirty(zeroFriction);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
            if (playerCollider != null)
            {
                playerCollider.sharedMaterial = zeroFriction;
                Debug.Log("ZeroFrictionMaterial assigned to Player BoxCollider2D.");
            }
        }

        // ========================================
        // Phase D: Force Camera 2D (merged from ForceCamera2D.cs)
        // ========================================
        Debug.Log("--- Forcing Camera to 2D ---");

        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.orthographic = true;
            mainCam.transform.rotation = Quaternion.identity;
            mainCam.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, -10f);
            Debug.Log("Main Camera fixed to orthographic 2D.");
        }

        var vcamObj = GameObject.Find("PlayerVCam");
        if (vcamObj != null)
        {
            vcamObj.transform.rotation = Quaternion.identity;
            vcamObj.transform.position = new Vector3(vcamObj.transform.position.x, vcamObj.transform.position.y, -10f);

            // Try to set via Reflection for Cinemachine 3
            var camComponent = vcamObj.GetComponent("Unity.Cinemachine.CinemachineCamera") as MonoBehaviour;
            if (camComponent != null)
            {
                var type = camComponent.GetType();
                var lensProp = type.GetProperty("Lens");
                if (lensProp != null)
                {
                    var lens = lensProp.GetValue(camComponent);
                    var lensType = lens.GetType();
                    var orthoField = lensType.GetField("Orthographic");
                    if (orthoField != null)
                    {
                        orthoField.SetValue(lens, true);
                        lensProp.SetValue(camComponent, lens);
                        Debug.Log("Cinemachine Lens Orthographic set to true.");
                    }
                }
            }

            var composer = vcamObj.GetComponent("Unity.Cinemachine.CinemachinePositionComposer") as MonoBehaviour;
            if (composer != null)
            {
                var composerType = composer.GetType();
                var distSetting = composerType.GetField("CameraDistance");
                if (distSetting != null)
                {
                    distSetting.SetValue(composer, 10f);
                    Debug.Log("PositionComposer CameraDistance set to 10.");
                }
            }

            Debug.Log("PlayerVCam fixed.");
        }

        // ========================================
        // Phase E: Verification Logs
        // ========================================
        Debug.Log("--- Verification ---");

        // Verify GroundTilemap layer
        if (groundObj != null)
        {
            string layerName = LayerMask.LayerToName(groundObj.layer);
            Debug.Log($"[CHECK] GroundTilemap.layer = {groundObj.layer} (\"{layerName}\")");

            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    bool layerMatch = (pc.groundLayer & (1 << groundObj.layer)) != 0;
                    Debug.Log($"[CHECK] PlayerController.groundLayer includes GroundTilemap layer: {layerMatch}");
                    if (!layerMatch)
                    {
                        Debug.LogError("[FAIL] Layer mismatch! Player's groundLayer does not include GroundTilemap's layer.");
                    }
                }
            }
        }

        // Verify WorldGenerator and BlockInteraction share the same Tilemap
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        BlockInteraction bi = player != null ? player.GetComponent<BlockInteraction>() : null;
        if (wg != null && bi != null)
        {
            bool sameRef = (wg.groundTilemap == bi.groundTilemap);
            Debug.Log($"[CHECK] WorldGenerator.groundTilemap == BlockInteraction.groundTilemap: {sameRef}");
            if (!sameRef)
            {
                Debug.LogError("[FAIL] Tilemap reference mismatch between WorldGenerator and BlockInteraction!");
            }
        }

        // ========================================
        // Phase F: Save
        // ========================================
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("=== Execute System Setup: Completed! ===");
    }
}
#endif
