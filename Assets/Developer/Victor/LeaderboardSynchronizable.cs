using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Alteruna;
using Avatar = Alteruna.Avatar;

public class LeaderboardSynchronizable : Synchronizable
{
    public string leaderboardString;

    [SerializeField] private GameObject goal;
    private List<Transform> playerTransforms = new List<Transform>();
    private Dictionary<Avatar, LeaderboardEntry> avatarsAndEntries = new Dictionary<Avatar, LeaderboardEntry>();
    private int instantiationIndex = 1;
    private Multiplayer multiplayer;
    private bool isHost = false;

    [SerializeField] private Vector3[] UIPositions = new Vector3[5];
    [SerializeField] private LeaderboardEntry entryPrefab;
    private Canvas canvas;

    public override void DisassembleData(Reader reader, byte LOD)
    {
        int amountOfEntries = reader.ReadInt();
        for (int i = 0; i < amountOfEntries; i++)
        {
            float readDistance = reader.ReadFloat();
            string readString = reader.ReadString();
            ushort index = reader.ReadUshort();
            LeaderboardEntry entryToFind = avatarsAndEntries.FirstOrDefault
                (x => x.Key.Possessor.Index == index).Value;

            if (entryToFind != null)
            {
                entryToFind.UpdateValues(readDistance, readString);
            }
            
        }
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        writer.Write(avatarsAndEntries.Count);
        foreach (KeyValuePair<Avatar, LeaderboardEntry> entry in avatarsAndEntries)
        {
            writer.Write(entry.Value.distanceToGoal);
            writer.Write(entry.Value.entryString);
            // this is the player index for this avatar
            writer.Write(entry.Key.Possessor.Index);
        }
    }

    private void Start()
    {
        if (!goal)
            goal = GameObject.Find("WinningBox");
        PlayerController.OnPlayerJoined += GetPlayerTransforms;
        PlayerController.OnPlayerLeft += UpdateEntries;
        multiplayer = FindObjectOfType<Multiplayer>();
        canvas = GetComponent<Canvas>();
        if (multiplayer.Me == multiplayer.GetUser(0))
        {
            isHost = true;
        }
    }

    private void Update()
    {
        // check if current player is the host, only do calculations if true
        if (!isHost)
        {
            return;
        }

        if (!goal || playerTransforms.Count < 2)
        {
            return;
        }

        var listOfAvatarsAndEntries = avatarsAndEntries
            .OrderBy(key => key.Value.distanceToGoal).ToList();
        int placementIndex = 1;
        //leaderboardString = "Leaderboard: " + "\n";
        foreach (KeyValuePair<Avatar, LeaderboardEntry> pair in listOfAvatarsAndEntries)
        {
            if (!pair.Key)
            {
                return;
            }
            avatarsAndEntries[pair.Key].entryString = "";
            avatarsAndEntries[pair.Key].transform.position = UIPositions[placementIndex - 1];
            avatarsAndEntries[pair.Key].distanceToGoal =
                Vector3.Distance(pair.Key.transform.position, goal.transform.position);
            avatarsAndEntries[pair.Key].entryString +=
                placementIndex + ": " + pair.Key.Possessor.Name + " " +
                pair.Value.distanceToGoal.ToString("n2") +
                "\n";
            placementIndex++;
        }

        foreach (KeyValuePair<Avatar, LeaderboardEntry> entry in avatarsAndEntries)
        {
            if (entry.Value.hasChanged)
            {
                Commit();
                entry.Value.hasChanged = false;
            }
        }

        base.SyncUpdate();
    }

    // this gets called on player join
    public void GetPlayerTransforms(Avatar avatar)
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (playerTransforms.Contains(player.transform) && avatarsAndEntries.ContainsKey(avatar))
            {
                return;
            }

            playerTransforms.Add(avatar.transform);

            Vector3 positionToInstantiateAt = UIPositions[instantiationIndex];//new Vector3(100f, 250f, 0f);

            LeaderboardEntry entry =
                Instantiate(entryPrefab, positionToInstantiateAt, Quaternion.identity,
                    gameObject.transform);
            avatarsAndEntries.Add(avatar, entry);
            entry.Initialize(goal);
            entry.GetComponent<RectTransform>().position = positionToInstantiateAt;
            instantiationIndex++;
        }

        if (GameObject.FindGameObjectsWithTag("Player").Length <= 1)
            canvas.enabled = false;
        else if (!canvas.enabled)
            canvas.enabled = true;
    }

    void UpdateEntries(Avatar avatar)
    {
        if (avatarsAndEntries[avatar])
            Destroy(avatarsAndEntries[avatar].gameObject);
        if (avatarsAndEntries.ContainsKey(avatar))
        {
            avatarsAndEntries.Remove(avatar);
            playerTransforms.Remove(avatar.transform);
            instantiationIndex--;
        }
    }
}