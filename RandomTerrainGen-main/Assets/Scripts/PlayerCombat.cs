using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public int damage;
    public float attackRange;
    public GameObject cam;

    private Inventory inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }

    public void Attack()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Animal"))
            {
                Health enemy = hit.collider.GetComponent<Health>();
                if (enemy != null)
                {
                    enemy.takeDamage(damage + inventory.getWeaponDamage());
                }
            }
            else if (hit.collider.CompareTag("Food"))
            {
                Drops enemy = hit.collider.GetComponent<Drops>();
                if (enemy != null)
                {
                    enemy.getItems(inventory);
                }
            }
        }
    }
}
