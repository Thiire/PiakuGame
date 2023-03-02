using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalSpin : MonoBehaviour
{
    [Range(5f, 50f)] public float RotateSpeed = 10f;
    void Update()
    {
        transform.Rotate(0f, RotateSpeed * Time.deltaTime, 0f, Space.Self);
    }
}
