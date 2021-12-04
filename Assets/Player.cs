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
        public Vector3 position;

        public PlayerState state;

        public GameObject playerObject;
    }
    [SerializeField]
    public PlayerList[] players = new PlayerList[10];

    [Serializable]
    public struct LocalPlayerStats {
        public int playerId;
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
                position = new Vector3((float)data["position"]["x"], 
                (float)data["position"]["y"], 
                (float)data["position"]["z"]), 
                state = (PlayerState)data["state"].ToObject<int>(),
                playerObject = Instantiate(transform.gameObject)
            };
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

    public PlayerList GetPlayer(int id){
        return players[id];
    }

    void Start() {
        for(var i = 0; i < players.Length; i++) {
            players[i] = new PlayerList() { playerId = i, position = new Vector3(0, 0, 0), state = PlayerState.nullState };
        }
        localPlayerStats.state = PlayerState.initializing;
    }

    void Update() {
        if(localPlayerStats.isLocalPlayer){
            Move();
        }
    }

    void Move() {
        // Improve this
        if(Input.GetKey(KeyCode.W)){
            transform.position += new Vector3(0, 0, 0.1f);
        }
        if(Input.GetKey(KeyCode.S)){
            transform.position += new Vector3(0, 0, -0.1f);
        }
        if(Input.GetKey(KeyCode.A)){
            transform.position += new Vector3(-0.1f, 0, 0);
        }
        if(Input.GetKey(KeyCode.D)){
            transform.position += new Vector3(0.1f, 0, 0);
        }

    }
}
