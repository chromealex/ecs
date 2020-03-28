namespace ME.Example.Game.Modules {

    /// <summary>
    /// We need to implement our own NetworkModule class without any logic just to catch your State type into ECS.Network
    /// You can use some overrides to setup history config for your project
    /// </summary>
    public class NetworkModule : ME.ECS.Network.NetworkModule<State> {

        protected override int GetRPCOrder() {

            return this.world.id;

        }

        protected override ME.ECS.Network.NetworkType GetNetworkType() {

            return ME.ECS.Network.NetworkType.SendToNet | ME.ECS.Network.NetworkType.RunLocal;

        }

        public FakeTransporter transporter;

        protected override void OnInitialize() {

            var tr = new FakeTransporter(this.GetNetworkType());
            var seed = this.world.id;
            UnityEngine.Random.InitState(seed);
            tr.randomState = UnityEngine.Random.Range(0, 1000);

            var instance = (ME.ECS.Network.INetworkModuleBase)this;
            instance.SetTransporter(tr);
            instance.SetSerializer(new FakeSerializer());

            this.transporter = tr;

        }

        public void SetWorldConnection(int connectToWorldId) {

            this.transporter.connectToWorldId = connectToWorldId;

        }

        public void SetDropPercent(int dropPercent) {

            this.transporter.dropPercent = dropPercent;

        }

    }

    public class FakeTransporter : ME.ECS.Network.ITransporter {

        private struct Buffer {

            public float delay;
            public byte[] data;

        }

        public int connectToWorldId;
        public int dropPercent;

        private readonly System.Collections.Generic.Queue<Buffer> buffers = new System.Collections.Generic.Queue<Buffer>();
        private readonly ME.ECS.Network.NetworkType networkType;
        private int sentCount;
        private int sentBytesCount;
        private int receivedCount;
        private int receivedBytesCount;
        public int randomState;
        private double ping;

        public FakeTransporter(ME.ECS.Network.NetworkType networkType) {

            this.networkType = networkType;

        }

        public void Send(byte[] bytes) {

            UnityEngine.Random.InitState(this.randomState);
            this.randomState = UnityEngine.Random.Range(0, 10000);
            var delay = UnityEngine.Random.Range(0.01f, 0.05f);
            var rnd = UnityEngine.Random.Range(0, 100);
            var isDrop = (rnd < this.dropPercent);

            if ((this.networkType & ME.ECS.Network.NetworkType.RunLocal) == 0) { // Add to local buffer if RunLocal flag is not set

                this.AddToBuffer(delay, bytes);

            }

            if (isDrop == true) {

                delay += UnityEngine.Random.Range(1f, 2f);

            }

            // Send event to connected world
            if (this.connectToWorldId > 0) {

                var connectedWorld = ME.ECS.Worlds<State>.GetWorld(this.connectToWorldId);
                if (connectedWorld != null) {

                    var networkModule = connectedWorld.GetModule<NetworkModule>();
                    networkModule.transporter.AddToBuffer(delay, bytes);

                }

            }

            this.sentBytesCount += bytes.Length;
            ++this.sentCount;

        }

        private void AddToBuffer(float delay, byte[] bytes) {

            this.buffers.Enqueue(new Buffer() {
                delay = delay,
                data = bytes,
            });

        }

        private Buffer currentBuffer;
        private float waitTime = -1f;

        public byte[] Receive() {

            if (this.waitTime >= 0f) {

                this.waitTime -= UnityEngine.Time.deltaTime;
                if (this.waitTime <= 0f) {

                    this.receivedBytesCount += this.currentBuffer.data.Length;
                    ++this.receivedCount;
                    this.waitTime = -1f;
                    return this.currentBuffer.data;

                }

                return null;

            }

            if (this.buffers.Count > 0) {

                var buffer = this.buffers.Dequeue();
                this.currentBuffer = buffer;
                this.waitTime = this.currentBuffer.delay;

            }

            return null;

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

    public class FakeSerializer : ME.ECS.Network.ISerializer {

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

    }

}