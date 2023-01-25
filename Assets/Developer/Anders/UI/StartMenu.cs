using Alteruna;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Avatar = Alteruna.Avatar;
using UnityEngine.UI;
using System.Security.Cryptography;

public class StartMenu : AttributesSync
{
    [SerializeField] private GameObject playerTextPrefab;
    [SerializeField] private List<KeyValuePair<Avatar,GameObject>> playerTexts = new List<KeyValuePair<Avatar, GameObject>>();
    [SerializeField] private GameObject environmentToActivate;
    [SerializeField] private GameObject readyButton;
    private Dictionary<Avatar,GameObject> playerTextMap = new Dictionary<Avatar,GameObject>();
    
    private Canvas canvas;
    private Canvas roomMenuCanvas;

    void Awake()
    {
        PlayerController.OnPlayerJoined += (avatar) =>
        {
            GameObject textObject = Instantiate(playerTextPrefab, avatar.transform);
            textObject.GetComponent<Canvas>().worldCamera = Camera.main;
            playerTextMap.Add(avatar, textObject);
            textObject.GetComponentInChildren<Text>().text = avatar.Possessor.Name;
            textObject.transform.localPosition = new Vector3(0, 2f, 0);

            Invoke("UpdateTextsLocal", 0.1f);
        };
        PlayerController.OnPlayerLeft += (avatar) =>
        {
            if (avatar)
            {
                var pair = playerTextMap.First(a => a.Key == avatar);
                if (pair.Value)
                {
                    Destroy(pair.Value);
                }

                if (playerTextMap.ContainsKey(avatar))
                {
                    playerTextMap.Remove(avatar);
                }
            }
        };
    }

    void Start()
    {
        canvas = GetComponent<Canvas>();
        GameStateManager.instance.PreRace += ActivateMenu;
        GameStateManager.instance.OnStartRace += InactivateMenu;
        GameStateManager.instance.GameState = State.PreRace;
        if (!readyButton) readyButton = transform.Find("ReadyButton").gameObject;
        if (!roomMenuCanvas) roomMenuCanvas = GameObject.Find("Room Menu")?.GetComponent<Canvas>();
        if (!environmentToActivate)
            environmentToActivate = GameObject.Find("Environment");
        
    }
    public void OnReadyButtonPressed()
    {
        transform.Find("ReadyButton").gameObject.SetActive(false);
        foreach (var player in GameManager.Players)
        {
            if (player.IsMe)
            {
                player.GetComponent<PlayerController>().SetPlayerReady(true);
                break;
            }
        }
        UpdateTexts();
        Invoke("CheckIfAllPlayersAreReady", 1);
    }

    private void CheckIfAllPlayersAreReady()
    {
        bool allReady = true;
        foreach (var player in GameManager.Players)
        {
            if (!player.GetComponent<PlayerController>().IsReadyToStart)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            GameManager.StartGame();
            InvokeRemoteMethod("InactivateMenuRemote", (ushort)UserId.AllInclusive);
        }
    }

    private void InactivateMenu()
    {
        canvas.enabled = false;
        foreach (var player in playerTextMap)
        {
            player.Value.SetActive(false);
            if (player.Key.IsMe)
            {
                player.Key.GetComponent<PlayerController>().SetPlayerReady(false);
            }
        }
        UpdateTexts();
        if (environmentToActivate)
            environmentToActivate.SetActive(true);
        if (roomMenuCanvas)
            roomMenuCanvas.enabled = false;
    }
    private void ActivateMenu()
    {
        // gameObject.SetActive(false);
        
        canvas.enabled = true;
        readyButton.SetActive(true);
        foreach (var player in playerTextMap)
        {
            player.Value.SetActive(true);
        }
        UpdateTexts();
        if (environmentToActivate)
            environmentToActivate.SetActive(false);
        if (roomMenuCanvas)
            roomMenuCanvas.enabled = true;
    }

    [SynchronizableMethod]
    public void InactivateMenuRemote()
    {
       InactivateMenu();
    }
    [SynchronizableMethod]
    public void ActivateMenuRemote()
    {
        ActivateMenu();
    }

    private void UpdateTexts()
    {
        InvokeRemoteMethod("UpdateTextsRemote", (ushort)UserId.AllInclusive);
    }
    private void UpdateTextsLocal()
    {
        foreach (var player in playerTextMap)
        {
            Text[] texts = player.Value.GetComponentsInChildren<Text>();

            if (player.Key.GetComponent<PlayerController>().IsReadyToStart)
            {
                texts[1].text = "is Ready!";
                texts[1].color = Color.green;
            }
            else
            {
                texts[1].text = "Not Ready";
                texts[1].color = Color.red;
            }
        }
    }

    [SynchronizableMethod]
    public void UpdateTextsRemote()
    {
        Debug.Log("Inside remote text update");
        foreach (var player in playerTextMap)
        {
            Text[] texts = player.Value.GetComponentsInChildren<Text>();

            if (player.Key.GetComponent<PlayerController>().IsReadyToStart)
            {
                texts[1].text = "is Ready!";
                texts[1].color = Color.green;
            }
            else
            {
                texts[1].text = "Not Ready";
                texts[1].color = Color.red;
            }
        }
    }
}
