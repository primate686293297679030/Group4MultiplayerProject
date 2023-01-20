using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Alteruna;
using UnityEngine.UIElements;


   public enum State
    {
        PreRace,
        DuringRace,
        PostRace,


    }

    

    
public class GameStateManager : AttributesSync
{
    public static GameStateManager instance;
    public State GameState;
    Alteruna.Multiplayer multiplayer;
   
    [SerializeField] GameObject canvas;
    
    public UnityAction playerCreatesRoom;

    public Action<State> OnStateUpdated;
    public Action PreRace;
    public Action OnStartRace;
    public Action PostRace;

    void Awake()
    {
 
        if (instance == null)
        {
             instance = this;
        }
        else
        {
             Destroy(this);
        }
 
    }
    void Start()
    {
        multiplayer = FindObjectOfType<Multiplayer>();

        multiplayer.RoomJoined.AddListener(RoomJoined);
        multiplayer.OtherUserJoined.AddListener(OtherPlayerJoined);
        PreRace +=OnPreRace;
        OnStartRace += OnDuringRace;
        //LobbyState.multiplayer.Connected.AddListener(playerJoined);
        playerCreatesRoom += CreatesRoom;
    }

    //player presses Start on RoomMenu
    public void CreatesRoom()
    {
        GameStateManager.instance.GameState = State.PreRace;
        if (GameTimer.instance)
            GameTimer.instance.Enable();
        GameStateManager.instance.NextState();
    }
    void RoomJoined(Multiplayer multiplayer, Room room, User user)
    {
        
        Quaternion target = Quaternion.Euler(14, 0, 0);
   

    }
    void OtherPlayerJoined(Multiplayer multiplayer, User user)
    {

    }
    void OnPreRace()
    {
        if (canvas)
        canvas.SetActive(true);
    }
    IEnumerator PreRaceState()
    {
        PreRace();
        Debug.Log("PreGame: Enter");
        while (GameState == State.PreRace)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Crawl: Exit");
        NextState();
    }

    IEnumerator DuringRaceState()
    {
        OnStartRace();
        Debug.Log("DuringGame: Enter");
        while (GameState == State.DuringRace)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("DuringGame: Exit");
        NextState();
    }

    IEnumerator PostRaceState()
    {

        Debug.Log("PostRace: Enter");
        while (GameState == State.PostRace)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("PostGame: Exit");
        NextState();
    }
   
    void OnDuringRace()
    {
        foreach(var player in GameManager.Players)
        {
            PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
            playerRespawn.checkpoint = 0;
            playerRespawn.CallRespawn();

        }

    }
 
    public void OnStartButton(GameObject ui)
    {
        InvokeRemoteMethod("UpdateGameState", (ushort)UserId.AllInclusive, (int)State.DuringRace);
        ui.SetActive(false);
      
    }

    [SynchronizableMethod]
    public void UpdateGameState(int state)
    {
        GameStateManager.instance.GameState = (State)state;
    }
    public void UpdateGameStateLocal(int state)
    {
        GameStateManager.instance.GameState = (State)state;
    }
    public  void NextState()
    {
      string methodName = GameState.ToString() + "State";
        System.Reflection.MethodInfo info =
            GetType().GetMethod(methodName,
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance);
        StartCoroutine((IEnumerator)info.Invoke(this, null));
    }

    void Update()
    {
    
  
    
    }

}

