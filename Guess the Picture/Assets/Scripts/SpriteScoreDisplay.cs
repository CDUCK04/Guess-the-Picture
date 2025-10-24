using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpriteScoreDisplay : MonoBehaviour
{
    [SerializeField] private List<Image> digitImages;
    [SerializeField] private Sprite[] numberSprites; // index 0–9

    public void SetDisplay(string toDisplay)
    {
        for (int i = 0; i < digitImages.Count; i++)
        {
            if (i < toDisplay.Length)
            {
                int num = toDisplay[toDisplay.Length - 1 - i] - '0';
                digitImages[digitImages.Count - 1 - i].sprite = numberSprites[num];
                digitImages[digitImages.Count - 1 - i].enabled = true;
            }
            else
            {
                digitImages[digitImages.Count - 1 - i].enabled = false;
            }
        }
    }
}