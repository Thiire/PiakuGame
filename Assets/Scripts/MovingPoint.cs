using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MovingPoint
{
    public Vector3 PositionPoint;
    public float SpeedPourcentIn;
    public float SpeedPourcentOut;

    MovingPoint(Vector3 point, float speedIn, float speedOut)
    {
        PositionPoint = point;
        SpeedPourcentIn = speedIn;
        SpeedPourcentOut = speedOut;
    }
}
