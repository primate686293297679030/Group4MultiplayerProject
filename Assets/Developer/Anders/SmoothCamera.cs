using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    public Transform Target;
    public bool FollowingOwner = true; // false if following other player
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, -8f);
    [SerializeField] private float moveSpeed = 0.05f;

    private Vector3 velocity = Vector3.zero;
    private float transitionMultiplier = 1f;
    private float timeSpentFollowingOtherPlayer;

    void Update()
    {
        if (Target == null) return;

        Vector3 targetPos = Target.position + offset;

        if (FollowingOwner)
        {
            transitionMultiplier = 1f;
        }
        else
        {
            timeSpentFollowingOtherPlayer += Time.deltaTime;
            float progress = Mathf.Clamp(timeSpentFollowingOtherPlayer / 3, 0, 1);
            transitionMultiplier = Mathf.SmoothStep(6f, 3f, progress);
        }
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, moveSpeed * transitionMultiplier);
    }
}
