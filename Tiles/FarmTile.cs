using UnityEngine;

public class FarmTile : MonoBehaviour
{
    [SerializeField] private int GrowthStage;
    [SerializeField] private float NextGrowthTime;
    [SerializeField] private float NextMoneyTime;

    [SerializeField] private float MoneyCooldown = 5;
    [SerializeField] private float MoneyAmount = 10;
    [SerializeField] private float GrowthSpeed = 20;
    [SerializeField] private TileClass TileSystem;

    private MoneySystem moneySystem;

    public void Setup()
    {
        TileSystem = GetComponent<TileClass>();
        GrowthStage = 0;
        TileSystem.SwitchToSprite(GrowthStage);
        moneySystem = FindFirstObjectByType<MoneySystem>();

        NextMoneyTime = Time.time + MoneyCooldown;
        NextGrowthTime = Time.time + GrowthSpeed;
    }

    void Update()
    {
        if (Time.time >= NextMoneyTime && gameObject.layer != LayerMask.NameToLayer("PlacedTiles"))
        {
            return;
        }
        if (Time.time >= NextMoneyTime)
        {
            NextMoneyTime = Time.time + MoneyCooldown;
            moneySystem.AddMoney(MoneyAmount);
        }

        if (Time.time >= NextGrowthTime && GrowthStage < 4)
        {
            NextGrowthTime = Time.time + GrowthSpeed;
            MoneyAmount *= 1.5f;
            GrowthStage++;
            TileSystem.SwitchToSprite(GrowthStage);
        }
    }
}