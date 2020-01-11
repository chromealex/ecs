/// <summary>
/// We need to implement our own NetworkModule class without any logic just to catch our State type into ECS.Network
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

}

public class FakeTransporter : ME.ECS.Network.ITransporter {

    public int connectToWorldId;
    
    private System.Collections.Generic.Dictionary<int, byte[]> buffers = new System.Collections.Generic.Dictionary<int, byte[]>();
    private int sentCount;
    private int receivedCount;

    public int randomState;
    private ME.ECS.Network.NetworkType networkType;

    public FakeTransporter(ME.ECS.Network.NetworkType networkType) {

        this.networkType = networkType;

    }

    public void Send(byte[] bytes) {
        
        UnityEngine.Random.InitState(this.randomState);
        var frame = UnityEngine.Time.frameCount + UnityEngine.Random.Range(10, 200);
        
        if ((this.networkType & ME.ECS.Network.NetworkType.RunLocal) == 0) { // Run local if RunLocal flag is not set
            
            this.AddToBuffer(frame, bytes);
            
        }

        // Send event to connected world
        if (this.connectToWorldId > 0) {

            var connectedWorld = ME.ECS.Worlds<State>.GetWorld(this.connectToWorldId);
            var networkModule = connectedWorld.GetModule<NetworkModule>();
            networkModule.transporter.AddToBuffer(frame, bytes);

        }
        
        ++this.sentCount;
        
    }

    private void AddToBuffer(int frame, byte[] bytes) {
        
        while (this.buffers.ContainsKey(frame) == true) {

            ++frame;

        }
        
        //UnityEngine.Debug.Log("Transporter Send: " + frame);
        this.buffers.Add(frame, bytes);
        
    }

    public byte[] Receive() {

        foreach (var item in this.buffers) {

            if (UnityEngine.Time.frameCount >= item.Key) {

                //UnityEngine.Debug.Log("Transporter Receive: " + item.Key);
                var buffer = item.Value;
                this.buffers.Remove(item.Key);
                ++this.receivedCount;
                return buffer;

            }

        }

        return null;

    }

    public int GetEventsSentCount() {

        return this.sentCount;

    }

    public int GetEventsReceivedCount() {

        return this.receivedCount;

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