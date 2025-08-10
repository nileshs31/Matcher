using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardsController : MonoBehaviour
{

    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;

    Card firstSelected, secSelected;

    readonly List<Card> _openCards = new List<Card>();                
    readonly Queue<(Card a, Card b)> _compareQueue = new Queue<(Card, Card)>();
    bool _isProcessingQueue;
    private void Start()
    {
        SetupSprite();
        CreateCards();
    }

    private void SetupSprite()  // no of sprites decided here!
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
            card.cardCon = this;
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

    public void SetSelected(Card card)
    {
        if (!card.isSelected)
        {
            card.Show();

            _openCards.Add(card);

            while (_openCards.Count >= 2)
            {
                var a = _openCards[0];
                var b = _openCards[1];
                _openCards.RemoveAt(0);
                _openCards.RemoveAt(0);
                _compareQueue.Enqueue((a, b));
            }

            if (!_isProcessingQueue)
                StartCoroutine(ProcessCompareQueue());
        }
    }
    IEnumerator ProcessCompareQueue()
    {
        _isProcessingQueue = true;

        while (_compareQueue.Count > 0)
        {
            var (a, b) = _compareQueue.Dequeue();

            yield return new WaitForSeconds(0.25f);

            if (a == null || b == null) continue;

            if (a.iconSprite == b.iconSprite)
            {
                a.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 8, 0.9f);
                b.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 8, 0.9f);

                var tempCol = Color.white;
                tempCol.a = 0.65f;
                a.iconImage.color = tempCol;
                b.iconImage.color = tempCol;
            }
            else
            {
                StartCoroutine(FlipBackAfterDelay(a, b, 0.35f));
            }

            yield return null;
        }

        _isProcessingQueue = false;
    }

    IEnumerator FlipBackAfterDelay(Card a, Card b, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (a != null && a.isSelected) a.Hide();
        if (b != null && b.isSelected) b.Hide();
    }

}
