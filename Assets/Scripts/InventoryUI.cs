using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// インベントリUI管理。
/// Hotbar（0-8）とBag（9-35）のスロットを管理し、
/// PlayerController.collectedKanji と毎フレーム同期する。
/// ドラッグ中のスロットは更新をスキップしてアイテム増殖を防止。
/// </summary>
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
    private List<DraggableItem> draggables = new List<DraggableItem>();

    void Start()
    {
        RefreshSlotList();
    }

    public void RefreshSlotList()
    {
        slots.Clear();
        draggables.Clear();

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
        // InventorySlot（IDropHandler）
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        if (slot != null) slot.Setup(player, index);

        // DraggableItem はスロット親オブジェクトにアタッチ
        DraggableItem drag = slotObj.GetComponent<DraggableItem>();
        if (drag == null)
        {
            // 旧設計: Text子要素にあった DraggableItem を削除
            DraggableItem oldDrag = slotObj.GetComponentInChildren<DraggableItem>();
            if (oldDrag != null && oldDrag.gameObject != slotObj)
            {
                // 古い DraggableItem コンポーネントを除去
                Destroy(oldDrag);
            }
            // スロット親に新たにアタッチ
            drag = slotObj.AddComponent<DraggableItem>();
        }

        // CanvasGroup が必要
        if (slotObj.GetComponent<CanvasGroup>() == null)
            slotObj.AddComponent<CanvasGroup>();

        drag.Setup(player, index);
        draggables.Add(drag);
    }

    void Update()
    {
        if (player == null) return;
        if (slots.Count == 0) RefreshSlotList();

        // Eキーでインベントリ開閉
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }

        // Sync all slots with player's collectedKanji
        for (int i = 0; i < 36; i++)
        {
            if (i >= slots.Count) break;

            // ドラッグ中のスロットはUI更新をスキップ（増殖バグ防止）
            if (i < draggables.Count && draggables[i] != null && draggables[i].isDragging)
                continue;

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
