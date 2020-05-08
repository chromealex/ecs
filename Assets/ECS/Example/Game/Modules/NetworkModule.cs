namespace ME.Example.Game.Modules {

    public class PhotonReceiver : Photon.Pun.MonoBehaviourPunCallbacks {

        [Photon.Pun.PunRPC]
        public void RPC_CALL(byte[] bytes) {

            var world = ME.ECS.Worlds<State>.currentWorld;
            var networkModule = world.GetModule<NetworkModule>();
            networkModule.AddToQueue(bytes);

        }

        [Photon.Pun.PunRPC]
        public void RPC_SYSTEM_CALL(byte[] bytes) {

            var world = ME.ECS.Worlds<State>.currentWorld;
            var networkModule = world.GetModule<NetworkModule>();
            networkModule.AddToQueue(bytes);

        }

        public override void OnConnectedToMaster() {
        
            base.OnConnectedToMaster();

            Photon.Pun.PhotonNetwork.JoinLobby(Photon.Realtime.TypedLobby.Default);

        }

        public override void OnJoinedRoom() {
            
            base.OnJoinedRoom();

            if (Photon.Pun.PhotonNetwork.InRoom == true) {
                
                var world = ME.ECS.Worlds<State>.currentWorld;
                var networkModule = world.GetModule<NetworkModule>();
                networkModule.SetRoom(Photon.Pun.PhotonNetwork.CurrentRoom);

                if (Photon.Pun.PhotonNetwork.IsMasterClient == true) {
                    
                    // Put server time into the room properties
                    var serverTime = Photon.Pun.PhotonNetwork.Time;
                    Photon.Pun.PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() {
                        { "t", serverTime }
                    });

                }
                
            }

        }

        public override void OnRoomListUpdate(System.Collections.Generic.List<Photon.Realtime.RoomInfo> roomList) {

            Photon.Pun.PhotonNetwork.JoinOrCreateRoom("TestRoom", new Photon.Realtime.RoomOptions() { MaxPlayers = 4 }, Photon.Realtime.TypedLobby.Default);
            
        }

        public void LateUpdate() {

            if (Photon.Pun.PhotonNetwork.InRoom == false) return;
            
            // Set up time update
            //if (Photon.Pun.PhotonNetwork.IsMasterClient == false) {
                
                // Set current time since start from master client
                var world = ME.ECS.Worlds<State>.currentWorld;
                var serverTime = Photon.Pun.PhotonNetwork.Time;
                var gameStartTime = serverTime - (double)Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties["t"];
                
                (world as ME.ECS.IWorldBase).SetTimeSinceStart(gameStartTime);

            //}

        }

    }

    /// <summary>
    /// We need to implement our own NetworkModule class without any logic just to catch your State type into ECS.Network
    /// You can use some overrides to setup history config for your project
    /// </summary>
    public class NetworkModule : ME.ECS.Network.NetworkModule<State> {

        private PhotonTransporter photonTransporter;

        protected override int GetRPCOrder() {

            return Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;

        }

        protected override ME.ECS.Network.NetworkType GetNetworkType() {

            return ME.ECS.Network.NetworkType.SendToNet | ME.ECS.Network.NetworkType.RunLocal;

        }

        public void AddToQueue(byte[] bytes) {
            
            this.photonTransporter.AddToQueue(bytes);
            
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

    public class PhotonTransporter : ME.ECS.Network.ITransporter {

        private System.Collections.Generic.Queue<byte[]> queue = new System.Collections.Generic.Queue<byte[]>();
        private Photon.Pun.PhotonView photonView;
        private Photon.Realtime.Room room;
        
        private int sentCount;
        private int sentBytesCount;
        private int receivedCount;
        private int receivedBytesCount;

        public PhotonTransporter(int id) {

            var photon = new UnityEngine.GameObject("PhotonTransporter", typeof(Photon.Pun.PhotonView), typeof(PhotonReceiver));
            var view = photon.GetComponent<Photon.Pun.PhotonView>();
            view.ViewID = id;
            Photon.Pun.PhotonNetwork.RegisterPhotonView(view);

            this.photonView = view;

            Photon.Pun.PhotonNetwork.ConnectUsingSettings();
            
        }

        public void SetRoom(Photon.Realtime.Room room) {

            this.room = room;

        }
        
        public bool IsConnected() {

            return Photon.Pun.PhotonNetwork.IsConnected == true && this.room != null;

        }

        public void SendSystem(byte[] bytes) {

            this.photonView.RPC("RPC_SYSTEM_CALL", Photon.Pun.RpcTarget.Others, bytes);
            
            this.sentBytesCount += bytes.Length;
            
        }

        public void Send(byte[] bytes) {

            this.photonView.RPC("RPC_CALL", Photon.Pun.RpcTarget.OthersBuffered, bytes);
            
            this.sentBytesCount += bytes.Length;
            ++this.sentCount;

        }

        public void AddToQueue(byte[] bytes) {
            
            this.queue.Enqueue(bytes);
            
        }

        public byte[] Receive() {

            if (this.queue.Count == 0) return null;
            
            var bytes = this.queue.Dequeue();
            
            ++this.receivedCount;
            this.receivedBytesCount += bytes.Length;
            
            return bytes;

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

    }

}