using Alteruna;
using UnityEngine;

public class RoadBlockBehavior : MonoBehaviour
{
    [SerializeField] private float pushForce = 666f;

    private Rigidbody body;
    private float respawnAt = -15f;
    private Vector3 spawnPoint;
    private Quaternion spawnOrientation;
    private Multiplayer multiplayer;

    private static readonly int Offset = Shader.PropertyToID("_Offset");

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        multiplayer = FindObjectOfType<Multiplayer>();
        spawnPoint = transform.position;
        spawnOrientation = transform.rotation;
        SeedMaterial();
    }

    private void SeedMaterial()
    {
        MaterialPropertyBlock matBlock = new();
        matBlock.SetFloat(Offset, Random.Range(-1000, 1000));
        GetComponent<MeshRenderer>().SetPropertyBlock(matBlock);
    }

    private void Update()
    {
        if (transform.position.y < respawnAt)
        {
            transform.SetPositionAndRotation(spawnPoint, spawnOrientation);
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.TryGetComponent(out Alteruna.Avatar avatar);
        if (!isPlayer || !avatar.IsMe)
        {
            return;
        }

        Vector3 pushDir = (transform.position - other.transform.position).normalized;
        body.AddForce(pushDir * pushForce);

        bool hasRespawnComp = other.TryGetComponent(out PlayerRespawn respawnComp);
        if (!hasRespawnComp) return;
        respawnAt = respawnComp.respawnAt;
        respawnComp.CallRespawnWithFade(.5f);
    }
}