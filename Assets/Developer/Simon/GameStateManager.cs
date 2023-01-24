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
        OnStartRace += OnDuringRace; 
    }  
    IEnumerator PreRaceState()
    {
        if (PreRace != null)
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
        if (OnStartRace != null)
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
        if (PostRace != null)
            PostRace();
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
        //foreach(var player in GameManager.Players)
        //{
        //    PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
        //    playerRespawn.checkpoint = 0;
        //    playerRespawn.CallRespawn();
        //
        //}

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

}

