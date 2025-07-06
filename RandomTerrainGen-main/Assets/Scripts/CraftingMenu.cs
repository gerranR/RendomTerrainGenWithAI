using UnityEngine;

public class CraftingMenu : MonoBehaviour
{
    [SerializeField] private craftingRecipy[] recipies;
    [SerializeField] private Inventory inventory;

    private void Start()
    {
        inventory = FindAnyObjectByType<Inventory>();
    }

    public void makeRecepy(int number)
    {
        if(recipies.Length > number)
        {
            recipies[number].make(inventory);
        }
    }

    
}

[CreateAssetMenu(fileName = "craftingRecipy", menuName = "ScriptableObjects/craftingRecipy", order = 4)]
public class craftingRecipy : ScriptableObject
{
    [SerializeField] Item Result;
    [SerializeField] Item[] cost;

    public bool canMake(Inventory inventory)
    {
        bool result = true;

        foreach (Item item in cost)
        {
            if(!inventory.hasItem(item))
            {
                return false;
            }
        }
        Debug.Log(result);
        return result;
    }

    public void make(Inventory inventory)
    {
        Debug.Log("test1");
        if(canMake(inventory))
        {
            foreach (Item item in cost)
            {
                inventory.removeItem(item);
            }

            inventory.addItem(Result);
            inventory.clearInv();
            inventory.PopulateInventory();
        }
    }

}
