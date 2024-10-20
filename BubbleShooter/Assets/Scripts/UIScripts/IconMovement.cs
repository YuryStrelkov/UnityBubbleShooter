using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Вызывается при нажатии на кнопку.
/// Слегка смещаент лейбл на кнопке в указанном напралении,
/// что бы при нажатии кнопка и лейбл двигались совместно.
/// </summary>
public class IconMovement: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// Трансофрма лейбла, который двигается при нажатии кнопки
    /// </summary>
    [SerializeField]
    RectTransform _transfrom;
    /// <summary>
    /// Параметры сдвига
    /// </summary>
    [SerializeField]
    Vector3 _delta = new Vector3(0, -10, 0);
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_transfrom == null) return;
        _transfrom.localPosition += _delta;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_transfrom == null) return;
        _transfrom.localPosition -= _delta;
    }

   
}
