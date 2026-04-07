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
                            int emptySlot = pc.collectedKanji.FindIndex(s => string.IsNullOrEmpty(s));
                            if (emptySlot == -1)
                            {
                                Debug.Log("インベントリがいっぱいです");
                                return;
                            }
                            pc.collectedKanji[emptySlot] = character;
                            Debug.Log($"『{character}』を取得しました！スロット: {emptySlot + 1}");
                        }
                    }
                    
                    groundTilemap.SetTile(cellPos, null);
                    Debug.Log($"Broken block at: {cellPos}");
                }
            }
            // Right Click: Place
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (groundTilemap.GetTile(cellPos) == null)
                {
                    // Check if player is in the way
                    Vector3 center = groundTilemap.GetCellCenterWorld(cellPos);
                    Collider2D hit = Physics2D.OverlapBox(center, new Vector2(0.9f, 0.9f), 0f);
                    if (hit != null && hit.CompareTag("Player"))
                    {
                        Debug.Log("プレイヤーが重なっているため設置できません");
                        return;
                    }

                    PlayerController pc = GetComponent<PlayerController>();
                    if (pc != null && pc.selectedIndex < pc.collectedKanji.Count && !string.IsNullOrEmpty(pc.collectedKanji[pc.selectedIndex]))
                    {
                        string kanjiToPlace = pc.collectedKanji[pc.selectedIndex];
                        TileBase tileToPlace = null;
                        if (kanjiToPlace == "木") tileToPlace = kiTile;
                        else if (kanjiToPlace == "金") tileToPlace = kinTile;
                        
                        if (tileToPlace != null)
                        {
                            groundTilemap.SetTile(cellPos, tileToPlace);
                            pc.collectedKanji[pc.selectedIndex] = ""; // Clear the slot
                            Debug.Log($"Placed {kanjiToPlace} from slot {pc.selectedIndex + 1} at: {cellPos}");
                        }
                        else if (groundTile != null) // Fallback to dirt
                        {
                            groundTilemap.SetTile(cellPos, groundTile);
                        }
                    }
                    else if (groundTile != null)
                    {
                        groundTilemap.SetTile(cellPos, groundTile);
                    }
                }
            }
        }
    }
}
