using UnityEngine;

public class WallTile : MonoBehaviour
{
    [SerializeField] private float Health = 100;
    private TileClass TileSystem;

    // Update is called once per frame
    public void Setup()
    {
        TileSystem = GetComponent<TileClass>();
        TileSystem.SwitchToSprite(UnityEngine.Random.Range(0, 4));
    }
    public void DamageWall(float Amount)
    {
        Health -= Amount;
        if (Health < 1)
        {
            Destroy(gameObject);
        }
    }
}
