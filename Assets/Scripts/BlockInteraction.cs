using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class BlockInteraction : MonoBehaviour
{
    public Tilemap groundTilemap;
    public TileBase groundTile;
    public TileBase kanjiWoodTile;
    public bool simulateBreakKanji = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        
        // Auto-assign references if they are missing
        if (groundTilemap == null)
        {
            GameObject groundObj = GameObject.Find("GroundTilemap");
            if (groundObj != null) groundTilemap = groundObj.GetComponent<Tilemap>();
        }
    }

    void Update()
    {
        if (simulateBreakKanji)
        {
            simulateBreakKanji = false;
            KanjiBlock[] blocks = Object.FindObjectsByType<KanjiBlock>(FindObjectsSortMode.None);
            if (blocks.Length > 0)
            {
                Destroy(blocks[0].gameObject);
                PlayerController pc = GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.collectedKanji.Add("木");
                    Debug.Log($"『木』のプレハブパーツを取得しました！現在所持数: {pc.collectedKanji.Count}");
                    InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                    if (invUI != null && invUI.inventoryPanel.activeSelf)
                    {
                        invUI.inventoryText.text = "所持品:\n" + string.Join("\n", pc.collectedKanji);
                    }
                }
                return;
            }
            Debug.Log("シミュレーション失敗：KanjiWoodプレハブが見つかりませんでした。");
        }

        if (mainCamera == null || groundTilemap == null || Mouse.current == null) return;

        // Step 2: Coordinate conversion with rigid Z-correction
        Vector2 mouseInput = Mouse.current.position.ReadValue();
        // Use a non-zero Z for ScreenToWorldPoint to avoid camera-plane issues (10 is standard for 2D)
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseInput.x, mouseInput.y, 10f));
        worldPos.z = 0f; // Force Z to 0 exactly as requested

        // Step 3: Reach distance check
        float distance = Vector2.Distance(transform.position, worldPos);

        if (distance <= 5.0f)
        {
            Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);

            // Left Click: Break
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Collider2D col = Physics2D.OverlapPoint(worldPos);
                if (col != null && col.GetComponent<KanjiBlock>() != null)
                {
                    Destroy(col.gameObject);
                    PlayerController pc = GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        pc.collectedKanji.Add("木");
                        Debug.Log($"『木』のプレハブパーツを取得しました！現在所持数: {pc.collectedKanji.Count}");
                        InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                        if (invUI != null && invUI.inventoryPanel.activeSelf)
                        {
                            invUI.inventoryText.text = "所持品:\n" + string.Join("\n", pc.collectedKanji);
                        }
                    }
                    return;
                }

                TileBase targetTile = groundTilemap.GetTile(cellPos);
                if (targetTile != null)
                {
                    groundTilemap.SetTile(cellPos, null);
                    Debug.Log($"Broken block at: {cellPos}");
                }
            }
            // Right Click: Place
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (groundTile != null && groundTilemap.GetTile(cellPos) == null)
                {
                    groundTilemap.SetTile(cellPos, groundTile);
                    Debug.Log($"Placed block at: {cellPos}");
                }
            }
        }
    }
}