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

        // 1. Setup Ground Tilemap
        GameObject groundObj = GameObject.Find("Ground");
        if (groundObj != null)
        {
            Tilemap tilemap = groundObj.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                if (groundObj.GetComponent<TilemapCollider2D>() == null)
                {
                    groundObj.AddComponent<TilemapCollider2D>();
                }
            }
        }

        // 2. Setup Player
        GameObject player = GameObject.Find("Player");
        if (player == null) player = new GameObject("Player");

        if (player.GetComponent<SpriteRenderer>() == null) player.AddComponent<SpriteRenderer>();
        if (player.GetComponent<Rigidbody2D>() == null) player.AddComponent<Rigidbody2D>();
        if (player.GetComponent<BoxCollider2D>() == null) player.AddComponent<BoxCollider2D>();
        
        if (player.GetComponent("PlayerController") == null) player.AddComponent(System.Type.GetType("PlayerController"));
        if (player.GetComponent("BlockInteraction") == null) player.AddComponent(System.Type.GetType("BlockInteraction"));

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/player_sprite.png");
        player.transform.localScale = new Vector3(1, 2, 1);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D bc = player.GetComponent<BoxCollider2D>();
        bc.size = new Vector2(0.9f, 0.9f); // Slightly smaller to avoid friction

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.moveSpeed = 8f;
            pc.jumpForce = 15f;
            if (player.transform.childCount == 0)
            {
                GameObject gc = new GameObject("GroundCheck");
                gc.transform.parent = player.transform;
                gc.transform.localPosition = new Vector3(0, -0.6f, 0);
            }
            pc.groundCheck = player.transform.Find("GroundCheck");
            pc.groundLayer = LayerMask.GetMask("Ground");
        }

        BlockInteraction bi = player.GetComponent<BlockInteraction>();
        if (bi != null)
        {
            bi.groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
            bi.groundTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/DirtTile.asset");
        }

        // 3. Setup Tiles
        Tile dirtTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/DirtTile.asset");
        if (dirtTile != null)
        {
            dirtTile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/dirt_sprite.png");
            EditorUtility.SetDirty(dirtTile);
        }

        // 4. World Generator
        GameObject wgObj = GameObject.Find("WorldGeneratorLogic");
        if (wgObj != null)
        {
            WorldGenerator wg = wgObj.GetComponent<WorldGenerator>();
            if (wg != null)
            {
                wg.groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
                wg.backgroundTilemap = GameObject.Find("Background").GetComponent<Tilemap>();
                wg.groundTile = dirtTile;
                wg.GenerateWorld();
                
                // Reposition player
                player.transform.position = wg.GetSpawnPosition();
            }
        }

        // 5. Camera Fix
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.transform.position = new Vector3(50, 30, -10);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Scene consistency check completed (Fix for Spawning & Collision)!");
    }
}