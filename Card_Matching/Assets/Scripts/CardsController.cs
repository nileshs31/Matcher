using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardsController : MonoBehaviour
{

    [SerializeField] Card cardPrefab;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;

    Card firstSelected, secSelected;

    readonly List<Card> _openCards = new List<Card>();                
    readonly Queue<(Card a, Card b)> _compareQueue = new Queue<(Card, Card)>();
    bool _isProcessingQueue;
    int matchedPairs = 0;
    int rows, col; 
    // PlayerPrefs keys
    public const string RowsKey = "Rows";
    public const string ColsKey = "Cols";

    void Awake()
    {
        ApplyGridSettings();
    }

    void ApplyGridSettings()
    {
        rows = PlayerPrefs.GetInt(RowsKey);
        col = PlayerPrefs.GetInt(ColsKey);

        if ((rows * col) % 2 != 0)
        {
            // trim one column (keeps UX predictable)
            col = Mathf.Max(1, col - 1);
        }

        if (grid == null) return;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = col;
        if (col > 4)
            grid.cellSize = new Vector2(150f, 160f);
        else
            grid.cellSize = new Vector2(200f, 210f);
    }

    private void Start()
    {
        SetupSprite();
        CreateCards();
    }

    private void SetupSprite()  // number of sprites decided here!
    {
        int totalCards = rows * col;
        int pairsNeeded = totalCards / 2;

        spritePairs = new List<Sprite>(totalCards);

        for (int i = 0; i < pairsNeeded; i++)
        {
            Sprite face = sprites[i % sprites.Length];
            spritePairs.Add(face);
            spritePairs.Add(face);
        }

        ShuffleSprites(spritePairs);
    }


    void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, grid.transform);
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
                SoundManager.Instance.PlayMatch();
                ScoreManager.Instance.OnPairMatched();

                matchedPairs++;
                if (matchedPairs >= (rows * col) / 2)
                {
                    ScoreManager.Instance.OnGameWon();
                    SoundManager.Instance.PlayGameOver();
                }

            }
            else
            {
                SoundManager.Instance.PlayMismatch();
                ScoreManager.Instance.OnPairMismatched();
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
