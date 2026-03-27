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
    public List<string> collectedKanji = new List<string>();

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Disable physics until world is ready
        rb.simulated = false;
    }

    void Start()
    {
        // Try to spawn at correct location if WorldGenerator exists
        WorldGenerator wg = FindFirstObjectByType<WorldGenerator>();
        if (wg != null)
        {
            transform.position = wg.GetSpawnPosition();
            rb.linearVelocity = Vector2.zero;
        }
        
        // Re-enable physics now that terrain is (likely) ready
        rb.simulated = true;
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
        // New Input System handling (Simple poll for now as script is basic)
        if (Keyboard.current != null)
        {
            moveInput = 0;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        isGrounded = groundCheck != null ? Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer) : Mathf.Abs(rb.linearVelocity.y) < 0.01f;
    }

    void FixedUpdate()
    {
        

        // Handle Hotbar Selection
        HandleHotbarInput();
rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}