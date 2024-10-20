using System.Collections;
using TMPro;
using UnityEngine;


/// <summary>
/// ��������� - ������ ��� ������ JSON ����
/// </summary>
[System.Serializable]
public struct GameSessionInfo {
    /// <summary>
    /// ���������� �����, ������� ����� ���������� �� ������
    /// </summary>
    public int playerBallsCount;
    /// <summary>
    /// ������ � ������ ����� � ����� ������� ����� ����������� ������ �� ������� ��������.
    /// </summary>
    public Vector2Int levelBallsGridRes;
    /// <summary>
    /// ����� �������, ���������� ���������.
    /// </summary>
    public string[] levelBalls;
}

public class GameSession : MonoBehaviour
{
    /// <summary>
    /// ����� ��� ����������� ������� � �����, ��� ������ �������� ���
    /// </summary>
    [SerializeField]
    GameObject _spawnedBallsContainer = null;
    
    /// <summary>
    /// UI, ������� ������������ � ����� ���� 
    /// </summary>
    [SerializeField]
    GameObject _gameOverUI = null;
    
    /// <summary>
    ///  ��������� � ������� �������� ������ ��� ��������
    /// </summary>
    GameObject _ballSootingBtn = null;

    /// <summary>
    /// ��������� � ������� �������� UI - Text ��� ������ ����� ����
    /// </summary>
    GameObject _gameOverStatusAndScoreText = null;

    /// <summary>
    /// JSON ���� � ����������� �������� ��������
    /// </summary>
    [SerializeField]
    private TextAsset _ballsSpawnInfo;

    /// <summary>
    ///  JSON ���� � ����������� �������� ��������, ����������� � �������������� � ���� ��������� - �������.
    /// </summary>
    [SerializeField]
    private GameSessionInfo _gameSessionInfo;

    /// <summary>
    /// ���������� �������, ������� ��������
    /// </summary>
    private int _ballsRemain;

    public int BallsRemain => _ballsRemain;

    /// <summary>
    /// ��������� ���������� ������ ������
    /// </summary>
    /// <returns></returns>
    private GameSessionInfo LoadSettings() {
        return JsonUtility.FromJson<GameSessionInfo>(_ballsSpawnInfo.text);
    }

    /// <summary>
    /// ����������� ������ �� �������� ����
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
    /// �������, ������� ������������ ��� ��������. ��������� �������� �� ��� ����.
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
    /// ��������� ��� ���������� ����. ������� ���������� � �������� �������� � ���...,
    /// � ��� �� ���� � ��������� - ������� ��� ��������
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
