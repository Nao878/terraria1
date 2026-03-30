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
        Debug.Log("SetupHelper: Starting full scene setup...");

        // 1. Fix Sprite PPU and Settings
        string[] sprites = { "Assets/dirt_sprite.png", "Assets/wall_sprite.png", "Assets/player_sprite.png" };
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

        // 2. Setup Grid & Tilemaps
        GameObject gridObj = GameObject.Find("Grid") ?? new GameObject("Grid");
        if (gridObj.GetComponent<Grid>() == null) gridObj.AddComponent<Grid>();

        // Ground Tilemap
        GameObject groundObj = GameObject.Find("GroundTilemap");
        if (groundObj == null)
        {
            groundObj = new GameObject("GroundTilemap");
            groundObj.transform.parent = gridObj.transform;
        }
        groundObj.layer = LayerMask.NameToLayer("Ground");

        Rigidbody2D groundRb = groundObj.GetComponent<Rigidbody2D>() ?? groundObj.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D composite = groundObj.GetComponent<CompositeCollider2D>() ?? groundObj.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Outlines;

        Tilemap tilemap = groundObj.GetComponent<Tilemap>() ?? groundObj.AddComponent<Tilemap>();
        if (groundObj.GetComponent<TilemapRenderer>() == null) groundObj.AddComponent<TilemapRenderer>();
        
        TilemapCollider2D tmCollider = groundObj.GetComponent<TilemapCollider2D>() ?? groundObj.AddComponent<TilemapCollider2D>();
        tmCollider.usedByComposite = true;

        // Background Tilemap
        GameObject bgObj = GameObject.Find("Background") ?? new GameObject("Background");
        bgObj.transform.parent = gridObj.transform;
        bgObj.transform.localPosition = new Vector3(0, 0, 1);
        if (bgObj.GetComponent<Tilemap>() == null) bgObj.AddComponent<Tilemap>();
        if (bgObj.GetComponent<TilemapRenderer>() == null) bgObj.AddComponent<TilemapRenderer>();

        // 3. Setup Player
        GameObject player = GameObject.Find("Player") ?? new GameObject("Player");
        if (player.GetComponent<SpriteRenderer>() == null)
        {
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/player_sprite.png");
        }
        if (player.GetComponent<Rigidbody2D>() == null) player.AddComponent<Rigidbody2D>();
        
        CapsuleCollider2D cap = player.GetComponent<CapsuleCollider2D>() ?? player.AddComponent<CapsuleCollider2D>();
        cap.size = new Vector2(0.85f, 1.8f);
        cap.direction = CapsuleDirection2D.Vertical;

        PlayerController pc = player.GetComponent<PlayerController>() ?? player.AddComponent<PlayerController>();
        if (player.transform.Find("GroundCheck") == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = player.transform;
            gc.transform.localPosition = new Vector3(0, -1.0f, 0);
        }
        pc.groundCheck = player.transform.Find("GroundCheck");
        pc.groundLayer = LayerMask.GetMask("Ground");

        BlockInteraction bi = player.GetComponent<BlockInteraction>() ?? player.AddComponent<BlockInteraction>();
        bi.groundTilemap = tilemap;

        // 4. UI Setup (Consolidated)
        GameObject canvasObj = GameObject.Find("Canvas") ?? new GameObject("Canvas");
        if (canvasObj.GetComponent<Canvas>() == null)
        {
            canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject eventSystem = GameObject.Find("EventSystem") ?? new GameObject("EventSystem");
        if (eventSystem.GetComponent<EventSystem>() == null)
        {
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // UI Cleanup & Rebuild
        string[] toDelete = { "InventoryPanel", "HotbarPanel", "BagPanel", "InventoryToggleButton" };
        foreach (string name in toDelete)
        {
            GameObject obj;
            while ((obj = GameObject.Find(name)) != null) Object.DestroyImmediate(obj);
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NotoSansJP-Bold SDF.asset");
        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/InventorySlot.prefab");

        InventoryUI invUI = canvasObj.GetComponent<InventoryUI>() ?? canvasObj.AddComponent<InventoryUI>();
        invUI.player = pc;
        invUI.slotPrefab = slotPrefab;

        // Hotbar
        GameObject hotbarPanel = new GameObject("HotbarPanel");
        hotbarPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform hotRt = hotbarPanel.AddComponent<RectTransform>();
        hotRt.anchorMin = hotRt.anchorMax = hotRt.pivot = new Vector2(0.5f, 0f);
        hotRt.sizeDelta = new Vector2(820, 90);
        hotRt.anchoredPosition = new Vector2(0, 20);
        hotbarPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        
        HorizontalLayoutGroup hg = hotbarPanel.AddComponent<HorizontalLayoutGroup>();
        hg.childAlignment = TextAnchor.MiddleCenter; hg.spacing = 10;
        hg.childControlWidth = hg.childControlHeight = false;
        
        invUI.hotbarGrid = hotbarPanel.transform;
        if (slotPrefab != null) {
            for (int i = 0; i < 9; i++) PrefabUtility.InstantiatePrefab(slotPrefab, hotbarPanel.transform);
        }

        // Bag
        GameObject bagPanel = new GameObject("BagPanel");
        bagPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bagRt = bagPanel.AddComponent<RectTransform>();
        bagRt.anchorMin = bagRt.anchorMax = bagRt.pivot = new Vector2(0.5f, 0f);
        bagRt.sizeDelta = new Vector2(820, 280);
        bagRt.anchoredPosition = new Vector2(0, 120);
        bagPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);

        GameObject bagGridObj = new GameObject("BagGrid");
        bagGridObj.transform.SetParent(bagPanel.transform, false);
        RectTransform gridRt = bagGridObj.AddComponent<RectTransform>();
        gridRt.anchorMin = Vector2.zero; gridRt.anchorMax = Vector2.one; gridRt.sizeDelta = new Vector2(-20, -20);

        GridLayoutGroup glg = bagGridObj.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(80, 80); glg.spacing = new Vector2(10, 10);
        glg.childAlignment = TextAnchor.UpperCenter;

        invUI.bagGrid = bagGridObj.transform;
        invUI.bagPanel = bagPanel;
        bagPanel.SetActive(false);

        if (slotPrefab != null) {
            for (int i = 0; i < 27; i++) PrefabUtility.InstantiatePrefab(slotPrefab, bagGridObj.transform);
        }

        // Toggle Button
        GameObject toggleBtnObj = new GameObject("InventoryToggleButton");
        toggleBtnObj.transform.SetParent(canvasObj.transform, false);
        Button btn = toggleBtnObj.AddComponent<Button>();
        toggleBtnObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        RectTransform btnRt = toggleBtnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = btnRt.anchorMax = btnRt.pivot = new Vector2(1, 1);
        btnRt.anchoredPosition = new Vector2(-20, -20);
        btnRt.sizeDelta = new Vector2(140, 45);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(toggleBtnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "カバン開閉 (E)";
        btnText.color = Color.white; btnText.alignment = TextAlignmentOptions.Center; btnText.fontSize = 18;
        if (fontAsset != null) btnText.font = fontAsset;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, invUI.ToggleInventory);

        // 5. World Generation Trigger
        WorldGenerator wg = Object.FindFirstObjectByType<WorldGenerator>();
        if (wg != null && !wg.isGenerated) {
            wg.GenerateWorld();
            player.transform.position = wg.GetSpawnPosition();
        }

        EditorUtility.SetDirty(invUI);
        AssetDatabase.SaveAssets();
        Debug.Log("SetupHelper: RunSetup (Full Scene + UI) Completed.");
    }
}
