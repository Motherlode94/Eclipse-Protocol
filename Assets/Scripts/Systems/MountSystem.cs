using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountSystem : MonoBehaviour
{
    public Transform mountPosition;
    public GameObject currentMount;

    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void Mount(GameObject mount)
    {
        if (currentMount != null) return;

        currentMount = mount;
        player.transform.position = mountPosition.position;
        player.transform.SetParent(mount.transform);
        Debug.Log("Joueur monté sur " + mount.name);
    }

    public void Dismount()
    {
        if (currentMount == null) return;

        player.transform.SetParent(null);
        currentMount = null;
        Debug.Log("Joueur descendu de la monture.");
    }
}
