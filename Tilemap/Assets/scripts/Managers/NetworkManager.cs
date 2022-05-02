using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
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
    private void Awake()

    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        if(PlayerPrefs.HasKey("RunNumber"))
        {
            PlayerPrefs.SetInt("RunNumber", PlayerPrefs.GetInt("RunNumber") + 1);
            string saveString = File.ReadAllText(Application.streamingAssetsPath + "/map1.map");
            File.WriteAllText(Application.persistentDataPath + "/map1.map", saveString);
            File.WriteAllText(Application.persistentDataPath + "/new deck.deck", "");
            //only use this method before creating the build
            //resetPlayerPrefs();
        }
        else
        {
            //ChangeScene("ProfileSetup");
            Debug.Log("First run");
            PlayerPrefs.SetInt("RunNumber", 1);
            string saveString = File.ReadAllText(Application.streamingAssetsPath + "/map1.map");
            File.WriteAllText(Application.persistentDataPath + "/map1.map", saveString);
            File.WriteAllText(Application.persistentDataPath + "/new deck.deck", "");
        }
    }

    private void resetPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("RunNumber");
        PlayerPrefs.DeleteKey("mapname");
        PlayerPrefs.DeleteKey("decksize");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("DialogueNumber");
        for (int i = 0; i < 100; i++)
        {
            if(PlayerPrefs.HasKey("selecteddeck" + i.ToString()))
            {
                PlayerPrefs.DeleteKey("selecteddeck" + i.ToString());
            }
        }
        PlayerPrefs.DeleteKey("AIorHuman");
        Debug.Log("playerprefs resetted");
    }
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnConnectedToMaster()
    {
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
        joinedRoom = true;
    }
    public void CreateRoom (string roomName)
    {
        joinedRoom = false;
        PhotonNetwork.CreateRoom(roomName);
    }

    public override void OnCreatedRoom()
    {
        joinedRoom = false;
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
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
            photonView.RPC("ChangeScene", RpcTarget.All, "testMap");
        }
    }
    /*
    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "testMap")
        {
            LoadScene();
        }
        if (currentScene.name == "mapEditor")
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.OfflineMode = true;
        }
    }
    */
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
        if(currentScene.name == "AI")
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.OfflineMode = true;
            LoadScene();
        }
    }

    [PunRPC]
    public void AgainstAI()
    {
        PlayerPrefs.SetString("AIorHuman", "AI");
    }
    public void AgainstHuman()
    {
        PlayerPrefs.SetString("AIorHuman", "Human");
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
        if (joinedRoom && PlayerPrefs.GetString("AIorHuman") == "Human")
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
