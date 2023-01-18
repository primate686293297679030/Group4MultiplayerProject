using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the input of a player
/// The script should only exist on the users own avatar (avatar.IsMe)
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private float tapInterval = 0.2f;
    public Action<KeyCode> OnKeyDoubleTapped = null;
    public Action OnJumpAttempt = null;
    public Action OnAbilityAttempt = null;
    public Action OnTempResetGame = null;

    private Dictionary<KeyCode, float> keyTimestamps = new()
    {
        { KeyCode.LeftArrow, 0f }, { KeyCode.RightArrow, 0f }, { KeyCode.UpArrow, 0f }, { KeyCode.DownArrow, 0f }
    };

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                CheckKeyTiming(KeyCode.LeftArrow);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                CheckKeyTiming(KeyCode.RightArrow);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                CheckKeyTiming(KeyCode.UpArrow);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                CheckKeyTiming(KeyCode.DownArrow);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJumpAttempt();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                OnAbilityAttempt();
            }
        }
    }

    private void CheckKeyTiming(KeyCode key)
    {
        if (Time.time - keyTimestamps[key] < tapInterval)
        {
            OnKeyDoubleTapped(key);
        }
        keyTimestamps[key] = Time.time;
    }
}
