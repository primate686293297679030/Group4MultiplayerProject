using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Alteruna;
using Alteruna.Trinity;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class WebProjectile : MonoBehaviour
{
    public bool IsActive;
    private float lifeTime = 3f;
    private float lifeTimeRemaining;
    private Vector3 minSize = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 maxSize = new Vector3(6, 6, 6);
    private Vector3 stuckSize = new Vector3(2, 2, 2);
    private float stickToPlayerTime = 1f;
    private bool hasCollided;
    public int ownerID;
    private Rigidbody rigidbody;

    [SerializeField] bool isOwnedByThisUser;

    private Multiplayer multiplayer;

    private RigidbodySynchronizable rigidbodySync;
    //WebProjectileAttributes syncAttributes;

    public void Initialize(int ID)
    {
        ownerID = ID;
        rigidbody = GetComponent<Rigidbody>();
        transform.SetParent(SpawnManager.WebProjectileParent);
        multiplayer = FindObjectOfType<Multiplayer>();
        rigidbodySync = GetComponent<RigidbodySynchronizable>();
        isOwnedByThisUser = multiplayer.Me.Index == ownerID;
        multiplayer.RegisterRemoteProcedure("StickToPlayer", StickToPlayer);
    }
    public void Activate()
    {
        IsActive = true;
        lifeTimeRemaining = lifeTime;
        hasCollided = false;
        //rigidbodySync.SendData = isOwnedByThisUser;
    }
    public void DeActivate(float delayedDespawn = 0.1f)
    {
        IsActive = false;
        lifeTimeRemaining = 0.0f;
        //gameObject.SetActive(false);
        rigidbody.velocity = Vector3.zero;
        transform.localScale = minSize;
        Destroy(GetComponent<SphereCollider>());
        Destroy(GetComponent<MeshRenderer>());

        //SpawnManager.DespawnObject(gameObject);

        if (isOwnedByThisUser)
        {
            // delay the despawn to make sure that all instances are inactivated first
            StartCoroutine(DelayedDespawn(1));
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(this);
        }
    }

    private IEnumerator DelayedDespawn(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnManager.DespawnObject(gameObject);
    }
    void Update()
    {
        if (!IsActive) return;

        if (lifeTimeRemaining > 0 && !hasCollided)
        {
            lifeTimeRemaining -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(maxSize, minSize, lifeTimeRemaining / lifeTime);
        }
        else if (!hasCollided)
        {
            DeActivate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsActive) return;

        var avatar = other.GetComponent<Alteruna.Avatar>();
        if (!avatar) return;

        // makes sure that only the colliding player is checking collisions
        // and ignores collisions with the owner of the projectile
        if (avatar.Possessor.Index != multiplayer.Me.Index || avatar.Possessor.Index == ownerID) return; 

        PlayerControllerTest player = other.transform.GetComponent<PlayerControllerTest>();
        if (player)
        {
            //player.Activated = false;
            //hasCollided = true;
            ProcedureParameters parameters = new ProcedureParameters();
            parameters.Set("UserIndex", (int)avatar.Possessor.Index);
            multiplayer.InvokeRemoteProcedure("StickToPlayer", UserId.All, parameters);
            rigidbodySync.SendData = false;
            StartCoroutine(StickToPlayerRoutine(player));
        }
    }

    void StickToPlayer(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        int userIndex = parameters.Get("UserIndex", 0);
        GameObject player = GameObject.Find("Player" + userIndex);
        if (player == null) return;

        hasCollided = true;
        rigidbody.velocity = Vector3.zero;
        StartCoroutine(StickToPlayerRoutine(player.GetComponent<PlayerControllerTest>()));
        
    }
    IEnumerator StickToPlayerRoutine(PlayerControllerTest player)
    {
        float elapsedTime = 0;
        float transportationTime = 1f;
        float duration = stickToPlayerTime + transportationTime;
        rigidbody.velocity = Vector3.zero;
        Vector3 offset = new Vector3(0, -0.5f, 0); 
        //player.Activated = false;
        player.slowMultiplier = 0.2f;
        hasCollided = true;
        bool transportationComplete = false;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transportationTime;
            if (elapsedTime < transportationTime)
            {
               // Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref velocity, 1f);
                transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, progress);
                transform.localScale = Vector3.Lerp(transform.localScale, stuckSize, progress);
            }
            else if (!transportationComplete)
            {
                transform.SetParent(player.transform);
                transform.position = player.transform.position + offset;
                transportationComplete = true;
            }
            yield return new WaitForEndOfFrame();
        }
        //player.Activated = true;
        player.slowMultiplier = 1f;

        DeActivate();
    }
}
