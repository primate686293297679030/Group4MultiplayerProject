using Alteruna;
using System;
using System.Collections.Generic;
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
        PlayerController.OnPlayerJoined += (avatar) => { Players.Add(avatar); Debug.Log(avatar.name + " has joined!"); };
        PlayerController.OnPlayerLeft += (avatar) => { if (Players.Contains(avatar)) Players.Remove(avatar); };
    }
    
    public static void OnWin(GameObject winner)
    {
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

        Instance.Invoke("ResetGame", 3);
        Instance.Invoke("FadeOut", 1.98f);
        Instance.Invoke("FadeIn", 3);
    }

    public void ResetGame()
    {
        Instance.InvokeRemoteMethod(0, (ushort)UserId.AllInclusive, true);
    }

    public void FadeIn()
    {
        Instance.InvokeRemoteMethod("FadeInAllCameras", (ushort)UserId.AllInclusive);
    }
    public void FadeOut()
    {
        Instance.InvokeRemoteMethod("FadeOutAllCameras", (ushort)UserId.AllInclusive);
    }

    [SynchronizableMethod]
    public void RemoteResetGame(bool resetToSpawnLocation)
    {
        //Multiplayer multiplayer = FindObjectOfType<Multiplayer>();
        foreach (var player in Players)
        {
            player.GetComponent<PlayerController>().InitializePlayer();

            // old respawn
            //if(resetToSpawnLocation && player.GetComponent<Avatar>().IsMe)
            //    player.GetComponent<PlayerController>().LockPlayerPosition(multiplayer.AvatarSpawnLocation.position, 0.05f);
        }
        OnGameReset.Invoke();
    }
    [SynchronizableMethod]
    public void FadeOutAllCameras()
    {
        FindObjectOfType<SmoothCamera>().FadeOut(1);
    }
    [SynchronizableMethod]
    public void FadeInAllCameras()
    {
        FindObjectOfType<SmoothCamera>().FadeIn(1);
    }
}
