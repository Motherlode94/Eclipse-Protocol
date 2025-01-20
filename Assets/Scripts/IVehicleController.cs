using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVehicleController
{
    void EnableControl(); // Activer les contrôles du véhicule
    void DisableControl(); // Désactiver les contrôles du véhicule
    void ResetVehicle(); // Réinitialiser l'état du véhicule
}
