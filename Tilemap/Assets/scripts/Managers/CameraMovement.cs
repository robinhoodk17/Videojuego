using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    Camera cam;
    [SerializeField]
    float zoomStep = .25f;
    [SerializeField]
    float minCamSize = 1;
    [SerializeField]
    float maxCamSize = 20;
    [SerializeField]
    Tilemap map;

    float lastStep;
    public float timeBetweenSteps = 0.15f;

    private float mapminx, mapmaxx, mapminy, mapmaxy;
    // Update is called once per frame

    private void Awake()
    {
        mapminx = map.cellBounds.xMin;
        mapmaxx = map.cellBounds.xMax;
        mapminy = map.cellBounds.yMin;
        mapmaxy = map.cellBounds.yMax;
    }
    void LateUpdate()
    {
        if(Input.anyKey)
        {
            PanCamera();
            Zoom();
        }

    }

    private void PanCamera()
    {

        if(Input.GetKey(KeyCode.W))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 up = new Vector3(0, .5f, 0);
                cam.transform.position = ClampCamera(cam.transform.position + up);
            }
            
        }

        if (Input.GetKey(KeyCode.S))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 down = new Vector3(0, -.5f, 0);
                cam.transform.position = ClampCamera(cam.transform.position + down);
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 right = new Vector3(.5f, 0, 0);
                cam.transform.position = ClampCamera(cam.transform.position + right);
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 left = new Vector3(-.5f, 0, 0);
                cam.transform.position = ClampCamera(cam.transform.position + left);
            }
        }

        

    }

    private void Zoom()
    {
        if(Input.GetKey(KeyCode.Z))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                float newSize = cam.orthographicSize - zoomStep;
                cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
                cam.transform.position = ClampCamera(cam.transform.position);
            }
        }
        if (Input.GetKey(KeyCode.X))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                float newSize = cam.orthographicSize + zoomStep;
                cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
                cam.transform.position = ClampCamera(cam.transform.position);
            }
        }
    }

    private Vector3 ClampCamera(Vector3 targetPosition)
    {
        float camHeight = cam.orthographicSize/2;
        float camWidth = cam.orthographicSize * cam.aspect/2;

        float minX = mapminx + camWidth;
        float maxX = mapmaxx - camWidth;
        float minY = mapminy + camHeight;
        float maxY = mapmaxy - camHeight;

        float newX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, targetPosition.z);
    }
}
