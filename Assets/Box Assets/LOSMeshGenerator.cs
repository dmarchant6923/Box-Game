using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOSMeshGenerator : MonoBehaviour
{
    public float radius = 10;
    public int numPoints = 100;
    public int edgeResolveIterations = 4;
    public float minDistThreshold = 1;
    public float extraBufferDist = 0.1f;
    float startAngle;

    public bool includePerimeter;
    LineRenderer perimeter;

    public bool generate = true;

    int obstacleLM;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    Material newMaterial;
    Mesh mesh;

    void Awake()
    {
        obstacleLM = LayerMask.GetMask("Obstacles");

        mesh = new Mesh();
        meshFilter.mesh = mesh;

        newMaterial = Instantiate(meshRenderer.material);
        meshRenderer.material = newMaterial;

        if (includePerimeter)
        {
            perimeter = GetComponent<LineRenderer>();
        }
    }

    public void GenerateMesh()
    {
        mesh.Clear();
        if (includePerimeter)
        {
            perimeter.positionCount = 0;
        }

        if (generate == false)
        {
            return;
        }

        RayCastInfo lastRaycast = new RayCastInfo();
        RayCastInfo thisRaycast = new RayCastInfo();
        List<Vector2> meshPoints = new List<Vector2>();
        for (int i = 0; i < numPoints; i++)
        {
            float angle = startAngle + (float)i * 360 / (float)numPoints;
            Vector2 vector = Tools.AngleToVector(angle);
            RaycastHit2D raycast = Physics2D.Raycast(transform.position, vector, radius, obstacleLM);
            float distance = radius;
            if (raycast.collider != null)
            {
                distance = raycast.distance;
            }

            thisRaycast = new RayCastInfo(raycast.collider != null, vector, raycast.normal, distance, angle);

            bool checkEdge = false;
            bool checkNormals = false;
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
                checkNormals = true;
                if (i == 2)
                {
                    Debug.DrawRay(transform.position, vector * distance, Color.red);
                    Debug.DrawRay(transform.position, lastRaycast.vector * lastRaycast.distance, Color.red);
                }
                checkEdge = false;
            }

            if (i > 0 && (checkEdge || checkNormals))
            {
                RayCastInfo maxRay = thisRaycast;
                RayCastInfo minRay = lastRaycast;

                int iterations = 0;

                while (iterations < edgeResolveIterations)
                {
                    iterations++;

                    float newAngle = (maxRay.angle + minRay.angle) / 2;
                    Vector2 newVector = Tools.AngleToVector(newAngle);
                    RaycastHit2D newRaycast = Physics2D.Raycast(transform.position, newVector, radius, obstacleLM);
                    float newDistance = radius;
                    if (newRaycast.collider != null)
                    {
                        newDistance = newRaycast.distance;
                    }

                    RayCastInfo newRay = new RayCastInfo(newRaycast.collider != null, newVector, newRaycast.normal, newDistance, newAngle);

                    //if (i == 3)
                    //{
                    //    Debug.DrawRay(rb.position, newRay.vector * newRay.distance, new Color(0, (float)iterations / edgeResolveIterations, (float)iterations / edgeResolveIterations));
                    //}

                    if (checkEdge)
                    {
                        if ((newRay.hit == minRay.hit) && Mathf.Abs(newRay.distance - minRay.distance) < minDistThreshold)
                        {
                            minRay = newRay;
                        }
                        else
                        {
                            maxRay = newRay;
                        }
                    }
                    else if (checkNormals)
                    {
                        if (newRay.hit && minRay.hit && Vector2.Dot(newRay.normal, minRay.normal) > 0.9f)
                        {
                            minRay = newRay;
                        }
                        else
                        {
                            maxRay = newRay;
                        }
                    }
                }

                minRay.distance = Mathf.Min(minRay.distance + extraBufferDist, radius);
                maxRay.distance = Mathf.Min(maxRay.distance + extraBufferDist, radius);

                meshPoints.Add(new Vector2(transform.position.x, transform.position.y) + minRay.vector * minRay.distance);
                meshPoints.Add(new Vector2(transform.position.x, transform.position.y) + maxRay.vector * maxRay.distance);

                if (includePerimeter)
                {
                    perimeter.positionCount++;
                    perimeter.SetPosition(meshPoints.Count - 2, new Vector2(transform.position.x, transform.position.y) + minRay.vector * minRay.distance);
                    
                    perimeter.positionCount++;
                    perimeter.SetPosition(meshPoints.Count - 1, new Vector2(transform.position.x, transform.position.y) + maxRay.vector * maxRay.distance);
                }
            }

            //Debug.DrawRay(rb.position, vector * distance, Color.red);

            distance = Mathf.Min(distance + extraBufferDist, radius);
            meshPoints.Add(new Vector2(transform.position.x, transform.position.y) + vector * distance);
            lastRaycast = thisRaycast;

            if (includePerimeter)
            {
                perimeter.positionCount++;
                perimeter.SetPosition(meshPoints.Count - 1, new Vector2(transform.position.x, transform.position.y) + vector * distance);
            }
        }

        int vertexCount = meshPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 1) * 3];
        //vertices[0] = rb.position;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(meshPoints[i]);

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

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        if (includePerimeter == false)
        {
            return;
        }
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
