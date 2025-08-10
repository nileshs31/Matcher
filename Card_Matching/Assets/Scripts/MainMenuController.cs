using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField rowsInput;
    [SerializeField] private TMP_InputField colsInput;
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [Header("Config")]
    [SerializeField] private string gameSceneName = "Game";

    // PlayerPrefs keys
    public const string RowsKey = "Rows";
    public const string ColsKey = "Cols";
    public const string ContinueKey = "ContinueGame";
    public const string SaveExistsKey = "SaveExists";

    private const int MinSize = 2;
    private const int MaxSize = 6;
    private const int DefaultRows = 2;
    private const int DefaultCols = 2;

    private void Awake()
    {
        
        int rows = Mathf.Clamp(PlayerPrefs.GetInt(RowsKey, DefaultRows), MinSize, MaxSize);
        int cols = Mathf.Clamp(PlayerPrefs.GetInt(ColsKey, DefaultCols), MinSize, MaxSize);

        rowsInput.text = rows.ToString();
        colsInput.text = cols.ToString();

        rowsInput.onEndEdit.AddListener(s => rowsInput.text = ClampToRange(s, MinSize, MaxSize).ToString());
        colsInput.onEndEdit.AddListener(s => colsInput.text = ClampToRange(s, MinSize, MaxSize).ToString());

        startButton.onClick.AddListener(OnStartClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        bool hasSave = PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
        continueButton.interactable = hasSave;
    }

    private void OnStartClicked()
    {
        int rows = ClampToRange(rowsInput.text, MinSize, MaxSize);
        int cols = ClampToRange(colsInput.text, MinSize, MaxSize);

        PlayerPrefs.SetInt(RowsKey, rows);
        PlayerPrefs.SetInt(ColsKey, cols);
        PlayerPrefs.SetInt(ContinueKey, 0);   
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnContinueClicked()
    {
        PlayerPrefs.SetInt(ContinueKey, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnQuitClicked()
    {
        PlayerPrefs.Save();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private static int ClampToRange(string s, int min, int max)
    {
        if (!int.TryParse(KeepDigits(s), out int v)) v = min;
        return Mathf.Clamp(v, min, max);
    }

    private static string KeepDigits(string s)
    {
        if (string.IsNullOrEmpty(s)) return "0";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(s.Length);
        foreach (char c in s)
            if (char.IsDigit(c)) sb.Append(c);
        return sb.Length == 0 ? "0" : sb.ToString();
    }
}
