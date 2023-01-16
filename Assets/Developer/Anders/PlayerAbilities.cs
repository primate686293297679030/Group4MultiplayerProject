using Alteruna;
using Alteruna.Trinity;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Avatar = Alteruna.Avatar;
using Vector3 = UnityEngine.Vector3;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private PlayerInput playerInput;
    private float cooldownLength = 0.66f;
    private float cooldownRemaining;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.OnAbilityAttempt += TryUseAbility;

    }

    // Update is called once per frame
    void Update()
    {
        if (cooldownRemaining > 0)
        {
            cooldownRemaining = Mathf.Clamp(cooldownRemaining - Time.deltaTime, 0, cooldownLength);
        }
    }

    void TryUseAbility()
    {
        if (cooldownRemaining <= 0)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 direction = (Input.mousePosition - screenPos).normalized;
            int ID = GetComponent<Avatar>().Possessor.Index;
            cooldownRemaining = cooldownLength;


            //Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject closestPlayer = FindClosestPlayer(transform.position);

            if (closestPlayer != null)
            {
                Vector3 directionToPlayer = (closestPlayer.transform.position - transform.position).normalized;
                //direction.z = directionToPlayer.z;
                direction = directionToPlayer;
            }
            
            SpawnWebProjectile(ID, direction);
        }
    }

    GameObject FindClosestPlayer(Vector3 position)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = float.MaxValue;
        GameObject closestPlayer = null;
        position.z = 0; // ignores z in both vectors
        foreach (var player in players)
        {
            if (player == gameObject) continue;
            float distance = Vector3.Distance(
                new Vector3(player.transform.position.x, player.transform.position.y,0), position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        return closestPlayer;
    }

    void SpawnWebProjectile(int ownerID, Vector3 direction)
    {
        // spawns and activates projectile on all clients
        WebProjectile projectile = SpawnManager.SpawnWebProjectile(transform.position, ownerID);

        if (projectile == null)
        {
            Debug.Log("Failed to spawn projectile");
            return;
        }
        projectile.gameObject.SetActive(true);
        projectile.transform.position = transform.position;
        projectile.Initialize(ownerID);
        projectile.Activate();
        projectile.GetComponent<Rigidbody>().AddForce(direction * 300, ForceMode.Impulse);
        StartCoroutine(CheckProjectileUpdating(0.02f, projectile.GetComponent<RigidbodySynchronizable>(), ownerID, direction));
    }

    // fail-safing the spawning function, which seems to fail every other spawn
    private IEnumerator CheckProjectileUpdating(float delay, RigidbodySynchronizable rbs, int ownerID, Vector3 direction)
    {
        yield return new WaitForSeconds(delay);
        if (rbs == null) yield return null;

        bool hasUpdated = false;
        foreach (var bucketBehavior in rbs.BucketBehaviors)
        {
            if (bucketBehavior.LastUpdated != 0)
            {
                hasUpdated = true;
                break;
            }
        }

        if (!hasUpdated)
        {
            rbs.GetComponent<WebProjectile>().DeActivate();
            SpawnManager.DeActivateProjectile(rbs.gameObject);
            SpawnWebProjectile(ownerID, direction); // try to spawn again
        }
    }
}
