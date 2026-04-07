using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public int slotIndex; // 0-35
    private PlayerController player;

    public void Setup(PlayerController pc, int index)
    {
        player = pc;
        slotIndex = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
            if (draggedItem != null && player != null)
            {
                // Swap data in the model
                string temp = player.collectedKanji[slotIndex];
                player.collectedKanji[slotIndex] = player.collectedKanji[draggedItem.originalIndex];
                player.collectedKanji[draggedItem.originalIndex] = temp;

                Debug.Log($"横長スロット{draggedItem.originalIndex + 1}からスロット{slotIndex + 1}へ入れ替えました");

                // Note: The UI will refresh via InventoryUI.Update
                // But as the user requested: "ドラッグしてきたアイテムを自分のスロットの子要素にし"
                // The parent setting is handled by DraggableItem.OnEndDrag back to parentAfterDrag
                // So we should update parentAfterDrag if we want the UI elements to actually swap positions
                draggedItem.parentAfterDrag = transform; // This slot becomes the new parent
            }
        }
    }
}
