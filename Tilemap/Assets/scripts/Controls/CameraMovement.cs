using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
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
    float maxCamSize = 40;
    [SerializeField]
    Tilemap map;

    float lastStep;
    float CenteredStep;
    public float timeBetweenSteps = 0.15f;
    private float timeBetweenCenters = .4f;
    public float panborderthickness = 2f;
    public float zoomspeedwithwheel = 20f;
    public int invertZoomWheel = 1;
    public bool paused = false;

    private float mapminx, mapmaxx, mapminy, mapmaxy;

    public bool wpressed, apressed, spressed, dpressed, zpressed, xpressed = false;

    public int wcounter, acounter, scounter, dcounter, zcounter, xcounter = 0;
    float previousSize = 0;
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
        if (wpressed || Mouse.current.position.ReadValue().y >= Screen.height - panborderthickness && !paused)
        {    
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                CenteredStep = Time.time;
                Vector3 up = new Vector3(0, .5f, 0);
                cam.transform.position = ClampCamera(cam.transform.position + up);
            }
        }
        //move down
        if (spressed || Mouse.current.position.ReadValue().y <= panborderthickness && !paused)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                CenteredStep = Time.time;
                Vector3 down = new Vector3(0, -.5f, 0);
                cam.transform.position = ClampCamera(cam.transform.position + down);
            }
        }
        //move left
        if (apressed || Mouse.current.position.ReadValue().x <= panborderthickness && !paused)
        {
            if (Time.time - lastStep > timeBetweenSteps )
            {
                lastStep = Time.time;
                CenteredStep = Time.time;
                Vector3 left = new Vector3(-.5f, 0, 0);
                cam.transform.position = ClampCamera(cam.transform.position + left);
            }
        }
        //move right
        if (dpressed || Mouse.current.position.ReadValue().x >= Screen.width - panborderthickness && !paused)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                CenteredStep = Time.time;
                Vector3 right = new Vector3(.5f, 0, 0);
                cam.transform.position = ClampCamera(cam.transform.position + right);
            }
        }


        //zoom in
        if (zpressed && !paused)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                CenteredStep = Time.time;
                float newSize = cam.orthographicSize - zoomStep;
                cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);
                cam.transform.position = ClampCamera(cam.transform.position);
            }
        }
        //zoom out
        if (xpressed && !paused)
        {
            if (Time.time - lastStep > timeBetweenSteps)
            {
                lastStep = Time.time;
                CenteredStep = Time.time;
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
        if(cam.orthographicSize != previousSize)
        {
            CenterCameraonPosition(cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
            CenteredStep = Time.time;
            previousSize = cam.orthographicSize;
        }
    }

    public void ReceiveW()
    {
        wcounter++;
        if(wcounter == 2)
        {
            wcounter = 0;
            if(wpressed)    { wpressed = false;}
            else    {wpressed = true;}
        }
    }
    public void ReceiveA()
    {
        acounter++;
        if(acounter == 2)
        {
            acounter = 0;
            if(apressed)    { apressed = false;}
            else    {apressed = true;}
        }
    }
    public void ReceiveS()
    {
        scounter++;
        if(scounter == 2)
        {
            scounter = 0;
            if(spressed)    { spressed = false;}
            else    {spressed = true;}
        }
    }
    public void ReceiveD()
    {
        dcounter++;
        if(dcounter == 2)
        {
            dcounter = 0;
            if(dpressed)    { dpressed = false;}
            else    {dpressed = true;}
        }
    }
    public void ReceiveZ()
    {
        zcounter++;
        if(zcounter == 2)
        {
            zcounter = 0;
            if(zpressed)    { zpressed = false;}
            else    {zpressed = true;}
        }
    }
    public void ReceiveX()
    {
        xcounter++;
        if(xcounter == 2)
        {
            xcounter = 0;
            if(xpressed)    { xpressed = false;}
            else    {xpressed = true;}
        }
    }

    public void CenterCameraonPosition(Vector2 Position)
    {
        if(Time.time - CenteredStep > timeBetweenCenters)
        {
            //Setting the camera to the unit we are about to move
            float cam_x = Position.x;
            float cam_y = Position.y;
            float cam_z = cam.transform.position.z;
            cam.transform.position =new Vector2(cam_x, cam_y);
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
