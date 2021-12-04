const express = require('express');
import { createServer } from 'http';
// import WebSocket from 'ws';
import { Server } from 'ws';

const app = express();
const server = createServer(app);

// create a websocket server
const wss = new Server({ server });

interface PlayerList {
    id: number;
    name: string;
    position: { x: number; y: number, z: number };
    state: number;
}

let playerList: PlayerList[] = [];

// Setup empty playerlist with null values
function setupPlayerList() {
    console.log("Server setup has begun!");
    // Change this to be a variable shared from a config
    for (let i = 0; i < 10; i++) {
        playerList.push({
            id: i,
            name: "EmptyPlayerSlot",
            position: { x: 0, y: 0, z: 0 },
            state: 0
        });
        console.log("EmptyPlayerSlot " + i + " has been added to the list.");
    }
};

setupPlayerList();

// listen to the websocket server
wss.on('connection', function connection(ws) {
    console.log("A client is trying to connect!");
    // listen to the message event
    ws.on('message', function incoming(message) {
        // convert RawData to json object
        const jsonData = JSON.parse(message.toString());
        switch(jsonData.type) {
            case "initial":
                const newPlayerID = playerList.findIndex(x => x.state === 0);
                if(newPlayerID !== -1) {

                    playerList[newPlayerID] = {
                        id: newPlayerID,
                        name: jsonData.name,
                        position: { x: 0, y: 0, z: 0 },
                        state: 1
                    };                
                    
                    console.log("Generated ID: " + playerList[newPlayerID].id + `${jsonData.name}` + " | PlayerList: " + JSON.stringify(playerList));
                    ws.send(JSON.stringify({
                        type: "initial_response",
                        id: newPlayerID,
                        name: jsonData.name,
                        position: {x: 0, y: 0, z: 0},
                        state: 1
                    }));

                    wss.clients.forEach(function each(client) {
                        client.send(JSON.stringify({
                            type: "add_player",
                            id: newPlayerID,
                            name: jsonData.name,
                            position: {x: 0, y: 0, z: 0},
                            state: 1,
                        }));
                    });
                } else {
                    ws.send(JSON.stringify({
                        type: "error",
                        message: "Server is full!"
                    }));
                    ws.close(1013, "full-server");
                }
                break;
            case "get_all_connected_clients":
                // return all connected clients to the client
                console.log("PlayerID: " + jsonData.id + " has requested all connected clients");
                ws.send(JSON.stringify({
                    type: "get_all_connected_clients_response",
                    playerList: playerList
                }));
                break;
            case "update_position":
                if(jsonData.state != 0){
                    console.log("PlayerID: " + jsonData.id + " has updated their position");
                    playerList[jsonData.id].position = jsonData.position;
                    playerList[jsonData.id].state = jsonData.state;
                    
                    wss.clients.forEach(function each(client) {
                        client.send(JSON.stringify({
                            type: "update_position_response",
                            id: jsonData.id,
                            position: jsonData.position,
                            state: playerList[jsonData.id].state
                        }));
                    });
                }
                break;
            case "disconnect":
                console.log("PlayerID: " + jsonData.id + " has disconnected");
                playerList[jsonData.id] = {
                    id: jsonData.id,
                    name: "EmptyPlayerSlot",
                    position: { x: 0, y: 0, z: 0 },
                    state: 0
                };
                wss.clients.forEach(function each(client) {
                    client.send(JSON.stringify({
                        type: "disconnect_response",
                        reason: `Client ${jsonData.id} ${jsonData.name} disconnect from server.`,
                        id: jsonData.id
                    }));
                });
            default:
                break;
        }
    });
    ws.on('close', function(ws) {
        console.log("A client has disconnected.");
    });
});

// listen to the http server
server.listen(3000, () => {
    console.log('listening on *:3000');
});

