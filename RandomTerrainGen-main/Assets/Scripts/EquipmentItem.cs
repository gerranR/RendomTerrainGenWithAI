using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentItem", menuName = "ScriptableObjects/EquipmentItem", order = 3)]
public class EquipmentItem : Item
{
    public bool isArmor;
    public int armor;

    [Space(20)]

    public bool isWeapon;
    public int weaponDamage;
}
