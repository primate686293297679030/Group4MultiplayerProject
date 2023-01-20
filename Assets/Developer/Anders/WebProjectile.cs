using System.Collections;
using Alteruna;
using Alteruna.Trinity;
using EasingFunctions;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class WebProjectile : MonoBehaviour
{
    public bool IsActive;
    private float lifeTime = 3f;
    private float lifeTimeRemaining;
    private Vector3 minSize = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 maxSize = new Vector3(6, 6, 6);
    private Vector3 stuckSize = new Vector3(2.25f, 2.25f, 2.25f);
    private float stickToPlayerTime = 1f;
    //public int ownerID;
    private Rigidbody rb;

    [SerializeField] bool isOwnedByThisUser;

    private Multiplayer multiplayer;

    private ProjectileSynchronizable syncAttributes;

    private Coroutine stickToPlayerRoutine;

    PlayerController stuckPlayer = null;
    //WebProjectileAttributes syncAttributes;

    public void Initialize(int ID)
    {
        //ownerID = ID;
        transform.SetParent(SpawnManager.WebProjectileParent);
        multiplayer = FindObjectOfType<Multiplayer>();
        syncAttributes = GetComponent<ProjectileSynchronizable>();
        rb = GetComponent<Rigidbody>();
        isOwnedByThisUser = multiplayer.Me.Index == ID;

        if (isOwnedByThisUser) 
            syncAttributes.OwnerID = ID;
        multiplayer.RegisterRemoteProcedure("StickToPlayer", StickToPlayer);
    }
    public void Activate()
    {
        IsActive = true;
        lifeTimeRemaining = lifeTime;
        //syncAttributes.SendData = isOwnedByThisUser;
    }
    public void DeActivate(float delayedDespawn = 0.1f)
    {
        IsActive = false;
        if (isOwnedByThisUser) rb.velocity *= 0.2f;

        Destroy(GetComponent<SphereCollider>());
        syncAttributes.SendData = false;
        syncAttributes.enabled = false;
        transform.SetParent(null);
        StartCoroutine(FadeOutRoutine(0.5f));
    }

    private IEnumerator DelayedDespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnManager.DespawnObject(gameObject);
    }
    private IEnumerator FadeOutRoutine(float duration)
    {

        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float easedProgress = Ease.Out(progress, 4);
            renderer.GetPropertyBlock(properties);
            properties.SetFloat("_FadeoutProgress", easedProgress + 0.1f);

            renderer.SetPropertyBlock(properties);
            transform.localScale += transform.localScale * Time.deltaTime * (easedProgress + 0.1f) * 0.5f;
            yield return new WaitForEndOfFrame();
        }
        if (isOwnedByThisUser)
        {
            // duration the despawn to make sure that all instances are inactivated first
            StartCoroutine(DelayedDespawnRoutine(10));
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
    void Update()
    {
        if (!IsActive) return;

        if (lifeTimeRemaining > 0 && stuckPlayer == null)
        {
            lifeTimeRemaining -= Time.deltaTime;
            transform.localScale = Vector3.Lerp(maxSize, minSize, lifeTimeRemaining / lifeTime);
        }
        else if (stuckPlayer == null)
        {
            DeActivate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsActive) return;

        if (other.GetComponent<WebProjectile>() && stuckPlayer == null)
        {
            if (isOwnedByThisUser)
            {
                rb.velocity *= -1;
                rb.AddForce(rb.velocity * -2f, ForceMode.Impulse);
                syncAttributes.OwnerID = -1;
            }
            Debug.Log("Collided with another bubble");
            return;
        }

        var avatar = other.GetComponent<Alteruna.Avatar>();
        if (!avatar) return;

        // makes sure that only the colliding player is checking collisions
        // and ignores collisions with the owner of the projectile
        if (avatar.Possessor.Index != multiplayer.Me.Index || avatar.Possessor.Index == syncAttributes.OwnerID) return; 

        PlayerController player = other.transform.GetComponent<PlayerController>();
        if (player)
        {
            //player.Activated = false;
            //hasCollided = true;
            
            ProcedureParameters parameters = new ProcedureParameters();
            parameters.Set("UserIndex", (int)avatar.Possessor.Index);
            multiplayer.InvokeRemoteProcedure("StickToPlayer", UserId.AllInclusive, parameters);
            //syncAttributes.SendData = false;
            //stickToPlayerRoutine = StartCoroutine(StickToPlayerRoutine(player));
        }
    }

    void StickToPlayer(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        if (stuckPlayer != null)
        {
            stuckPlayer.slowMultiplier = 1;
            transform.SetParent(null);
            if (isOwnedByThisUser)
                syncAttributes.OwnerID = -1;
            //ownerID = -1;
            lifeTimeRemaining = lifeTime;

            if (stickToPlayerRoutine != null)
                StopCoroutine(stickToPlayerRoutine);
        }

        int userIndex = parameters.Get("UserIndex", 0);
        GameObject player = GameObject.Find("Player" + userIndex);
        if (player == null) return;
        
        rb.velocity = Vector3.zero;
        stickToPlayerRoutine = StartCoroutine(StickToPlayerRoutine(player.GetComponent<PlayerController>()));
    }
    IEnumerator StickToPlayerRoutine(PlayerController player)
    {
        stuckPlayer = player;
        float elapsedTime = 0;
        float transportationTime = 1f;
        float duration = stickToPlayerTime + transportationTime;
        float minSlowMult = 0.2f;
        float startDistance = Vector3.Distance(player.transform.position, transform.position);
        rb.velocity = Vector3.zero;
        Vector3 offset = new Vector3(0, 0, 0);
        player.slowMultiplier = 0.2f;
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

               float currentDistance = Vector3.Distance(player.transform.position, transform.position);
               player.slowMultiplier = Mathf.Lerp(minSlowMult, 0.75f, currentDistance / startDistance);
                
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
        stuckPlayer = null;
        DeActivate();
        
    }
}
