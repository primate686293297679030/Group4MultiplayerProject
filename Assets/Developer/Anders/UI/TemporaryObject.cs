using UnityEngine;

/// <summary>
/// Put this script on an object that should be destroyed
/// by a specific game event
/// </summary>
public class TemporaryObject : MonoBehaviour
{
    [SerializeField] private DestroyCondition destroyCondition;

    public enum DestroyCondition
    {
        OnGameStart,
        OnGameReset,
        OnGamePause,
        OnGameResume
    }

    void Start()
    {
        switch (destroyCondition)
        {
            case DestroyCondition.OnGameReset:
                GameManager.OnGameReset += DestroyThis;
                break;
            case DestroyCondition.OnGamePause:
                //todo - get pause event
                break;
            case DestroyCondition.OnGameResume:
                //todo - get game resume event
                break;
            case DestroyCondition.OnGameStart:
                //todo - get game start event
                break;
        }

    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        switch (destroyCondition)
        {
            case DestroyCondition.OnGameReset:
                GameManager.OnGameReset -= DestroyThis;
                break;
            case DestroyCondition.OnGamePause:
                //todo - get pause event
                break;
            case DestroyCondition.OnGameResume:
                //todo - get game resume event
                break;
            case DestroyCondition.OnGameStart:
                //todo - get game start event
                break;
        }
    }
}