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

    

    
public class GameStateManager : MonoBehaviour
{
   
    // Start is called before the first frame update
    public static GameStateManager instance;
    public State GameState;
    List<Transform> stateTransforms=new List<Transform>();

    Transform ActiveTranfrom;
    public int StateIndex;
    public Action<State> OnStateUpdated;
    public Action PreRace;
    public Action OnStartRace;
    public Action PostRace;
    [SerializeField]GameObject canvas;
    
    int[] next = new int[3] { 0, 1, 2 };
    enum Actions
    {

        next,
        wait,
        idle,


    }
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
        PreRace+=OnPreRace;
        OnStartRace += OnDuringRace;
        GameObject trans2 = GameObject.FindGameObjectWithTag("SpawnLocation");
        GameObject trans1 = GameObject.FindGameObjectWithTag("LobbySpawnPoint");
      //  [SerializeField] Canvas LobbyMenu;
        Transform aewrgeqr = gameObject.transform;
        stateTransforms.Add(aewrgeqr);
        stateTransforms.Add(trans2.transform);
        OnStateUpdated += SynchStates;
       
        

        // RoomMenuBase.multiplayer.Connected.AddListener(playerJoined);
    }
    
   void OnPreRace()
    {
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
        ui.SetActive(false);
        GameState = State.DuringRace;
    }



  // IEnumerator IdleState()
  // {
  //     Debug.Log("PostGame: Enter");
  //     while (GameState == State.PostGame)
  //     {
  //         yield return 0;
  //     }
  //     Debug.Log("PostGame: Exit");
  // }



  public  void NextState()
    {
      

        string methodName = GameState.ToString() + "State";
        System.Reflection.MethodInfo info =
            GetType().GetMethod(methodName,
                                System.Reflection.BindingFlags.NonPublic |
                                System.Reflection.BindingFlags.Instance);
        StartCoroutine((IEnumerator)info.Invoke(this, null));
    }
    // Update is called once per frame
    void Update()
    {
    

    
    }
    void PauseState()
    {

    }


    void SynchStates(State gameState)
    {

    //    StateSynchronizable.StateSynch.SynchronizedInt = ((int)gameState);
    }

   
    


}


public class StateSynchronizable : Synchronizable
{
   public static StateSynchronizable StateSynch;
    void Awake()
    {

        if (StateSynch == null)
        {
            StateSynch = this;
        }
        else
        {
            Destroy(this);
        }

    }
    // Data to be synchronized with other players in our playroom.
    public int SynchronizedInt=0;

    // Used to store the previous version of our data so that we know when it has changed.
    private int _oldSynchronizedInt=0;

    public override void DisassembleData(Reader reader, byte LOD)
    {
        // Set our data to the updated value we have recieved from another player.
        SynchronizedInt = reader.ReadInt();

        // Save the new data as our old data, otherwise we will immediatly think it changed again.
        _oldSynchronizedInt = SynchronizedInt;
    }

    public override void AssembleData(Writer writer, byte LOD)
    {
        // Write our data so that it can be sent to the other players in our playroom.
        writer.Write(SynchronizedInt);
    }

    private void Update()
    {
        // If the value of our float has changed, sync it with the other players in our playroom.
        if (SynchronizedInt != _oldSynchronizedInt)
        {
            // Store the updated value
            _oldSynchronizedInt = SynchronizedInt;

            // Tell Alteruna Multiplayer that we want to commit our data.
            Commit();
        }

        // Update the Synchronizable
        base.SyncUpdate();
    }
}
