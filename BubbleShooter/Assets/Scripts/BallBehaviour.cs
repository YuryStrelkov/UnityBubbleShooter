using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ѕроста€ анимаци€ через корутины.
/// ѕросто мен€ет маштаб шарика и отправлет его в кеш в конце.
/// </summary>
[System.Serializable]
public struct BallCollapseAnimation
{
    /// </summary>
    /// ‘лаг, чтобы не запустить корутину дважды
    /// </summary>
    private bool _running;
    
    /// <summary>
    /// јнимационна€ крива€
    /// </summary>
    [SerializeField]
    AnimationCurve _curve;
    /// <summary>
    /// ¬рем€ анимации
    /// </summary>
    [SerializeField]
    float _duration;
    /// <summary>
    /// јнимаци€ активна или нет
    /// </summary>
    public bool IsRunning => _running;
   
    public IEnumerator PlayAnimation(BallBehaviour ball){
        _running = true;
        Vector3 scale = ball.transform.lossyScale;
        float factor = 0.0f;
        while (factor < _duration) {
            ball.transform.localScale = scale * _curve.Evaluate(factor / _duration);
            factor += Time.deltaTime;
            yield return null;
        }
        ball.transform.localScale = scale;
        BallBehaviour.DestroyBall(ball);
        _running = false;
    }
}

/// <summary>
/// ѕеречисление типов м€чиков
/// </summary>
public enum BallType { 
    Target,
    Projectile
}

/// <summary>
/// ѕеречисление типов цвета м€чиков
/// </summary>
public enum BallColor
{
    Red,
    Green,
    Blue
}

