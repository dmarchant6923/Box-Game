using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tools : MonoBehaviour
{
    public static Vector2 AngleToVector(float angle)
    {
        Vector2 vector =  new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
        return vector;
    }

    public static Vector3 AngleToVector3(float angle)
    {
        Vector3 vector = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
        return vector;
    }

    public static float VectorToAngle(Vector2 vector)
    {
        vector = vector.normalized;
        float angle = -Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
        return angle;     
    }

    //used to detect obstacles in a line of sight, built to ignore fences. returns TRUE if there is nothing, FALSE otherwise.
    //DO NOT normalize vector before passing it through.
    public static bool LineOfSight(Vector2 startPosition, Vector2 fullVector)
    {
        RaycastHit2D[] raycastAll = Physics2D.RaycastAll(startPosition, fullVector.normalized, fullVector.magnitude, LayerMask.GetMask("Obstacles"));
        bool result = true;

        foreach (RaycastHit2D item in raycastAll)
        {
            if (item.collider.tag != "Fence")
            {
                result = false;
                break;
            }
        }

        return result;
    }

    //inputs are the initial direction, input direction from the control stick, and a chosen multiplier.
    //Outputs a normalized result vector.
    public static Vector2 DI(Vector2 initialDirection, Vector2 inputDirection, float mult)
    {
        initialDirection = initialDirection.normalized;
        if (inputDirection.magnitude < 0.1f)
        {
            inputDirection = Vector2.zero;
        }
        if (inputDirection.magnitude > 1)
        {
            inputDirection = inputDirection.normalized;
        }
        inputDirection *= mult;
        Vector2 perpVector = Vector2.Perpendicular(initialDirection) * Vector2.Dot(inputDirection, Vector2.Perpendicular(initialDirection));
        Vector2 resultVector = (initialDirection + perpVector).normalized;
        if (Vector2.Dot(resultVector, initialDirection) > 0.999f)
        {
            resultVector = initialDirection;
        }

        return resultVector;
    }

    public static void DebugDrawBox(Vector2 position, float length, float height)
    {
        DebugDrawBox(position, length, height, Color.white);
    }

    public static void DebugDrawBox(Vector2 position, float length, float height, Color color)
    {
        Debug.DrawRay(position + Vector2.up * height / 2 + Vector2.left * length / 2, Vector2.right * length, color);
        Debug.DrawRay(position + Vector2.down * height / 2 + Vector2.left * length / 2, Vector2.right * length, color);

        Debug.DrawRay(position + Vector2.left * length / 2 + Vector2.down * height / 2, Vector2.up * height, color);
        Debug.DrawRay(position + Vector2.right * length / 2 + Vector2.down * height / 2, Vector2.up * height, color);
    }

    public static void DebugDrawMarker(Vector2 position)
    {
        DebugDrawMarker(position, Color.white);
    }
    public static void DebugDrawMarker(Vector2 position, Color color)
    {
        Debug.DrawRay(position + Vector2.one * 0.5f, -Vector2.one, color);
        Debug.DrawRay(position + new Vector2(-1, 1) * 0.5f, new Vector2(1, -1), color);
    }
}
