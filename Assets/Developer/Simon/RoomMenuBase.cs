using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;

public class RoomMenuBase : MonoBehaviour
{
    RoomMenu rMenu;
   [SerializeField] private GameObject _gameStateManager;
    Alteruna.Multiplayer multiplayer;
    // Start is called before the first frame update
    void Start()
    {
     
        multiplayer = FindObjectOfType<Multiplayer>();
        
        rMenu = FindObjectOfType<RoomMenu>();
        multiplayer = FindObjectOfType<Multiplayer>();
        multiplayer.RoomJoined.AddListener(RoomJoined);
      
    }
    void RoomJoined(Multiplayer multiplayer, Room room, User user)
    {
        if(multiplayer.CurrentRoom!=null&& multiplayer.GetUsers().Count==1)
        {
           
           
          //  Instantiate(_gameStateManager);
          //  
          //  _gameStateManager.tag = "GameStateManager";
          //  
          //  GameStateManager.instance = _gameStateManager.GetComponent<GameStateManager>();
          // 
          //  _gameStateManager.AddComponent<AttributesSync>();
          //
        
        
        
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

