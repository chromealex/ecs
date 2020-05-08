namespace Prototype.Modules {
    
    using ME.ECS;
    using TState = PrototypeState;
    
    /// <summary>
    /// We need to implement our own NetworkModule class without any logic just to catch your State type into ECS.Network
    /// You can use some overrides to setup history config for your project
    /// </summary>
    public class NetworkModule : ME.ECS.Network.NetworkModule<TState> {

        private int orderId;
        private PhotonTransporter photonTransporter;

        public ME.ECS.Network.ISerializer GetSerializer() {

            return this.serializer;

        }

        protected override int GetRPCOrder() {

            return this.orderId;// + Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

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

        }
        
    }
    
    public class PhotonReceiver : Photon.Pun.MonoBehaviourPunCallbacks {

        public string roomName;
        
        [Photon.Pun.PunRPC]
        public void RPC_HISTORY_CALL(byte[] bytes) {

            var world = ME.ECS.Worlds<TState>.currentWorld;
            var storageNetworkModule = world.GetModule<NetworkModule>();
            var networkModule = world.GetModule<ME.ECS.Network.INetworkModuleBase>();
            var storage = storageNetworkModule.GetSerializer().DeserializeStorage(bytes); 
            networkModule.LoadHistoryStorage(storage);

        }

        [Photon.Pun.PunRPC]
        public void RPC_CALL(byte[] bytes) {

            var world = ME.ECS.Worlds<TState>.currentWorld;
            var networkModule = world.GetModule<NetworkModule>();
            networkModule.AddToQueue(bytes);

        }

        [Photon.Pun.PunRPC]
        public void RPC_SYSTEM_CALL(byte[] bytes) {

            var world = ME.ECS.Worlds<TState>.currentWorld;
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

            var ww = ME.ECS.Worlds<TState>.currentWorld;
            WorldUtilities.ReleaseWorld(ref ww);
            UnityEngine.Debug.Log(ME.ECS.Worlds<TState>.currentWorld.isActive);
            this.timeSyncedConnected = false;
            this.timeSynced = false;
            this.timeSyncDesiredTick = Tick.Zero;
            ME.ECS.Worlds<TState>.currentWorld = null;

            var go = UnityEngine.GameObject.Find("Game");
            go.gameObject.SetActive(false);
            UnityEngine.GameObject.DestroyImmediate(go);

            UnityEngine.Debug.Log("World destroyed");

            var idx = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.LoadSceneMode> action = null;
            action = (arg0, mode) => {

                if (arg0.name == "Empty") {
                    
                    UnityEngine.SceneManagement.SceneManager.sceneLoaded -= action;
                    UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene, UnityEngine.SceneManagement.LoadSceneMode> action2 = null;
                    action2 = (arg1, mode1) => {

                        if (arg1.name == "Space") {

                            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= action2;
                            //UnityEngine.GameObject.Find("Game").GetComponent<SupernovaInitializer>().Update();
                            //UnityEngine.GameObject.Find("Game").GetComponent<SupernovaInitializer>().updateWorld = false;

                        }

                    };

                    UnityEngine.SceneManagement.SceneManager.sceneLoaded += action2;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(idx, UnityEngine.SceneManagement.LoadSceneMode.Single);
                    
                }

            };
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += action;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Empty", UnityEngine.SceneManagement.LoadSceneMode.Single);
            
        }

        public override void OnJoinedRoom() {
            
            base.OnJoinedRoom();

            if (Photon.Pun.PhotonNetwork.InRoom == true) {
                
                UnityEngine.Debug.Log("OnJoinedRoom. IsMaster: " + Photon.Pun.PhotonNetwork.IsMasterClient);
                
                var world = ME.ECS.Worlds<TState>.currentWorld;
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
                
                var playersFeature = world.GetFeature<Prototype.Features.PlayersFeature>();
                if (playersFeature != null) playersFeature.SetActivePlayer(Photon.Pun.PhotonNetwork.LocalPlayer);

            }

        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {
            
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
            
            var world = ME.ECS.Worlds<TState>.currentWorld;
            var networkModule = world.GetModule<NetworkModule>();
            if ((networkModule as ME.ECS.Network.INetworkModuleBase).GetRPCOrder() == 0) {

                var orderId = (int)Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties["cc"];
                networkModule.SetOrderId(orderId);

            }

        }

        private bool timeSyncedConnected = false;
        private bool timeSynced = false;
        private Tick timeSyncDesiredTick;
        public void UpdateTime() {
            
            if (Photon.Pun.PhotonNetwork.InRoom == false) return;

            if (Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("t") == true) {

                // Set current time since start from master client
                var world = (ME.ECS.IWorldBase)ME.ECS.Worlds<TState>.currentWorld;
                var serverTime = Photon.Pun.PhotonNetwork.Time;
                var gameStartTime = serverTime - (double)Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties["t"];

                world.SetTimeSinceStart(gameStartTime);
                var timeSinceGameStart = (long)(world.GetTimeSinceStart() * 1000L);
                var desiredTick = (Tick)System.Math.Floor(timeSinceGameStart / (world.GetTickTime() * 1000d));
                this.timeSynced = true;
                this.timeSyncDesiredTick = desiredTick;

            }

        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
            
            base.OnPlayerEnteredRoom(newPlayer);

            if (Photon.Pun.PhotonNetwork.IsMasterClient == true) {

                var world = ME.ECS.Worlds<TState>.currentWorld;
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

                var world = ME.ECS.Worlds<TState>.currentWorld;
                world.GetFeature<Prototype.Features.PlayersFeature>().OnPlayerDisconnected(otherPlayer);

            }

        }

        public override void OnRoomListUpdate(System.Collections.Generic.List<Photon.Realtime.RoomInfo> roomList) {

            Photon.Pun.PhotonNetwork.JoinOrCreateRoom(this.roomName, new Photon.Realtime.RoomOptions() { MaxPlayers = 16, PublishUserId = true }, Photon.Realtime.TypedLobby.Default);
            
        }

        public void LateUpdate() {

            this.UpdateTime();

            var world = ME.ECS.Worlds<TState>.currentWorld;
            if (this.timeSynced == true && this.timeSyncedConnected == false && world.GetCurrentTick() > this.timeSyncDesiredTick) {

                var networkModule = world.GetModule<NetworkModule>();
                if (((ME.ECS.Network.INetworkModuleBase)networkModule).GetRPCOrder() > 0) {

                    if (Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount == 2 || PrototypeInitializer.playWithBot == true) {

                        this.timeSyncedConnected = true;
                        world.GetFeature<Prototype.Features.PlayersFeature>().OnPlayerConnectedTimeSynced();

                    }
                    
                }

            }

        }

    }

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

            this.photonView.RPC("RPC_CALL", Photon.Pun.RpcTarget.Others, bytes);
            
            this.sentBytesCount += bytes.Length;
            ++this.sentCount;

        }

        public void SendSystem(byte[] bytes) {

            this.photonView.RPC("RPC_SYSTEM_CALL", Photon.Pun.RpcTarget.Others, bytes);
            
            this.sentBytesCount += bytes.Length;
            //++this.sentCount;

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
            
                //++this.receivedCount;
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

    public class FSSerializer : ME.ECS.Network.ISerializer {

        private readonly FullSerializer.fsSerializer fsSerializer = new FullSerializer.fsSerializer();

        public byte[] SerializeStorage(ME.ECS.StatesHistory.HistoryStorage historyStorage) {

            FullSerializer.fsData data;
            this.fsSerializer.TrySerialize(typeof(ME.ECS.StatesHistory.HistoryStorage), historyStorage, out data).AssertSuccessWithoutWarnings();
            var str = FullSerializer.fsJsonPrinter.CompressedJson(data);
            return System.Text.Encoding.UTF8.GetBytes(str);

        }

        public ME.ECS.StatesHistory.HistoryStorage DeserializeStorage(byte[] bytes) {

            var fsData = System.Text.Encoding.UTF8.GetString(bytes);
            FullSerializer.fsData data = FullSerializer.fsJsonParser.Parse(fsData);
            object deserialized = null;
            this.fsSerializer.TryDeserialize(data, typeof(ME.ECS.StatesHistory.HistoryStorage), ref deserialized).AssertSuccessWithoutWarnings();

            return (ME.ECS.StatesHistory.HistoryStorage)deserialized;

        }

        public byte[] Serialize(ME.ECS.StatesHistory.HistoryEvent historyEvent) {

            FullSerializer.fsData data;
            this.fsSerializer.TrySerialize(typeof(ME.ECS.StatesHistory.HistoryEvent), historyEvent, out data).AssertSuccessWithoutWarnings();
            var str = FullSerializer.fsJsonPrinter.CompressedJson(data);
            return System.Text.Encoding.UTF8.GetBytes(str);

        }

        public ME.ECS.StatesHistory.HistoryEvent Deserialize(byte[] bytes) {

            var fsData = System.Text.Encoding.UTF8.GetString(bytes);
            FullSerializer.fsData data = FullSerializer.fsJsonParser.Parse(fsData);
            object deserialized = null;
            this.fsSerializer.TryDeserialize(data, typeof(ME.ECS.StatesHistory.HistoryEvent), ref deserialized).AssertSuccessWithoutWarnings();

            return (ME.ECS.StatesHistory.HistoryEvent)deserialized;

        }

    }

}