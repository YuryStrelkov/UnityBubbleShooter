using UnityEngine;

/// <summary>
/// ¬спомогательный скрипт, который настраивает расположение коллайдеров по границе диспле€.
/// </summary>
public class PlayFieldBorder : MonoBehaviour
{
    [SerializeField]
    BoxCollider2D[] _topColliders;
    [SerializeField]
    BoxCollider2D[] _downColliders;
    [SerializeField]
    BoxCollider2D[] _leftColliders;
    [SerializeField]
    BoxCollider2D[] _rightColliders;
    private static void SetUpCollider(BoxCollider2D collider, Vector2 offset, Vector2 size) {
        collider.offset = offset;
        collider.size = size;
    }

    private static void SetUpColliders(BoxCollider2D[] colliders, Vector2 offset, Vector2 size)
    {
        if (colliders == null)
            return;
        foreach (BoxCollider2D collider in colliders) SetUpCollider(collider, offset, size);
    }
    public void SetUpColliders() {
        Vector3 worldSpaceRes = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float width  = worldSpaceRes.x * 2.0f;
        float height = worldSpaceRes.y * 2.0f;
        float colliderWidth = 1.0f;
        SetUpColliders(_topColliders,   new Vector2(0,  height * 0.5f + colliderWidth * 0.5f), new Vector2(width, colliderWidth));
        SetUpColliders(_downColliders,  new Vector2(0, -height * 0.5f - colliderWidth * 0.5f), new Vector2(width, colliderWidth));
        SetUpColliders(_leftColliders,  new Vector2(-width * 0.5f - colliderWidth * 0.5f, 0), new Vector2(colliderWidth, height));
        SetUpColliders(_rightColliders, new Vector2( width * 0.5f + colliderWidth * 0.5f, 0), new Vector2(colliderWidth, height));
    }
    void Start() => SetUpColliders();
}
