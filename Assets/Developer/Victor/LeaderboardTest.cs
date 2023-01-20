using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Alteruna;
using UnityEngine.UI;
using Avatar = Alteruna.Avatar;

public class LeaderboardTest : Synchronizable
{
    public string synchronizedDistanceString = null;
    private string _oldSynchronizedDistanceString = null;


    private GameObject goal;
    private List<Transform> playerTransforms = new List<Transform>();
    private Dictionary<Avatar, LeaderboardEntry> avatarsAndEntries = new Dictionary<Avatar, LeaderboardEntry>();
    private int instantiationIndex = 1;
    private Multiplayer multiplayer;
    private bool isHost = false;
    
    [SerializeField] private Vector3[] UIPositions = new Vector3[5];
    [SerializeField] private LeaderboardEntry entryPrefab;

    public override void DisassembleData(Reader reader, byte LOD)
    {
        // int amountOfEntries = reader.ReadInt();
        // for (int i = 0; i < amountOfEntries; i++)
        // {
        //     reader.ReadFloat();
        //     reader.ReadString();
        // }
        synchronizedDistanceString = reader.ReadString();
        _oldSynchronizedDistanceString = synchronizedDistanceString;
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        
        // writer.Write(entries.Count);
        // foreach (LeaderboardEntry entry in entries)
        // {
        //     writer.Write(entry.distanceToGoal);
        //     writer.Write(entry.entryString);
        // }
        writer.Write(synchronizedDistanceString);
    }

    private void Start()
    {
        goal = GameObject.Find("WinningBox");
        PlayerController.OnPlayerJoined += GetPlayerTransforms;
        PlayerController.OnPlayerLeft += RemovePlayerUI;
        multiplayer = FindObjectOfType<Multiplayer>();
        if (multiplayer.Me == multiplayer.GetUser(0))
        {
            isHost = true;
        }
    }

    private void Update()
    {
        // create an if statement here that checks if host (Multiplayer.Index of some sort) IsMe
        // and only do calculations if true

        // if (!isHost)
        // {
        //     return;
        // }
        
        if (!goal || playerTransforms.Count < 2)
        {
            return;
        }

        // compare entries on distanceToGoal, then update values (reintroduce UpdateValues method in
        // LeaderboardEntry?)
        var listOfAvatarsAndEntries = avatarsAndEntries
            .OrderBy(key => key.Value.distanceToGoal).ToList();
        int placementIndex = 1;
        synchronizedDistanceString = "Leaderboard: " + "\n";
        foreach (KeyValuePair<Avatar, LeaderboardEntry> pair in listOfAvatarsAndEntries)
        {
            avatarsAndEntries[pair.Key].transform.position = UIPositions[placementIndex - 1];
            avatarsAndEntries[pair.Key].distanceToGoal =
                Vector3.Distance(pair.Key.transform.position, goal.transform.position);
            avatarsAndEntries[pair.Key].entryString +=
                (placementIndex + ": " + (pair.Key.IsMe ? "Me " : "Other player #")) + instantiationIndex +
                avatarsAndEntries[pair.Key] +
                "\n";
            placementIndex++;
        }
        
        if (synchronizedDistanceString != _oldSynchronizedDistanceString)
        {
            _oldSynchronizedDistanceString = synchronizedDistanceString;

            Commit();
        }

        base.SyncUpdate();
    }

    // todo: sort entries in UI
    // todo: synchronization?
    // todo: fix avatar null check
    
    // this gets called on player join
    private void GetPlayerTransforms(Avatar avatar)
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (playerTransforms.Contains(player.transform) && avatarsAndEntries.ContainsKey(avatar))
            {
                return;
            }

            playerTransforms.Add(avatar.transform);

            Vector3 positionToInstantiateAt = new Vector3(100f, 250f, 0f);
            // if (instantiationIndex > 1)
            // {
            //     positionToInstantiateAt = new Vector3(positionToInstantiateAt.x,
            //         positionToInstantiateAt.y - instantiationIndex * 20f,
            //         positionToInstantiateAt.z);
            // }

            LeaderboardEntry entry =
                Instantiate(entryPrefab, positionToInstantiateAt, Quaternion.identity,
                    gameObject.transform);
            avatarsAndEntries.Add(avatar, entry);
            entry.GetComponent<RectTransform>().position = positionToInstantiateAt;
            Debug.Log(instantiationIndex);
            instantiationIndex++;
        }
    }

    private void RemovePlayerUI(Avatar avatar)
    {
        Destroy(avatarsAndEntries[avatar].gameObject);
        avatarsAndEntries.Remove(avatar);
        playerTransforms.Remove(avatar.transform);
    }
}