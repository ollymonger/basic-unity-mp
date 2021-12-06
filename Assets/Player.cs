using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine.InputSystem;

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

        public Quaternion rotation;

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

        public Canvas playerCanvas;

        public TMPro.TMP_Text playerNameText;
    }
    [SerializeField]
    public LocalPlayerStats localPlayerStats = new LocalPlayerStats();

    private Localplayer bindings;
    private InputAction movementBindings;
    private InputAction lookBindings;
    private InputAction fireBindings;

    public void AddPlayer(JObject data) {
        int playerId = (int)data["playerId"];

        if(localPlayerStats.playerId == players[playerId].playerId){
            
            players[playerId] = new PlayerList() { 
                playerId = playerId, 
                playerName = (string)data["playerName"],
                position = new Vector3((float)data["position"]["x"], 
                (float)data["position"]["y"], 
                (float)data["position"]["z"]), 
                rotation = new Quaternion((float)data["rotation"]["x"],
                (float)data["rotation"]["y"],
                (float)data["rotation"]["z"],
                (float)data["rotation"]["w"]),
                state = (PlayerState)data["state"].ToObject<int>(),
                playerObject = transform.gameObject
            };
            players[playerId].playerObject.transform.GetChild(0).GetComponent<Canvas>().enabled = false;
            players[playerId].playerObject.GetComponent<Player>().localPlayerStats.isLocalPlayer = true;
        } else {
            GameObject clone = Instantiate(transform.gameObject);
            clone.name = (string)data["playerName"];
            players[playerId] = new PlayerList() { 
                playerId = playerId, 
                playerName = (string)data["playerName"],
                position = new Vector3((float)data["position"]["x"], 
                (float)data["position"]["y"], 
                (float)data["position"]["z"]),
                rotation = new Quaternion((float)data["rotation"]["x"],
                (float)data["rotation"]["y"],
                (float)data["rotation"]["z"],
                (float)data["rotation"]["w"]),
                state = (PlayerState)data["state"].ToObject<int>(),
                playerObject = clone
            };
            players[playerId].playerObject.transform.position = players[playerId].position;
            players[playerId].playerObject.transform.rotation = players[playerId].rotation;
            players[playerId].playerObject.GetComponent<Multiplayer>().enabled = false;
            players[playerId].playerObject.GetComponent<Player>().localPlayerStats.isLocalPlayer = false;
            players[playerId].playerObject.GetComponent<Player>().localPlayerStats.playerName = players[playerId].playerName;
            players[playerId].playerObject.transform.GetChild(0).GetComponent<Canvas>().enabled = true;
            players[playerId].playerObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TMPro.TMP_Text>().SetText(players[playerId].playerName);
        }
    }

    public void UpdatePlayer(JObject data){
        int playerId = (int)data["playerId"];
        players[playerId].position = new Vector3((float)data["position"]["x"], 
        (float)data["position"]["y"], 
        (float)data["position"]["z"]);
        players[playerId].rotation = new Quaternion((float)data["rotation"]["x"],
        (float)data["rotation"]["y"],
        (float)data["rotation"]["z"],
        (float)data["rotation"]["w"]);

        players[playerId].state = (PlayerState)data["state"].ToObject<int>();
        players[playerId].playerObject.transform.position = players[playerId].position;
        players[playerId].playerObject.transform.rotation = players[playerId].rotation;
    }

    public void RemovePlayer(int data){
        int playerId = (int)data;
        Destroy(players[playerId].playerObject);
        players[playerId] = new PlayerList() { playerId = playerId, position = new Vector3(0, 0, 0), state = PlayerState.nullState, playerName = "EmptyPlayerSlot" };
    }

    public PlayerList GetPlayer(int id){
        return players[id];
    }

      private Vector3 offset;

    void Start() {
        offset = new Vector3(transform.position.x, transform.position.y + 8.0f, transform.position.z + 7.0f);
        for(var i = 0; i < players.Length; i++) {
            players[i] = new PlayerList() { playerId = i, position = new Vector3(0, 0, 0), state = PlayerState.nullState, playerName = "EmptyPlayerSlot" };
        }
        Cursor.visible = false;
        localPlayerStats.state = PlayerState.initializing;
        localPlayerStats.playerCanvas = transform.GetChild(0).GetComponent<Canvas>();
        localPlayerStats.playerNameText = localPlayerStats.playerCanvas.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TMP_Text>();
        localPlayerStats.playerNameText.SetText(localPlayerStats.playerName);
        Debug.Log("Player Name: " + localPlayerStats.playerName);
        localPlayerStats.playerCanvas.worldCamera = Camera.main;
        
        // listen to fire binding context
        fireBindings.started += ctx => Fire();
        
        if(GameObject.Find("GlobalVariables") != null && GameObject.Find("GlobalVariables").GetComponent<GlobalVariables>().connectToServer == false) {
            localPlayerStats.playerId = 0;
            localPlayerStats.isLocalPlayer = true;
            localPlayerStats.state = PlayerState.idle;
        }
    }

    void Awake() {
        bindings = new Localplayer();
        bindings.Enable();
        movementBindings = bindings.Player.Move;
        lookBindings = bindings.Player.Look;
        fireBindings = bindings.Player.Fire;
    }

    float turnSmoothVelocity;
    Vector3 velocity;

    void Update() {
        if(localPlayerStats.isLocalPlayer){
            Vector2 movement = movementBindings.ReadValue<Vector2>();
            Vector3 movementVector = new Vector3(movement.x, 0, movement.y);

            Vector2 look = lookBindings.ReadValue<Vector2>();
            Vector3 lookVector = new Vector3(look.x, 0, look.y);
            float targetAngle = Mathf.Atan2(lookVector.x, lookVector.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 moveVelocity = movementVector.z * moveDir + movementVector.x * transform.right;
            velocity = moveVelocity * 10.0f;
            Vector3 targetPosition = transform.position + velocity * Time.deltaTime;
            transform.position = targetPosition;

            offset = Quaternion.AngleAxis(look.x * 0.5f, Vector3.up) * offset;
            Camera.main.transform.position = transform.position + offset; 
            Camera.main.transform.LookAt(transform.position);
        }
    }

    void Fire() {
        // Send this command to the server
        var data = new JObject();
        data["playerId"] = localPlayerStats.playerId;
        data["type"] = "fire";
        transform.GetComponent<Multiplayer>().SendCommand(data);
    }

    void LateUpdate(){
        if(localPlayerStats.isLocalPlayer){
          // Get look Delta
        }
    }
}
