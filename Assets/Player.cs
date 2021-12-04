using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

public class Player : MonoBehaviour
{
    public enum PlayerState {

        nullState,
        initializing,
        readyToGetOthers,
        idle
    }

    [Serializable]
    public struct PlayerList { 
        public int playerId;

        public string playerName;

        public Vector3 position;

        public PlayerState state;

        public GameObject playerObject;
    }
    [SerializeField]
    public PlayerList[] players = new PlayerList[10];

    [Serializable]
    public struct LocalPlayerStats {
        public int playerId;

        public string playerName;

        public PlayerState state;

        public bool isLocalPlayer;
    }
    [SerializeField]
    public LocalPlayerStats localPlayerStats = new LocalPlayerStats();

    public void AddPlayer(JObject data) {
        int playerId = (int)data["playerId"];

        if(localPlayerStats.playerId == players[playerId].playerId){
            
            players[playerId] = new PlayerList() { 
                playerId = playerId, 
                playerName = (string)data["playerName"],
                position = new Vector3((float)data["position"]["x"], 
                (float)data["position"]["y"], 
                (float)data["position"]["z"]), 
                state = (PlayerState)data["state"].ToObject<int>(),
                playerObject = transform.gameObject
            };

            players[playerId].playerObject.GetComponent<Player>().localPlayerStats.isLocalPlayer = true;
        } else {

            players[playerId] = new PlayerList() { 
                playerId = playerId, 
                playerName = (string)data["playerName"],
                position = new Vector3((float)data["position"]["x"], 
                (float)data["position"]["y"], 
                (float)data["position"]["z"]), 
                state = (PlayerState)data["state"].ToObject<int>(),
                playerObject = Instantiate(transform.gameObject)
            };
            players[playerId].playerObject.transform.position = players[playerId].position;
            players[playerId].playerObject.GetComponent<Multiplayer>().enabled = false;
            
            players[playerId].playerObject.GetComponent<Player>().localPlayerStats.isLocalPlayer = false;
        
        }
    }

    public void UpdatePlayer(JObject data){
        int playerId = (int)data["playerId"];
        players[playerId].position = new Vector3((float)data["position"]["x"], 
        (float)data["position"]["y"], 
        (float)data["position"]["z"]);
        players[playerId].state = (PlayerState)data["state"].ToObject<int>();
        players[playerId].playerObject.transform.position = players[playerId].position;
    }

    public void RemovePlayer(int data){
        int playerId = (int)data;
        Destroy(players[playerId].playerObject);
        players[playerId] = new PlayerList() { playerId = playerId, position = new Vector3(0, 0, 0), state = PlayerState.nullState, playerName = "EmptyPlayerSlot" };
    }

    public PlayerList GetPlayer(int id){
        return players[id];
    }

    void Start() {
        for(var i = 0; i < players.Length; i++) {
            players[i] = new PlayerList() { playerId = i, position = new Vector3(0, 0, 0), state = PlayerState.nullState, playerName = "EmptyPlayerSlot" };
        }
        localPlayerStats.state = PlayerState.initializing;
        localPlayerStats.playerName = GameObject.Find("GlobalVariables").GetComponent<GlobalVariables>().selectedName;
        if(GameObject.Find("GlobalVariables") != null && GameObject.Find("GlobalVariables").GetComponent<GlobalVariables>().connectToServer == false) {
            localPlayerStats.playerId = 0;
            localPlayerStats.isLocalPlayer = true;
            localPlayerStats.state = PlayerState.idle;
        }
    }

    void Update() {
        if(localPlayerStats.isLocalPlayer){
            Move();
        }
    }

    void Move() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        transform.position = Vector3.Lerp(transform.position, transform.position + movement, 0.1f);

        Camera.main.transform.position = new Vector3(transform.position.x + 0.33f, Camera.main.transform.position.y, transform.position.z - 5.1f);
    }
}
