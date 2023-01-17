using UnityEngine;
using System.Collections.Generic;

public class CheckpointBehavior : MonoBehaviour
{
    public static Dictionary<int, Vector3> checkpoints = new Dictionary<int, Vector3>();

    [SerializeField] private int number = 1;

    private void Start()
    {
        checkpoints.Add(number, transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.TryGetComponent(out Alteruna.Avatar avatar);
        if (!isPlayer || !avatar.IsMe) return;

        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();
        if(respawn.checkpoint == number - 1) //can't go back or skip checkpoints
        {
            respawn.checkpoint++;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1f);
    }
}