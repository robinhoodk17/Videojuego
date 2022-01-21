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
    public float panborderthickness = 2f;
    public float zoomspeedwithwheel = 20f;
    public int invertZoomWheel = 1;

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

        //move up
        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - panborderthickness)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 up = new Vector3(0, .5f, 0);
                cam.transform.position = ClampCamera(cam.transform.position + up);
            }

        }

        //move down
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= panborderthickness)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 down = new Vector3(0, -.5f, 0);
                cam.transform.position = ClampCamera(cam.transform.position + down);
            }
        }


        //move left
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= panborderthickness)
        {
            if (Time.time - lastStep > timeBetweenSteps )
            {
                lastStep = Time.time;
                Vector3 left = new Vector3(-.5f, 0, 0);
                cam.transform.position = ClampCamera(cam.transform.position + left);
            }
        }


        //move right
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - panborderthickness)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                Vector3 right = new Vector3(.5f, 0, 0);
                cam.transform.position = ClampCamera(cam.transform.position + right);
            }
        }


        //zoom in
        if (Input.GetKey(KeyCode.Z))
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                float newSize = cam.orthographicSize - zoomStep;
                cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
                cam.transform.position = ClampCamera(cam.transform.position);
            }
        }


        //zoom out
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


        //zoom with mouse wheel
        float newsize = Input.GetAxis("Mouse ScrollWheel");
        //the invert zoom wheel can get the value 1 or -1, depending on the key bindings
        cam.orthographicSize += invertZoomWheel * newsize * zoomspeedwithwheel * 20 * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minCamSize, maxCamSize);
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
