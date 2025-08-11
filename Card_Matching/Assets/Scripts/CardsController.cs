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

    public int score;
    public float timeLeft;
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
    private List<int> _matchedFromSave = new List<int>();
    private int _scoreFromSave = 0;
    private float _timeFromSave = 0f;
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
            TurnOnMatchedCardsAndRestoreHUD();
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

        // NEW: snapshot HUD
        if (ScoreManager.Instance != null)
        {
            data.score = ScoreManager.Instance.Score;
            data.timeLeft = ScoreManager.Instance.TimeLeft;
        }

        try
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(OrderFilePath, json);

            PlayerPrefs.SetInt(MainMenuController.SaveExistsKey, 1);
            PlayerPrefs.Save();

        }
        catch (Exception e)
        {
            Debug.LogError($"[CardsController] Save order failed: {e}");
        }
    }


    void SaveProgressAfterMatch(int indexA, int indexB)
    {
        try
        {
            OrderData data;

            if (File.Exists(OrderFilePath))
            {
                var json = File.ReadAllText(OrderFilePath);
                data = JsonUtility.FromJson<OrderData>(json) ?? new OrderData();
            }
            else
            {
                data = new OrderData();
            }

            data.rows = rows;
            data.cols = col;

            if (data.matchedIndex == null) data.matchedIndex = new List<int>();
            void AddIfNew(int idx)
            {
                if (idx < 0 || idx >= spritePairs.Count) return;
                if (!data.matchedIndex.Contains(idx)) data.matchedIndex.Add(idx);
            }
            AddIfNew(indexA);
            AddIfNew(indexB);

            // also persist score/time every match
            if (ScoreManager.Instance != null)
            {
                data.score = ScoreManager.Instance.Score;
                data.timeLeft = ScoreManager.Instance.TimeLeft;
            }

            File.WriteAllText(OrderFilePath, JsonUtility.ToJson(data, true));

            PlayerPrefs.SetInt(MainMenuController.SaveExistsKey, 1);
            PlayerPrefs.Save();

            // Debug.Log($"[CardsController] Progress saved with matched: {indexA},{indexB}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[CardsController] SaveProgressAfterMatch error: {e}");
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

            // keep PlayerPrefs in sync because ApplyGridSettings() reads them
            PlayerPrefs.SetInt(RowsKey, rows);
            PlayerPrefs.SetInt(ColsKey, col);
            PlayerPrefs.Save();

            // Map names -> sprites
            var nameToSprite = new Dictionary<string, Sprite>(sprites.Length);
            foreach (var s in sprites) if (s != null) nameToSprite[s.name] = s;

            // Rebuild spritePairs exactly
            spritePairs = new List<Sprite>(data.names.Count);
            foreach (var n in data.names)
            {
                if (!nameToSprite.TryGetValue(n, out var spr)) spr = sprites.Length > 0 ? sprites[0] : null;
                spritePairs.Add(spr);
            }

            // Buffer matched + HUD for after CreateCards()
            _matchedFromSave = (data.matchedIndex != null) ? new List<int>(data.matchedIndex) : new List<int>();
            _scoreFromSave = data.score;
            _timeFromSave = data.timeLeft;

            Debug.Log($"[CardsController] Order loaded: {OrderFilePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[CardsController] Load order failed: {e}");
            return false;
        }
    }


    void TurnOnMatchedCardsAndRestoreHUD()
    {
        if (_matchedFromSave != null && _matchedFromSave.Count > 0)
        {
            matchedPairs = 0;
            foreach (int idx in _matchedFromSave)
            {
                var card = grid.transform.GetChild(idx+1).GetComponent<Card>();
                card.iconImage.sprite = card.iconSprite;
                card.isSelected = true;                   
                var c = Color.white; c.a = 0.65f;         
                card.iconImage.color = c;
                matchedPairs++;
            }
            matchedPairs /= 2;
        }

        // Restore HUD (score + time)
        ScoreManager.Instance?.LoadFromSave(_scoreFromSave, _timeFromSave);
    }


    public void DeleteSaveFile()
    {
        if (File.Exists(OrderFilePath))
        {
            File.Delete(OrderFilePath);
            Debug.Log("[CardsController] Save file deleted.");
        }

        PlayerPrefs.SetInt("SaveExists", 0);
        PlayerPrefs.SetInt("ContinueGame", 0);
        PlayerPrefs.Save();
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
                SaveProgressAfterMatch(a.index, b.index);
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
