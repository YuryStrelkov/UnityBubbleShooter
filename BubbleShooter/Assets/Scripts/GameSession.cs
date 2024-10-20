using System.Collections;
using TMPro;
using UnityEngine;


/// <summary>
/// Структура - шаблон для чтения JSON фала
/// </summary>
[System.Serializable]
public struct GameSessionInfo {
    /// <summary>
    /// Количество шаров, которые можно выстрелить на уровне
    /// </summary>
    public int playerBallsCount;
    /// <summary>
    /// Ширина и высота сетки в узлах которой будут расположены шарики по которым стреляем.
    /// </summary>
    public Vector2Int levelBallsGridRes;
    /// <summary>
    /// Цвета шариков, записанные построчно.
    /// </summary>
    public string[] levelBalls;
}

public class GameSession : MonoBehaviour
{
    /// <summary>
    /// Нужен для поддержания порядка в сцене, все шарики хранятся там
    /// </summary>
    [SerializeField]
    GameObject _spawnedBallsContainer = null;
    
    /// <summary>
    /// UI, который отображается в конце игры 
    /// </summary>
    [SerializeField]
    GameObject _gameOverUI = null;
    
    /// <summary>
    ///  Контейнер в котором хранится кнопка для стрельбы
    /// </summary>
    GameObject _ballSootingBtn = null;

    /// <summary>
    /// Контейнер в котором хранится UI - Text для вывода счёта игры
    /// </summary>
    GameObject _gameOverStatusAndScoreText = null;

    /// <summary>
    /// JSON файл с настройками игрового процесса
    /// </summary>
    [SerializeField]
    private TextAsset _ballsSpawnInfo;

    /// <summary>
    ///  JSON файл с настройками игрового процесса, прочитанный и представленный в виде структура - шаблона.
    /// </summary>
    [SerializeField]
    private GameSessionInfo _gameSessionInfo;

    /// <summary>
    /// Количество шариков, которые остались
    /// </summary>
    private int _ballsRemain;

    public int BallsRemain => _ballsRemain;

    /// <summary>
    /// Загружает информацию игрвой сессии
    /// </summary>
    /// <returns></returns>
    private GameSessionInfo LoadSettings() {
        return JsonUtility.FromJson<GameSessionInfo>(_ballsSpawnInfo.text);
    }

    /// <summary>
    /// Расставляет шарики по игровому полю
    /// </summary>
    private void SpawnLevelBalls() {
        _ballsRemain = _gameSessionInfo.playerBallsCount;
        Vector3 worldSpaceRes = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float width = worldSpaceRes.x * 2.0f;
        float height = worldSpaceRes.y * 2.0f;
        int cols = _gameSessionInfo.levelBallsGridRes.x;
        int rows = _gameSessionInfo.levelBallsGridRes.y;
        // GameObject ball;
        Vector2 lu = new Vector2(height * 0.45f, width * 0.45f);
        float delta = Mathf.Min(width * 0.9f / (cols - 1), height * 0.9f / (rows - 1));
        for (int ballIndex = 0; ballIndex < _gameSessionInfo.levelBalls.Length; ballIndex++) {
            float row = lu.x - delta * (ballIndex / cols);
            float col = -lu.y + delta * (ballIndex % cols);
            BallBehaviour.SpawnBall(new Vector3(col, row, 0), _gameSessionInfo.levelBalls[ballIndex], _spawnedBallsContainer);
        }
    }

    private void Start() => MakeGameField();

    private void Awake()
    {
        _gameOverUI = GameObject.Find("GameOverPannel");
        _spawnedBallsContainer = GameObject.Find("SpawnedBallsContainer");
        _ballSootingBtn = GameObject.Find("BallSootingBtn");
        _gameOverStatusAndScoreText = GameObject.Find("GameOverStatusAndScoreText");
        _gameOverUI?.SetActive(false);
    }

    /// <summary>
    /// Функция, котороя используется при выстреле. Проверяет остались ли ещё шары.
    /// </summary>
    /// <returns></returns>
    public bool TekeBall() {
        if (_ballsRemain == 0)
        {
            return false;
        }
        _ballsRemain--;
        return true;
    }

    /// <summary>
    /// MakeGameField
    /// </summary>
    public void MakeGameField() {
        if (_ballsSpawnInfo) _gameSessionInfo = LoadSettings();
        if (_gameSessionInfo.levelBalls.Length != 0) SpawnLevelBalls();
    }
    
    /// <summary>
    /// RestartGame
    /// </summary>
    public void RestartGame() {
        _gameOverUI?.SetActive(false);
        _ballSootingBtn?.SetActive(true);
        SpawnLevelBalls();
        ScoreCounter.Instance.ResetScore();
    }
    /// <summary>
    /// Вызывется при завершении игры. Выводит интрерфейс с кнопками рестарта и тда...,
    /// а так же счёт и результат - выйграл или проиграл
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameOverSequence() {
        BallBehaviour.CollapseAllBalls();
        yield return new WaitForSeconds(1);
        _ballSootingBtn?.SetActive(false);
        _gameOverUI?.SetActive(true);
        TMP_Text text = _gameOverStatusAndScoreText?.GetComponent<TMP_Text>();
        if (text) {
            if (ScoreCounter.Instance.Score > _gameSessionInfo.levelBalls.Length / 2) {
                text.text = $"YOU WON!!!\nTHE SCORE IS:{ScoreCounter.Instance.Score}";
            } else { 
                text.text = $"YOU FAILED!!!\nTHE SCORE IS:{ScoreCounter.Instance.Score}";
            }
        }
    }

    private void FixedUpdate()
    {
        if (BallsRemain == 0)
        {
            _ballsRemain = 1;
            StartCoroutine(GameOverSequence());
        }

    }
}
