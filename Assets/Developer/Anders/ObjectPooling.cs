using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Alteruna;
using Alteruna.Trinity;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance;
    public static Transform WebProjectileParent;

    [SerializeField] private GameObject webProjectilePrefab;
    [SerializeField] private int webProjectilePoolSize = 10;
    //[SerializeField] private GameObject[] webProjectiles;
    private WebProjectile[] webProjectiles;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        WebProjectileParent = new GameObject("WebProjectiles").transform;
        WebProjectileParent.transform.SetParent(transform);

        webProjectiles = new WebProjectile[webProjectilePoolSize];

        for (int i = 0; i < webProjectilePoolSize; i++)
        {
            GameObject spawnedObject = Instantiate(webProjectilePrefab);
            spawnedObject.SetActive(false);
            spawnedObject.transform.SetParent(WebProjectileParent);
            webProjectiles[i] = spawnedObject.GetComponent<WebProjectile>();
        }

    }

    void Update()
    {
        //Debug.Log("Spawned object count: " + Instance.spawner.SpawnedObjects.Count);
    }


    public static WebProjectile GetWebProjectile() 
    {
        WebProjectile foundProjectile = null;

        foreach (var projectile in Instance.webProjectiles)
        {
            if (!projectile.gameObject.activeSelf)
            {
                projectile.gameObject.SetActive(true);
                projectile.Activate();
                foundProjectile = projectile;
                break;
            }
        }
        if (foundProjectile == null)
        {
            Debug.Log("Warning: The object pool of web projectiles is too small. All instances were active at once.");
        }
        return foundProjectile;
    }
}
