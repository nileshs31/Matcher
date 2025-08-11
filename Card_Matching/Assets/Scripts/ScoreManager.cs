using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private GameObject gameoverScreen;

    [SerializeField] private float secondsPerCard = 3f;

    [SerializeField] private int scoreConstantK = 10;

    public const string RowsKey = "Rows";
    public const string ColsKey = "Cols";

    public int Score { get; private set; }
    public int Combo { get; private set; } 
    public float TotalTime { get; private set; }
    public float TimeLeft { get; private set; }
    public bool TimeUp { get; private set; }


    [SerializeField] private RectTransform toastPanel; 
    [SerializeField] private TMP_Text toastText;      
    [SerializeField] private float toastShowY = -50f; 
    [SerializeField] private float toastHideY = 150f;
    [SerializeField] CardsController cardsController;

    private Tween toastTween;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        int rows = Mathf.Clamp(PlayerPrefs.GetInt(RowsKey, 4), 1, 6);
        int cols = Mathf.Clamp(PlayerPrefs.GetInt(ColsKey, 4), 1, 6);

        if ((rows * cols) % 2 != 0) cols = Mathf.Max(1, cols - 1);

        TotalTime = Mathf.Max(10f, rows * cols * Mathf.Max(0.5f, secondsPerCard));
        TimeLeft = TotalTime;

        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = TotalTime;
            timeSlider.value = TimeLeft;
        }

        Combo = 0;
        Score = 0;
        UpdateScoreUI();

        if (toastPanel != null)
        {
            toastPanel.anchoredPosition = new Vector2(toastPanel.anchoredPosition.x, toastHideY);
            toastPanel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (TimeUp) return;

        TimeLeft -= Time.deltaTime;
        if (timeSlider != null) timeSlider.value = Mathf.Max(0f, TimeLeft);

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            TimeUp = true;
            SoundManager.Instance.PlayGameOver();
            gameoverScreen.SetActive(true);
            cardsController.DeleteSaveFile();
        }
    }
    public void OnPairMatched()
    {
        Combo = Mathf.Min(Combo + 1, 10);

        float timeFactor = (TotalTime <= 0f) ? 0f : (TimeLeft / TotalTime);
        int delta = Mathf.CeilToInt(timeFactor * Combo * scoreConstantK);
        Score += Mathf.Max(0, delta);

        UpdateScoreUI();
        ShowComboToast();
    }

    public void OnPairMismatched()
    {
        Combo = 0;
    }

    public void OnGameWon()
    {
        if (!TimeUp)
        {
            TimeUp = true;
            SoundManager.Instance.PlayGameOver();
            gameoverScreen.SetActive(true);
            FindObjectOfType<CardsController>()?.DeleteSaveFile();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Score}";
    }

    private void ShowComboToast()
    {
        if (toastPanel == null || toastText == null) return;
        if (Combo <= 1) return;

        toastText.text = $"Combo {Combo}x";
        toastPanel.gameObject.SetActive(true);

        toastTween?.Kill();
        toastPanel.anchoredPosition = new Vector2(toastPanel.anchoredPosition.x, toastHideY);

        toastTween = toastPanel.DOAnchorPosY(toastShowY, 0.25f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(1.0f, () =>
                {
                    toastPanel.DOAnchorPosY(toastHideY, 0.25f)
                        .SetEase(Ease.InCubic)
                        .OnComplete(() =>
                        {
                            toastPanel.gameObject.SetActive(false);
                        });
                });
            });
    }

    public void LoadFromSave(int score, float timeLeft)
    {
        Score = Mathf.Max(0, score);
        TimeLeft = Mathf.Clamp(timeLeft, 0f, TotalTime);

        if (timeSlider != null)
        {
            timeSlider.minValue = 0f;
            timeSlider.maxValue = TotalTime;
            timeSlider.value = TimeLeft;
        }
        if (scoreText != null)
            scoreText.text = $"Score: {Score}";
    }

}
