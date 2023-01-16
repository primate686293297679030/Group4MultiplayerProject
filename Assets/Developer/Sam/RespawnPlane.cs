using UnityEngine;

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

        bool hasCharacterController = other.TryGetComponent(out CharacterController cc);
        if(hasCharacterController) //for normal player
        {
            cc.enabled = false;
            other.transform.position = new Vector3(0, 2, 0); //hard coded position n rotation for now, should respawn at players current check point
            other.transform.rotation = Quaternion.identity;
            cc.Move(Vector3.zero);
            cc.enabled = true;
        }

        bool hasRigidbody = other.TryGetComponent(out Rigidbody body);
        if(hasRigidbody) //for strög player
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            other.transform.position = new Vector3(0, 2, 0); //hard coded position for now, should respawn at players current check point
            other.transform.rotation = Quaternion.identity;
        }
    }
}