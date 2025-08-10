using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{

    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;


    public void SetIconSprite(Sprite sprite)
    {
        iconSprite = sprite;
    }


    public void Show()
    {
        iconImage.sprite = iconSprite;
        isSelected = true;
    }

    public void Hide()
    {
        iconImage.sprite = hiddenIconSprite;
        isSelected = false;

    }

    // Start is called before the first frame update

}
