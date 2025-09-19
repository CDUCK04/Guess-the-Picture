using UnityEngine;
using UnityEngine.UI;

public class AnswerSlot : MonoBehaviour
{
    public Button PlacedButton { get; private set; }
    public bool IsEmpty => PlacedButton == null;

    public void Init()
    {
        PlacedButton = null;
        if (!TryGetComponent<Image>(out _))
            gameObject.AddComponent<Image>();
    }

    public void SetPlaced(Button btn)
    {
        PlacedButton = btn;
    }

    public void ClearPlaced()
    {
        PlacedButton = null;
    }
}