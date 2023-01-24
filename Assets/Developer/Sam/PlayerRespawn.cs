using UnityEngine;
using Alteruna;
using Alteruna.Trinity;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    public float respawnAt = -10f;

    [HideInInspector] public int checkpoint;

    [SerializeField] float fadeDuration = 1f;

    private bool isMe;
    private CharacterController cc;
    private Multiplayer multiplayer;
    private SmoothCamera camBehaviour;
    private PlayerController playerController;
    private bool fading;

    private void Start()
    {
        multiplayer = FindObjectOfType<Multiplayer>();
        multiplayer.RegisterRemoteProcedure("RespawnRPC", RespawnRPC);
        isMe = GetComponent<Alteruna.Avatar>().IsMe;
        cc = GetComponent<CharacterController>();
        camBehaviour = Camera.main.GetComponent<SmoothCamera>();
        playerController = GetComponent<PlayerController>();

        if (!isMe) return;
        Vector3 spawn = multiplayer.AvatarSpawnLocations[multiplayer.Me.Index].position;
        CheckpointBehavior.checkpoints.Add(0, spawn);
    }

    private void FixedUpdate()
    {
        if (!isMe) return;

        if (transform.position.y < respawnAt && !fading)
        {
            CallRespawnWithFade(fadeDuration);
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
        playerController.StopDashing(0f);
    }

    public void CallRespawn()
    {
        ProcedureParameters parameters = new ProcedureParameters();
        Vector3 position = CheckpointBehavior.checkpoints[checkpoint];
        parameters.Set("X", position.x);
        parameters.Set("Y", position.y);
        parameters.Set("Z", position.z);
        multiplayer.InvokeRemoteProcedure("RespawnRPC", UserId.All, parameters);
        Respawn(position);
    }

    private IEnumerator CallRespawnWithFadeCo(float delay)
    {
        camBehaviour.FadeOut(delay);
        yield return new WaitForSeconds(delay);
        CallRespawn();
        camBehaviour.FadeIn(delay);
        fading = false;
    }

    public void CallRespawnWithFade(float delay)
    {
        if (fading) return;
        fading = true;
        StartCoroutine(CallRespawnWithFadeCo(delay / 2f)); //both fade in and out so total fade time will be twice as big
    }
}