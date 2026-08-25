using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class NPCData : MonoBehaviour
{
    public Vector2 LastPos;
    public float NextMoveTime;
    public float IdleTime;
    public float MoveSpeed;
    public float IdleDuration;
    public GameObject Sprite;
    public Animator animator;
    public Rigidbody2D rb;
    [SerializeField] private float lockedZAngle = 0f;
    public float WalkSpeed;
    public float globalSpeed;
    public float MaxSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = Sprite.GetComponent<Animator>();
        NextMoveTime = UnityEngine.Random.Range(0f, 5f);

    }
    void LateUpdate()
    {
        Sprite.transform.rotation = Quaternion.Euler(0, 0, lockedZAngle);
    }
}
