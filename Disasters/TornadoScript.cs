using UnityEngine;
using UnityEngine.AI;

public class TornadoScript : MonoBehaviour
{
    public float tornadoSpeed = 5f;
    private Rigidbody2D rb;
    private float EndingTime;
    public float LifeSpan;

    void Start()
    {
        EndingTime = Time.time + LifeSpan;
        //initialize rigidbody
        rb = GetComponent<Rigidbody2D>();
        //pick a random direction
        float startingDirection = Random.Range(-45, 45);
        //turn direction into vector
        Vector2 movementDirection = new Vector2(Mathf.Cos(startingDirection), Mathf.Sin(startingDirection));
        //apply velocity
        rb.linearVelocity = movementDirection * tornadoSpeed * Time.deltaTime;

    }
    void Update()
    {

        if (Time.time >= EndingTime)
        {
            Destroy(gameObject);
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Hit detected! Apply damage logic here.
            collision.gameObject.GetComponent<WallTile>().DamageWall(100);
        }

        if (collision.gameObject.CompareTag("PlacedTiles"))
        {
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("NPCs"))
        {
            Destroy(collision.gameObject);
        }
    }
}
