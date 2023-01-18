using UnityEngine;

public class RoadBlockBehavior : MonoBehaviour
{
    [SerializeField] private float pushForce = 666f;
    private Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool isPlayer = collision.gameObject.TryGetComponent(out Alteruna.Avatar avatar);
        if (!isPlayer || !avatar.IsMe)
        {
            return;
        }

        Vector3 pushDir = (transform.position - collision.transform.position).normalized;
        body.AddForce(pushDir * pushForce);

        bool hasRespawnComp = collision.gameObject.TryGetComponent(out PlayerRespawn respawnComp);
        if (!hasRespawnComp) return;
        respawnComp.CallRespawnWithFade(.5f);
    }
}
