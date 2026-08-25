using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using NUnit.Framework;

public class DisasterManager : MonoBehaviour
{
    public Menu menu;
    private bool GameWon;
    public BuildingSystem buildsystem;
    [Header("Flood Settings")]
    public GameObject FloodTilePrefab;
    public Transform FloodSpawnPoint;

    public float FloodSpeed = 0.2f;
    public int FloodHeight = 5;
    public int MaxFloodTiles = 500;
    [SerializeField] private TextMeshProUGUI uiTextElement;

    [Header("Grid Settings")]
    public float TileSize = 1f;


    [Header("Flood Bounds Object")]
    public Transform FloodBoundsObject;


    [Header("Layers")]
    public LayerMask PlacedTilesLayer;
    public LayerMask PeopleLayer;

    public GameObject TornadoPrefab;

    private List<Collider2D> FloodBounds = new List<Collider2D>();

    private List<Vector2> FloodPositions = new List<Vector2>();
    private Queue<Vector2> FloodQueue = new Queue<Vector2>();

    private List<GameObject> SpawnedFloodTiles = new List<GameObject>();

    private bool FloodActive;
    private bool FloodRetracting;
    public Transform TornadoSpawn;
    private float FloodTimer;
    public float DisasterCooldown;
    [SerializeField] private float NextDisasterTime;
    private int DisasterStage = 0;

    [Header("Meteor")]
    public GameObject MetiorPrefab;
    public GameObject MetiorBounds;

    [Header("Movement")]
    public float RaiseHeight = 5f;
    public float FallSpeed = 10f;

    [Header("Impact")]
    public float HitboxRadius = .5f;
    public GameObject ImpactParticlePrefab;

    [Header("What Can Be Destroyed")]
    public LayerMask DestructibleLayers;

    public GameObject NPC;

    private bool GameOver = false;


    public void CheckNPCs()
    {
        // If there are no NPCs left
        if (NPC.transform.childCount == 0 && !GameOver)
        {
            GameOver = true;
            StartCoroutine(GameOverScreen());
        }
    }

