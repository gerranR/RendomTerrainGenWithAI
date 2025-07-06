using UnityEngine;

public class Health : MonoBehaviour
{
    public int health;
    public Item[] drops;
    public bool isPlayer;
    private Inventory inventory;

    private void Start()
    {
        if (isPlayer)
        {
            inventory = GetComponent<Inventory>();
        } 
    }

    public void takeDamage(int damage)
    {
        if(isPlayer)
        {
            damage -= inventory.getArmor();
            if(damage < 0)
            {
                damage = 0;
            }    
        }
        health -= damage;

        if(health <= 0)
        {
            if (!isPlayer)
            {
                if(inventory == null)
                {
                    inventory = FindAnyObjectByType<Inventory>();
                }
                foreach(Item item in drops)
                {
                    inventory.addItem(item);
                }
            }

            Destroy(gameObject);
        }
    }
}
