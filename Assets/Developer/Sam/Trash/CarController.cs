using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] float linearAcc = 25f;
    [SerializeField] float maxLinearSpeed = 20f;
    [SerializeField] float angularAcc = 2f;
    [SerializeField] float maxAngularSpeed = 2f;

    private float sqrMaxLinearSpeed;
    private float sqrMaxAngularSpeed;
    private bool isMe;
    private Rigidbody body;
    private float drifting;

    void Start()
    {
        isMe = GetComponent<Alteruna.Avatar>().IsMe;
        body = GetComponent<Rigidbody>();
        sqrMaxLinearSpeed = maxLinearSpeed * maxLinearSpeed;
        sqrMaxAngularSpeed= maxAngularSpeed * maxAngularSpeed;

        if (isMe)
        {
            Camera.main.gameObject.GetComponent<SmoothCamera2>().Target = transform;
        }
    }

    void FixedUpdate()
    {
        if (!isMe) return;

        if (Input.GetKey(KeyCode.W))
        {
            Drive(1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Drive(-1);
        }

        bool backing = body.velocity.sqrMagnitude > .025f && Vector3.Dot(transform.forward, body.velocity.normalized) < 0f;
        int turnDirection = backing ? -1 : 1;

        if (Input.GetKey(KeyCode.A))
        {
            Turn(turnDirection * -1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Turn(turnDirection);
        }
    }

    private void Drive(int direction)
    {
        float deltaSpeed = Time.fixedDeltaTime * linearAcc;
        Vector3 carDirection = transform.forward * direction;
        Vector3 driveVelocity = body.velocity;

        //reduce speed to change direction when at max speed
        if (driveVelocity.sqrMagnitude >= sqrMaxLinearSpeed)
        {
            float newDirDiff = Vector3.Dot(driveVelocity.normalized, carDirection);
            float reducePrecent = Mathf.Clamp01(1 + newDirDiff);
            driveVelocity -= driveVelocity.normalized * (deltaSpeed * reducePrecent);
        }

        driveVelocity += carDirection * deltaSpeed;
        drifting = 1f - Mathf.Abs(Vector3.Dot(driveVelocity.normalized, carDirection));
        body.velocity = driveVelocity;
    }

    private void Turn(int direction)
    {
        float deltaSpeed = Time.fixedDeltaTime * angularAcc;
        Vector3 rotateDirection = Vector3.up * direction;
        Vector3 angularVelocity = body.angularVelocity;

        //reduce speed to turn when at max speed
        if (angularVelocity.sqrMagnitude> sqrMaxAngularSpeed)
        {
            float newDirDiff = Vector3.Dot(angularVelocity.normalized, rotateDirection);
            float reducePrecent = Mathf.Clamp01(1 + newDirDiff);
            angularVelocity -= angularVelocity.normalized * (deltaSpeed * reducePrecent);
        }

        angularVelocity += rotateDirection * deltaSpeed;
        body.angularVelocity = angularVelocity;
    }
}
