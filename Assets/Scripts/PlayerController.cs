using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public List<string> collectedKanji = new List<string>(); // Initialize as empty, Awake will handle sizing

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure list has exactly 36 elements
        if (collectedKanji == null) collectedKanji = new List<string>();
        
        // Remove extra elements if any
        if (collectedKanji.Count > 36)
        {
            collectedKanji.RemoveRange(36, collectedKanji.Count - 36);
        }
        
        // Add elements if fewer than 36
        while (collectedKanji.Count < 36)
        {
            collectedKanji.Add("");
        }

        // Initialize empty slots with empty strings if they are null
        for (int i = 0; i < 36; i++)
        {
            if (collectedKanji[i] == null) collectedKanji[i] = "";
        }

        // Disable physics until world is ready
        if (rb != null) rb.simulated = false;
    }

    void Start()
    {
        // Try to spawn at correct location if WorldGenerator exists
        WorldGenerator wg = FindFirstObjectByType<WorldGenerator>();
        if (wg != null)
        {
            transform.position = wg.GetSpawnPosition();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        
        // Re-enable physics now that terrain is (likely) ready
        if (rb != null) rb.simulated = true;
    }

    [Header("Hotbar Settings")]
    public int selectedIndex = 0;

    void HandleHotbarInput()
    {
        if (Keyboard.current != null)
        {
            for (int i = 0; i < 9; i++)
            {
                if (Keyboard.current[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                {
                    selectedIndex = i;
                    Debug.Log($"Selected Slot: {selectedIndex + 1}");
                }
            }
        }

        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0) selectedIndex = (selectedIndex - 1 + 9) % 9;
            else if (scroll < 0) selectedIndex = (selectedIndex + 1) % 9;
        }
    }


    void Update()
    {
        HandleHotbarInput();

        if (Keyboard.current != null)
        {
            moveInput = 0;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                if (rb != null) rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        isGrounded = groundCheck != null ? Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer) : (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.01f);
    }

    void FixedUpdate()
    {
        if (rb != null) rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}