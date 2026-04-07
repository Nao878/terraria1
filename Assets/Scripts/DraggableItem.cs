using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform parentAfterDrag;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI text;
    public int originalIndex;
    private PlayerController player;
    private Vector3 originalLocalPos;

    public void Setup(PlayerController pc, int index)
    {
        player = pc;
        originalIndex = index;
        originalLocalPos = transform.localPosition;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        text = GetComponent<TextMeshProUGUI>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(text.text))
        {
            eventData.pointerDrag = null; // Do not drag empty slots
            return;
        }

        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root); // Move to top level for dragging
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        transform.localPosition = originalLocalPos;
        canvasGroup.blocksRaycasts = true;

        // If not dropped on a slot (or slot with IDropHandler), discard it
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<InventorySlot>() == null)
        {
            DiscardItemInternal();
        }
    }

    private void DiscardItemInternal()
    {
        if (player == null) return;

        string kanji = player.collectedKanji[originalIndex];
        if (!string.IsNullOrEmpty(kanji))
        {
            // Update data model
            player.collectedKanji[originalIndex] = "";
            Debug.Log($"『{kanji}』を捨てました");

            // Spawn DropItemEntity from Resources
            GameObject prefab = Resources.Load<GameObject>("DropItemEntity");
            if (prefab != null)
            {
                // Spawn offset: In front of player
                float direction = player.transform.localScale.x > 0 ? 1f : -1f;
                // If the player doesn't have localScale flip, we can use a movement variable or just a fixed offset
                Vector3 spawnPos = player.transform.position + Vector3.up * 1.0f + Vector3.right * direction * 1.5f;
                
                GameObject dropped = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
                DropItem item = dropped.GetComponent<DropItem>();
                if (item != null)
                {
                    item.kanji = kanji;
                }
            }
            else
            {
                Debug.LogError("DropItemEntity prefab not found in Resources folder!");
            }
        }
    }
}
