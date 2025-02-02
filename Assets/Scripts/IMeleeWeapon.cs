using UnityEngine;

/// <summary>
/// Interface pour les armes de mêlée.
/// </summary>
public interface IMeleeWeapon
{
    float Damage { get; }       // Dégâts infligés par l'arme
    float AttackSpeed { get; }  // Vitesse d'attaque (par exemple, coups par seconde)
    void Attack();              // Méthode pour effectuer une attaque
    void Equip();               // Méthode pour équiper l'arme
    void Unequip();             // Méthode pour déséquiper l'arme
}
