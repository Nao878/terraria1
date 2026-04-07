using UnityEngine;
using TMPro;

public class DropItem : MonoBehaviour
{
    public string kanji;
    public TextMeshPro textMesh;

    private void Start()
    {
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null) textMesh.text = kanji;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                // Find empty slot to pickup
                int emptySlot = pc.collectedKanji.FindIndex(s => string.IsNullOrEmpty(s));
                if (emptySlot != -1)
                {
                    pc.collectedKanji[emptySlot] = kanji;
                    Debug.Log($"『{kanji}』を拾いました！");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("インベントリが一杯のため拾えません");
                }
            }
        }
    }
}
