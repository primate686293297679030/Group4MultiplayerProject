using Alteruna;

/// <summary>
/// An extension of rigidbodysynchronizable
/// Might add more variables like lifetime etc
/// </summary>
public class ProjectileSynchronizable : RigidbodySynchronizable
{
    // Data to be synchronized with other Players in our playroom.
    public int OwnerID = 0;

    // Used to store the previous version of our data so that we know when it has changed.
    private int oldOwnerID;

    public override void DisassembleData(Reader reader, byte LOD)
    {
        // Set our data to the updated value we have recieved from another player.
        OwnerID = reader.ReadInt();

        // Save the new data as our old data, otherwise we will immediatly think it changed again.
        oldOwnerID = OwnerID;
        base.DisassembleData(reader, LOD);
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        // Write our data so that it can be sent to the other Players in our playroom.
        writer.Write(OwnerID);
        base.AssembleData(writer, LOD);
    }

    private void Update()
    {
        // If the value of our float has changed, sync it with the other Players in our playroom.
        if (OwnerID != oldOwnerID)
        {
            // Store the updated value
            oldOwnerID = OwnerID;

            // Tell Alteruna that we want to commit our data.
            Commit();
        }

        // Update the Synchronizable
        base.SyncUpdate();
    }
}
