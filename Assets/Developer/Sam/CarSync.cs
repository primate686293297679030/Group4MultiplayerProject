using Alteruna;
using UnityEngine;

public class CarSync : Synchronizable
{
    private bool isMe;
    private Rigidbody body;

    private void Start()
    {
        isMe = GetComponent<Alteruna.Avatar>().IsMe;
        body = GetComponent<Rigidbody>();
    }

    public override void DisassembleData(Reader reader, byte LOD)
    {
        if (isMe) return; //never overwrite your player 

        body.velocity = reader.ReadVector3();
        body.angularVelocity = reader.ReadVector3();
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        if (!isMe) return; //only your own player can write data

        writer.Write(body.velocity);
        writer.Write(body.angularVelocity);
    }

    private void FixedUpdate()
    {
        //test is me here
        Commit(); 
        SyncUpdate();
    }
}
