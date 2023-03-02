using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(LineRenderer))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Different point to move to")]
    public MovingPoint[] movingPoint;
    [Header("Line to help see movement")]
    public Vector3 initialLineDisplacement;
    public VisualEffect effectForNode;
    public VisualEffect effectPlacementNode;
    public float timeToMoveIn = 5f;
    public float timeToMoveOut = 5f;
    public Vector3 velocity { get; private set; }
    private Vector3 initialPosition;
    private int currentNumber = 0;
    private Vector3 actualPoint;
    private Vector3 nextPoint;
    private float elapsedTime = 0f;
    private bool reverseMode = false;

    private float[] pointTimingIn;
    private float[] pointTimingOut;
    private float[] pointDistance;

    private LineRenderer lineRdr;
    private VisualEffect myPlacementEffect;
    void Start()
    {
        float totalDistance = 0f;
        lineRdr = GetComponent<LineRenderer>();

        initialPosition = transform.position;
        if (movingPoint != null)
        {
            pointDistance = new float[movingPoint.Length];
            pointTimingIn = new float[movingPoint.Length];
            pointTimingOut = new float[movingPoint.Length];

            Vector3 globalPos = transform.position + initialLineDisplacement;
            actualPoint = transform.position;
            nextPoint = transform.position + movingPoint[currentNumber].PositionPoint;

            lineRdr.positionCount = movingPoint.Length + 1;
            lineRdr.SetPosition(0, globalPos);
            VisualEffect tmpOrb = Instantiate(effectForNode, globalPos, Quaternion.identity);
            myPlacementEffect = Instantiate(effectPlacementNode, globalPos, Quaternion.identity);
            myPlacementEffect.gameObject.layer = gameObject.layer;
            tmpOrb.gameObject.layer = gameObject.layer;
            for (int i = 0; i < movingPoint.Length; i++)
            {
                pointDistance[i] = Vector3.Magnitude(globalPos - (globalPos + movingPoint[i].PositionPoint));
                totalDistance += pointDistance[i];

                globalPos += movingPoint[i].PositionPoint;
                lineRdr.SetPosition(i + 1, globalPos);
                tmpOrb = Instantiate(effectForNode, globalPos, Quaternion.identity);
                tmpOrb.gameObject.layer = gameObject.layer;
            }
            for (int i = 0; i < pointDistance.Length; i++)
            {
                pointTimingIn[i] = pointDistance[i] / totalDistance * timeToMoveIn * movingPoint[i].SpeedPourcentIn;
                pointTimingOut[i] = pointDistance[i] / totalDistance * timeToMoveOut * movingPoint[i].SpeedPourcentOut;
            }
        }

    }

    void Update()
    {
        if (movingPoint != null)
        {
            myPlacementEffect.transform.position = transform.position + initialLineDisplacement;
            elapsedTime += Time.deltaTime;
            if (reverseMode)
            {
                if (currentNumber >= 0)
                {
                    velocity = Vector3.Lerp(actualPoint, nextPoint, elapsedTime / pointTimingOut[currentNumber]) - transform.position;
                    transform.position = Vector3.Lerp(actualPoint, nextPoint, elapsedTime / pointTimingOut[currentNumber]);
                }
            }
            else
            {
                if (currentNumber >= 0)
                {
                    velocity = Vector3.Lerp(actualPoint, nextPoint, elapsedTime / pointTimingIn[currentNumber]) - transform.position;
                    transform.position = Vector3.Lerp(actualPoint, nextPoint, elapsedTime / pointTimingIn[currentNumber]);
                }
            }
            if (Vector3.SqrMagnitude(transform.position - nextPoint) < 0.01)
            {
                velocity = nextPoint - transform.position;
                transform.position = nextPoint;
                if (!reverseMode)
                {
                    if (currentNumber < movingPoint.Length - 1)
                    {
                        currentNumber++;
                        actualPoint = nextPoint;
                        nextPoint += movingPoint[currentNumber].PositionPoint;
                        elapsedTime = 0f;
                    }
                    else if (currentNumber == movingPoint.Length - 1)
                    {
                        currentNumber++;
                        reverseMode = true;
                        elapsedTime = 0f;
                    }
                }
                if (reverseMode)
                {
                    if (currentNumber == 0)
                    {
                        reverseMode = false;
                        currentNumber--;
                        actualPoint = nextPoint;
                        nextPoint = initialPosition;
                        elapsedTime = 0f;
                    }
                    else
                    {
                        currentNumber--;
                        actualPoint = nextPoint;
                        nextPoint -= movingPoint[currentNumber].PositionPoint;
                        elapsedTime = 0f;
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + initialLineDisplacement, 1);
        Gizmos.color = Color.blue;
        Vector3 globalPoint = (initialPosition.Equals(new Vector3())) ? transform.position : initialPosition;
        if (movingPoint != null)
        {
            for (int i = 0; i < movingPoint.Length; i++)
            {
                Gizmos.DrawLine(globalPoint, globalPoint + movingPoint[i].PositionPoint);
                globalPoint += movingPoint[i].PositionPoint;
                Gizmos.DrawSphere(globalPoint, 0.5f);
            }
        }
    }
}
