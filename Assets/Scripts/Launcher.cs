using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;
    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    public GameObject menuButtons;
    void Start()
    {
        CloseMenus();
        //Connecting to the server
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to the server...";
        //To connect with photon server
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connected using settings ON");
    }

    public void CloseMenus(){
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        //Connecting to the lobby
        PhotonNetwork.JoinLobby();
        Debug.Log("Joined Lobby");
        loadingText.text = "Joining in the Lobby";
        
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
