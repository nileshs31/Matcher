using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.IO;                   
using System;                       

[Serializable]
class OrderData
{
    public int rows;
    public int cols;
    public List<string> names = new List<string>();
    public List<int> matchedIndex = new List<int>();
}



public class CardsController : MonoBehaviour
{

    const string OrderFileName = "board_order.json";

    string OrderFilePath => Path.Combine(Application.persistentDataPath, OrderFileName);
    [SerializeField] Card cardPrefab;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;

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
        // CONTINUE-
        bool cont = PlayerPrefs.GetInt(MainMenuController.ContinueKey, 0) == 1;
        if (cont && TryLoadOrderFromFile())
        {
            ApplyGridSettings();
            CreateCards();
            return;
        }

        // Fresh game
        rows = PlayerPrefs.GetInt(RowsKey);
        col = PlayerPrefs.GetInt(ColsKey);
        if ((rows * col) % 2 != 0) col = Mathf.Max(1, col - 1);

        ApplyGridSettings();
        SetupSprite();
        CreateCards();
        SaveShuffledOrderToFile();
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
        SaveShuffledOrderToFile();
    }

    void SaveShuffledOrderToFile()
    {
        var data = new OrderData { rows = rows, cols = col };
        if (spritePairs != null)
        {
            for (int i = 0; i < spritePairs.Count; i++)
                data.names.Add(spritePairs[i] ? spritePairs[i].name : "");
        }

        try
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(OrderFilePath, json);
            Debug.Log($"[CardsController] Order saved: {OrderFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CardsController] Save order failed: {e}");
        }
    }

    bool TryLoadOrderFromFile()
    {
        if (!File.Exists(OrderFilePath)) return false;

        try
        {
            var json = File.ReadAllText(OrderFilePath);
            var data = JsonUtility.FromJson<OrderData>(json);
            if (data == null || data.names == null || data.names.Count == 0) return false;

            rows = data.rows;
            col = data.cols;

            var nameToSprite = new Dictionary<string, Sprite>(sprites.Length);
            foreach (var s in sprites) if (s != null) nameToSprite[s.name] = s;


            spritePairs = new List<Sprite>(data.names.Count);
            foreach (var n in data.names)
            {
                if (!nameToSprite.TryGetValue(n, out var spr)) spr = sprites.Length > 0 ? sprites[0] : null;
                spritePairs.Add(spr);
            }

            Debug.Log($"[CardsController] Order loaded: {OrderFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CardsController] Load order failed: {e}");
            return false;
        }
    }

    void CreateCards()
    {
        for (int i = 0; i < spritePairs.Count; i++)
        {
            Card card = Instantiate(cardPrefab, grid.transform);
            card.SetIconSprite(spritePairs[i]);
            card.cardCon = this;
            card.index = i;
        }
    }


    void ShuffleSprites(List<Sprite> spritesList)
    {
        for (int i = spritesList.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
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
