using UnityEngine;
using TMPro;

/// <summary>
/// Скрипт, который отвечает за отображение текущего игрового счёта.
/// Для глобального доступа реализован Singleton
/// </summary>
public class ScoreCounter : MonoBehaviour
{
    private static ScoreCounter _instance;
    public static ScoreCounter Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ScoreCounter");
                _instance = go.AddComponent<ScoreCounter>();
            }
            return _instance;
        }
    }
    private int _score;

    TMP_Text _scroreText;
    public int Score => _score;
    public void AddScore(int score = 1) {
        _score++;
        if (_scroreText)
            _scroreText.text = $"score:{_score}";
    }
    public void ResetScore() => AddScore(-_score);
    private void OnTriggerEnter2D(Collider2D collision) => AddScore();
    private void Awake()
    {
        GameObject scoreTextGo = GameObject.Find("ScoreText");
        _scroreText = scoreTextGo.GetComponent<TMP_Text>();
        if (_scroreText)
            _scroreText.text = $"score:{0}";
    }
}
