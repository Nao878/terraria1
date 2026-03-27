using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetupHelper : MonoBehaviour
{
    public static void RunSetup()
    {
        // 0. Fix Sprite PPU
        string[] sprites = { "Assets/dirt_sprite.png", "Assets/wall_sprite.png", "Assets/player_sprite.png", "Assets/kanji_wood_sprite.png" };
        foreach (string path in sprites)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.spritePixelsPerUnit = 16; 
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // Setup wall_sprite.png
        Texture2D wallTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/wall_sprite.png");
        if (wallTex != null)
        {
            string path = AssetDatabase.GetAssetPath(wallTex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && (importer.spritePixelsPerUnit != 16 || importer.filterMode != FilterMode.Point))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16; 
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // 1. Setup Grid
        GameObject gridObj = GameObject.Find("Grid");
        if (gridObj == null) gridObj = new GameObject("Grid");
        if (gridObj.GetComponent<Grid>() == null) gridObj.AddComponent<Grid>();

        // 1b. Cleanup redundant objects (Grid/Ground)
        Transform legacyGround = gridObj.transform.Find("Ground");
        if (legacyGround != null)
        {
            Debug.Log("Found legacy 'Ground' object. Deleting to prevent duplication...");
            Object.DestroyImmediate(legacyGround.gameObject);
        }

        // 2. Setup Ground Tilemap (unified name: "GroundTilemap")
        GameObject groundObj = GameObject.Find("GroundTilemap");
        if (groundObj == null)
        {
            groundObj = new GameObject("GroundTilemap");
            groundObj.transform.parent = gridObj.transform;
        }

        // --- NEW: Physics Fix for Ghost Collisions ---
        Rigidbody2D groundRb = groundObj.GetComponent<Rigidbody2D>();
        if (groundRb == null) groundRb = groundObj.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D composite = groundObj.GetComponent<CompositeCollider2D>();
        if (composite == null) composite = groundObj.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Outlines;

        // Ensure GroundTilemap is on the "Ground" layer
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex >= 0)
        {
            groundObj.layer = groundLayerIndex;
        }

        Tilemap tilemap = groundObj.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = groundObj.AddComponent<Tilemap>();
        if (groundObj.GetComponent<TilemapRenderer>() == null) groundObj.AddComponent<TilemapRenderer>();
        
        TilemapCollider2D tmCollider = groundObj.GetComponent<TilemapCollider2D>();
        if (tmCollider == null) tmCollider = groundObj.AddComponent<TilemapCollider2D>();
        tmCollider.usedByComposite = true;
        
        // 2b. Setup Background Tilemap
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj == null)
        {
            bgObj = new GameObject("Background");
            bgObj.transform.parent = gridObj.transform;
            bgObj.transform.localPosition = new Vector3(0, 0, 1); // Set slightly backwards
        }
        Tilemap bgTilemap = bgObj.GetComponent<Tilemap>();
        if (bgTilemap == null) bgTilemap = bgObj.AddComponent<Tilemap>();
        if (bgObj.GetComponent<TilemapRenderer>() == null) bgObj.AddComponent<TilemapRenderer>();

        // 3. Setup Player
        GameObject player = GameObject.Find("Player");
        if (player == null) player = new GameObject("Player");

        if (player.GetComponent<SpriteRenderer>() == null) player.AddComponent<SpriteRenderer>();
        if (player.GetComponent<Rigidbody2D>() == null) player.AddComponent<Rigidbody2D>();
        // --- NEW: Use CapsuleCollider2D for Smooth Movement ---
        BoxCollider2D oldBc = player.GetComponent<BoxCollider2D>();
        if (oldBc != null) Object.DestroyImmediate(oldBc);

        CapsuleCollider2D cap = player.GetComponent<CapsuleCollider2D>();
        if (cap == null) cap = player.AddComponent<CapsuleCollider2D>();
        cap.size = new Vector2(0.85f, 1.8f);
        cap.direction = CapsuleDirection2D.Vertical;

        // Apply shared friction-less material
        PhysicsMaterial2D zeroFriction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/ZeroFriction.physicsMaterial2D");
        if (zeroFriction != null) cap.sharedMaterial = zeroFriction;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.moveSpeed = 8f;
            pc.jumpForce = 15f;
            if (player.transform.Find("GroundCheck") == null)
            {
                GameObject gc = new GameObject("GroundCheck");
                gc.transform.parent = player.transform;
                gc.transform.localPosition = new Vector3(0, -1.0f, 0); // Bottom of 2-unit tall player
            }
            pc.groundCheck = player.transform.Find("GroundCheck");
            pc.groundLayer = LayerMask.GetMask("Ground");
        }

        // 4. BlockInteraction - reference the SAME GroundTilemap
        BlockInteraction bi = player.GetComponent<BlockInteraction>();
        if (bi != null)
        {
            bi.groundTilemap = groundObj.GetComponent<Tilemap>();
            bi.groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/DirtTile.asset");
        }

        // 5. Setup Tiles (handled in ProjectSetup or basic assignment here)
        Tile dirtTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/DirtTile.asset");
        if (dirtTile != null)
        {
            dirtTile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/dirt_sprite.png");
            EditorUtility.SetDirty(dirtTile);
        }

        Tile wallTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/WallTile.asset");
        if (wallTile == null)
        {
            wallTile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(wallTile, "Assets/WallTile.asset");
        }
        wallTile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/wall_sprite.png");
        EditorUtility.SetDirty(wallTile);

        // 6. World Generator - reference the SAME GroundTilemap
        // Find existing or create WorldGenerator object
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        GameObject wgObj = wg != null ? wg.gameObject : null;

        if (wgObj == null)
        {
            // Try by legacy name first
            wgObj = GameObject.Find("WorldGeneratorLogic");
            if (wgObj == null)
            {
                wgObj = new GameObject("WorldGeneratorLogic");
            }
            wg = wgObj.GetComponent<WorldGenerator>();
            if (wg == null) wg = wgObj.AddComponent<WorldGenerator>();
        }

        if (wg != null)
        {
            wg.groundTilemap = tilemap;
            wg.backgroundTilemap = bgTilemap;
            wg.groundTile = dirtTile;
            wg.wallTile = wallTile;
            
            EditorUtility.SetDirty(wg);
            player.transform.position = wg.GetSpawnPosition();
        }

        // 9. Setup Inventory UI
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NotoSansJP-Bold SDF.asset");
        if (fontAsset == null) Debug.LogWarning("NotoSansJP font not found!");

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        GameObject toggleBtnObj = GameObject.Find("InventoryToggleButton");
        if (toggleBtnObj == null)
        {
            toggleBtnObj = new GameObject("InventoryToggleButton");
            toggleBtnObj.transform.SetParent(canvasObj.transform, false);
            Button btn = toggleBtnObj.AddComponent<Button>();
            Image img = toggleBtnObj.AddComponent<Image>();
            img.color = Color.white;
            RectTransform btnRt = toggleBtnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 1);
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.pivot = new Vector2(1, 1);
            btnRt.anchoredPosition = new Vector2(-20, -20);
            btnRt.sizeDelta = new Vector2(100, 50);

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(toggleBtnObj.transform, false);
            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "カバン";
            btnText.color = Color.black;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 24;
            if (fontAsset != null) btnText.font = fontAsset;
            RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.sizeDelta = Vector2.zero;
        }

        // Ensure InventoryPanel is an instance in the scene and assigned to InventoryUI
        InventoryUI invUI = canvasObj.GetComponent<InventoryUI>();
        if (invUI == null) invUI = canvasObj.AddComponent<InventoryUI>(); // Ensure InventoryUI component exists

        GameObject panelObj = null;
        // Search in scene first
        foreach (Transform child in canvasObj.transform)
        {
            if (child.name == "InventoryPanel")
            {
                panelObj = child.gameObject;
                break;
            }
        }

        if (panelObj == null)
        {
            panelObj = new GameObject("InventoryPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRt = panelObj.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0f);
            panelRt.anchorMax = new Vector2(0.5f, 0f);
            panelRt.pivot = new Vector2(0.5f, 0f);
            panelRt.sizeDelta = new Vector2(820, 90);
            panelRt.anchoredPosition = new Vector2(0, 20);
            
            panelObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            HorizontalLayoutGroup hGroup = panelObj.AddComponent<HorizontalLayoutGroup>();
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.spacing = 10;
            hGroup.childControlWidth = false;
            hGroup.childControlHeight = false;
            hGroup.childForceExpandWidth = false;
            hGroup.childForceExpandHeight = false;
        }

        // Cleanup legacy text
        Transform legacyText = panelObj.transform.Find("InventoryText");
        if (legacyText != null) Object.DestroyImmediate(legacyText.gameObject);

        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InventorySlot.prefab");
        if (slotPrefab == null)
        {
            GameObject tempSlot = new GameObject("InventorySlot");
            RectTransform slotRt = tempSlot.AddComponent<RectTransform>();
            slotRt.sizeDelta = new Vector2(80, 80);
            Image slotImg = tempSlot.AddComponent<Image>();
            slotImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(tempSlot.transform, false);
            TextMeshProUGUI slotText = textObj.AddComponent<TextMeshProUGUI>();
            slotText.text = "木";
            slotText.color = Color.white;
            slotText.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
            slotText.fontSize = 40;
            if (fontAsset != null) slotText.font = fontAsset;
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            textRt.pivot = new Vector2(0.5f, 0.5f);
            
            TextMeshProUGUI slotTxt = textObj.GetComponent<TextMeshProUGUI>();
            slotTxt.horizontalAlignment = HorizontalAlignmentOptions.Center;
            slotTxt.verticalAlignment = VerticalAlignmentOptions.Middle;
            slotTxt.color = Color.black;

            slotPrefab = PrefabUtility.SaveAsPrefabAsset(tempSlot, "Assets/InventorySlot.prefab");
            Object.DestroyImmediate(tempSlot);
        }

        invUI = canvasObj.GetComponent<InventoryUI>();
        if (invUI == null) invUI = canvasObj.AddComponent<InventoryUI>();
        invUI.inventoryPanel = panelObj;
        invUI.gridParent = panelObj.transform;
        invUI.slotPrefab = slotPrefab;
        invUI.player = player.GetComponent<PlayerController>();
        EditorUtility.SetDirty(invUI);

        Button toggleBtn = toggleBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(toggleBtn.onClick, invUI.ToggleInventory);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(toggleBtn.onClick, invUI.ToggleInventory);

        if (wg != null && !wg.isGenerated)
        {
            wg.GenerateWorld();
            if (player != null) player.transform.position = wg.GetSpawnPosition();
        }

        // 11. Camera Fix (Force strict 2D Orthographic)
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -10f);
            cam.transform.rotation = Quaternion.identity;
        }

        // 8. Cinemachine Fix (Force 2D)
        GameObject vcamObj = GameObject.Find("PlayerVCam");
        if (vcamObj != null)
        {
            vcamObj.transform.rotation = Quaternion.identity;

            var vcam = vcamObj.GetComponent("CinemachineVirtualCamera") as MonoBehaviour;
            if (vcam != null)
            {
                // Add CameraZoom support
                if (!cam.GetComponent<CameraZoom>())
                {
                    cam.gameObject.AddComponent<CameraZoom>();
                }

                Debug.Log("Camera setup complete.");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("SetupHelper: Scene consistency check completed!");
    }
}
