using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsController : MonoBehaviour
{

    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;


    private void Start()
    {
        SetupSprite();
        CreateCards();
    }

    private void SetupSprite()
    {
        spritePairs = new List<Sprite>();
        for(int i = 0; i < sprites.Length; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }

        ShuffleSprites(spritePairs);
    }

    void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
        }
    }

    void ShuffleSprites(List<Sprite> spritesList)
    {
        for (int i = spritesList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = spritesList[i];
            spritesList[i] = spritesList[randomIndex];
            spritesList[randomIndex] = temp;
        }
    }
}
