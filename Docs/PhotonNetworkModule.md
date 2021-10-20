# Photon NetworkModule source code
Here are some classes for understanding how to set up **NetworkModule** properly for [Photon PUN](https://www.photonengine.com/en-US/sdks#pun-sdkpununity).
All what you need is to register your project on their website and fill up the photon settings form inside Unity. Do not foget to choose "TCP" in protocol section.

> Note: FSSerializer depends on [FullSerializer](https://github.com/jacobdufault/fullserializer).

Steps to implement Photon NetworkModule:
1. [Markers](#first-of-all-you-need-to-define-some-markers-in-your-world)
2. [Network Module](#then-replace-your-modulesnetworkmodulecs-with-the-code-below)
3. [Photon Receiver](#photon-receiver-is-a-monobehaviour-class-which-allows-networkmodule-receive-callbacks-from-photon)
4. [ITransporter Implementation](#add-itransporter-implementation)
5. [ISerializer Implementation](#add-iserializer-implementation)

#### First of all you need to define some markers in your world:
```csharp
public struct NetworkSetActivePlayer : IMarker {

    public Photon.Realtime.Player player;

}

public struct NetworkPlayerDisconnected : IMarker {

    public Photon.Realtime.Player player;

}

public struct NetworkPlayerConnectedTimeSynced : IMarker {}
```

#### Then replace your Modules/NetworkModule.cs with the code below:
```csharp
public class NetworkModule : ME.ECS.Network.NetworkModule<TState> {

    private int orderId;
    private PhotonTransporter photonTransporter;

    public ME.ECS.Network.ISerializer GetSerializer() {

        return this.serializer;

    }

    protected override int GetRPCOrder() {

        return this.orderId;

    }

    protected override ME.ECS.Network.NetworkType GetNetworkType() {

        return ME.ECS.Network.NetworkType.SendToNet | ME.ECS.Network.NetworkType.RunLocal;

    }

    public void SetOrderId(int orderId) {

        this.orderId = orderId;

    }

    public void AddToQueue(byte[] bytes) {
        
        this.photonTransporter.AddToQueue(bytes);
        
    }

    public void AddToSystemQueue(byte[] bytes) {
        
        this.photonTransporter.AddToSystemQueue(bytes);
        
    }

    public void SetRoomName(string name) {

        this.photonTransporter.SetRoomName(name);

    }

    public void SetRoom(Photon.Realtime.Room room) {

        this.photonTransporter.SetRoom(room);

    }

    protected override void OnInitialize() {

        var tr = new PhotonTransporter(this.world.id);
        var instance = (ME.ECS.Network.INetworkModuleBase)this;
        instance.SetTransporter(tr);
        instance.SetSerializer(new FSSerializer());

        this.photonTransporter = tr;

        this.SetRoomName("TestRoom");

    }

}
```

#### Photon Receiver is a MonoBehaviour class which allows NetworkModule receive callbacks from Photon.
```csharp
public class PhotonReceiver : Photon.Pun.MonoBehaviourPunCallbacks {

    public string roomName;
    
    private bool timeSyncedConnected = false;
    private bool timeSynced = false;
    
    [Photon.Pun.PunRPC]
    public void RPC_HISTORY_CALL(byte[] bytes) {

        var world = ME.ECS.Worlds.currentWorld;
        var storageNetworkModule = world.GetModule<NetworkModule>();
        var networkModule = world.GetModule<ME.ECS.Network.INetworkModuleBase>();
        var storage = storageNetworkModule.GetSerializer().DeserializeStorage(bytes); 
        networkModule.LoadHistoryStorage(storage);

    }

    [Photon.Pun.PunRPC]
    public void RPC_CALL(byte[] bytes) {

        var world = ME.ECS.Worlds.currentWorld;
        var networkModule = world.GetModule<NetworkModule>();
        networkModule.AddToQueue(bytes);

    }

    [Photon.Pun.PunRPC]
    public void RPC_SYSTEM_CALL(byte[] bytes) {

        var world = ME.ECS.Worlds.currentWorld;
        var networkModule = world.GetModule<NetworkModule>();
        networkModule.AddToSystemQueue(bytes);

    }

    public void Initialize() {
        
        Photon.Pun.PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster() {
    
        base.OnConnectedToMaster();

        Photon.Pun.PhotonNetwork.JoinLobby(Photon.Realtime.TypedLobby.Default);

    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause) {
        
        base.OnDisconnected(cause);
        
        UnityEngine.Debug.Log("Disconnected because of " + cause);

        var ww = ME.ECS.Worlds.currentWorld;
        WorldUtilities.ReleaseWorld<TState>(ref ww);
        this.timeSyncedConnected = false;
        this.timeSynced = false;
        ME.ECS.Worlds.currentWorld = null;

        var go = UnityEngine.GameObject.FindObjectOfType<InitializerBase>();
        if (go != null) {
            
            go.gameObject.SetActive(false);
            UnityEngine.GameObject.DestroyImmediate(go);
            
        }

        UnityEngine.Debug.Log("World destroyed");

    }

    public override void OnJoinedRoom() {
        
        base.OnJoinedRoom();

        if (Photon.Pun.PhotonNetwork.InRoom == true) {
            
            UnityEngine.Debug.Log("OnJoinedRoom. IsMaster: " + Photon.Pun.PhotonNetwork.IsMasterClient);

            var world = ME.ECS.Worlds.currentWorld;
            var networkModule = world.GetModule<NetworkModule>();
            networkModule.SetRoom(Photon.Pun.PhotonNetwork.CurrentRoom);

            if (Photon.Pun.PhotonNetwork.IsMasterClient == true) {
                
                // Put server time into the room properties
                var serverTime = Photon.Pun.PhotonNetwork.Time;
                Photon.Pun.PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() {
                    { "t", serverTime },
                    { "cc", 1 }
                });
                
            }

            this.timeSyncedConnected = false;
            this.timeSynced = false;
            this.UpdateTime();

            world.AddMarker(new NetworkSetActivePlayer() {
                player = Photon.Pun.PhotonNetwork.LocalPlayer
            });
            
        }

    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
        
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        
        var world = ME.ECS.Worlds.currentWorld;
        var networkModule = world.GetModule<NetworkModule>();
        if ((networkModule as ME.ECS.Network.INetworkModuleBase).GetRPCOrder() == 0) {

            var orderId = (int)Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties["cc"];
            networkModule.SetOrderId(orderId);

        }

    }

    private void UpdateTime() {
        
        if (Photon.Pun.PhotonNetwork.InRoom == false) return;

        if (Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("t") == true) {

            // Set current time since start from master client
            var world = ME.ECS.Worlds.currentWorld;
            var serverTime = Photon.Pun.PhotonNetwork.Time;
            var gameStartTime = serverTime - (double)Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties["t"];

            world.SetTimeSinceStart(gameStartTime);
            this.timeSynced = true;

        }

    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        
        base.OnPlayerEnteredRoom(newPlayer);

        if (Photon.Pun.PhotonNetwork.IsMasterClient == true) {

            var world = ME.ECS.Worlds.currentWorld;
            var props = Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties;
            props["cc"] = (int)props["cc"] + 1;
            Photon.Pun.PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            // Send all history events to client
            var networkModule = world.GetModule<NetworkModule>();
            var history = world.GetModule<ME.ECS.StatesHistory.IStatesHistoryModuleBase>().GetHistoryStorage();
            this.photonView.RPC("RPC_HISTORY_CALL", newPlayer, networkModule.GetSerializer().SerializeStorage(history));
            
        }

    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        
        base.OnPlayerLeftRoom(otherPlayer);

        if (Photon.Pun.PhotonNetwork.IsMasterClient == true) {

            var world = ME.ECS.Worlds.currentWorld;
            world.AddMarker(new NetworkPlayerDisconnected() {
                player = otherPlayer
            });

        }

    }

    public override void OnRoomListUpdate(System.Collections.Generic.List<Photon.Realtime.RoomInfo> roomList) {

        Photon.Pun.PhotonNetwork.JoinOrCreateRoom(this.roomName, new Photon.Realtime.RoomOptions() { MaxPlayers = 16, PublishUserId = true }, Photon.Realtime.TypedLobby.Default);
        
    }

    public void LateUpdate() {

        this.UpdateTime();

        var world = ME.ECS.Worlds.currentWorld;
        if (this.timeSynced == true && this.timeSyncedConnected == false) {

            var networkModule = world.GetModule<NetworkModule>();
            if (((ME.ECS.Network.INetworkModuleBase)networkModule).GetRPCOrder() > 0) {

                // Here we are check if all required players connected to the game
                // So we could start the game sending the special message
                if (Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount == 2) {

                    this.timeSyncedConnected = true;
                    world.AddMarker(new NetworkPlayerConnectedTimeSynced());

                }
                
            }

        }

    }

}
```

#### Add ITransporter implementation:
```csharp
public class PhotonTransporter : ME.ECS.Network.ITransporter {

    private System.Collections.Generic.Queue<byte[]> queue = new System.Collections.Generic.Queue<byte[]>();
    private System.Collections.Generic.Queue<byte[]> queueSystem = new System.Collections.Generic.Queue<byte[]>();
    private Photon.Pun.PhotonView photonView;
    private PhotonReceiver photonReceiver;
    private Photon.Realtime.Room room;
    
    private int sentCount;
    private int sentBytesCount;
    private int receivedCount;
    private int receivedBytesCount;

    public PhotonTransporter(int id) {

        var photon = new UnityEngine.GameObject("PhotonTransporter", typeof(Photon.Pun.PhotonView), typeof(PhotonReceiver));
        this.photonReceiver = photon.GetComponent<PhotonReceiver>();
        var view = photon.GetComponent<Photon.Pun.PhotonView>();
        view.ViewID = id;
        Photon.Pun.PhotonNetwork.RegisterPhotonView(view);

        this.photonView = view;

        this.photonReceiver.Initialize();

    }
    
    public void SetRoomName(string name) {

        this.photonReceiver.roomName = name;

    }
    
    public void SetRoom(Photon.Realtime.Room room) {

        this.room = room;

    }
    
    public bool IsConnected() {

        return Photon.Pun.PhotonNetwork.IsConnectedAndReady == true && this.room != null;

    }

    public void Send(byte[] bytes) {
        
        // Just send RPC calls to others because of network type (RunLocal flag)
        this.photonView.RPC("RPC_CALL", Photon.Pun.RpcTarget.Others, bytes);

        this.sentBytesCount += bytes.Length;
        ++this.sentCount;
        
    }

    public void SendSystem(byte[] bytes) {

        this.photonView.RPC("RPC_SYSTEM_CALL", Photon.Pun.RpcTarget.Others, bytes);
        
        this.sentBytesCount += bytes.Length;

    }

    public void AddToQueue(byte[] bytes) {
        
        this.queue.Enqueue(bytes);
        
    }

    public void AddToSystemQueue(byte[] bytes) {
        
        this.queueSystem.Enqueue(bytes);
        
    }

    public byte[] Receive() {

        if (this.queue.Count == 0) {

            if (this.queueSystem.Count == 0) return null;
            
            var bytes = this.queueSystem.Dequeue();
            this.receivedBytesCount += bytes.Length;
        
            return bytes;

        } else {

            var bytes = this.queue.Dequeue();

            ++this.receivedCount;
            this.receivedBytesCount += bytes.Length;

            return bytes;

        }

    }

    public int GetEventsSentCount() {

        return this.sentCount;

    }

    public int GetEventsBytesSentCount() {

        return this.sentBytesCount;

    }

    public int GetEventsReceivedCount() {

        return this.receivedCount;

    }

    public int GetEventsBytesReceivedCount() {
        
        return this.receivedBytesCount;
        
    }

}
```

#### Add ISerializer implementation:
```csharp
public class FSSerializer : ME.ECS.Network.ISerializer {

    public byte[] SerializeStorage(ME.ECS.StatesHistory.HistoryStorage historyEvent) {

        return ME.ECS.Serializer.Serializer.Pack(historyEvent);

    }

    public ME.ECS.StatesHistory.HistoryStorage DeserializeStorage(byte[] bytes) {

        return ME.ECS.Serializer.Serializer.Unpack<ME.ECS.StatesHistory.HistoryStorage>(bytes);
        
    }

    public byte[] Serialize(ME.ECS.StatesHistory.HistoryEvent historyEvent) {

        return ME.ECS.Serializer.Serializer.Pack(historyEvent);

    }

    public ME.ECS.StatesHistory.HistoryEvent Deserialize(byte[] bytes) {

        return ME.ECS.Serializer.Serializer.Unpack<ME.ECS.StatesHistory.HistoryEvent>(bytes);

    }

    public byte[] Serialize(ME.ECS.StatesHistory.HistoryEvent historyEvent) => ME.ECS.Serializer.Serializer.Pack(historyEvent);
    public ME.ECS.StatesHistory.HistoryEvent Deserialize(byte[] bytes) => ME.ECS.Serializer.Serializer.Unpack<ME.ECS.StatesHistory.HistoryEvent>(bytes);

}
```

[![](Footer.png)](/../../#glossary)
