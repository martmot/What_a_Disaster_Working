using System;
using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    public GameObject NPCs;
    public LayerMask targetLayer;
    public GameObject[] NPCPrefabs;
    public Menu menu;

    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            MakeNewNPC();
        }
    }

    void FixedUpdate()
    {
        if (menu.GameStart == false)
        {
            return;
        }
        if (Time.time < 1)
        {
            return;
        }
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            if ((targetLayer.value & (1 << go.layer)) != 0)
            {
                NPCTick(go);
            }
        }
    }

    void NPCTick(GameObject NPC)
    {
        NPCData NPCdata = NPC.GetComponentInChildren<NPCData>();
        if (NPCdata.animator == null)
        {
            NPCdata.animator = NPCdata.GetComponent<Animator>();
        }
        Rigidbody2D npcRb = NPC.GetComponent<Rigidbody2D>();

        if (npcRb == null || NPCdata == null) return;

        if (Time.time >= NPCdata.NextMoveTime)
        {
            NPCdata.IdleTime += Time.fixedDeltaTime;

            if (NPCdata.IdleTime >= NPCdata.IdleDuration)
            {
                Vector2 randomPoint = new Vector2(
                    UnityEngine.Random.Range(-7f, -1f),
                    UnityEngine.Random.Range(-3.5f, 3f)
                );

                bool outOfBounds =
                    NPC.transform.position.x < -7f ||
                    NPC.transform.position.x > -1f ||
                    NPC.transform.position.y < -3.5f ||
                    NPC.transform.position.y > 3f;

                if (outOfBounds)
                {
                    randomPoint = new Vector2(-4f, 0f);
                }

                Vector2 rotationVector = randomPoint - (Vector2)NPC.transform.position;

                float direction = Mathf.Atan2(
                    rotationVector.y,
                    rotationVector.x
                ) * Mathf.Rad2Deg;


                npcRb.SetRotation(direction);

                // Stop old momentum when changing direction
                npcRb.linearVelocity = Vector2.zero;


                NPCdata.IdleDuration = UnityEngine.Random.Range(0.5f, 1.5f);

                // Prevent crazy speed spikes
                NPCdata.MoveSpeed = UnityEngine.Random.Range(2f, NPCdata.MaxSpeed);

                NPCdata.NextMoveTime = Time.time + UnityEngine.Random.Range(0.6f, 1.4f);

                NPCdata.IdleTime = 0f;

                NPCdata.animator.SetBool("Walking", true);
            }

            NPCdata.animator.SetBool("Walking", false);
        }
        else
        {
            // Physics-based movement
            Vector2 movement = NPC.transform.right * NPCdata.MoveSpeed;

            npcRb.linearVelocity = movement;

            NPCdata.animator.SetBool("Walking", true);
        }


        // Hard speed cap
        if (npcRb.linearVelocity.magnitude > NPCdata.MaxSpeed)
        {
            npcRb.linearVelocity =
                npcRb.linearVelocity.normalized * NPCdata.MaxSpeed;
        }
    }


    void MakeNewNPC()
    {
        int randomIndex = UnityEngine.Random.Range(0, NPCPrefabs.Length);

        GameObject selectedObject = NPCPrefabs[randomIndex];

        GameObject NewNPC = Instantiate(selectedObject);

        NewNPC.transform.SetParent(NPCs.transform);


        Collider2D collider = NewNPC.GetComponent<Collider2D>();

        if (collider != null)
        {
            collider.enabled = true;
        }


        Vector2 jitter = UnityEngine.Random.insideUnitCircle * 0.5f;


        NewNPC.transform.position = new Vector3(
            UnityEngine.Random.Range(-7, -1) + jitter.x,
            UnityEngine.Random.Range(-3.5f, 3) + jitter.y,
            0
        );
    }
}