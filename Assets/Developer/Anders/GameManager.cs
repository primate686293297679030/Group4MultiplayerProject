using System;
using Unity.VisualScripting;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject winningUI;
    [SerializeField] private GameObject losingUI;
    public static GameManager instance;


    void Awake()
    {
        // Singleton for now
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        PlayerControllerTest.OnPlayerJoined += (trans) => { Debug.Log(trans.name + " has joined!"); };
    }

    public static void OnWin(GameObject winner)
    {
        SmoothCamera camera = Camera.main.GetComponent<SmoothCamera>();
        if (camera.Target != winner.transform)
        {
            camera.FollowingOwner = false;
            Camera.main.GetComponent<SmoothCamera>().Target = winner.transform;
        }

        var playerControllers = FindObjectsOfType<PlayerControllerTest>();
        foreach (var controller in playerControllers)
        {
            if (controller.gameObject != winner)
            {
               // controller.Activated = false;
                GameObject _losingUIObject = Instantiate(instance.losingUI);
                _losingUIObject.transform.SetParent(controller.transform);
                _losingUIObject.transform.localPosition = new Vector3(0, 1f, 0);
            }
        }


        GameObject _winningUIObject = Instantiate(instance.winningUI);
        _winningUIObject.transform.SetParent(winner.transform);
        _winningUIObject.transform.localPosition = new Vector3(0, 1f, 0);
    }
}
