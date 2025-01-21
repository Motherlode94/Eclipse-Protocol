using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Building Elements")]
    public GameObject[] blocks; // Blocs de base (fondations, étages)
    public GameObject[] walls; // Murs avec ou sans fenêtres
    public GameObject[] windows; // Fenêtres
    public GameObject[] doors; // Portes
    public GameObject[] roofs; // Toits
    public GameObject[] balconies; // Balcons
    public GameObject[] stairs; // Escaliers

    [Header("Building Settings")]
    public int width = 5; // Largeur du bâtiment (en blocs)
    public int height = 3; // Hauteur du bâtiment (en étages)
    public float blockSize = 2f; // Taille d'un bloc (par défaut à 2x2)
    public int doorChance = 30; // Probabilité d'ajouter une porte (%)
    public int balconyChance = 20; // Probabilité d'ajouter un balcon (%)

    [Header("Biomes Settings")]
    public bool enableBiomes = false;
    public Color buildingColor; // Couleur pour le biome actuel (appliquée aux matériaux)

    [Header("Parent Object")]
    public Transform buildingParent; // Parent pour organiser les éléments du bâtiment

    private void Start()
    {
        GenerateBuilding();
    }

    public void GenerateBuilding()
    {
        // Générer chaque étage du bâtiment
        for (int y = 0; y < height; y++)
        {
            GenerateFloor(y);
        }

        // Ajouter un toit au sommet
        GenerateRoof();

        // Appliquer des paramètres spécifiques aux biomes si activés
        if (enableBiomes)
        {
            ApplyBiomeSettings();
        }
    }

    private void GenerateFloor(int floorLevel)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                Vector3 position = new Vector3(x * blockSize, floorLevel * blockSize, z * blockSize);

                // Détermine si c'est un mur extérieur ou une partie interne
                bool isEdge = (x == 0 || x == width - 1 || z == 0 || z == width - 1);

                // Place un mur, une fenêtre, une porte ou un bloc en fonction de l'emplacement
                if (isEdge)
                {
                    if (floorLevel == 0 && Random.Range(0, 100) < doorChance && (x == 0 || x == width - 1 || z == 0 || z == width - 1))
                    {
                        PlaceDoor(position);
                    }
                    else
                    {
                        PlaceWallOrWindow(position);
                    }

                    // Ajoute éventuellement un balcon au-dessus du mur
                    if (Random.Range(0, 100) < balconyChance && floorLevel > 0)
                    {
                        PlaceBalcony(position);
                    }
                }
                else
                {
                    PlaceBlock(position);
                }
            }
        }

        // Ajouter des escaliers reliant les étages
        if (floorLevel < height - 1)
        {
            PlaceStairs(floorLevel);
        }
    }

    private void GenerateRoof()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                Vector3 position = new Vector3(x * blockSize, height * blockSize, z * blockSize);

                // Place un toit
                if (roofs.Length > 0)
                {
                    GameObject roof = roofs[Random.Range(0, roofs.Length)];
                    Instantiate(roof, position, Quaternion.identity, buildingParent);
                }
            }
        }
    }

    private void PlaceWallOrWindow(Vector3 position)
    {
        // Aléatoirement place un mur ou une fenêtre
        GameObject element = Random.Range(0, 2) == 0 ? GetRandomElement(walls) : GetRandomElement(windows);
        if (element != null)
        {
            Instantiate(element, position, Quaternion.identity, buildingParent);
        }
    }

    private void PlaceDoor(Vector3 position)
    {
        GameObject door = GetRandomElement(doors);
        if (door != null)
        {
            Instantiate(door, position, Quaternion.identity, buildingParent);
        }
    }

    private void PlaceBlock(Vector3 position)
    {
        GameObject block = GetRandomElement(blocks);
        if (block != null)
        {
            Instantiate(block, position, Quaternion.identity, buildingParent);
        }
    }

    private void PlaceBalcony(Vector3 position)
    {
        GameObject balcony = GetRandomElement(balconies);
        if (balcony != null)
        {
            Vector3 balconyPosition = position + new Vector3(0, blockSize, 0);
            Instantiate(balcony, balconyPosition, Quaternion.identity, buildingParent);
        }
    }

    private void PlaceStairs(int floorLevel)
    {
        if (stairs.Length > 0)
        {
            Vector3 position = new Vector3(width * blockSize / 2, floorLevel * blockSize, blockSize / 2);
            GameObject stair = stairs[Random.Range(0, stairs.Length)];
            Instantiate(stair, position, Quaternion.identity, buildingParent);
        }
    }

    private GameObject GetRandomElement(GameObject[] array)
    {
        if (array.Length == 0) return null;
        return array[Random.Range(0, array.Length)];
    }

    private void ApplyBiomeSettings()
    {
        Renderer[] renderers = buildingParent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            material.color = buildingColor; // Applique la couleur du biome
        }
    }
}
