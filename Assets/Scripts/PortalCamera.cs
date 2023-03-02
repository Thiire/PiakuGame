using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    [Header("Position Reference")]
    public Transform originalToCopy;
    public Transform myFlag;
    public Transform otherFlag;
    void Start()
    {

    }

    void Update()
    {
        if (originalToCopy == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 playerOffset = originalToCopy.position - otherFlag.position;
        transform.position = myFlag.position + playerOffset;

        float angularDifference = Quaternion.Angle(myFlag.rotation, otherFlag.rotation);

        Quaternion flagRotationDifference = Quaternion.AngleAxis(angularDifference, originalToCopy.up);
        Vector3 newCameraRotation = flagRotationDifference * originalToCopy.forward;
        transform.rotation = Quaternion.LookRotation(newCameraRotation, originalToCopy.up);
    }
}