/// <summary>
/// ќсновной скрипт, описывающий поведение м€чика в игровом поле
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BallBehaviour : MonoBehaviour
{
    /// <summary>
    ///  еш м€чиков по цветам
    /// </summary>
    private static Dictionary<BallColor, Stack<BallBehaviour>> _ballsCashe = new Dictionary<BallColor, Stack<BallBehaviour>>();
    /// <summary>
    /// —писок всех созданных м€чиков
    /// </summary>
    private static List<BallBehaviour> _instancedBalls = new List<BallBehaviour>();

    /// <summary>
    /// Id - сло€ деспавнера м€чиков (нужно дл€ уничтожени€ упавших вниз)
    /// </summary>
    private static int BallsDespawnerLayer;
    /// <summary>
    /// Id - сло€ м€чиков. 
    /// </summary>
    private static int BallsLayer;
    /// <summary>
    /// ¬озвращает м€чик из кеша с указанным цветом. ≈сли м€чика нет, то null
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private static BallBehaviour GetBallFromCashe(BallColor color)
    {
        if (!_ballsCashe.ContainsKey(color))
            return null;
        if (_ballsCashe[color].Count == 0)
        {
            _ballsCashe.Remove(color);
            return null;
        }
        return _ballsCashe[color].Pop();
    }
    /// <summary>
    /// ќтправл€ет м€чик в кеш, распредел€€ его по цвету.
    /// </summary>
    private static void StoreBallInCashe(BallBehaviour ball)
    {
        if (!_ballsCashe.ContainsKey(ball.ballColor))
            _ballsCashe.Add(ball.ballColor, new Stack<BallBehaviour>());
        _ballsCashe[ball.ballColor].Push(ball);

    }
    /// <summary>
    /// "”ничтожает"м€чик.
    /// Ќа самом деле отправлет в кеш, выключает физику и отображение.
    /// </summary>
    /// <param name="ball"></param>
    public static void DestroyBall(BallBehaviour ball)
    {
        StoreBallInCashe(ball);
        ball.ballRigidbody.simulated = false;
        ball.ballRigidbody.velocity = Vector2.zero;
        ball.gameObject.SetActive(false);
    }
    /// <summary>
    /// Ќастраивает отдельную пружинку.
    /// </summary>
    /// <param name="joint"></param>
    /// <param name="anchorDelta"></param>
    /// <returns></returns>
    private static SpringJoint2D SetupSpringJoint(SpringJoint2D joint, Vector3 anchorDelta)
    {
        joint.autoConfigureDistance = false;
        joint.dampingRatio = 0.125f;
        joint.distance = anchorDelta.magnitude;
        joint.frequency = 1;
        joint.connectedAnchor = joint.gameObject.transform.position + anchorDelta;
        return joint;
    }
    private static SpringJoint2D CreateSpringJoint(GameObject gameObject, Vector3 anchorDelta)
    {
        SpringJoint2D joint = gameObject.AddComponent<SpringJoint2D>();
        return SetupSpringJoint(joint, anchorDelta);
    }
    private static void CreateBallSpringJoints(BallBehaviour ball)
    {
        if (ball.ballJoints != null)
        {
            SetupSpringJoint(ball.ballJoints[0], Vector3.up * 0.5f);
            SetupSpringJoint(ball.ballJoints[1], Vector3.down * 0.5f);
            SetupSpringJoint(ball.ballJoints[2], Vector3.right * 0.5f);
            SetupSpringJoint(ball.ballJoints[3], Vector3.left * 0.5f);
            return;
        }
        ball._ballJoints = new SpringJoint2D[] {
            CreateSpringJoint(ball.gameObject, Vector3.up    * 0.5f),
            CreateSpringJoint(ball.gameObject, Vector3.down  * 0.5f),
            CreateSpringJoint(ball.gameObject, Vector3.right * 0.5f),
            CreateSpringJoint(ball.gameObject, Vector3.left  * 0.5f)
        };
    }
    /// <summary>
    /// —оздаЄт м€чик указаного цвета
    /// </summary>
    /// <param name="spawnPosition">координата, где создаЄм</param>
    /// <param name="ballColor">цвет, с которым создаЄм</param>
    /// <returns></returns>
    private static BallBehaviour SpawnBall(Vector3 spawnPosition, BallColor ballColor)
    {
        BallBehaviour ball;
        if (ball = GetBallFromCashe(ballColor))
        {
            ball.gameObject.transform.position = spawnPosition;
            ball.gameObject.SetActive(true);
            ball.ballRigidbody.simulated = true;
            return ball;
        }
        GameObject ballObject;
        switch (ballColor)
        {
            case BallColor.Red: ballObject = Instantiate(Resources.Load<GameObject>("RedBall"), spawnPosition, Quaternion.identity); break;
            case BallColor.Green: ballObject = Instantiate(Resources.Load<GameObject>("GreenBall"), spawnPosition, Quaternion.identity); break;
            case BallColor.Blue: ballObject = Instantiate(Resources.Load<GameObject>("BlueBall"), spawnPosition, Quaternion.identity); break;
            default: ballObject = Instantiate(Resources.Load<GameObject>("RedBall"), spawnPosition, Quaternion.identity); break;
        }
        ball = ballObject?.GetComponent<BallBehaviour>();
        _instancedBalls.Add(ball);
        return ball;
    }
    /// <summary>
    /// —оздаЄт м€чик указаного цвета. »спользуетс€ дл€ загрузки из файла.
    /// </summary>
    /// <param name="spawnPosition">координата, где создаЄм</param>
    /// <param name="ballType">тип шарика (цвет) </param>
    /// <param name="spawnedBallsContainer">контейнер в котором шарики будут хранитьс€</param>
    /// <returns></returns>
    public static BallBehaviour SpawnBall(Vector3 spawnPosition, string ballType, GameObject spawnedBallsContainer = null)
    {
        BallBehaviour ball;
        switch (ballType)
        {
            case "r": ball = SpawnBall(spawnPosition, BallColor.Red); break;
            case "g": ball = SpawnBall(spawnPosition, BallColor.Green); break;
            case "b": ball = SpawnBall(spawnPosition, BallColor.Blue); break;
            default: ball  = SpawnBall(spawnPosition, BallColor.Red); break;
        }
        if (ball)
        {
            CreateBallSpringJoints(ball);
            ball.SetSpringJointsEnable(true);
            ball.ballType = BallType.Target;
            if (spawnedBallsContainer)
                ball.transform.parent = spawnedBallsContainer.transform;
            return ball;
        }
        return null;
    }
    /// <summary>
    /// —оздаЄт шарик, которым стрел€ем (шарики выбираютс€ случайно)
    /// </summary>
    /// <param name="spawnPosition">цвет шарика</param>
    /// <param name="spawnedBallsContainer">контейнер в котором шарики будут хранитьс€</param>
    /// <returns></returns>
    public static BallBehaviour SpawnProjectileBall(Vector3 spawnPosition, GameObject spawnedBallsContainer = null)
    {
        BallBehaviour ball = null;
        switch (Random.Range(0, 3))
        {
            case 0: ball = SpawnBall(spawnPosition, BallColor.Red); break;
            case 1: ball = SpawnBall(spawnPosition, BallColor.Green); break;
            case 2: ball = SpawnBall(spawnPosition, BallColor.Blue); break;
        };
        if (ball) {
            ball.ballType = BallType.Projectile;
            ball.SetSpringJointsEnable(false);
            if (spawnedBallsContainer)
                ball.transform.parent = spawnedBallsContainer.transform;
            return ball;
        }
        return null;
    }
    
    /// <summary>
    /// ‘ункци€ вызвывает анимации у всех шариков. Ќужна в завершении игрового цикла.
    /// </summary>
    public static void CollapseAllBalls() { 
        foreach(BallBehaviour ball in _instancedBalls) ball.CollapseBall();
    }
    /// <summary>
    /// ¬ключает / выключает пружины у шариков.
    /// </summary>
    /// <param name="value"></param>
    private void SetSpringJointsEnable(bool value) {
        if (_ballJoints == null)
            return;
        foreach (var joint in _ballJoints) {
            joint.enabled = value;
        }
    }

    [SerializeField]
    private BallCollapseAnimation _ballCollapceAnimation;

    [SerializeField]
    private BallColor _ballColor;
    
    [SerializeField]
    public BallType ballType = BallType.Target;
    
    private SpringJoint2D[] _ballJoints = null;
    
    private CircleCollider2D _ballCollider;
    
    private Rigidbody2D _ballRigidBody;
    public SpringJoint2D[] ballJoints => _ballJoints;
    public Rigidbody2D ballRigidbody => _ballRigidBody;
    public CircleCollider2D ballCollider => _ballCollider;
    public BallColor ballColor => _ballColor;
    void Start()
    {
        _ballCollider = GetComponent<CircleCollider2D>();
        _ballRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Awake() {
        BallsDespawnerLayer = LayerMask.NameToLayer("BallsDespawner");
        BallsLayer = LayerMask.NameToLayer("BallsLayer");
    }

    public void CollapseBall() {
        if (!gameObject.activeSelf) return;
        if (_ballCollapceAnimation.IsRunning) return;
        StartCoroutine(_ballCollapceAnimation.PlayAnimation(this));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != BallsDespawnerLayer)
            return;
        CollapseBall();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != BallsLayer)
            return;
        BallBehaviour bs;
        if (bs = collision.gameObject.GetComponent<BallBehaviour>())
            ResolveContact(bs);
    }
    /// <summary>
    /// ¬зрывает шарики одного цвета р€дом с шариком, которым стрел€ли
    /// </summary>
    /// <param name="target"></param>
    /// <param name="distance"></param>
    private static void CollapseBallsAround(BallBehaviour target, float distance = 0.50f) {
        foreach (BallBehaviour ball in _instancedBalls)
        {
            if (ball.ballColor != target.ballColor)
                continue;
            if((ball.transform.position - target.transform.position).magnitude > distance)
                continue;
            ball.CollapseBall();
        }
    } 

    /// <summary>
    /// –ешает что нам делать при столкновении двух шариков
    /// </summary>
    /// <param name="other"></param>
    private void ResolveContact(BallBehaviour other) {
        if (other.ballType == ballType) 
            return;
        float velosity = (ballRigidbody.velocity + other.ballRigidbody.velocity).magnitude;
        if (velosity > 2.50f)
        {
            CollapseBall();
            other.CollapseBall();
            ScoreCounter.Instance.AddScore(2);
        }
        else
        {
            if (other.ballType == BallType.Projectile)
            {
                other.ballType = ballType;
                CreateBallSpringJoints(other);
                other.SetSpringJointsEnable(true);
                if (ballColor == other.ballColor)
                    CollapseBallsAround(other);
            }
        }
    }

}
