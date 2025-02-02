using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IRangedWeapon
{
    float Damage { get; }
    float Range { get; }
    void Equip();
    void Fire();
    void Reload();
    void Unequip(); // Permet de déséquiper l'arme
}
