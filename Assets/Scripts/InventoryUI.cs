using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public TextMeshProUGUI inventoryText;
    public PlayerController player;

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);

            if (isActive && inventoryText != null && player != null)
            {
                if (player.collectedKanji.Count == 0)
                {
                    inventoryText.text = "所持品:\n(なし)";
                }
                else
                {
                    inventoryText.text = "所持品:\n" + string.Join("\n", player.collectedKanji);
                }
            }
        }
    }
}
