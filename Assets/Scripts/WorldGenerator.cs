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
    public TileBase wallTile;
    public TileBase kanjiWoodTile;
    public GameObject kanjiWoodPrefab;

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
        int maxTerrainHeight = 0;

        // Clean up old KanjiBlocks
        KanjiBlock[] oldBlocks = Object.FindObjectsByType<KanjiBlock>(FindObjectsSortMode.None);
        foreach (var block in oldBlocks)
        {
            if (Application.isPlaying) Destroy(block.gameObject); else DestroyImmediate(block.gameObject);
        }

        for (int x = 0; x < width; x++)
        {
            int currentHeight = GetNoiseHeight(x);
            if (currentHeight > maxTerrainHeight) maxTerrainHeight = currentHeight;

            // 完全に groundTile で埋め尽くす（穴なし）
            for (int y = 0; y < currentHeight; y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                tileCount++;
            }
        }

        for (int x = 0; x < width; x++)
        {
            // Center platform should preferably not have trees so the player doesn't get blocked
            int tempCenterX = width / 2;
            if (Mathf.Abs(x - tempCenterX) <= 3) continue;

            if (kanjiWoodPrefab != null && UnityEngine.Random.value < 0.05f)
            {
                int tempPlatformY = GetNoiseHeight(x);
                int stack = UnityEngine.Random.Range(2, 4);
                for (int i = 0; i < stack; i++)
                {
                    Vector3 pos = new Vector3(x + 0.5f, tempPlatformY + i + 0.5f, 0);
                    Instantiate(kanjiWoodPrefab, pos, Quaternion.identity);
                }
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

        // 2. Generate Background (Mountains and Underground Walls)
        if (backgroundTilemap != null && wallTile != null)
        {
            for (int x = 0; x < width; x++)
            {
                // Different offset for mountain silhouette
                float mountainNoise = Mathf.PerlinNoise((x + 1000f) / (scale * 1.5f), 0);
                int mountainOffset = Mathf.FloorToInt(mountainNoise * 10f); // 0 to 10 blocks
                int bgTopY = maxTerrainHeight + mountainOffset;

                for (int y = 0; y <= bgTopY; y++)
                {
                    backgroundTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                }
            }
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