using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// カード型ドラッグ＆ドロップ。スロット（親）にアタッチ。
/// ドラッグ中はゴースト（半透明カード）を生成し、元のUI要素は動かさない。
/// これにより InventoryUI.Update の毎フレーム同期との競合を防ぎ、アイテム増殖バグを解消。
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int slotIndex;
    [HideInInspector] public PlayerController player;

    // ドラッグ中かどうか（InventoryUI から参照）
    [System.NonSerialized] public bool isDragging = false;

    private Canvas rootCanvas;
    private GameObject ghostObj;
    private CanvasGroup canvasGroup;
    private bool validDrag = false;

    public void Setup(PlayerController pc, int index)
    {
        player = pc;
        slotIndex = index;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // ─── Begin Drag ───────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        validDrag = false;

        if (player == null) { eventData.pointerDrag = null; return; }
        if (slotIndex >= player.collectedKanji.Count) { eventData.pointerDrag = null; return; }

        string kanji = player.collectedKanji[slotIndex];
        if (string.IsNullOrEmpty(kanji))
        {
            eventData.pointerDrag = null; // 空スロットはドラッグ不可
            return;
        }

        validDrag = true;
        isDragging = true;

        // ルートCanvasを取得
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        // ゴーストカード生成
        CreateGhost(kanji);

        // 元スロットを半透明に
        canvasGroup.alpha = 0.4f;
    }

    // ─── Drag ─────────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        if (!validDrag || ghostObj == null) return;
        ghostObj.transform.position = eventData.position;
    }

    // ─── End Drag ─────────────────────────────────────────
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!validDrag) return;

        // ゴースト破棄
        if (ghostObj != null) Destroy(ghostObj);

        // 元スロットの透明度を戻す
        canvasGroup.alpha = 1f;
        isDragging = false;

        // ドロップ先がスロットなら InventorySlot.OnDrop で処理済み
        // ドロップ先がスロット外ならアイテムをワールドにドロップ
        bool droppedOnSlot = false;
        if (eventData.pointerEnter != null)
        {
            InventorySlot targetSlot = eventData.pointerEnter.GetComponentInParent<InventorySlot>();
            if (targetSlot != null) droppedOnSlot = true;
        }

        if (!droppedOnSlot)
        {
            DiscardItem();
        }
    }

    // ─── ゴースト生成 ─────────────────────────────────────
    private void CreateGhost(string kanji)
    {
        ghostObj = new GameObject("DragGhost");
        ghostObj.transform.SetParent(rootCanvas.transform, false);
        ghostObj.transform.SetAsLastSibling();

        RectTransform rt = ghostObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);

        // カード背景
        Image bg = ghostObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.6f, 0.9f, 0.85f); // 青みのあるカード色

        // Raycast を通さない（他のスロットの OnDrop を受けるため）
        bg.raycastTarget = false;

        // CanvasGroup
        CanvasGroup cg = ghostObj.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.9f;

        // テキスト
        GameObject textObj = new GameObject("GhostText");
        textObj.transform.SetParent(ghostObj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = kanji;
        txt.fontSize = 40;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.raycastTarget = false;

        // 元スロットのフォントを参照
        TextMeshProUGUI srcTxt = GetComponentInChildren<TextMeshProUGUI>();
        if (srcTxt != null && srcTxt.font != null) txt.font = srcTxt.font;

        // カーソル位置に移動
        ghostObj.transform.position = Input.mousePosition;
    }

    // ─── アイテム破棄（ワールドにドロップ） ───────────────
    private void DiscardItem()
    {
        if (player == null) return;
        if (slotIndex >= player.collectedKanji.Count) return;

        string kanji = player.collectedKanji[slotIndex];
        if (string.IsNullOrEmpty(kanji)) return;

        // データモデルをクリア
        player.collectedKanji[slotIndex] = "";
        Debug.Log($"『{kanji}』を捨てました");

        // DropItemEntity をスポーン
        GameObject prefab = Resources.Load<GameObject>("DropItemEntity");
        if (prefab != null)
        {
            float direction = player.transform.localScale.x > 0 ? 1f : -1f;
            Vector3 spawnPos = player.transform.position + Vector3.up * 1.0f + Vector3.right * direction * 1.5f;

            GameObject dropped = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            DropItem item = dropped.GetComponent<DropItem>();
            if (item != null) item.kanji = kanji;
        }
        else
        {
            Debug.LogError("DropItemEntity prefab not found in Resources folder!");
        }
    }
}
