using UnityEngine;
using Alteruna;
using Alteruna.Trinity;

public class PlayerRespawn : MonoBehaviour
{
    [HideInInspector] public int checkpoint;

    [SerializeField] float respawnAt = -10f;

    private bool isMe;
    private CharacterController cc;
    private Multiplayer multiplayer;

    private void Start()
    {
        multiplayer = FindObjectOfType<Multiplayer>();
        multiplayer.RegisterRemoteProcedure("RespawnRPC", RespawnRPC);
        isMe = GetComponent<Alteruna.Avatar>().IsMe;
        cc = GetComponent<CharacterController>();

        if (!isMe) return;
        Vector3 spawn = multiplayer.AvatarSpawnLocations[multiplayer.Me.Index].position;
        CheckpointBehavior.checkpoints.Add(0, spawn);
    }

    private void FixedUpdate()
    {
        if (!isMe) return;

        if (transform.position.y < respawnAt)
        {
            CallRespawn();
        }
    }

    private void RespawnRPC(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        float x = parameters.Get("X", 0);
        float y = parameters.Get("Y", 0);
        float z = parameters.Get("Z", 0);

        Respawn(new(x, y, z));
    }

    private void Respawn(Vector3 position)
    {
        cc.enabled = false;
        transform.position = position;
        cc.enabled = true;
        cc.Move(Vector3.zero);
    }

    private void CallRespawn()
    {
        ProcedureParameters parameters = new ProcedureParameters();
        Vector3 position = CheckpointBehavior.checkpoints[checkpoint];
        parameters.Set("X", position.x);
        parameters.Set("Y", position.y);
        parameters.Set("Z", position.z);
        multiplayer.InvokeRemoteProcedure("RespawnRPC", UserId.All, parameters);
        Respawn(position);
    }
}