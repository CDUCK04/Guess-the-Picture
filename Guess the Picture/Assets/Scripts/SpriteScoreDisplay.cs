using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpriteScoreDisplay : MonoBehaviour
{
    [SerializeField] private List<Image> digitImages;
    [SerializeField] private Sprite[] numberSprites; // index 0–9

    public void SetDisplay(string toDisplay)
    {
        // Pad with leading zeros to match total digits
        toDisplay = toDisplay.PadLeft(digitImages.Count, '0');

        for (int i = 0; i < digitImages.Count; i++)
        {
            int num = toDisplay[i] - '0';
            digitImages[i].sprite = numberSprites[num];
            digitImages[i].enabled = true; // always visible now
        }
    }
}