using UnityEngine;
using TMPro;

[RequireComponent(typeof(TouchMovmentHandle))]
public class BallsShooter : MonoBehaviour{
    /// <summary>
    /// ���������� ������� �������� ������� ��� �������
    /// </summary>
    [SerializeField]
    TMP_Text _ballsRemainText;
    
    /// <summary>
    /// ��������� ������� ������
    /// </summary>
    [SerializeField]
    GameSession _gameSession; 
    
    /// <summary>
    /// ����� ��� ����������� ������� � �����, ��� ������ �������� ���
    /// </summary>
    [SerializeField]
    GameObject _spawnedBallsContainer;
    
    /// <summary>
    /// ������� pre-view ��������� �����
    /// </summary>
    [SerializeField]
    TrajectoryPreview _trajectoryPreview;
    
    /// <summary>
    /// Scaler ���� ��������
    /// </summary>
    [SerializeField]
    float _forceFactor = 1.0f;
    
    /// <summary>
    /// ����� ��� ���������� ����� ���������������� ���� 
    /// </summary>
    TouchMovmentHandle _touchHandle;

    /// <summary>
    /// ������� ���� ��������
    /// </summary>
    public Vector3 ShotingForce => -_forceFactor * _touchHandle.TouchDeltaPosition;

    /// <summary>
    /// �������, ������� ���������� �� ����� ��������
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
    /// �������, ������� ���������� �� ����� ������������
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
         Physics2D.gravity = new Vector3(0.0f, -1.0f, 0.0f); // ����� ��� �� ������ ���������� � ���� ������
        if (_trajectoryPreview == null || _gameSession == null) return;
        _touchHandle = GetComponent<TouchMovmentHandle>();
        _touchHandle.AppendTouchEndedCallback(ShootBall);
        _touchHandle.AppendTouchEndedCallback((touch) => { _trajectoryPreview.ShowTrajectory(false);});
        _touchHandle.AppendTouchBeganCallback((touch) => { if(_gameSession.BallsRemain != 0)  _trajectoryPreview.ShowTrajectory(true);});
        _touchHandle.AppendTouchMovedCallback(DrawTrajectory);
        _ballsRemainText.text = $"{_gameSession.BallsRemain}";
    }
}
