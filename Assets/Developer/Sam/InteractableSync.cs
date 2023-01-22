using Alteruna;
using Alteruna.Trinity;
using UnityEngine;

public class InteractableSync : MonoBehaviour
{   //dosn't work, don't use
    private int owner;
    private Multiplayer multiplayer;
    private Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        multiplayer = FindObjectOfType<Multiplayer>();

        multiplayer.RegisterRemoteProcedure("UpdateOwnerRPC", UpdateOwnerRPC);
        multiplayer.RegisterRemoteProcedure("UpdateTransformRPC", UpdateTransformRPC);

        UpdateBody();
    }

    public int GetOwner() => owner;

    public void UpdateOwner(int newOwner)
    {
        owner = newOwner;
        UpdateBody();
        ProcedureParameters parameters = new();
        parameters.Set("Owner", owner);
        multiplayer.InvokeRemoteProcedure("UpdateOwnerRPC", UserId.All, parameters);
    }

    private void UpdateOwnerRPC(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        owner = parameters.Get("Owner", 0);
        UpdateBody();
    }

    private void UpdateBody()
    {
        bool isMe = owner == multiplayer.Me.Index; 
        body.isKinematic = !isMe;
    }

    private void FixedUpdate()
    {
        if (owner != multiplayer.Me.Index) //only owner can send data
            return;

        UpdateTransform();
    }

    private void UpdateTransform()
    {
        ProcedureParameters parameters = new ProcedureParameters();
        parameters.Set("posX", transform.position.x);
        parameters.Set("posY", transform.position.y);
        parameters.Set("posZ", transform.position.z);

        parameters.Set("rotX", transform.rotation.x);
        parameters.Set("rotY", transform.rotation.y);
        parameters.Set("rotZ", transform.rotation.z);
        parameters.Set("rotW", transform.rotation.w);
        multiplayer.InvokeRemoteProcedure("UpdateTransformRCP", UserId.All, parameters);
    }

    private void UpdateTransformRPC(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        float posX = parameters.Get("posX", transform.position.x);
        float posY = parameters.Get("posY", transform.position.y);
        float posZ = parameters.Get("posZ", transform.position.z);
        transform.position = new(posX, posY, posZ);

        float rotX = parameters.Get("rotX", transform.rotation.x);
        float rotY = parameters.Get("rotY", transform.rotation.y);
        float rotZ = parameters.Get("rotZ", transform.rotation.z);
        float rotW = parameters.Get("rotW", transform.rotation.w);
        transform.rotation = new(rotX, rotY, rotZ, rotW);
    }
}