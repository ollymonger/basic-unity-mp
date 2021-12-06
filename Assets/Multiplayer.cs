using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

using NativeWebSocket;

public class Multiplayer : MonoBehaviour
{
    public WebSocket websocket;

    public Player player;

    async void Start()
    {
        if(GameObject.Find("GlobalVariables").GetComponent<GlobalVariables>().connectToServer)
            {
            websocket = new WebSocket("ws://"+GameObject.Find("GlobalVariables").GetComponent<GlobalVariables>().IPAddress+"/");

            websocket.OnOpen += () =>
            {
                Debug.Log("Connected");
                SendInitialMessage();
            };

            websocket.OnError += (e) =>
            {
            };

            websocket.OnClose += (e) =>
            {
                 Debug.Log(e.ToString());
            };

            websocket.OnMessage += (bytes) =>
            {
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                var obj = JObject.Parse(json);
                // handle message.type
                switch(obj["type"].ToString()){
                    case "initial_response":
                        HandleInitialResponse(obj);
                        player.localPlayerStats.state = Player.PlayerState.readyToGetOthers;
                        break;
                    case "get_all_connected_clients_response":
                        HandleGetAllConnectedClientsResponse(obj);
                        break;
                    case "update_position_and_rotation_response":
                        HandleUpdatePositionAndRotationResponse(obj);
                        break;
                    case "add_player":
                        HandleAddPlayer(obj);
                        break;
                    case "fire_response":
                        Debug.Log(obj);
                        break;
                    case "disconnect_response":
                        HandleDisconnectResponse(obj);
                        break;
                    default:
                        break;
                }
            };

            // Keep sending messages at every 0.3s
            InvokeRepeating("SendUpdatePositionAndRotationMessage", 0.0005f, 0.01f);

            await websocket.Connect();
        }
  }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if(GameObject.Find("GlobalVariables").GetComponent<GlobalVariables>().connectToServer){
            websocket.DispatchMessageQueue();
        }
        #endif
    }

    async void SendInitialMessage() {
        if(websocket.State == WebSocketState.Open){
            var message = new JObject();
            message["type"] = "initial";
            if(player.localPlayerStats.playerName != null && player.localPlayerStats.playerName.Trim() == ""){
                message["name"] = "Player";
            } else {
                message["name"] = player.localPlayerStats.playerName;
            }
            var json = System.Text.Encoding.UTF8.GetBytes(message.ToString());
            await websocket.Send(json);
        }
    }

    void HandleInitialResponse(JObject data){
      if(websocket.State == WebSocketState.Open){
          if(data["type"].ToObject<string>() == "error"){
              Debug.Log("Error: " + data["message"].ToObject<string>());
              websocket.Close();
        } else {
            player.localPlayerStats.playerId = data["id"].ToObject<int>();
            SendGetAllConnectedClients();
          }
      }
  }

    async void SendGetAllConnectedClients(){
        if(websocket.State == WebSocketState.Open){
            var message = new JObject();
            message["type"] = "get_all_connected_clients";
            message["id"] = player.localPlayerStats.playerId;
            var json = System.Text.Encoding.UTF8.GetBytes(message.ToString());
            await websocket.Send(json);
        }
  }

    public void SendCommand(JObject data){
        if((string)data["type"].ToObject<string>() == "fire") {
            if(websocket.State == WebSocketState.Open){
                var json = System.Text.Encoding.UTF8.GetBytes(data.ToString());
                websocket.Send(json);
            }
        }    
    }

    void HandleGetAllConnectedClientsResponse(JObject data){
      
        if(websocket.State == WebSocketState.Open){
            var players = data["players"];
            player.localPlayerStats.state = Player.PlayerState.idle;
            foreach(var client in players){
                if(client["state"] != null && client["state"].ToObject<int>() > 0){
                    int playerid;
                    Vector3 position;
                    playerid = client["id"].ToObject<int>();
                    position = new Vector3(client["position"]["x"].ToObject<float>(), client["position"]["y"].ToObject<float>(), client["position"]["z"].ToObject<float>());

                    var json = new JObject();
                    json["playerId"] = playerid;
                    json["playerName"] = client["name"].ToObject<string>();
                    json["position"] = new JObject();
                    json["position"]["x"] = position.x;
                    json["position"]["y"] = position.y;
                    json["position"]["z"] = position.z;
                    json["rotation"] = new JObject();
                    json["rotation"]["x"] = client["rotation"]["x"].ToObject<float>();
                    json["rotation"]["y"] = client["rotation"]["y"].ToObject<float>();
                    json["rotation"]["z"] = client["rotation"]["z"].ToObject<float>();
                    json["rotation"]["w"] = client["rotation"]["w"].ToObject<float>();
                    json["state"] = client["state"].ToObject<int>();
                    Debug.Log(json.ToString());
                    player.AddPlayer(json);
                }
            }
        }
  }

    async void SendUpdatePositionAndRotationMessage(){
        if(websocket.State == WebSocketState.Open && player.localPlayerStats.state == Player.PlayerState.idle){
            // Check to see if the player has even moved 
            
            if(transform.GetComponent<Rigidbody>().velocity.magnitude > 0)
            {
                var message = new JObject();
                message["type"] = "update_position_and_rotation";
                message["id"] = player.localPlayerStats.playerId;
                message["position"] = new JObject();
                message["position"]["x"] = player.transform.position.x;
                message["position"]["y"] = player.transform.position.y;
                message["position"]["z"] = player.transform.position.z;
                message["rotation"] = new JObject();
                message["rotation"]["x"] = player.transform.rotation.x;
                message["rotation"]["y"] = player.transform.rotation.y;
                message["rotation"]["z"] = player.transform.rotation.z;
                message["rotation"]["w"] = player.transform.rotation.w;

                message["state"] = ((int)player.localPlayerStats.state);
                var json = System.Text.Encoding.UTF8.GetBytes(message.ToString());
                await websocket.Send(json);
            }
        }
    }
    void HandleUpdatePositionAndRotationResponse(JObject data){
        if(websocket.State == WebSocketState.Open && data["id"].ToObject<int>() != player.localPlayerStats.playerId){
            var playerToUpdate = player.GetPlayer(data["id"].ToObject<int>());
            Debug.Log(data.ToString());
            if(playerToUpdate.playerObject != null){
                var obj = new JObject();

                obj["playerId"] = data["id"];
                obj["state"] = data["state"];
                obj["position"] = new JObject();
                obj["position"]["x"] = data["position"]["x"].ToObject<float>();
                obj["position"]["y"] = data["position"]["y"].ToObject<float>();
                obj["position"]["z"] = data["position"]["z"].ToObject<float>();
                obj["rotation"] = new JObject();
                obj["rotation"]["x"] = data["rotation"]["x"].ToObject<float>();
                obj["rotation"]["y"] = data["rotation"]["y"].ToObject<float>();
                obj["rotation"]["z"] = data["rotation"]["z"].ToObject<float>();
                obj["rotation"]["w"] = data["rotation"]["w"].ToObject<float>();

                player.UpdatePlayer(obj);
            }            
        }
    }

    void HandleAddPlayer(JObject data){
        if(websocket.State == WebSocketState.Open){
            var playerToUpdate = player.GetPlayer(data["id"].ToObject<int>());

            if(playerToUpdate.playerObject == null && player.localPlayerStats.state >= Player.PlayerState.readyToGetOthers){
                var obj = new JObject();

                obj["playerId"] = data["id"].ToObject<int>();
                obj["playerName"] = data["name"].ToObject<string>();
                obj["state"] = data["state"].ToObject<int>();
                obj["position"] = new JObject();
                obj["position"]["x"] = data["position"]["x"].ToObject<float>();
                obj["position"]["y"] = data["position"]["y"].ToObject<float>();
                obj["position"]["z"] = data["position"]["z"].ToObject<float>();
                obj["rotation"] = new JObject();
                obj["rotation"]["x"] = data["rotation"]["x"].ToObject<float>();
                obj["rotation"]["y"] = data["rotation"]["y"].ToObject<float>();
                obj["rotation"]["z"] = data["rotation"]["z"].ToObject<float>();
                obj["rotation"]["w"] = data["rotation"]["w"].ToObject<float>();

                player.AddPlayer(obj);
            }            
        }
    }

    void HandleDisconnectResponse(JObject data){
        if(websocket.State == WebSocketState.Open){
            var playerToRemove = player.GetPlayer(data["id"].ToObject<int>());

            if(playerToRemove.playerObject != null){
                player.RemovePlayer(data["id"].ToObject<int>());
            }            
        }
    }

    private async void OnApplicationQuit()
    {
        var disconnectMessage = new JObject();
        disconnectMessage["type"] = "disconnect";
        disconnectMessage["id"] = player.localPlayerStats.playerId;
        var json = System.Text.Encoding.UTF8.GetBytes(disconnectMessage.ToString());
        await websocket.Send(json);
        await websocket.Close();
    }
    
}
