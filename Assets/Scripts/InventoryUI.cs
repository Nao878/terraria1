using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject bagPanel; // The 3x9 grid panel (toggleable)
    
    [Header("Grids")]
    public Transform hotbarGrid; // 1x9 slots (persistent)
    public Transform bagGrid;    // 3x9 slots (inside bagPanel)
    
    public GameObject slotPrefab;
    public PlayerController player;
    
    private List<GameObject> slots = new List<GameObject>();

    void Start()
    {
        RefreshSlotList();
    }

    public void RefreshSlotList()
    {
        slots.Clear();
        // The list order MUST be: 0-8 (Hotbar), 9-35 (Bag)
        if (hotbarGrid != null)
        {
            int i = 0;
            foreach (Transform child in hotbarGrid)
            {
                slots.Add(child.gameObject);
                SetupSlot(child.gameObject, i++);
            }
        }
        if (bagGrid != null)
        {
            int i = 9;
            foreach (Transform child in bagGrid)
            {
                slots.Add(child.gameObject);
                SetupSlot(child.gameObject, i++);
            }
        }
    }

    private void SetupSlot(GameObject slotObj, int index)
    {
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        if (slot != null) slot.Setup(player, index);
        
        DraggableItem drag = slotObj.GetComponentInChildren<DraggableItem>();
        if (drag != null) drag.Setup(player, index);
    }

    void Update()
    {
        if (player == null) return;
        if (slots.Count == 0) RefreshSlotList();

        // Sync all slots with player's collectedKanji
        for (int i = 0; i < 36; i++)
        {
            if (i >= slots.Count) break;
            
            // Slot Content
            TextMeshProUGUI txt = slots[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                if (i < player.collectedKanji.Count)
                {
                    txt.text = player.collectedKanji[i];
                }
                else
                {
                    txt.text = "";
                }
            }

            // Highlighting (Only for hotbar 0-8)
            Image img = slots[i].GetComponent<Image>();
            if (img != null)
            {
                if (i < 9 && i == player.selectedIndex)
                {
                    img.color = new Color(1, 1, 0.4f, 0.7f); // Yellow highlight for selected hotbar slot
                }
                else
                {
                    img.color = new Color(0.15f, 0.15f, 0.15f, 0.8f); // Default dark
                }
            }
        }
    }

    public void ToggleInventory()
    {
        if (bagPanel != null)
        {
            bagPanel.SetActive(!bagPanel.activeSelf);
        }
    }
}