    private System.Collections.IEnumerator GameOverScreen()
    {
        // Create Canvas
        GameObject canvasObject = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Create text
        GameObject textObject = new GameObject("GameOverText");
        textObject.transform.SetParent(canvasObject.transform, false);

        Text gameOverText = textObject.AddComponent<Text>();

        gameOverText.text = "GAME OVER";
        gameOverText.color = Color.red;
        gameOverText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gameOverText.fontSize = 100;
        gameOverText.alignment = TextAnchor.MiddleCenter;

        // Fill the screen
        RectTransform rect = gameOverText.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Flash the text
        while (true)
        {
            gameOverText.enabled = true;
            yield return new WaitForSeconds(0.5f);

            gameOverText.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SpawnMetior()
    {
        CircleCollider2D circle = MetiorBounds.GetComponent<CircleCollider2D>();

        if (circle == null)
        {
            Debug.LogError("MetiorBounds needs a CircleCollider2D!");
            return;
        }

        // Get circle center in world space
        Vector2 center = circle.transform.TransformPoint(circle.offset);

        // Get the world-space radius
        float radius = circle.radius * Mathf.Max(
            circle.transform.lossyScale.x,
            circle.transform.lossyScale.y
        );

        // Random position inside the circle
        Vector2 randomPosition = center + UnityEngine.Random.insideUnitCircle * radius;

        // Spawn meteor
        GameObject Metior = Instantiate(
            MetiorPrefab,
            new Vector3(randomPosition.x, randomPosition.y, MetiorPrefab.transform.position.z),
            Quaternion.identity
        );

        // Start meteor movement
        StartCoroutine(MetiorFall(Metior, randomPosition));
    }

    private System.Collections.IEnumerator MetiorFall(GameObject Metior, Vector2 impactPosition)
    {
        // Raise it up
        Vector3 startPosition = Metior.transform.position;
        Vector3 raisedPosition = startPosition + Vector3.up * RaiseHeight;

        Metior.transform.position = raisedPosition;

        // Let it fall
        while (Metior != null && Metior.transform.position.y > startPosition.y)
        {
            Metior.transform.position = Vector3.MoveTowards(
                Metior.transform.position,
                startPosition,
                FallSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (Metior == null)
            yield break;

        // Make sure it lands exactly at the impact point
        Metior.transform.position = new Vector3(
            impactPosition.x,
            impactPosition.y,
            Metior.transform.position.z
        );

        // Spawn impact particles
        if (ImpactParticlePrefab != null)
        {
            GameObject particles = Instantiate(
                ImpactParticlePrefab,
                impactPosition,
                Quaternion.identity
            );

            particles.GetComponent<ParticleSystem>().Emit(50);

            Destroy(particles, 5f);
        }

        // Detect everything in the hitbox
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(
            impactPosition,
            HitboxRadius
        );

        foreach (Collider2D hit in hitObjects)
        {
            if (hit == null)
                continue;

            if (hit.CompareTag("NPCs") ||
                hit.CompareTag("PlacedTiles") ||
                hit.CompareTag("Walls"))
            {
                Destroy(hit.gameObject);
            }
        }

        // Destroy meteor
        Destroy(Metior);
    }

    private void OnDrawGizmosSelected()
    {
        if (MetiorBounds != null)
        {
            CircleCollider2D circle = MetiorBounds.GetComponent<CircleCollider2D>();

            if (circle != null)
            {
                Vector2 center = circle.transform.TransformPoint(circle.offset);

                float radius = circle.radius * Mathf.Max(
                    circle.transform.lossyScale.x,
                    circle.transform.lossyScale.y
                );

                Gizmos.DrawWireSphere(center, radius);
            }
        }

        // Meteor impact hitbox
        Gizmos.DrawWireSphere(transform.position, HitboxRadius);
    }

    void Start()
    {
        SetupBounds();
        NextDisasterTime += DisasterCooldown;
    }



    void SetupBounds()
    {
        FloodBounds.Clear();


        if (FloodBoundsObject == null)
            return;


        // Gets all colliders inside the empty object and children
        Collider2D[] colliders = FloodBoundsObject.GetComponentsInChildren<Collider2D>();


        foreach (Collider2D col in colliders)
        {
            FloodBounds.Add(col);
        }
    }



    void Update()
    {
        if (menu.GameStart == false)
        {
            return;
        }
        if (GameOver == false || GameWon == false)
        {
            uiTextElement.text = "Time Until Disaster: " + math.round(NextDisasterTime - Time.time);
        }
        else
        {
            return;
        }

        if (NextDisasterTime <= Time.time)
        {
            NextDisasterTime += DisasterCooldown;
            DoDisaster(DisasterStage);
            DisasterStage++;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            SpawnMetior();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flood();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnTornado();
        }
        if (!FloodActive && !FloodRetracting)
            return;


        FloodTimer += Time.deltaTime;


        if (FloodTimer >= FloodSpeed)
        {
            FloodTimer = 0;


            if (FloodActive)
                UpdateFlood();

            else
                RemoveFlood();
        }
        if (Time.time > 1)
        {
            CheckNPCs();
        }

    }

    public void Flood()
    {
        FloodActive = true;
        FloodRetracting = false;


        Vector2 start = SnapToGrid(FloodSpawnPoint.position);


        for (int i = 0; i < FloodHeight; i++)
        {
            Vector2 pos = start + Vector2.up * (i * TileSize);


            if (InsideBounds(pos))
            {
                SpawnFloodTile(pos);

                FloodPositions.Add(pos);
                FloodQueue.Enqueue(pos);
            }
        }
    }



    void UpdateFlood()
    {
        if (FloodQueue.Count == 0 || FloodPositions.Count >= MaxFloodTiles)
        {
            FloodActive = false;
            FloodRetracting = true;
            return;
        }


        Vector2 current = FloodQueue.Dequeue();


        Vector2[] directions =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };


        foreach (Vector2 direction in directions)
        {
            Vector2 next = SnapToGrid(current + direction * TileSize);


            if (CanFlood(next))
            {
                SpawnFloodTile(next);

                FloodPositions.Add(next);
                FloodQueue.Enqueue(next);
            }
        }
    }



    bool CanFlood(Vector2 position)
    {
        if (FloodPositions.Contains(position))
            return false;


        if (!InsideBounds(position))
            return false;



        Collider2D[] hits = Physics2D.OverlapCircleAll(
            position,
            TileSize / 2,
            PlacedTilesLayer | PeopleLayer
        );


        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Wall"))
            {
                return false;
            }
            //Damage
            //hit.GetComponent<WallTile>().DamageWall(50);


            Destroy(hit.gameObject);
        }


        return true;
    }



    void SpawnFloodTile(Vector2 position)
    {
        GameObject water = Instantiate(
            FloodTilePrefab,
            position,
            Quaternion.identity
        );


        SpawnedFloodTiles.Add(water);
    }



    void RemoveFlood()
    {
        if (SpawnedFloodTiles.Count == 0)
        {
            FloodRetracting = false;
            FloodPositions.Clear();
            return;
        }


        int index = SpawnedFloodTiles.Count - 1;


        Destroy(SpawnedFloodTiles[index]);

        SpawnedFloodTiles.RemoveAt(index);


        if (FloodPositions.Count > 0)
            FloodPositions.RemoveAt(FloodPositions.Count - 1);
    }



    bool InsideBounds(Vector2 position)
    {
        // No bounds assigned = unlimited
        if (FloodBounds.Count == 0)
            return true;


        foreach (Collider2D col in FloodBounds)
        {
            if (col.OverlapPoint(position))
            {
                return true;
            }
        }


        return false;
    }
    Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.x / TileSize) * TileSize,
            Mathf.Round(position.y / TileSize) * TileSize
        );
    }

    public void SpawnTornado()
    {
        GameObject NewTornado = Instantiate(TornadoPrefab);
        Vector3 Offset = new Vector3(0, UnityEngine.Random.Range(-2, -2), 0);
        NewTornado.transform.position = TornadoSpawn.position;
    }
    public void DoDisaster(int Level)
    {
        StartCoroutine(DisasterRoutine(Level));
    }

    private System.Collections.IEnumerator DisasterRoutine(int Level)
    {
        // Disable building
        buildsystem.BuildDisabled = true;

        if (Level == 0)
        {
            Flood();
        }
        else if (Level == 1)
        {
            for (int i = 0; i < 20; i++)
            {
                SpawnTornado();
            }
        }
        else if (Level == 2)
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnMetior();
            }
        }
        else if (Level > 2)
        {
            GameWon = true;
            uiTextElement.text = "Game Won!";
        }

        // Keep building disabled while disaster is active
        yield return new WaitForSeconds(10f);

        // Enable building again
        buildsystem.BuildDisabled = false;
    }


}