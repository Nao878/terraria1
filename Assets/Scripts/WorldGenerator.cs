using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int width = 100;
    public int height = 50;
    public float scale = 20f;
    public float heightOffset = 10f;
    public float surfaceHeight = 25f;

    [Header("References")]
    public Tilemap groundTilemap;
    public Tilemap backgroundTilemap;
    public TileBase groundTile;
    public TileBase backgroundTile;

    public bool isGenerated { get; private set; }

    void Awake()
    {
        GenerateWorld();
    }

    [ContextMenu("Generate")]
    public void GenerateWorld()
    {
        if (groundTilemap == null || groundTile == null)
        {
            Debug.LogError("GroundTilemap or GroundTile is not assigned!");
            return;
        }

        groundTilemap.ClearAllTiles();
        if (backgroundTilemap != null) backgroundTilemap.ClearAllTiles();

        int tileCount = 0;
        for (int x = 0; x < width; x++)
        {
            int currentHeight = GetNoiseHeight(x);

            for (int y = 0; y < currentHeight; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                tileCount++;
            }
        }

        // 1. Force a 7-block wide flat platform at the center
        int centerX = width / 2;
        int platformY = GetNoiseHeight(centerX);
        for (int x = centerX - 3; x <= centerX + 3; x++)
        {
            if (x >= 0 && x < width)
            {
                // Fill up to platformY to ensure no holes
                for (int y = 0; y <= platformY; y++)
                {
                    if (groundTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
                    {
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                        tileCount++;
                    }
                }
                // Clear above platformY to ensure flatness
                for (int y = platformY + 1; y < height; y++)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
        
        // Ensure TilemapCollider2D is updated
        if (!groundTilemap.GetComponent<TilemapCollider2D>())
        {
            groundTilemap.gameObject.AddComponent<TilemapCollider2D>();
        }

        isGenerated = true;
        Debug.Log($"World generation finished. Total tiles set: {tileCount}. Tile used: {(groundTile != null ? groundTile.name : "NULL")}");
    }

    public int GetNoiseHeight(int x)
    {
        float noise = Mathf.PerlinNoise(x / scale, 0);
        return Mathf.FloorToInt(noise * heightOffset + surfaceHeight);
    }

    public Vector2 GetSpawnPosition()
    {
        int centerX = width / 2;
        int platformY = GetNoiseHeight(centerX);
        // Place player slightly above the platform center to avoid overlapping
        return new Vector2(centerX + 0.5f, platformY + 3.0f);
    }
}