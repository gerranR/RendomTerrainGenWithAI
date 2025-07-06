using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    public Item[] drops;
    public bool isPlayer;
    private Inventory inventory;

    [SerializeField] Slider healthBar;

    private void Start()
    {
        if (isPlayer)
        {
            inventory = GetComponent<Inventory>();
        } 

        if(healthBar != null)
        {
            healthBar.maxValue = health;
            healthBar.value = health;
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

        if (healthBar != null)
        {
            healthBar.value = health;
        }
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
