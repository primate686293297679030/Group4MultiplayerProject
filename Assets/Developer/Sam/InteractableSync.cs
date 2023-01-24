using Alteruna;
using Alteruna.Trinity;
using UnityEngine;

public class InteractableSync : MonoBehaviour
{
    private Multiplayer multiplayer;
    private Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        multiplayer = FindObjectOfType<Multiplayer>();

        //multiplayer.RegisterRemoteProcedure("UpdateOwnerRPC", UpdateOwnerRPC);
        multiplayer.RegisterRemoteProcedure("UpdateTransformRPC", UpdateTransformRPC);

        //UpdateBody();
    }

    private void FixedUpdate()
    {
        //if (owner != multiplayer.Me.Index) //only owner can update
        if(multiplayer.Me.Index != 0) //only host can update
            return;

        UpdateTransform();
    }

    private void UpdateTransform()
    {
        ProcedureParameters parameters = new ProcedureParameters();
        parameters.Set("linX", body.velocity.x);
        parameters.Set("linY", body.velocity.y);
        parameters.Set("linZ", body.velocity.z);

        parameters.Set("angX", body.angularVelocity.x);
        parameters.Set("angY", body.angularVelocity.y);
        parameters.Set("angZ", body.angularVelocity.z);
        multiplayer.InvokeRemoteProcedure("UpdateTransformRCP", UserId.All, parameters);
    }

    private void UpdateTransformRPC(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        float linX = parameters.Get("linX", body.velocity.x);
        float linY = parameters.Get("linY", body.velocity.y);
        float linZ = parameters.Get("linZ", body.velocity.z);
        body.velocity = new(linX, linY, linZ);

        float angX = parameters.Get("angX", body.angularVelocity.x);
        float angY = parameters.Get("angY", body.angularVelocity.y);
        float angZ = parameters.Get("angZ", body.angularVelocity.z);
        body.angularVelocity = new(angX, angY, angZ);
    }

    //owner pls
    //public int GetOwner() => owner;

    //private int owner;

    //public void UpdateOwner(int newOwner)
    //{
    //    owner = newOwner;
    //    UpdateBody();
    //    ProcedureParameters parameters = new();
    //    parameters.Set("Owner", owner);
    //    multiplayer.InvokeRemoteProcedure("UpdateOwnerRPC", UserId.All, parameters);
    //}

    //private void UpdateOwnerRPC(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    //{
    //    owner = parameters.Get("Owner", 0);
    //    UpdateBody();
    //}

    //private void UpdateBody()
    //{
    //    bool isMe = owner == multiplayer.Me.Index; 
    //    body.isKinematic = !isMe;
    //}
}