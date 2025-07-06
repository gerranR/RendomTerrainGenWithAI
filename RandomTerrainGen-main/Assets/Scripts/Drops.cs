using UnityEngine;

public class Drops : MonoBehaviour
{
    public Item[] drops;

    public void getItems(Inventory inventory)
    {
        foreach (Item item in drops)
        {
            inventory.addItem(item);
        }

        Destroy(gameObject);
    }
}
