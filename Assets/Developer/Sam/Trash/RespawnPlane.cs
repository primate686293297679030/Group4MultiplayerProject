using UnityEngine;
using Alteruna;

public class RespawnPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.TryGetComponent(out Alteruna.Avatar avatar);
        if(!isPlayer)
        {
            Destroy(other.gameObject);
            return;
        }

        Multiplayer multiplayer = FindObjectOfType<Multiplayer>();
        Transform spawn = multiplayer.AvatarSpawnLocations[multiplayer.Me.Index];

        bool hasCharacterController = other.TryGetComponent(out CharacterController cc);
        if(hasCharacterController) //for normal player
        {
            cc.enabled = false;
            other.transform.position = spawn.position;
            other.transform.rotation = spawn.rotation;
            cc.Move(Vector3.zero);
            cc.enabled = true;
        }

        bool hasRigidbody = other.TryGetComponent(out Rigidbody body);
        if(hasRigidbody) //for strög player
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            other.transform.position = spawn.position;
            other.transform.rotation = spawn.rotation;
        }
    }
}