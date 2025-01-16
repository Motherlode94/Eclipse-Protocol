using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostItem : MonoBehaviour
{
    public enum BoostType { Speed, Invisibility, Damage, StaminaRegen }
    public BoostType boostType;
    public float boostDuration = 5f; // Durée du boost en secondes
    public float boostValue = 1.5f; // Multiplicateur ou valeur ajoutée (ex : 50% de vitesse supplémentaire)

    public void ApplyBoost(PlayerStats playerStats, PlayerController playerController)
    {
        switch (boostType)
        {
            case BoostType.Speed:
                playerController.sprintSpeed *= boostValue;
                break;
            case BoostType.Invisibility:
                playerController.gameObject.layer = LayerMask.NameToLayer("Invisible"); // Exemple
                break;
            case BoostType.Damage:
                playerStats.maxHealth = Mathf.RoundToInt(playerStats.maxHealth * boostValue);
                break;
            case BoostType.StaminaRegen:
                playerStats.staminaRegenRate = Mathf.RoundToInt(playerStats.staminaRegenRate * boostValue);
                break;
        }

        // Remettre les valeurs par défaut après un délai
        playerController.StartCoroutine(ResetBoost(playerStats, playerController));
    }

    private System.Collections.IEnumerator ResetBoost(PlayerStats playerStats, PlayerController playerController)
    {
        yield return new WaitForSeconds(boostDuration);

        // Rétablir les valeurs par défaut après le boost
        switch (boostType)
        {
            case BoostType.Speed:
                playerController.sprintSpeed /= boostValue;
                break;
            case BoostType.Invisibility:
                playerController.gameObject.layer = LayerMask.NameToLayer("Default");
                break;
            case BoostType.Damage:
                playerStats.maxHealth = Mathf.RoundToInt(playerStats.maxHealth / boostValue);
                break;
            case BoostType.StaminaRegen:
                playerStats.staminaRegenRate = Mathf.RoundToInt(playerStats.staminaRegenRate / boostValue);
                break;
        }
    }
}
