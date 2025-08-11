using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverHandler : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;
    [SerializeField] private Button homeButton, homeButton2;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        homeButton.onClick.AddListener(OnHomeClicked);
        homeButton2.onClick.AddListener(OnHomeClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    void Update()
    {
        scoreText.text = "Score : " + ScoreManager.Instance.Score;
    }
    private void OnHomeClicked()
    {
        SceneManager.LoadScene("MainScene");  
    }
    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
