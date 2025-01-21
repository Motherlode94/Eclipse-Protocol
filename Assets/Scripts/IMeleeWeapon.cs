using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface pour les armes de mêlée.
/// </summary>
public interface IMeleeWeapon
{
    void Attack(); // Effectuer une attaque de mêlée
}

/// <summary>
/// Interface pour les armes à distance.
/// </summary>
public interface IRangedWeapon
{
    void Fire(); // Effectuer un tir
}
