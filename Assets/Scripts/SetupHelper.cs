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

        // Ensure GroundTilemap is on the "Ground" layer
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex >= 0)
        {
            groundObj.layer = groundLayerIndex;
        }

        Tilemap tilemap = groundObj.GetComponent<Tilemap>();
        if (tilemap == null) tilemap = groundObj.AddComponent<Tilemap>();
        if (groundObj.GetComponent<TilemapRenderer>() == null) groundObj.AddComponent<TilemapRenderer>();
        if (groundObj.GetComponent<TilemapCollider2D>() == null) groundObj.AddComponent<TilemapCollider2D>();
        
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
        if (player.GetComponent<BoxCollider2D>() == null) player.AddComponent<BoxCollider2D>();
        
        if (player.GetComponent(typeof(PlayerController)) == null) player.AddComponent(System.Type.GetType("PlayerController"));
        if (player.GetComponent(typeof(BlockInteraction)) == null) player.AddComponent(System.Type.GetType("BlockInteraction"));

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/player_sprite.png");
        // Sprite is 16x32px at PPU=16 = 1x2 units natively, no scale needed
        player.transform.localScale = new Vector3(1, 1, 1);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D bc = player.GetComponent<BoxCollider2D>();
        bc.size = new Vector2(0.9f, 1.8f); // 2-block tall hitbox

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

        // 5. Setup Tiles
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
            AssetDatabase.SaveAssets();
        }
        wallTile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/wall_sprite.png");
        EditorUtility.SetDirty(wallTile);

        Tile kanjiWoodTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/KanjiWoodTile.asset");
        if (kanjiWoodTile == null)
        {
            kanjiWoodTile = ScriptableObject.CreateInstance<Tile>();
            AssetDatabase.CreateAsset(kanjiWoodTile, "Assets/KanjiWoodTile.asset");
            AssetDatabase.SaveAssets();
        }
        kanjiWoodTile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/kanji_wood_sprite.png");
        EditorUtility.SetDirty(kanjiWoodTile);

        if (bi != null)
        {
            bi.kanjiWoodTile = kanjiWoodTile;
        }

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
            // === CRITICAL FIX: Set to the SAME GroundTilemap object ===
            wg.groundTilemap = tilemap;
            wg.backgroundTilemap = bgTilemap;
            wg.groundTile = dirtTile;
            wg.wallTile = wallTile;
            wg.kanjiWoodTile = kanjiWoodTile;
            EditorUtility.SetDirty(wg);
            wg.GenerateWorld();

            // Reposition player
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

        GameObject panelObj = GameObject.Find("InventoryPanel");
        if (panelObj == null)
        {
            panelObj = new GameObject("InventoryPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            panelObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        }

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(300, 300);

        GridLayoutGroup grid = panelObj.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = panelObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(80, 80);
        grid.spacing = new Vector2(10, 10);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        // Cleanup legacy text
        Transform legacyText = panelObj.transform.Find("InventoryText");
        if (legacyText != null) Object.DestroyImmediate(legacyText.gameObject);

        panelObj.SetActive(false);

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
            textRt.sizeDelta = Vector2.zero;

            slotPrefab = PrefabUtility.SaveAsPrefabAsset(tempSlot, "Assets/InventorySlot.prefab");
            Object.DestroyImmediate(tempSlot);
        }

        InventoryUI invUI = canvasObj.GetComponent<InventoryUI>();
        if (invUI == null) invUI = canvasObj.AddComponent<InventoryUI>();
        invUI.inventoryPanel = panelObj;
        invUI.gridParent = panelObj.transform;
        invUI.slotPrefab = slotPrefab;
        invUI.player = Object.FindFirstObjectByType<PlayerController>();

        Button toggleBtn = toggleBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.RemovePersistentListener(toggleBtn.onClick, invUI.ToggleInventory);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(toggleBtn.onClick, invUI.ToggleInventory);

        // 10. KanjiWood Prefab
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/KanjiWood.prefab");
        if (prefabAsset == null)
        {
            GameObject tempWood = new GameObject("KanjiWood");
            KanjiBlock kbWood = tempWood.AddComponent<KanjiBlock>();
            kbWood.kanjiCharacter = "木";
            BoxCollider2D kanjiBc = tempWood.AddComponent<BoxCollider2D>();
            kanjiBc.size = new Vector2(1, 1);
            
            GameObject textObj = new GameObject("KanjiText");
            textObj.transform.SetParent(tempWood.transform, false);
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = "木";
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
            textMesh.fontSize = 8;
            textMesh.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.localPosition = Vector3.zero;
            textMesh.margin = Vector4.zero;
            textMesh.rectTransform.sizeDelta = new Vector2(1, 1);
            textMesh.GetComponent<MeshRenderer>().sortingOrder = 5;
            if (fontAsset != null) textMesh.font = fontAsset;

            prefabAsset = PrefabUtility.SaveAsPrefabAsset(tempWood, "Assets/KanjiWood.prefab");
            Object.DestroyImmediate(tempWood);
        }

        // 11. KanjiGold Prefab
        GameObject goldPrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/KanjiGold.prefab");
        if (goldPrefabAsset == null)
        {
            GameObject tempGold = new GameObject("KanjiGold");
            KanjiBlock kbGold = tempGold.AddComponent<KanjiBlock>();
            kbGold.kanjiCharacter = "金";
            BoxCollider2D goldBc = tempGold.AddComponent<BoxCollider2D>();
            goldBc.size = new Vector2(1, 1);
            
            GameObject textObj = new GameObject("KanjiText");
            textObj.transform.SetParent(tempGold.transform, false);
            TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
            textMesh.text = "金";
            textMesh.color = new Color(1f, 0.84f, 0f); // #FFD700
            textMesh.alignment = TextAlignmentOptions.Center | TextAlignmentOptions.Midline;
            textMesh.fontSize = 8;
            textMesh.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            textMesh.rectTransform.localPosition = Vector3.zero;
            textMesh.margin = Vector4.zero;
            textMesh.rectTransform.sizeDelta = new Vector2(1, 1);
            textMesh.GetComponent<MeshRenderer>().sortingOrder = 5;
            if (fontAsset != null) textMesh.font = fontAsset;

            goldPrefabAsset = PrefabUtility.SaveAsPrefabAsset(tempGold, "Assets/KanjiGold.prefab");
            Object.DestroyImmediate(tempGold);
        }
        
        if (wg != null)
        {
            wg.kanjiWoodPrefab = prefabAsset;
            wg.kanjiGoldPrefab = goldPrefabAsset;
            EditorUtility.SetDirty(wg);
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
                Debug.Log("PlayerVCam transform rotation fixed.");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("SetupHelper: Scene consistency check completed!");
    }
}