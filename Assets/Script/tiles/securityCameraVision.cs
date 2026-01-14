using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class securityCameraVision : MonoBehaviour
{
    public SecurityCamera securityCamera;
    public string playerTag = "Player";

    // FOV parameters
    public float viewRadius;
    public float viewAngle;
    public int rayCount = 50;
    public LayerMask obstacleMask;

    // Rotation parameters
    public float rotationSpeed;
    [Range(-180, 180)] public float rotationAngleStart;
    [Range(-180, 180)] public float rotationAngleEnd;
    public int rotationPause;

    // Private variables 
    Mesh mesh;
    Vector3 origin;
    float startingAngle;

    private bool isRotationPaused;

    private float targetAngle;

    private void Start()
    {
        targetAngle = rotationAngleEnd;
        isRotationPaused = false;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        securityCamera = GetComponentInParent<SecurityCamera>();
    }

    private void Update()
    {
        if (!isRotationPaused && !PlayerInVision())
        {
            float step = rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, step);

            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle)) < Math.Abs(step) + 0.1f)
            {
                StartCoroutine(PauseAndSwitchTarget());
            }   
        }
    }

    private void LateUpdate()
    {
        origin = transform.position;
        startingAngle = GetAngleFromDirection(transform.right) - 90f;
        DrawFOV();
    }

    void DrawFOV()
    {
        float angle = startingAngle - viewAngle / 2f;
        float angleIncrease = viewAngle / rayCount;
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[(rayCount) * 3];

        vertices[0] = transform.InverseTransformPoint(origin);

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 dir = GetDirectionFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, obstacleMask);
            Vector3 vertex;

            if (hit.collider == null)
            {
                vertex = origin + dir * viewRadius;
            }
            else
            {
                vertex = hit.point;
            }

            vertices[vertexIndex] = transform.InverseTransformPoint(vertex);

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            angle += angleIncrease;

        }

            mesh.Clear();
            mesh.vertices = vertices;   
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
    }

    float GetAngleFromDirection(Vector3 dir)
    {
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return angle;
    }

        Vector3 GetDirectionFromAngle(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    IEnumerator PauseAndSwitchTarget()
    {
        isRotationPaused = true;
            yield return new WaitForSeconds(rotationPause);
            isRotationPaused = false;
            if (Mathf.Approximately(targetAngle, rotationAngleEnd))
            {
                targetAngle = rotationAngleStart;
                rotationSpeed = -rotationSpeed;
            }
            else
            {
                targetAngle = rotationAngleEnd;
                rotationSpeed = -rotationSpeed;
            }
    }

    public bool PlayerInVision()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Vector3[] pointsToCheck = new Vector3[5];

        pointsToCheck[0] = playerCollider.bounds.center;
        pointsToCheck[1] = new Vector3(playerCollider.bounds.min.x, playerCollider.bounds.min.y);
        pointsToCheck[2] = new Vector3(playerCollider.bounds.min.x, playerCollider.bounds.max.y);
        pointsToCheck[3] = new Vector3(playerCollider.bounds.max.x, playerCollider.bounds.min.y);
        pointsToCheck[4] = new Vector3(playerCollider.bounds.max.x, playerCollider.bounds.max.y);

        foreach (Vector3 point in pointsToCheck)
        {
            Vector3 dirToPoint = (point - origin);
            float distanceToPoint = dirToPoint.magnitude;

            if (distanceToPoint > viewRadius) continue;
            dirToPoint.Normalize();

            Vector3 forward = Quaternion.Euler(0, 0, -90) * transform.right;
            float angleToPoint = Vector3.Angle(forward, dirToPoint);
            if (angleToPoint > viewAngle / 2f) continue;

            RaycastHit2D hit = Physics2D.Raycast(origin, dirToPoint, distanceToPoint, obstacleMask);
            if (hit.collider == null)
            {
                Debug.DrawLine(origin, point, Color.red);
                return true;
            }

        }

        return false;
    }
}

// NOTES
// - Unity Mesh
// - GetAngleFromDirection
// - GetDirectionFromAngle
// - Quaternion.Euler