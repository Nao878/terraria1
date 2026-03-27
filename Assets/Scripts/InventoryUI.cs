using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform gridParent;
    public GameObject slotPrefab;
    public PlayerController player;
    
    private List<GameObject> slots = new List<GameObject>();
    private int lastSelectedIndex = -1;

    void Start()
    {
        // Setup initial 9 slots
        if (gridParent != null && slotPrefab != null)
        {
            foreach (Transform child in gridParent) Destroy(child.gameObject);
            
            for (int i = 0; i < 9; i++)
            {
                GameObject slot = Instantiate(slotPrefab, gridParent);
                slots.Add(slot);
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // Update Slot Contents
        for (int i = 0; i < 9; i++)
        {
            if (i >= slots.Count) break;
            
            TextMeshProUGUI txt = slots[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                if (i < player.collectedKanji.Count)
                {
                    txt.text = player.collectedKanji[i];
                    // Ensure Wood is brownish if needed, though color can be set in SetupHelper
                }
                else
                {
                    txt.text = "";
                }
            }

            // Highlighting
            Image img = slots[i].GetComponent<Image>();
            if (img != null)
            {
                if (i == player.selectedIndex)
                {
                    img.color = new Color(1, 1, 0, 0.5f); // Yellow highlight
                }
                else
                {
                    img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Default dark
                }
            }
        }
    }

    public void ToggleInventory()
    {
        // Toggle is no longer needed if it's a persistent hotbar, 
        // but keeping the method for compatibility with existing UI buttons.
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}
