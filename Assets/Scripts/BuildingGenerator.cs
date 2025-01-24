using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Building Elements")]
    public GameObject[] backgroundBuilding;
    public GameObject[] ceilingTrim;
    public GameObject[] floor;
    public GameObject[] railing;
    public GameObject[] railingPillar;
    public GameObject[] railingRail;
    public GameObject[] blocks;
    public GameObject[] walls;
    public GameObject[] windows;
    public GameObject[] doors;
    public GameObject[] roofs;
    public GameObject[] balconies;
    public GameObject[] stairs;
    public GameObject[] corners;

    [Header("Building Settings")]
    public int width = 5;
    public int height = 3;
    public float blockSize = 2f;
    public int doorChance = 30;
    public int balconyChance = 20;

    [Header("Biome Settings")]
    public bool enableBiomes = false;
    public Color buildingColor = Color.white;

    [Header("Debug Settings")]
    public bool previewMode = false;

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
        if (buildingParent == null)
        {
            Debug.LogError("Building Parent is not assigned! Please assign a Transform to 'buildingParent'.");
            return;
        }

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
                bool isCorner = (x % (width - 1) == 0 && z % (width - 1) == 0);

                if (isEdge)
                {
                    if (isCorner)
                    {
                        PlaceCorner(position);
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

                // Add a floor at each position
                PlaceFloor(position);
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

                if (x == 0 || z == 0 || x == width - 1 || z == width - 1)
                {
                    PlaceCeilingTrim(position);
                }

                PlaceRoof(position);
            }
        }
    }

    private void PlaceFloor(Vector3 position)
    {
        GameObject floorElement = GetRandomElement(floor);
        if (floorElement != null)
        {
            Instantiate(floorElement, position, Quaternion.identity, buildingParent);
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
            GameObject stair = GetRandomElement(stairs);
            Instantiate(stair, position, Quaternion.identity, buildingParent);
        }
    }

    private void PlaceCeilingTrim(Vector3 position)
    {
        GameObject ceilingTrimElement = GetRandomElement(ceilingTrim); // Fixed name conflict
        if (ceilingTrimElement != null)
        {
            Instantiate(ceilingTrimElement, position, Quaternion.identity, buildingParent);
        }
    }

    private void PlaceRoof(Vector3 position)
    {
        GameObject roof = GetRandomElement(roofs);
        if (roof != null)
        {
            Instantiate(roof, position, Quaternion.identity, buildingParent);
        }
    }

    private GameObject GetRandomElement(GameObject[] array)
    {
        return (array.Length > 0) ? array[Random.Range(0, array.Length)] : null;
    }

    private void ApplyBiomeSettings()
    {
        Renderer[] renderers = buildingParent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            material.color = buildingColor;
        }
    }
}
