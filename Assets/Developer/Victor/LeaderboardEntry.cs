using System;
using System.Collections;
using System.Collections.Generic;
using Alteruna;
using UnityEngine;
using UnityEngine.UI;


public class LeaderboardEntry : MonoBehaviour
{
   
    [SerializeField]
    private Image progressBar;
    [SerializeField]
    private Text displayText;

    public string entryString;
    private string _oldEntryString;
    public float distanceToGoal;
    private float _oldDistanceToGoal;

    public bool hasChanged = false;
    private float maxDistance;

    public void Initialize(GameObject goal)
    {
        maxDistance = Vector3.Distance(FindObjectOfType<Multiplayer>().AvatarSpawnLocations[0].position, goal.transform.position);
    }

    private void Update()
    {
        displayText.text = entryString;
        progressBar.fillAmount = 1f - (distanceToGoal / maxDistance);
        
        if (entryString != _oldEntryString || !distanceToGoal.Equals(_oldDistanceToGoal))
        {
            _oldEntryString = entryString;
            _oldDistanceToGoal = distanceToGoal;
            hasChanged = true;
        }
    }

    public void UpdateValues(float newDistance, string newString)
    {
        _oldEntryString = newString;
        entryString = newString;

        _oldDistanceToGoal = newDistance;
        distanceToGoal = newDistance;
    }
}
