using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Building Elements")]
    public GameObject[] backgroundBuilding;
    public GameObject[] ceiling_Trim;
    public GameObject[] floor;
    public GameObject[] railing;
    public GameObject[] railing_Pillar;
    public GameObject[] railing_Rail;
    public GameObject[] railing_railStairs;
    public GameObject[] railing_railStraight;

    public GameObject[] blocks;
    public GameObject[] walls;
    public GameObject[] windows;
    public GameObject[] doors;
    public GameObject[] roofs;
    public GameObject[] balconies;
    public GameObject[] stairs;
    public GameObject[] corners; // Coins pour les bords

    [Header("Building Settings")]
    public int width = 5;
    public int height = 3;
    public float blockSize = 2f;
    public int doorChance = 30;
    public int balconyChance = 20;

    [Header("Biome Settings")]
    public bool enableBiomes = false; // Active/désactive les biomes
    public Color buildingColor = Color.white; // Couleur par défaut

    [Header("Debug Settings")]
    public bool previewMode = false; // Mode prévisualisation

    [Header("Parent Object")]
    public Transform buildingParent;

    private void Start()
    {
        if (!previewMode)
        {
            GenerateBuilding();
        }
    }

    public void GenerateBuilding()
    {
        for (int y = 0; y < height; y++)
        {
            GenerateFloor(y);
        }
        GenerateRoof();

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
                bool isEdge = (x == 0 || x == width - 1 || z == 0 || z == width - 1);

                if (isEdge)
                {
                    if (x == 0 && z == 0 || x == width - 1 && z == 0 || x == 0 && z == width - 1 || x == width - 1 && z == width - 1)
                    {
                        PlaceCorner(position); // Placer un coin
                    }
                    else if (floorLevel == 0 && Random.Range(0, 100) < doorChance)
                    {
                        PlaceDoor(position);
                    }
                    else
                    {
                        PlaceWallOrWindow(position);
                    }

                    if (Random.Range(0, 100) < balconyChance && floorLevel > 0)
                    {
                        PlaceBalcony(position);
                    }
                }
                else
                {
                    PlaceBlock(position);
                }

                // Ajoute un sol sur chaque position
                PlaceFloor(position, floorLevel);
            }
        }

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

                // Place un toit ou une garniture de plafond
                if (roofs.Length > 0 && (x == 0 || z == 0 || x == width - 1 || z == width - 1))
                {
                    GameObject ceilingTrim = GetRandomElement(ceiling_Trim);
                    if (ceilingTrim != null)
                    {
                        Instantiate(ceilingTrim, position, Quaternion.identity, buildingParent);
                    }
                }

                if (roofs.Length > 0)
                {
                    GameObject roof = roofs[Random.Range(0, roofs.Length)];
                    Instantiate(roof, position, Quaternion.identity, buildingParent);
                }
            }
        }
    }

    private void PlaceFloor(Vector3 position, int floorLevel)
    {
        if (floor.Length > 0)
        {
            GameObject floorElement = GetRandomElement(floor);
            if (floorElement != null)
            {
                Instantiate(floorElement, position, Quaternion.identity, buildingParent);
            }
        }
    }

    private void PlaceWallOrWindow(Vector3 position)
    {
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

    private void PlaceCorner(Vector3 position)
    {
        GameObject corner = GetRandomElement(corners);
        if (corner != null)
        {
            Instantiate(corner, position, Quaternion.identity, buildingParent);
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
