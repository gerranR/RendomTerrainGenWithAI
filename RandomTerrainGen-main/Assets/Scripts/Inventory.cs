using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<Item> items = new List<Item>();

    public EquipmentItem weapon;
    public EquipmentItem armor;

    public GameObject inventoryUI;
    public GameObject itemPrefabUI;
    public Transform contentPanel;

    public TextMeshProUGUI weaponText;
    public TextMeshProUGUI armorText;

    private FirstPersonControler personControler;

    private void Start()
    {
        personControler = GetComponent<FirstPersonControler>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            if (!inventoryUI.activeSelf)
            {
                OpenInv();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                personControler.canMove = false;
            }
            else
            {
                clearInv();
                inventoryUI.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                personControler.canMove = true;
            }
        }
    }

    public int getArmor()
    {
        int armorNum = 0;

        if(armor != null)
        {
            armorNum = armor.armor;
        }

        return armorNum;
    }

    public int getWeaponDamage()
    {
        int Damage = 0;

        if (weapon != null)
        {
            Damage = weapon.weaponDamage;
        }

        return Damage;
    }

    public void Equip(EquipmentItem item)
    {
        if(item.isArmor)
        {
            EquipArmor(item);
        }
        else if(item.isWeapon)
        {
            EquipWeapon(item);
        }
    }

    public void EquipArmor(EquipmentItem item)
    {
        if(item.isArmor)
        {
            armor = item;
            armorText.text = armor.getName();
        }
    }
    public void EquipWeapon(EquipmentItem item)
    {
        if (item.isWeapon)
        {
            weapon = item;
            weaponText.text = weapon.getName();
        }
    }


    public void addItem(Item item)
    {
        items.Add(item);
    }

    public void removeItem(Item item)
    {
        if(items.Contains(item))
        {
            items.Remove(item);
        }
    }

    public bool hasItem(Item item)
    {
        return items.Contains(item);
    }
    
    public void OpenInv()
    {
        inventoryUI.SetActive(true);
        PopulateInventory();
    }


    public void clearInv()
    {
        for (int i = contentPanel.childCount; i > 0; i--)
        {
            Destroy(contentPanel.GetChild(i - 1).gameObject);
        }
    }
    public void PopulateInventory()
    {
        foreach (Item item in items)
        {
            GameObject newItem = Instantiate(itemPrefabUI, contentPanel);

            newItem.GetComponentInChildren<TextMeshProUGUI>().text = item.getName();
            if (item is EquipmentItem)
            { 
                Button btn = newItem.GetComponent<Button>();
                btn.onClick.AddListener(() => Equip((EquipmentItem)items[items.IndexOf(item)]));
            }

        }
        if (weapon != null)
            weaponText.text = weapon.getName();
        if(armor != null)
            armorText.text = armor.getName();
    }
}
