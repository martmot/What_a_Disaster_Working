using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] private BuildingSlots BuildSlotData;
    [SerializeField] private GameObject PrefabCurrentBuildingObj;
    [SerializeField] private GameObject CurrentBuildingObj;
    [SerializeField] private float CurrentSlotIndex = 1;
    [SerializeField] private float TileSpacing = 1;
    [SerializeField] private float MaxBuildX;
    [SerializeField] private float MaxBuildY;
    [SerializeField] private float MinBuildX;
    [SerializeField] private float MinBuildY;
    private MoneySystem moneySystem;
    [SerializeField] private GameObject TilesEmpty;
    private Vector2 targetPosition;
    public LayerMask targetLayer;
    public bool InBuildMode = false;
    public bool BuildDisabled;
    public Menu menu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moneySystem = GetComponent<MoneySystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (menu.GameStart == false)
        {
            return;
        }
        if (BuildDisabled == true)
        {
            return;
        }
        if (InBuildMode)
        {
            BuildingTick();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {

            InBuildMode = !InBuildMode; // toggles the building mode
            if (InBuildMode == true)
            {
                PrefabCurrentBuildingObj = BuildSlotData.Slots[1];
                Destroy(CurrentBuildingObj);
                CurrentBuildingObj = Instantiate(PrefabCurrentBuildingObj);

                //turning it translucent
                SpriteRenderer spriteRenderer = CurrentBuildingObj.GetComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 5; //making it render on top
                Color spriteColor = spriteRenderer.color;
                spriteColor.a = 0.5f;
                spriteRenderer.color = spriteColor;
            }
            if (InBuildMode == false)
            {
                DestroyImmediate(CurrentBuildingObj, true);

            }
        }


    }

    void BuildingTick() //loops when in building mode
    {

        //chaning Slots

        float scrollInput = Input.mouseScrollDelta.y;
        if (scrollInput != 0f)
        {
            CurrentSlotIndex -= (int)Mathf.Sign(scrollInput); //sign does -1 if negative and +1 if positive
            CurrentSlotIndex = (int)math.clamp(CurrentSlotIndex, 1, BuildSlotData.Slots.Length);
        }
        // 1. Check if the main data holder exists
        if (BuildSlotData == null)
        {
            Debug.LogError("BuildSlotData is not assigned!");
            return;
        }

        // 2. Check if the array itself exists
        if (BuildSlotData.Slots == null)
        {
            Debug.LogError("Slots array is not initialized!");
            return;
        }

        // 3. Check if the index is within valid bounds
        int targetIndex = (int)CurrentSlotIndex - 1;
        if (targetIndex >= 0 && targetIndex < BuildSlotData.Slots.Length)
        {
            // 4. Check if the specific slot item is not null
            if (BuildSlotData.Slots[targetIndex] != null)
            {
                if (PrefabCurrentBuildingObj != BuildSlotData.Slots[targetIndex])
                {
                    PrefabCurrentBuildingObj = BuildSlotData.Slots[targetIndex];
                    Destroy(CurrentBuildingObj);
                    CurrentBuildingObj = Instantiate(PrefabCurrentBuildingObj);

                    //turning it translucent
                    SpriteRenderer spriteRenderer = CurrentBuildingObj.GetComponent<SpriteRenderer>();
                    spriteRenderer.sortingOrder = 5; //making it render on top
                    Color spriteColor = spriteRenderer.color;
                    spriteColor.a = 0.5f;
                    spriteRenderer.color = spriteColor;

                }

            }
            else
            {
                Debug.LogWarning("Slot at index " + targetIndex + " is null.");
            }
        }
        else
        {
            Debug.LogWarning("CurrentSlotIndex is out of array bounds: " + targetIndex);
        }


        //BuildPos
        // The total distance to jump from one tile center to the next
        float gridStep = TileSpacing;

        // 1. Get the raw world position from the mouse
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. Clamp the RAW position first to keep it inside your build area limits
        float clampedX = Mathf.Clamp(worldPosition.x, MinBuildX, MaxBuildX);
        float clampedY = Mathf.Clamp(worldPosition.y, MinBuildY, MaxBuildY);

        // 3. Snap the clamped coordinates to your custom grid step
        float snappedX = Mathf.Round(clampedX / gridStep) * gridStep;
        float snappedY = Mathf.Round(clampedY / gridStep) * gridStep;

        // 4. Final tile position
        Vector2 TilePos = new Vector2(snappedX, snappedY);
        targetPosition = TilePos;


        if (CurrentBuildingObj != null)
        {
            CurrentBuildingObj.transform.position = TilePos;

            //Placing

            if (Input.GetMouseButtonDown(0) && CanPlace() && TileCheck() == true)
            {
                GameObject PlacedBuildObj = Instantiate(PrefabCurrentBuildingObj);
                PlacedBuildObj.transform.SetParent(TilesEmpty.transform);
                PlacedBuildObj.GetComponent<SpriteRenderer>().sortingOrder = 1;
                PlacedBuildObj.transform.position = TilePos;
                int PlacedLayer = LayerMask.NameToLayer("PlacedTiles");
                PlacedBuildObj.layer = PlacedLayer;
            }
            if (Input.GetMouseButtonDown(1))
            {
                DestroyTile();
            }
        }


    }
    //money check and stuff
    bool CanPlace()
    {
        if (moneySystem.IsBroke())
        {
            return false;
        }
        return true;
    }
    bool TileCheck() //returns false if theres an object
    {
        // Returns the first Collider2D found at the position
        Collider2D hitCollider = Physics2D.OverlapPoint(targetPosition, targetLayer);

        if (hitCollider != null)
        {
            GameObject detectedObject = hitCollider.gameObject;
            Debug.Log("Found object: " + detectedObject.name + " at position " + targetPosition);
            return false;
        }
        return true;
    }
    void DestroyTile()
    {
        // Returns the first Collider2D found at the position
        Collider2D hitCollider = Physics2D.OverlapPoint(targetPosition, targetLayer);

        if (hitCollider != null)
        {
            GameObject detectedObject = hitCollider.gameObject;
            Debug.Log("Found object: " + detectedObject.name + " at position " + targetPosition);
            TileClass ObjectTileClass = detectedObject.GetComponent<TileClass>();
            moneySystem.AddMoney(ObjectTileClass.Cost * 0.5f);
            Destroy(detectedObject);
        }
    }
}