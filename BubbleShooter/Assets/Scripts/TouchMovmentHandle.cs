using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Скрипт, который отвечает за обработку пользователського ввода.
/// </summary>
[DefaultExecutionOrder(-1)]
public class TouchMovmentHandle : MonoBehaviour
{
    /// <summary>
    /// Делегат для функции, которая будет вызываться при начале, завершении или движении касания пальцем экрана
    /// </summary>
    /// <param name="touch"></param>
    public delegate void TouchCallback(Touch touch);

    /// <summary>
    /// Словарь[тип события : делегат функции] 
    /// </summary>
    private Dictionary<TouchPhase, HashSet<TouchCallback>> _touchCallbacks;

    /// <summary>
    /// Возвращает перетянутый объект в исходную точку по завершении движения
    /// </summary>
    [SerializeField]
    public bool moveToStartAtTouchEnd = true;

    /// <summary>
    /// Расстояние на которое можно перетянуть объект от его исходного положения
    /// </summary>
    [SerializeField]
    private float _touchMovementRange = 150.0f;

    /// <summary>
    /// Начальное время касания
    /// </summary>
    private float  _touchStartTime;

    /// <summary>
    /// Текущая точка касания
    /// </summary>
    private Vector3 _touchPosition;

    /// <summary>
    /// Начальная точка касания
    /// </summary>
    private Vector3 _touchStartPosition;

    /// <summary>
    /// Исходное положение объекта, который тянем
    /// </summary>
    private Vector3 _transfromOrigin;
    
    /// <summary>
    /// Ниже функции подписки/отписки функций обратного вызова для начала, движени, конца касания.
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
    /// Расстояние на которое можно перетянуть объект от его исходного положения
    /// </summary>
    public float TouchMovementRange {
        get => _touchMovementRange;
        set => _touchMovementRange = Mathf.Max(0.1f, value);
    }
    
    /// <summary>
    /// Время текущего касания
    /// </summary>
    public float TouchTime => Time.time - _touchStartTime;

    /// <summary>
    /// Текущая точка касания
    /// </summary>
    public Vector3 TouchPosition => _touchPosition;

    /// <summary>
    /// Начальная точка касания
    /// </summary>
    public Vector3 TouchStartPosition => _touchStartPosition;
    
    /// <summary>
    /// Перемещение касания
    /// </summary>
    public Vector3 TouchDeltaPosition => TouchPosition - TouchStartPosition;

    /// <summary>
    /// Обрабатывает касание к экрану в зависимости от фазы касания.
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
    /// Вызывается в начале касания
    /// </summary>
    /// <param name="touch"></param>
    private void OnTouchBegan(Touch touch)
    {
        _touchPosition = _touchStartPosition = touch.position;
        _touchStartTime = Time.time;
        foreach (var callback in _touchCallbacks[TouchPhase.Began]) callback(touch);
    }

    /// <summary>
    /// Вызывается в конце касания
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
    /// Вызывается при движении касания
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
