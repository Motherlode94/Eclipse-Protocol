using UnityEngine;

public interface IEnemy
{
    Transform GetTransform(); // Récupère la position de l'entité
    void TakeDamage(float damage); // Permet de recevoir des dégâts
    void AttackPlayer(); // Attaque un joueur
    void BecomeAggressive(); // Devient agressif envers une cible

    float GetThreatLevel(); // Retourne le niveau de menace
}

