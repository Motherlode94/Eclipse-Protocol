using UnityEngine;

public abstract class BaseRangedWeapon : MonoBehaviour, IRangedWeapon
{
    public float Damage { get; protected set; }
    public float Range { get; protected set; }
    public int AmmoCapacity { get; protected set; }
    public int CurrentAmmo { get; protected set; }
    public float ReloadTime { get; protected set; }
    public AudioClip FireSound { get; protected set; }
    public GameObject MuzzleFlash { get; protected set; }

    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public virtual void Equip()
    {
        Debug.Log($"{gameObject.name} équipée.");
    }

    public virtual void Fire()
    {
        if (CurrentAmmo > 0)
        {
            CurrentAmmo--;
            if (FireSound != null)
            {
                audioSource.PlayOneShot(FireSound);
            }
            if (MuzzleFlash != null)
            {
                Instantiate(MuzzleFlash, transform.position, Quaternion.identity);
            }
            Debug.Log($"{gameObject.name} a tiré. Munitions restantes : {CurrentAmmo}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} n'a plus de munitions !");
        }
    }

    public virtual void Reload()
    {
        CurrentAmmo = AmmoCapacity;
        Debug.Log($"{gameObject.name} rechargée. Munitions : {CurrentAmmo}");
    }

    public virtual void Unequip()
    {
        Debug.Log($"{gameObject.name} déséquipée.");
    }
}
