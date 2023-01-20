using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Alteruna;
using UnityEngine.UI;
using Avatar = Alteruna.Avatar;

public class LeaderboardSynchronizable : Synchronizable
{
    public string synchronizedDistanceString = null;
    private string _oldSynchronizedDistanceString = null;

    private GameObject goal;
    private List<Transform> playerTransforms = new List<Transform>();
    private List<float> distances;
    private Dictionary<Avatar, float> avatarsAndDistances = new Dictionary<Avatar, float>();

    [SerializeField] private Text UIText;

    public override void DisassembleData(Reader reader, byte LOD)
    {
        synchronizedDistanceString = reader.ReadString();
        _oldSynchronizedDistanceString = synchronizedDistanceString;
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        writer.Write(synchronizedDistanceString);
    }

    private void Start()
    {
        goal = GameObject.Find("WinningBox");
        PlayerControllerTest.OnPlayerJoined += GetPlayerTransforms;
    }

    private void Update()
    {
        if (!goal || playerTransforms.Count < 2)
        {
            return;
        }


        int placementIndex = 1;
        synchronizedDistanceString = "Leaderboard: " + "\n";
        foreach (KeyValuePair<Avatar, float> pair in avatarsAndDistances.OrderBy(key => key.Value).ToList())
        {
            avatarsAndDistances[pair.Key] = Vector3.Distance(pair.Key.transform.position, goal.transform.position);
            synchronizedDistanceString += (placementIndex + ": " + (pair.Key.IsMe ? "Player 1 " : "Player 2 ")) +
                                          avatarsAndDistances[pair.Key] + "\n";
            placementIndex++;
        }

        UIText.text = synchronizedDistanceString;

        if (synchronizedDistanceString != _oldSynchronizedDistanceString)
        {
            _oldSynchronizedDistanceString = synchronizedDistanceString;

            Commit();
        }

        base.SyncUpdate();
    }

    // this gets called on player join
    public void GetPlayerTransforms(Avatar avatar)
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (playerTransforms.Contains(player.transform) && avatarsAndDistances.ContainsKey(avatar))
            {
                return;
            }

            playerTransforms.Add(avatar.transform);
            avatarsAndDistances.Add(avatar, 0f);
        }
    }
}