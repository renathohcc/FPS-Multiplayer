using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public static PlayerSpawner instance;

    void Awake(){
        instance = this;
    }

    public GameObject playerPrefab;
    private GameObject player;
    void Start()
    {
        if(PhotonNetwork.IsConnected){
            SpawnPlayer();
        }
    }

    public void SpawnPlayer(){
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

    }

}
