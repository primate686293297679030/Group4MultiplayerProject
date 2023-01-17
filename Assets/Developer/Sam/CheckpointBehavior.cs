using UnityEngine;

public class CheckpointBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.TryGetComponent(out Alteruna.Avatar avatar);
        if (!isPlayer || !avatar.IsMe) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        respawn.checkpoint = transform;
    }
}
