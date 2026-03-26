using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class SetupHelper : MonoBehaviour
{
    public static void RunSetup()
    {
        // 0. Fix Sprite PPU
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
            EditorUtility.SetDirty(wg);
            wg.GenerateWorld();

            // Reposition player
            player.transform.position = wg.GetSpawnPosition();
        }

        // 7. Camera Fix (Force strict 2D Orthographic)
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