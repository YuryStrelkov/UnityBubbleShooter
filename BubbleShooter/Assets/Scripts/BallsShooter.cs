using UnityEngine;
using TMPro;

[RequireComponent(typeof(TouchMovmentHandle))]
public class BallsShooter : MonoBehaviour{
    /// <summary>
    /// Отображает сколько осталось шариков для стрльбы
    /// </summary>
    [SerializeField]
    TMP_Text _ballsRemainText;
    
    /// <summary>
    /// Настройки игровой сессии
    /// </summary>
    [SerializeField]
    GameSession _gameSession; 
    
    /// <summary>
    /// Нужен для поддержания порядка в сцене, все шарики хранятся там
    /// </summary>
    [SerializeField]
    GameObject _spawnedBallsContainer;
    
    /// <summary>
    /// Рисуент pre-view траетории полёта
    /// </summary>
    [SerializeField]
    TrajectoryPreview _trajectoryPreview;
    
    /// <summary>
    /// Scaler силы выстрела
    /// </summary>
    [SerializeField]
    float _forceFactor = 1.0f;
    
    /// <summary>
    /// Нужен для управления через пользовательский ввод 
    /// </summary>
    TouchMovmentHandle _touchHandle;

    /// <summary>
    /// Текущая сила выстрела
    /// </summary>
    public Vector3 ShotingForce => -_forceFactor * _touchHandle.TouchDeltaPosition;

    /// <summary>
    /// Функция, которая вызывается во время выстрела
    /// </summary>
    /// <param name="touch"></param>
    private void ShootBall(Touch touch) {
        if (!_gameSession.TekeBall())
            return;
        _ballsRemainText.text = $"{_gameSession.BallsRemain}";
       Vector3 position = Camera.main.ScreenToWorldPoint(transform.position);
       position.z = 0;
        BallBehaviour ball = null;
       if (ball = BallBehaviour.SpawnProjectileBall(position, _spawnedBallsContainer)) {
            ball.GetComponent<Rigidbody2D>().AddForce(ShotingForce);
        }
    }

    /// <summary>
    /// Функция, которая вызывается во время прицеливания
    /// </summary>
    /// <param name="touch"></param>
    private void DrawTrajectory(Touch touch)
    {
        if (!_trajectoryPreview) return;
        Vector3 position = Camera.main.ScreenToWorldPoint(transform.position);
        position.z = 0;
        _trajectoryPreview.RecalcTrajectory(ShotingForce, position);
    }

    void Start()
    {
         Physics2D.gravity = new Vector3(0.0f, -1.0f, 0.0f); // нужно что бы сопала траектория с путём шарика
        if (_trajectoryPreview == null || _gameSession == null) return;
        _touchHandle = GetComponent<TouchMovmentHandle>();
        _touchHandle.AppendTouchEndedCallback(ShootBall);
        _touchHandle.AppendTouchEndedCallback((touch) => { _trajectoryPreview.ShowTrajectory(false);});
        _touchHandle.AppendTouchBeganCallback((touch) => { if(_gameSession.BallsRemain != 0)  _trajectoryPreview.ShowTrajectory(true);});
        _touchHandle.AppendTouchMovedCallback(DrawTrajectory);
        _ballsRemainText.text = $"{_gameSession.BallsRemain}";
    }
}
