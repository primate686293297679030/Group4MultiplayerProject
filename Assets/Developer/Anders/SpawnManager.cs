using System;
using System.Collections.Generic;
using System.Linq;

using Alteruna;
using Alteruna.Trinity;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public static Transform WebProjectileParent;

    private Spawner spawner;

    private Multiplayer multiplayer;

    void Start()
    {

        if (Instance == null)
            Instance = this;
        else 
            Destroy(this);

        spawner = GetComponent<Spawner>();
        multiplayer = FindObjectOfType<Multiplayer>();
        
        if (multiplayer)
        {
            multiplayer.RegisterRemoteProcedure("ActivateProjectile", ActivateProjectile);
            multiplayer.RegisterRemoteProcedure("DeActivateProjectile", DeActivateProjectile);
        }
        WebProjectileParent = new GameObject("WebProjectiles").transform;
        WebProjectileParent.transform.SetParent(transform);
    }

    public static WebProjectile SpawnWebProjectile(Vector3 position, int ownerID)
    {
        GameObject spawnedObject = Instance.spawner.Spawn(0, position);
        string guidStr = Instance.spawner.SpawnedObjects.Find(g => g.Item1 == spawnedObject).Item2.ToString();

        ProcedureParameters parameters = new ProcedureParameters();
        parameters.Set("OwnerID", ownerID);
        parameters.Set("Guid", guidStr);

        Instance.multiplayer.InvokeRemoteProcedure("ActivateProjectile", UserId.All, parameters);

        return spawnedObject.GetComponent<WebProjectile>();
    }

    void ActivateProjectile(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        string strGuid = parameters.Get("Guid", "");
        int ownerID = parameters.Get("OwnerID", 0);

        Guid guid = new Guid(strGuid);
        var gameObject = spawner.SpawnedObjects.Find(v => v.Item2 == guid).Item1;

        if (gameObject)
        {
            WebProjectile projectile = gameObject.GetComponent<WebProjectile>();
            projectile.Initialize(ownerID);
            projectile.Activate();
        }
        else
        {
            Debug.Log("Tried to activate projectile remotely but the gameObject was not found");
        }
    }
    public static void DeActivateProjectile(GameObject obj)
    {
        var spawnedObject = Instance.spawner.SpawnedObjects.Find(g => g.Item1 == obj);
        string guidStr = spawnedObject.Item2.ToString();
        ProcedureParameters parameters = new ProcedureParameters();
        parameters.Set("Guid", guidStr);
        Instance.multiplayer.InvokeRemoteProcedure("DeActivateProjectile", UserId.All, parameters);
    }
    void DeActivateProjectile(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        string strGuid = parameters.Get("Guid", "");
        Guid guid = new Guid(strGuid);
        var gameObject = spawner.SpawnedObjects.Find(v => v.Item2 == guid).Item1;

        if (gameObject)
        {
            gameObject.SetActive(false);
            gameObject.GetComponent<WebProjectile>().DeActivate(0.1f);
        }
        else
        {
            Debug.Log("Tried to deactivate projectile but none was found");
        }
    }

    public static void DespawnObject(GameObject objectToDespawn)
    {
        if (!objectToDespawn) return;
        if (Instance.spawner.SpawnedObjects.Count(g => g.Item1 == objectToDespawn) > 0)
            Instance.spawner.Despawn(objectToDespawn);
    }
}
