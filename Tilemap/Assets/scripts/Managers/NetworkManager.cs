using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    public int firstRun = 0;
    private SelectionManager selectionManager;
    private MapManager mapManager;
    private bool joinedRoom = true;
    private bool startedGame = false;
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            gameObject.SetActive(false);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        firstRun = PlayerPrefs.GetInt("firstRun");
        if(firstRun == 0)
        {
            PlayerPrefs.SetInt("firstRun", 1);
            ChangeScene("ProfileSetup");
        }
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        startedGame = false;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master server");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(null, roomOptions);
        joinedRoom = false;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected");
        joinedRoom = true;
    }
    public void CreateRoom (string roomName)
    {
        joinedRoom = false;
        PhotonNetwork.CreateRoom(roomName);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
        joinedRoom = false;
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Player " + PlayerPrefs.GetString("PlayerName") +" joined a room");
    }

    public void JoinRoom (string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount >1)
        {
            PhotonNetwork.LoadLevel("testMap");
            photonView.RPC("ChangeScene", RpcTarget.Others, "testMap");
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if(currentScene.name == "testMap")
        {
            LoadScene();
        }
        if(currentScene.name == "mapEditor")
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.OfflineMode = true;
        }
    }
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);

    }

    [PunRPC]
    public void LoadScene()
    {
        selectionManager = GameObject.FindGameObjectWithTag("SelectionManager").GetComponent<SelectionManager>();
        mapManager = GameObject.FindGameObjectWithTag("MapManager").GetComponent<MapManager>();
        if (joinedRoom)
        {
            selectionManager.thisistheplayer = 2;
            mapManager.thisistheplayer = 2;
        }
        else
        {
            selectionManager.thisistheplayer = 1;
            mapManager.thisistheplayer = 1;
        }

    }
}
