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
- 
- After this initial state, the localplayer will call the: ```SendGetAllConnectedClients``` which sends an ```get_all_connected_clients``` message type with their playerID to the server.

### Things to add
- Implement way to select IP and username