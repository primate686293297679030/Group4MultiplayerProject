using Alteruna;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Avatar = Alteruna.Avatar;


public class GameManager : AttributesSync
{
    [SerializeField] private GameObject winningUI;
    [SerializeField] private GameObject losingUI;
    public static GameManager Instance;

    public static List<Avatar> Players = new List<Avatar>();

    public static Action OnGameReset = delegate { };

    void Awake()
    {
        // Singleton for now
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        PlayerController.OnPlayerJoined += (avatar) =>
        {
            Players = FindObjectsOfType<Avatar>().ToList();
            // Players.Add(avatar);
            Debug.Log(avatar.name + " has joined!");
        };
        PlayerController.OnPlayerLeft += (avatar) => { if (Players.Contains(avatar)) Players.Remove(avatar); };
    }
    
    public static void OnWin(ushort winnerID)
    {
        Instance.InvokeRemoteMethod("OnWinRemote", (ushort)UserId.All, winnerID);
        Instance.OnWinLocal(winnerID);
    }



    public void ResetGame()
    {
        Instance.InvokeRemoteMethod("RemoteResetGame");
        Instance.ResetGameLocal();
    }

    public static void StartGame()
    {
        Instance.InvokeRemoteMethod("RemoteStartGame");
        Instance.StartGameLocal();
    }

    public void FadeIn()
    {
        FindObjectOfType<SmoothCamera>().FadeIn(1);
        //Instance.InvokeRemoteMethod("FadeInAllCameras", (ushort)UserId.AllInclusive);
    }
    public void FadeOut()
    {
        FindObjectOfType<SmoothCamera>().FadeOut(1);
        //Instance.InvokeRemoteMethod("FadeOutAllCameras", (ushort)UserId.AllInclusive);
    }
    [SynchronizableMethod]
    public void OnWinRemote(ushort winnerID)
    {
        OnWinLocal(winnerID);
    }

    private void OnWinLocal(ushort winnerID)
    {
        GameObject winner = Players.Find(avatar => avatar.Possessor == winnerID).gameObject;
        winner.GetComponent<PlayerController>().UpdateMaterials();

        SmoothCamera camera = Camera.main.GetComponent<SmoothCamera>();
        if (camera.Target != winner.transform)
        {
            camera.FollowingOwner = false;
            Camera.main.GetComponent<SmoothCamera>().Target = winner.transform;
        }

        var playerControllers = FindObjectsOfType<PlayerController>();
        foreach (var controller in playerControllers)
        {
            if (controller.gameObject != winner)
            {
                // controller.Activated = false;
                GameObject _losingUIObject = Instantiate(Instance.losingUI);
                _losingUIObject.transform.SetParent(controller.transform);
                _losingUIObject.transform.localPosition = new Vector3(0, 1f, 0);
            }
        }

        GameObject _winningUIObject = Instantiate(Instance.winningUI);
        _winningUIObject.transform.SetParent(winner.transform);
        _winningUIObject.transform.localPosition = new Vector3(0, 1f, 0);

        GameStateManager.instance.UpdateGameStateLocal((int)State.PostRace);
        //Instance.InvokeRemoteMethod("UpdateGameState", (ushort)UserId.AllInclusive, (int)State.PostRace);
        Instance.Invoke("ResetGame", 3);
        Instance.Invoke("FadeOut", 1.98f);
        Instance.Invoke("FadeIn", 3);
    }

    [SynchronizableMethod]
    public void RemoteResetGame()
    {
        ResetGameLocal();
    }

    private void ResetGameLocal()
    {
        foreach (var player in Players)
        {
            player.GetComponent<PlayerController>().InitializePlayer();
            PlayerRespawn respawner = player.GetComponent<PlayerRespawn>();
            respawner.checkpoint = 0;
            if (player.IsMe)
                player.GetComponent<PlayerRespawn>().CallRespawn();
        }
        OnGameReset.Invoke();
        GameStateManager.instance.UpdateGameStateLocal((int)State.PreRace);
        //InvokeRemoteMethod("UpdateGameState", (ushort)UserId.AllInclusive, (int)State.PreRace);
    }
    [SynchronizableMethod]
    public void RemoteStartGame()
    {
        StartGameLocal();   
    }

    private void StartGameLocal()
    {
        foreach (var player in Players)
        {
            player.GetComponent<PlayerController>().InitializePlayer();
            PlayerRespawn respawner = player.GetComponent<PlayerRespawn>();
            respawner.checkpoint = 0;
            if (player.IsMe)
                player.GetComponent<PlayerRespawn>().CallRespawn();
        }
        OnGameReset.Invoke();
        GameStateManager.instance.UpdateGameStateLocal((int)State.DuringRace);
        //InvokeRemoteMethod("UpdateGameState", (ushort)UserId.AllInclusive, (int)State.DuringRace);
    }
}
