using UnityEngine;

public class Knife : MonoBehaviour, IMeleeWeapon
{
    public float Damage { get; private set; } = 50f;
    public float AttackSpeed { get; private set; } = 1.2f;

    public void Equip()
    {
        Debug.Log("Épée équipée.");
    }

    public void Attack()
    {
        Debug.Log("Coup d'épée infligé.");
    }

    public void Unequip()
    {
        Debug.Log("Épée déséquipée.");
    }
}
