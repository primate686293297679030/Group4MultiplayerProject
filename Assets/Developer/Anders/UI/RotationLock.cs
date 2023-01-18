using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a lazy solution to when you set an objects parent
/// but want the child to ignore the parent's rotation
/// </summary>
public class RotationLock : MonoBehaviour
{
    private RectTransform rectTrans;

    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rectTrans)
        {
            if (rectTrans.rotation != Quaternion.identity)
            {
                rectTrans.rotation = Quaternion.identity;
            }
        }
        else
        {
            if (rectTrans.rotation != Quaternion.identity)
            {
                transform.rotation = Quaternion.identity;
            }
        }
    }
}
