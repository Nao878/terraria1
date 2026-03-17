using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class BlockInteraction : MonoBehaviour
{
    public Tilemap groundTilemap;
    public TileBase blockToPlace;

    void Update()
    {
        if (Mouse.current == null) return;

        bool leftClick = Mouse.current.leftButton.wasPressedThisFrame;
        bool rightClick = Mouse.current.rightButton.wasPressedThisFrame;

        if (leftClick) Interact(true);
        else if (rightClick) Interact(false);
    }

    void Interact(bool destroy)
    {
        if (groundTilemap == null || Camera.main == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        Vector3Int tilePos = groundTilemap.WorldToCell(new Vector3(worldPos.x, worldPos.y, 0));

        // Removing distance check for easier testing of the digging system
        if (destroy)
        {
            groundTilemap.SetTile(tilePos, null);
        }
        else if (blockToPlace != null && groundTilemap.GetTile(tilePos) == null)
        {
            groundTilemap.SetTile(tilePos, blockToPlace);
        }
    }
}