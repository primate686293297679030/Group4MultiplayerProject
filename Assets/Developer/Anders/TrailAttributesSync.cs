using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailAttributesSync : Synchronizable
{
    // Data to be synchronized with other players in our playroom.
    public float lifeTime;
    public int fadeOutTime;

    // Used to store the previous version of our data so that we know when it has changed.
    private float oldLifeTime;
    private int oldFadeOutTime;


    void Update()
    {
        bool shouldCommit = false;
        // If the value of our float has changed, sync it with the other players in our playroom.
        if (lifeTime != oldLifeTime)
        {
            oldLifeTime = lifeTime;
            shouldCommit = true;
        }
        if (fadeOutTime != oldFadeOutTime)
        {
            oldFadeOutTime = fadeOutTime;
            shouldCommit = true;
        }

        if (shouldCommit)
            Commit();

        // Update the Synchronizable
        base.SyncUpdate();
    }

    public override void DisassembleData(Reader reader, byte LOD)
    {
        // Set our data to the updated value we have recieved from another player.
        lifeTime = reader.ReadFloat();
        fadeOutTime = reader.ReadInt();

        // Save the new data as our old data, otherwise we will immediatly think it changed again.
        oldLifeTime = lifeTime;
        oldFadeOutTime = fadeOutTime;
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        // Write our data so that it can be sent to the other players in our playroom.
        writer.Write(lifeTime);
        writer.Write(fadeOutTime);
    }
}
