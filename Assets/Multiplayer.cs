using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

using NativeWebSocket;

public class Multiplayer : MonoBehaviour
{
    WebSocket websocket;

    public Player player;

    async void Start()
    {
        websocket = new WebSocket("ws://localhost:3000");

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
        };

        websocket.OnMessage += (bytes) =>
        {
            // convert bytes to json
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
                case "update_position_response":
                    HandleUpdatePositionResponse(obj);
                    break;
                case "add_player":
                    HandleAddPlayer(obj);
                    break;
                default:
                    break;
            }
            
        // getting the message as a string
        // var message = System.Text.Encoding.UTF8.GetString(bytes);
        // Debug.Log("OnMessage! " + message);
        };

        // Keep sending messages at every 0.3s
        InvokeRepeating("SendUpdatePositionMessage", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
  }

  void Update()
  {
    #if !UNITY_WEBGL || UNITY_EDITOR
      websocket.DispatchMessageQueue();
    #endif
  }

  async void SendInitialMessage() {
      if(websocket.State == WebSocketState.Open){
            // Send an initial message to the server, to hit a specific endpoint
            var message = new JObject();
            message["type"] = "initial";
            message["name"] = "Player";
            // convert json  to byte
            var json = System.Text.Encoding.UTF8.GetBytes(message.ToString());
            await websocket.Send(json);
      }
  }

  void HandleInitialResponse(JObject data){
      if(websocket.State == WebSocketState.Open){
          player.localPlayerStats.playerId = data["id"].ToObject<int>();
          
          SendGetAllConnectedClients();
      }
  }

  async void SendGetAllConnectedClients(){
        if(websocket.State == WebSocketState.Open){
            var message = new JObject();
            message["type"] = "get_all_connected_clients";
            message["id"] = player.localPlayerStats.playerId;
            // convert json  to byte
            var json = System.Text.Encoding.UTF8.GetBytes(message.ToString());
            await websocket.Send(json);
        }
  }

  void HandleGetAllConnectedClientsResponse(JObject data){
      
        if(websocket.State == WebSocketState.Open){
            var players = data["playerList"];
            player.localPlayerStats.state = Player.PlayerState.idle;
            // for each players in players array
            foreach(var client in players){
                // if player id is not equal to local player id
                    // push this client to the player list
                    int playerid;
                    Vector3 position;
                    playerid = client["id"].ToObject<int>();
                    position = new Vector3(client["position"]["x"].ToObject<float>(), client["position"]["y"].ToObject<float>(), client["position"]["z"].ToObject<float>());
                    // Createjson object
                    var json = new JObject();
                    json["playerId"] = playerid;
                    json["position"] = new JObject();
                    json["position"]["x"] = position.x;
                    json["position"]["y"] = position.y;
                    json["position"]["z"] = position.z;
                    json["state"] = client["state"].ToObject<string>();
                    Debug.Log(json.ToString());
                    player.AddPlayer(json);
                }
        }
  }

    async void SendUpdatePositionMessage(){
        if(websocket.State == WebSocketState.Open && player.localPlayerStats.state == Player.PlayerState.idle){
            var message = new JObject();
            message["type"] = "update_position";
            message["id"] = player.localPlayerStats.playerId;
            message["position"] = new JObject();
            message["position"]["x"] = player.transform.position.x;
            message["position"]["y"] = player.transform.position.y;
            message["position"]["z"] = player.transform.position.z;
            message["state"] = player.localPlayerStats.state.ToString();
            // convert json  to byte
            var json = System.Text.Encoding.UTF8.GetBytes(message.ToString());
            await websocket.Send(json);
        }
    }
    void HandleUpdatePositionResponse(JObject data){
        if(websocket.State == WebSocketState.Open){
            // select specific player from list using data.id
            var playerToUpdate = player.GetPlayer(data["id"].ToObject<int>());

            if(playerToUpdate.playerObject != null){
                // update player position
                var obj = new JObject();

                obj["playerId"] = data["id"].ToObject<int>();
                obj["state"] = data["state"].ToObject<int>();
                obj["position"] = new JObject();
                obj["position"]["x"] = data["position"]["x"].ToObject<float>();
                obj["position"]["y"] = data["position"]["y"].ToObject<float>();
                obj["position"]["z"] = data["position"]["z"].ToObject<float>();

                player.UpdatePlayer(obj);
            }            
        }
    }

    void HandleAddPlayer(JObject data){
        if(websocket.State == WebSocketState.Open){
            // select specific player from list using data.id
            var playerToUpdate = player.GetPlayer(data["id"].ToObject<int>());

            if(playerToUpdate.playerObject == null){
                // update player position
                var obj = new JObject();

                obj["playerId"] = data["id"].ToObject<int>();
                obj["state"] = data["state"].ToObject<int>();
                obj["position"] = new JObject();
                obj["position"]["x"] = data["position"]["x"].ToObject<float>();
                obj["position"]["y"] = data["position"]["y"].ToObject<float>();
                obj["position"]["z"] = data["position"]["z"].ToObject<float>();

                player.AddPlayer(obj);
            }            
        }
    }

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }
    
}
