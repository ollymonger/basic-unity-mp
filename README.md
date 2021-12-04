## Basic Multiplayer project
Thanks for checking out this repo.

Currently a WIP for the readme! Check back soon.
Feel free to use this as an example!

### Server Setup
The server has been written in Typescript and is used with ts-node.
- To begin: 
- - cd to the /assets/server/ directory: ```cd /Assets/Server```
- - Install the node packages: ```npm i```
- - Run the server: ```npm start```

### How does it work?
- Server sets its playerList up so that all the values match an EmptyPlayerSlot, (state: nullState).
- The ```Multiplayer.cs/Player.cs``` client connects to the remote server.
- Once connected, the client sends an initial JSON object ```json { type: "initial", name: "Player" }```.
- Server responds with either a initialResponse json object to the client, with the localPlayer's playerId, name, position and state. ```json { type: "initial_response", id: id, name: name, position: position, state: 1 }``` or an error response stating an error reason.
- - Local Player handles this response and sets its playerID to the returned ID
- Server also sends an "add_player" message containing the new players data to all connected clients
- - Other clients set the new player in their playerList
- After this initial state, the localplayer will call the: ```SendGetAllConnectedClients``` which sends an ```get_all_connected_clients``` message type with their playerID to the server.
- - Server responds by sending a ```get_all_connected_clients_response``` with the playerList attached
- Client updates the playerList per item. Client checks  to see if the playerList item's state > nullState and adds the player to the list.
- Client calls an InvokeRepeating function to update the player's position on the server
- - Server handles this by sending this new data to all connected clients
- Client disconnects 
- - Server sets the individuals playerList item back to EmptyPlayerSlot and sends this message back to all clients
- - Client remomes the disconnected player from their local playerList and destroys  the gameobject.

### Player workflow
Implement a menu, allowing the user to select singleplayer, or multiplayer.
- Scene must contain: GlobalVariables so that the user can send their connectToServer variable to the game scene.
- - Multiplayer Menu, allow players to select username & a target IP address (set global variables.);
- - Singleplayer Menu, allow player to select spawn and load game scene!

### Things to add
- Implement way to select IP and username
- Lerp movement
- Rotation