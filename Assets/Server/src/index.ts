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
    position: { x: number; y: number, z: number };
    state: number;
}

let playerList: PlayerList[] = [];

// listen to the websocket server
wss.on('connection', function connection(ws) {
    console.log("A client is trying to connect!");
    // listen to the message event
    ws.on('message', function incoming(message) {
        // convert RawData to json object
        const jsonData = JSON.parse(message.toString());
        switch(jsonData.type) {
            case "initial":
                // Generate a new player ID based off of the length of PlayerList array
                const newPlayerID = playerList.length;
                // Add the new player to the playerList array
                playerList.push({
                    id: newPlayerID,
                    position: {x: 0, y: 0, z: 0},
                    state: 1
                });
                
                console.log("Generated ID: " + newPlayerID + " | PlayerList: " + JSON.stringify(playerList));
                // return this new player's ID to the client
                ws.send(JSON.stringify({
                    type: "initial_response",
                    id: newPlayerID,
                    position: {x: 0, y: 0, z: 0},
                    state: 1
                }));

                wss.clients.forEach(function each(client) {
                    client.send(JSON.stringify({
                        type: "add_player",
                        id: newPlayerID,
                        position: {x: 0, y: 0, z: 0},
                        state: 1,
                    }));
                });
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
                // update the player's position
                console.log("PlayerID: " + jsonData.id + " has updated their position");
                // update the player in the playerList array
                playerList[jsonData.id].position = jsonData.position;
                playerList[jsonData.id].state = jsonData.state;
                
                // send this update to all connected clients
                wss.clients.forEach(function each(client) {
                    client.send(JSON.stringify({
                        type: "update_position_response",
                        id: jsonData.id,
                        position: jsonData.position,
                        state: playerList[jsonData.id].state
                    }));
                });
                break;
            default:
                break;
        }
    });
});

// listen to the http server
server.listen(3000, () => {
    console.log('listening on *:3000');
});

