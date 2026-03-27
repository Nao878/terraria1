using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class BlockInteraction : MonoBehaviour
{
    public Tilemap groundTilemap;
    public TileBase groundTile;
    public TileBase kiTile;
    public TileBase kinTile;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (groundTilemap == null)
        {
            GameObject groundObj = GameObject.Find("GroundTilemap");
            if (groundObj != null) groundTilemap = groundObj.GetComponent<Tilemap>();
        }
    }

    void Update()
    {
        if (mainCamera == null || groundTilemap == null || Mouse.current == null) return;

        Vector2 mouseInput = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseInput.x, mouseInput.y, 10f));
        worldPos.z = 0f;

        float distance = Vector2.Distance(transform.position, worldPos);

        if (distance <= 5.0f)
        {
            Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);

            // Left Click: Break
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TileBase targetTile = groundTilemap.GetTile(cellPos);
                if (targetTile != null)
                {
                    PlayerController pc = GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        string character = "";
                        if (targetTile == kiTile) character = "木";
                        else if (targetTile == kinTile) character = "金";
                        
                        if (!string.IsNullOrEmpty(character))
                        {
                            if (pc.collectedKanji.Count >= 9)
                            {
                                Debug.Log("インベントリがいっぱいです");
                                return;
                            }
                            pc.collectedKanji.Add(character);
                            Debug.Log($"『{character}』を取得しました！現在所持数: {pc.collectedKanji.Count}");
                            
                            // Reload UI if needed
                            InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                            if (invUI != null && invUI.inventoryPanel.activeSelf)
                            {
                                invUI.ToggleInventory(); // Refresh view
                                invUI.ToggleInventory();
                            }
                        }
                    }
                    
                    groundTilemap.SetTile(cellPos, null);
                    Debug.Log($"Broken block at: {cellPos}");
                }
            }
            // Right Click: Place
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                PlayerController pc = GetComponent<PlayerController>();
                if (pc != null && pc.collectedKanji.Count > 0 && groundTilemap.GetTile(cellPos) == null)
                {
                    // For now, place the last collected one just to test
                    // Phase 4 will use selectedIndex from Hotbar
                    string kanjiToPlace = pc.collectedKanji[pc.collectedKanji.Count - 1];
                    TileBase tileToPlace = null;
                    if (kanjiToPlace == "木") tileToPlace = kiTile;
                    else if (kanjiToPlace == "金") tileToPlace = kinTile;
                    
                    if (tileToPlace != null)
                    {
                        groundTilemap.SetTile(cellPos, tileToPlace);
                        pc.collectedKanji.RemoveAt(pc.collectedKanji.Count - 1);
                        Debug.Log($"Placed {kanjiToPlace} at: {cellPos}");
                        
                        InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
                        if (invUI != null && invUI.inventoryPanel.activeSelf)
                        {
                            invUI.ToggleInventory(); // Refresh view
                            invUI.ToggleInventory();
                        }
                    }
                    else if (groundTile != null) // Fallback to dirt
                    {
                        groundTilemap.SetTile(cellPos, groundTile);
                    }
                }
                else if (groundTile != null && groundTilemap.GetTile(cellPos) == null)
                {
                    groundTilemap.SetTile(cellPos, groundTile);
                }
            }
        }
    }
}
