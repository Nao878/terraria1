using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class BlockInteraction : MonoBehaviour
{
    public Tilemap groundTilemap;
    public TileBase groundTile;
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
                groundTilemap.SetTile(cellPos, null);
                Debug.Log($"Broken block at: {cellPos}");
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