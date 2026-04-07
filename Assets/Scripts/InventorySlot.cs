using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// インベントリスロット。IDropHandler でドロップを受け取り、
/// データモデル（PlayerController.collectedKanji）のみスワップする。
/// UI の反映は InventoryUI.Update に委譲。
/// </summary>
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
        if (eventData.pointerDrag == null) return;

        DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggedItem == null || player == null) return;

        int fromIndex = draggedItem.slotIndex;
        int toIndex = slotIndex;

        // 同じスロットへのドロップは何もしない
        if (fromIndex == toIndex) return;

        // データモデルのみスワップ
        if (fromIndex < player.collectedKanji.Count && toIndex < player.collectedKanji.Count)
        {
            string temp = player.collectedKanji[toIndex];
            player.collectedKanji[toIndex] = player.collectedKanji[fromIndex];
            player.collectedKanji[fromIndex] = temp;

            Debug.Log($"スロット{fromIndex + 1} ⇔ スロット{toIndex + 1} を入れ替えました");
        }
    }
}
