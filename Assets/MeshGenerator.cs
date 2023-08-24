using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public float radius = 10;
    public float offset = 1;
    public int numPoints = 100;
    public int edgeResolveIterations = 4;
    public float minDistThreshold = 1;
    float startAngle = 0;

    int obstacleLM;

    Rigidbody2D rb;

    public MeshFilter meshFilter;
    Mesh mesh;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        obstacleLM = LayerMask.GetMask("Obstacles");

        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    private void LateUpdate()
    {
        RayCastInfo lastRaycast = new RayCastInfo();
        RayCastInfo thisRaycast = new RayCastInfo();
        List<Vector2> meshPoints = new List<Vector2>();
        for (int i = 0; i < numPoints; i++)
        {
            float angle = startAngle + (float)i * 360 / (float)numPoints;
            Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2), Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2));
            RaycastHit2D raycast = Physics2D.Raycast(rb.position, vector, radius, obstacleLM);
            float distance = radius;
            if (raycast.collider != null)
            {
                distance = raycast.distance;
            }

            thisRaycast = new RayCastInfo(raycast.collider != null, vector, raycast.normal, distance, angle);

            bool checkEdge = false;
            if (Mathf.Abs(lastRaycast.distance - thisRaycast.distance) > minDistThreshold)
            {
                checkEdge = true;
            }
            if (thisRaycast.hit != lastRaycast.hit)
            {
                checkEdge = true;
            }
            if (thisRaycast.hit && lastRaycast.hit && Vector2.Dot(thisRaycast.normal, lastRaycast.normal) < 0.9f)
            {
                checkEdge = true;
            }

            if (i > 0 && checkEdge)
            {
                RayCastInfo maxRay = thisRaycast;
                RayCastInfo minRay = lastRaycast;

                int iterations = 0;

                while (iterations < edgeResolveIterations)
                {
                    iterations++;

                    float newAngle = (maxRay.angle + minRay.angle) / 2;
                    Vector2 newVector = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad + Mathf.PI / 2), Mathf.Sin(newAngle * Mathf.Deg2Rad + Mathf.PI / 2));
                    RaycastHit2D newRaycast = Physics2D.Raycast(rb.position, newVector, radius, obstacleLM);
                    float newDistance = radius;
                    if (newRaycast.collider != null)
                    {
                        newDistance = newRaycast.distance;
                    }

                    RayCastInfo newRay = new RayCastInfo(newRaycast.collider != null, newVector, newRaycast.normal, newDistance, newAngle);

                    //if (i == 3)
                    //{
                    //    Debug.DrawRay(rb.position, newRay.vector * newRay.distance, new Color (0, (float)iterations / edgeResolveIterations, (float)iterations / edgeResolveIterations));
                    //}

                    bool updateMin = true;
                    if (Mathf.Abs(newRay.distance - minRay.distance) > minDistThreshold)
                    {
                        updateMin = false;
                    }
                    if (newRay.hit != minRay.hit)
                    {
                        updateMin = false;
                    }
                    if (newRay.hit && minRay.hit && Vector2.Dot(thisRaycast.normal, lastRaycast.normal) < 0.9f)
                    {
                        updateMin = false;
                    }

                    if ((newRay.hit == minRay.hit) && Mathf.Abs(newRay.distance - minRay.distance) < minDistThreshold)
                    {
                        minRay = newRay;
                    }
                    else
                    {
                        maxRay = newRay;
                    }
                }

                meshPoints.Add(minRay.vector * minRay.distance);
                meshPoints.Add(maxRay.vector * maxRay.distance);


            }

            Debug.DrawRay(rb.position, vector * distance, Color.red);

            meshPoints.Add(vector * distance);
            lastRaycast = thisRaycast;
        }

        int vertexCount = meshPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 1) * 3];

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = meshPoints[i];

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        triangles[(vertexCount - 2) * 3] = 0;
        triangles[(vertexCount - 2) * 3 + 1] = 1;
        triangles[(vertexCount - 2) * 3 + 2] = vertexCount - 1;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    public struct RayCastInfo
    {
        public bool hit;
        public Vector2 vector;
        public Vector2 normal;
        public float distance;
        public float angle;

        public RayCastInfo(bool _hit, Vector2 _vector, Vector2 _normal, float _distance, float _angle)
        {
            hit = _hit;
            vector = _vector;
            normal = _normal;
            distance = _distance;
            angle = _angle;

        }
    }
}
