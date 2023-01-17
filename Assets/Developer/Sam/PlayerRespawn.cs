using UnityEngine;
using Alteruna;

public class PlayerRespawn : MonoBehaviour
{
    [HideInInspector] public Transform checkpoint;

    [SerializeField] float respawnAt = -10f;

    private Alteruna.Avatar avatar;
    private CharacterController cc;

    private void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();
        cc = GetComponent<CharacterController>();

        if (!avatar.IsMe) return;
        Multiplayer multiplayer = FindObjectOfType<Multiplayer>();
        checkpoint = multiplayer.AvatarSpawnLocations[multiplayer.Me.Index];
    }

    private void FixedUpdate()
    {
        if (!avatar.IsMe) return;

        if (transform.position.y < respawnAt)
        {
            Respawn(checkpoint.position);
        }
    }

    private void Respawn(Vector3 position) //use this for rcp???
    {
        cc.enabled = false;
        transform.position = position;
        //transform.rotation = checkpoint.rotation;
        cc.enabled = true;
        cc.Move(Vector3.zero);
    }
}
