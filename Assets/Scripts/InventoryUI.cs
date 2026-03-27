using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform gridParent;
    public GameObject slotPrefab;
    public PlayerController player;

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);

            if (isActive && gridParent != null && slotPrefab != null && player != null)
            {
                foreach (Transform child in gridParent)
                {
                    Destroy(child.gameObject);
                }

                foreach (string kanji in player.collectedKanji)
                {
                    GameObject slot = Instantiate(slotPrefab, gridParent);
                    TextMeshProUGUI txt = slot.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        txt.text = kanji;
                    }
                }
            }
        }
    }
}
