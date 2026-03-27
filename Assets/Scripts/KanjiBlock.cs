using UnityEngine;
using TMPro;

public class KanjiBlock : MonoBehaviour
{
    public string kanjiCharacter;

    void Start()
    {
        TextMeshPro tmp = GetComponentInChildren<TextMeshPro>();
        if (tmp != null)
        {
            tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.ForceMeshUpdate(true);
        }
    }
}
