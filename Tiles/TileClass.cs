using UnityEngine;

public class TileClass : MonoBehaviour
{
    public enum TileType
    {
        Normal,
        Farm,
        Wall,
        House
    }

    [SerializeField] private TileType type;
    [SerializeField] private Sprite[] spriteSheet;
    private SpriteRenderer spriteRenderer;
    [SerializeField] public float Cost;
    public GameObject BSystem;
    private MoneySystem moneySystem;

    private FarmTile farmTile;
    private WallTile wallTile;

    void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        switch (type)
        {
            case TileType.Farm:
                farmTile = gameObject.AddComponent<FarmTile>();
                farmTile.Setup();
                break;

            case TileType.Wall:
                wallTile = gameObject.AddComponent<WallTile>();
                wallTile.Setup();
                break;

            case TileType.House:
                // Add house behavior here
                break;
        }
    }
    public void SwitchToSprite(int index)
    {
        if (index >= 0 && index < spriteSheet.Length)
        {
            this.spriteRenderer.sprite = spriteSheet[index];
        }
        else
        {
            Debug.LogWarning("Sprite index out of bounds!");
        }
    }

    void Start()
    {
        if (this.gameObject.layer == LayerMask.NameToLayer("PlacedTiles"))
        {
            BSystem = GameObject.Find("BuildingSystem");
            moneySystem = BSystem.GetComponent<MoneySystem>();
            if (moneySystem != null)
            {
                moneySystem.SubtractMoney(Cost);
            }
        }
        return;
    }
}