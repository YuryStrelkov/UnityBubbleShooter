using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������, ������� �������� �� ��������� ����������������� �����.
/// </summary>
[DefaultExecutionOrder(-1)]
public class TouchMovmentHandle : MonoBehaviour
{
    /// <summary>
    /// ������� ��� �������, ������� ����� ���������� ��� ������, ���������� ��� �������� ������� ������� ������
    /// </summary>
    /// <param name="touch"></param>
    public delegate void TouchCallback(Touch touch);

    /// <summary>
    /// �������[��� ������� : ������� �������] 
    /// </summary>
    private Dictionary<TouchPhase, HashSet<TouchCallback>> _touchCallbacks;

    /// <summary>
    /// ���������� ����������� ������ � �������� ����� �� ���������� ��������
    /// </summary>
    [SerializeField]
    public bool moveToStartAtTouchEnd = true;

    /// <summary>
    /// ���������� �� ������� ����� ���������� ������ �� ��� ��������� ���������
    /// </summary>
    [SerializeField]
    private float _touchMovementRange = 150.0f;

    /// <summary>
    /// ��������� ����� �������
    /// </summary>
    private float  _touchStartTime;

    /// <summary>
    /// ������� ����� �������
    /// </summary>
    private Vector3 _touchPosition;

    /// <summary>
    /// ��������� ����� �������
    /// </summary>
    private Vector3 _touchStartPosition;

    /// <summary>
    /// �������� ��������� �������, ������� �����
    /// </summary>
    private Vector3 _transfromOrigin;
    
    /// <summary>
    /// ���� ������� ��������/������� ������� ��������� ������ ��� ������, �������, ����� �������.
    /// </summary>
    private bool AppendTouchCallback(TouchPhase phase, TouchCallback callback) {
        if (!_touchCallbacks.ContainsKey(phase))
            return false;
        if (_touchCallbacks[phase].Contains(callback))
            return false;
        _touchCallbacks[phase].Add(callback);
        return true;
    }
    private bool RemoveTouchCallback(TouchPhase phase, TouchCallback callback)
    {
        if (!_touchCallbacks.ContainsKey(phase))
            return false;
        if (!_touchCallbacks[phase].Contains(callback))
            return false;
        _touchCallbacks[phase].Remove(callback);
        return true;
    }
    public bool AppendTouchBeganCallback(TouchCallback callback) => AppendTouchCallback(TouchPhase.Began, callback);
    public bool AppendTouchMovedCallback(TouchCallback callback) => AppendTouchCallback(TouchPhase.Moved, callback);
    public bool AppendTouchEndedCallback(TouchCallback callback) => AppendTouchCallback(TouchPhase.Ended, callback);
    public bool RemoveTouchBeganCallback(TouchCallback callback) => RemoveTouchCallback(TouchPhase.Began, callback);
    public bool RemoveTouchMovedCallback(TouchCallback callback) => RemoveTouchCallback(TouchPhase.Moved, callback);
    public bool RemoveTouchEndedCallback(TouchCallback callback) => RemoveTouchCallback(TouchPhase.Ended, callback);
    
    /// <summary>
    /// ���������� �� ������� ����� ���������� ������ �� ��� ��������� ���������
    /// </summary>
    public float TouchMovementRange {
        get => _touchMovementRange;
        set => _touchMovementRange = Mathf.Max(0.1f, value);
    }
    
    /// <summary>
    /// ����� �������� �������
    /// </summary>
    public float TouchTime => Time.time - _touchStartTime;

    /// <summary>
    /// ������� ����� �������
    /// </summary>
    public Vector3 TouchPosition => _touchPosition;

    /// <summary>
    /// ��������� ����� �������
    /// </summary>
    public Vector3 TouchStartPosition => _touchStartPosition;
    
    /// <summary>
    /// ����������� �������
    /// </summary>
    public Vector3 TouchDeltaPosition => TouchPosition - TouchStartPosition;

    /// <summary>
    /// ������������ ������� � ������ � ����������� �� ���� �������.
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began: OnTouchBegan(touch); break;
            case TouchPhase.Moved: OnTouchMoved(touch); break;
            case TouchPhase.Ended: OnTouchEnded(touch); break;
        }
    }

    /// <summary>
    /// ���������� � ������ �������
    /// </summary>
    /// <param name="touch"></param>
    private void OnTouchBegan(Touch touch)
    {
        _touchPosition = _touchStartPosition = touch.position;
        _touchStartTime = Time.time;
        foreach (var callback in _touchCallbacks[TouchPhase.Began]) callback(touch);
    }

    /// <summary>
    /// ���������� � ����� �������
    /// </summary>
    private void OnTouchEnded(Touch touch)
    {
        // _currentlyTouchedObject = null;
        foreach (var callback in _touchCallbacks[TouchPhase.Ended]) callback(touch);
        _touchStartTime = 0.0f;
        _touchStartPosition = _touchPosition = new Vector3(0.0f, 0.0f, 0.0f);
        if(moveToStartAtTouchEnd) 
            transform.position = _transfromOrigin;
    }

    /// <summary>
    /// ���������� ��� �������� �������
    /// </summary>
    private void OnTouchMoved(Touch touch)
    {
        _touchPosition = touch.position;
        Vector3 touchDelta = TouchDeltaPosition;
        float tractionLength = touchDelta.magnitude;
        if (tractionLength < 1e-3f)
            return;
        touchDelta *= Mathf.Min(tractionLength, _touchMovementRange) / tractionLength;
        transform.position = _transfromOrigin + touchDelta;
        foreach (var callback in _touchCallbacks[TouchPhase.Moved]) callback(touch);
    }

    void Start()
    {
        _touchPosition = new Vector3(0.0f, 0.0f, 0.0f);
        _transfromOrigin = transform.position;
        _touchCallbacks = new Dictionary<TouchPhase, HashSet<TouchCallback>>();
        _touchCallbacks.Add(TouchPhase.Began, new HashSet<TouchCallback>());
        _touchCallbacks.Add(TouchPhase.Moved, new HashSet<TouchCallback>());
        _touchCallbacks.Add(TouchPhase.Ended, new HashSet<TouchCallback>());
    }
    void Update() => HandleTouchInput();
}
