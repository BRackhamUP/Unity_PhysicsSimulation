using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] public float turnSpeed = 4f;
    [SerializeField] public GameObject target;
    [SerializeField] private float targetDistance;

    [SerializeField] public float minTurnAngle = -90f;
    [SerializeField] public float maxTurnAngle = 0f;
    [SerializeField] private float rotX;


    void Start()
    {
        targetDistance = Vector3.Distance(transform.position, target.transform.forward);
    }

    void Update()
    {
        float y = Input.GetAxis("Mouse X") * turnSpeed;
        rotX += Input.GetAxis("Mouse Y") * turnSpeed;

        rotX = Mathf.Clamp(rotX, minTurnAngle, maxTurnAngle);

        transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + y, 0);

        transform.position = target.transform.position - (transform.forward * targetDistance);
    }
}
