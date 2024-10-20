using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����, ����������� ��������� ��������� ���������� ����� ������
/// </summary>
public class TrajectoryPreview : MonoBehaviour
{
    /// <summary>
    /// ��������� ���������� ������� (�� G)
    /// </summary>
    static readonly Vector3 freeFallAccel = new Vector3(0.0f, -1.0f, 0.0f);
    
    /// <summary>
    /// LineRenderer ��� ��������� ������ ���������
    /// </summary>
    [SerializeField]
    private LineRenderer _precisionTrajectoryRenderer;

    /// <summary>
    /// LineRenderer ��� ��������� ��������������� ���������
    /// </summary>
    [SerializeField]
    private LineRenderer _approximateTrajectoryRenderer;

    /// <summary>
    /// ������ ����� ���������
    /// </summary>
    [SerializeField]
    float _lineWidth = 0.1f;

    /// <summary>
    /// ��������� �������� ��� ������������ �� �������� �������� ����
    /// </summary>
    [SerializeField]
    float _bounceAbsorbtion = 0.9f;

    public float BounceAbsorbtion
    {
        get => _bounceAbsorbtion;
        set => _bounceAbsorbtion = Mathf.Clamp(value, 0.0f, 1.0f);
    }
    public float LineWidth {
        get => _lineWidth;
        set => _lineWidth = Mathf.Max(0.01f, value);
    }

    /// <summary>
    /// ������� �������������� ��������� �������� 
    /// </summary>
    /// <param name="force">��������� ����</param>
    /// <param name="startPosition">��������� ���������</param>
    /// <param name="pointsCount">���������� ����� �� ��������� (����������� ���������)</param>
    /// <param name="maxBounceCount">������������ ����� ��������</param>
    /// <returns></returns>
    private Vector3[] BuildTrajectoryNoPhysics(Vector3 force, Vector3 startPosition, int maxBounceCount = 3,  int pointsCount = 1000)
    {
        // �������� ������ ������ ���������� �  ������� �����������
        Vector3 worldSpaceRes = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float width  = worldSpaceRes.x;
        float height = worldSpaceRes.y;
        Vector3 velosity = force * Time.fixedDeltaTime; // force => impulse
        Vector3 position = startPosition;
        List<Vector3> trajectoryPoints = new List<Vector3>();
        int bounceCount = 0;
        for (int stepId = 0; stepId < pointsCount; stepId++) {
            // ����������� ��������� �������� 
            velosity += freeFallAccel * Time.fixedDeltaTime;
            position += velosity * Time.fixedDeltaTime;
            // ��������� ����������� �������� ����/������
            if (position.x > width || position.x < -width) {
                velosity.x *= -BounceAbsorbtion;
                force.x *= -BounceAbsorbtion;
                bounceCount++;
            }
            if (position.y > height || position.y < -height)
            {
                velosity.y *= -BounceAbsorbtion;
                force.y *= -BounceAbsorbtion;
                bounceCount++;
            }
            trajectoryPoints.Add(position);
            if (bounceCount == maxBounceCount)
                break;
        }
        return trajectoryPoints.ToArray();
    }

    /// <summary>
    /// ��������/��������� ����������� ���������
    /// </summary>
    /// <param name="show"></param>
    public void ShowTrajectory(bool show) {
        if (_precisionTrajectoryRenderer != null) 
            _precisionTrajectoryRenderer.enabled = show;
        if (_approximateTrajectoryRenderer != null)
            _approximateTrajectoryRenderer.enabled = show;
    }

    /// <summary>
    /// ������������� ���������� �����
    /// </summary>
    /// <param name="force"></param>
    /// <param name="startPosition"></param>
    public void RecalcTrajectory(Vector3 force, Vector3 startPosition) {
        if (_precisionTrajectoryRenderer == null &&  _approximateTrajectoryRenderer == null)
            return;
        
        Vector3[] points = null;
        points = BuildTrajectoryNoPhysics(force, startPosition);


        if (_precisionTrajectoryRenderer != null)
        {
            _precisionTrajectoryRenderer.positionCount = points.Length;
            _precisionTrajectoryRenderer.SetPositions(points);
            _precisionTrajectoryRenderer.startWidth = _lineWidth;
            _precisionTrajectoryRenderer.endWidth   = _lineWidth;
        }

        if (_approximateTrajectoryRenderer != null) {
            _approximateTrajectoryRenderer.positionCount = points.Length;
            _approximateTrajectoryRenderer.SetPositions(points);
            _approximateTrajectoryRenderer.startWidth = _lineWidth;
            _approximateTrajectoryRenderer.endWidth   = _lineWidth * force.magnitude * 0.075f;
        }
    }
}
