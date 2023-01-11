using Alteruna;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailAttributesSync : Synchronizable
{
    // Data to be synchronized with other players in our playroom.
    public float currentLifeTime;

    // Used to store the previous version of our data so that we know when it has changed.
    private float oldLifeTime;


    void Update()
    {
        // If the value of our float has changed, sync it with the other players in our playroom.
        if (currentLifeTime != oldLifeTime)
        {
            // Store the updated value
            oldLifeTime = currentLifeTime;

            // Tell Alteruna Multiplayer that we want to commit our data.
            Commit();
        }

        // Update the Synchronizable
        base.SyncUpdate();
    }

    public override void DisassembleData(Reader reader, byte LOD)
    {
        // Set our data to the updated value we have recieved from another player.
        currentLifeTime = reader.ReadFloat();

        // Save the new data as our old data, otherwise we will immediatly think it changed again.
        oldLifeTime = currentLifeTime;
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        // Write our data so that it can be sent to the other players in our playroom.
        writer.Write(currentLifeTime);
    }
}
