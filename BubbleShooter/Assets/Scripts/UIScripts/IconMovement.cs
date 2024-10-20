using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ���������� ��� ������� �� ������.
/// ������ �������� ����� �� ������ � ��������� ����������,
/// ��� �� ��� ������� ������ � ����� ��������� ���������.
/// </summary>
public class IconMovement: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// ���������� ������, ������� ��������� ��� ������� ������
    /// </summary>
    [SerializeField]
    RectTransform _transfrom;
    /// <summary>
    /// ��������� ������
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
