using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class Card : MonoBehaviour
{
    public Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;

    public bool isSelected;

    public CardsController cardCon;
    public void SetIconSprite(Sprite sprite)
    {
        iconSprite = sprite;
    }

    public void Show()
    {
        //iconImage.sprite = null;
        transform.DORotate(new Vector3(0f, 90f, 0f), 0.1f, RotateMode.Fast).OnComplete(() =>
        {
            iconImage.sprite = iconSprite;
            transform.DORotate(new Vector3(0f, 180f, 0f), 0.1f, RotateMode.Fast).OnComplete(() =>
            {
                isSelected = true;
            });
        });
        //iconImage.sprite = iconSprite;
        //DOVirtual.DelayedCall(0.1f, () => iconImage.sprite = iconSprite);
    }

    public void Hide()
    {
        transform.DORotate(new Vector3(0f, 90f, 0f), 0.1f, RotateMode.Fast).OnComplete(() =>
        {
            iconImage.sprite = hiddenIconSprite;
            transform.DORotate(new Vector3(0f, 0f, 0f), 0.1f, RotateMode.Fast).OnComplete(() =>
            {
                isSelected = false;
            });
        });

    }

    public void OnClick()
    {
        cardCon.SetSelected(this);
    }

    // Start is called before the first frame update

}
