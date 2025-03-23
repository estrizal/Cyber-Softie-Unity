using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform followTarget;
    [SerializeField] float distance = 5;
    [SerializeField] float minVerticalAngle = -45;
    [SerializeField] float maxVerticalAngle = +45;
    [SerializeField] Vector2 framingOffset;
    [SerializeField] float rotationspeed = 2f;
    // Update is called once per frame
    float rotationY;
    float rotationX;

    private void Update()
    {
        rotationX += Input.GetAxis("Mouse Y") * rotationspeed * -1;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationY += Input.GetAxis("Mouse X") * rotationspeed;

        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusposition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y, 0);

        transform.position = focusposition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }

    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);


    public Quaternion GetPlanarRotation() {
        return Quaternion.Euler(0, rotationY, 0);
    }
}
