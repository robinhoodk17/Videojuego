using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;

public class GridManager : MonoBehaviour
{
    private Camera _mainCamera;
    public Tilemap map;
    private PhotonView photonView;
    private void Start()
    {
        _mainCamera = Camera.main;
        photonView = PhotonView.Get(this);
    }
    public void selectedUnitWaits(unitScript selectedunit, Vector3Int gridposition)
    {
        photonView.RPC("selectedUnitWaits", RpcTarget.All, selectedunit, gridposition);
    }
    [PunRPC]
    public void Waiting(unitScript selectedunit, Vector3Int gridposition)
    {
        selectedunit.gameObject.transform.position = worldPosition(gridposition);
        selectedunit.exhausted = true;
        selectedunit.sprite.color = new Color(.6f, .6f, .6f);
    }
    public Vector3Int gridPosition(Vector3 position, bool screen = false)
    {
        if (screen)
        {
            position = _mainCamera.ScreenToWorldPoint(position);
        }

        Vector3Int gridposition = map.WorldToCell(position);
        return gridposition;
    }
    public Vector3 worldPosition(Vector3Int gridposition)
    {
        return map.CellToWorld(gridposition);
    }
}
