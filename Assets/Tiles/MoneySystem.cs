using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    [SerializeField] private float Money = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddMoney(float Amount)
    {
        //add safety checks here
        if (Amount <= 0)
        {
            return;
        }
        Money += Amount;
    }
    public bool SubtractMoney(float Amount)
    {
        //add safety checks here
        if (Amount <= 0)
        {
            return false;
        }
        Money -= Amount;
        return true;
    }
    public bool IsBroke()
    {
        if (Money <= 0)
        {
            return true;
        }
        return false;
    }
}
