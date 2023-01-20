using System;
using System.Collections;
using System.Collections.Generic;
using Alteruna;
using UnityEngine;
using UnityEngine.UI;


public class LeaderboardEntry : Synchronizable
{
   
    [SerializeField]
    private Image progressBar;
    [SerializeField]
    private Text displayText;

    public string entryString;
    private string _oldEntryString;
    public float distanceToGoal;
    private float _oldDistanceToGoal;

    public override void AssembleData(Writer writer, byte LOD = 100)
    {
        writer.Write(entryString);
        writer.Write(distanceToGoal);
    }

    public override void DisassembleData(Reader reader, byte LOD = 100)
    {
        entryString = reader.ReadString();
        _oldEntryString = entryString;
        
        distanceToGoal = reader.ReadFloat();
        _oldDistanceToGoal = distanceToGoal;
    }

    private void Update()
    {
        displayText.text = entryString;
        progressBar.fillAmount = 1f - (distanceToGoal / 100f);
        
        if (entryString != _oldEntryString)
        {
            _oldEntryString = entryString;

            Commit();
        }
        base.SyncUpdate();
        
        if (!distanceToGoal.Equals(_oldDistanceToGoal))
        {
            _oldDistanceToGoal = distanceToGoal;

            Commit();
        }
        base.SyncUpdate();
    }
}
