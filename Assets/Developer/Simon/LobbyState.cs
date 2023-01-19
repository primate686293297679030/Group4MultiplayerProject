using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;
using Avatar = Alteruna.Avatar;
using Alteruna.Trinity;
using UnityEngine.Bindings;
using UnityEngine.Events;
public class LobbyState : MonoBehaviour
{

    Camera mainCam;
    Alteruna.Multiplayer multiplayer;
    //public GameObject avatarGameObject;
  public static LobbyState instance;
   public UnityAction playerCreatesRoom;
    private void Awake()
    {
        if(instance==null)
        { 
        instance = this;

        }
        else
        { Destroy(this); }
            
        //player = GetComponent<Alteruna.Avatar>();

    }
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        multiplayer = FindObjectOfType<Multiplayer>();
        playerCreatesRoom += CreatesRoom;
        multiplayer.RoomJoined.AddListener(RoomJoined);
       // Debug.Log(multiplayer.AvatarSpawnLocations.Count);
    

    }
    void RoomJoined(Multiplayer multiplayer, Room room, User user)
    {
        Quaternion target = Quaternion.Euler(14, 0, 0);
        mainCam.transform.rotation=target;


     

        //Waiting To Join Race

        // 


    }
  
   public void CreatesRoom()
    {
        GameStateManager.instance.GameState = State.PreRace;
        GameTimer.instance.Enable();
        GameStateManager.instance.NextState();

    }
    // Update is called once per frame
    void Update()
    {
    
    }
}
