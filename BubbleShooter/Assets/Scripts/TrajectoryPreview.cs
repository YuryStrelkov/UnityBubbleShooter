using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, реализующий отрисовку возможной траектории полёта шарика
/// </summary>
public class TrajectoryPreview : MonoBehaviour
{
    /// <summary>
    /// Ускорение свободного падения (не G)
    /// </summary>
    static readonly Vector3 freeFallAccel = new Vector3(0.0f, -1.0f, 0.0f);
    
    /// <summary>
    /// LineRenderer для отрисовки точной траетории
    /// </summary>
    [SerializeField]
    private LineRenderer _precisionTrajectoryRenderer;

    /// <summary>
    /// LineRenderer для отрисовки приблизительной траетории
    /// </summary>
    [SerializeField]
    private LineRenderer _approximateTrajectoryRenderer;

    /// <summary>
    /// Ширина линии траетории
    /// </summary>
    [SerializeField]
    float _lineWidth = 0.1f;

    /// <summary>
    /// Декремент скорости при столкновении со стенками игрового поля
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
    /// Простое интегрирование уравнения движения 
    /// </summary>
    /// <param name="force">Начальная сила</param>
    /// <param name="startPosition">Начальное положение</param>
    /// <param name="pointsCount">Количество точек на траеторию (Маскимально возможное)</param>
    /// <param name="maxBounceCount">Максимальное число отскоков</param>
    /// <returns></returns>
    private Vector3[] BuildTrajectoryNoPhysics(Vector3 force, Vector3 startPosition, int maxBounceCount = 3,  int pointsCount = 1000)
    {
        // Получаем размер экрана устройства в  мировых координатах
        Vector3 worldSpaceRes = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float width  = worldSpaceRes.x;
        float height = worldSpaceRes.y;
        Vector3 velosity = force * Time.fixedDeltaTime; // force => impulse
        Vector3 position = startPosition;
        List<Vector3> trajectoryPoints = new List<Vector3>();
        int bounceCount = 0;
        for (int stepId = 0; stepId < pointsCount; stepId++) {
            // Интегрируем уравнение движения 
            velosity += freeFallAccel * Time.fixedDeltaTime;
            position += velosity * Time.fixedDeltaTime;
            // Учитываем ограничения игрового поля/экрана
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
    /// Включает/отключает отображение трактории
    /// </summary>
    /// <param name="show"></param>
    public void ShowTrajectory(bool show) {
        if (_precisionTrajectoryRenderer != null) 
            _precisionTrajectoryRenderer.enabled = show;
        if (_approximateTrajectoryRenderer != null)
            _approximateTrajectoryRenderer.enabled = show;
    }

    /// <summary>
    /// Пересчитывает траекторию полёта
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
