/// <summary>
/// We need to implement our own NetworkModule class without any logic just to catch our State type into ECS.Network
/// You can use some overrides to setup history config for your project
/// </summary>
public class NetworkModule : ME.ECS.Network.NetworkModule<State> {

    protected override int GetRPCOrder() {

        return 0;

    }
    
    protected override ME.ECS.Network.NetworkType GetNetworkType() {

        return ME.ECS.Network.NetworkType.SendToNet;

    }

    protected override void OnInitialize() {

        var instance = this as ME.ECS.Network.INetworkModule<State>;
        instance.SetTransporter(new FakeTransporter());
        instance.SetSerializer(new FakeSerializer());

    }

}

public class FakeTransporter : ME.ECS.Network.ITransporter {

    private System.Collections.Generic.Dictionary<int, byte[]> buffers = new System.Collections.Generic.Dictionary<int, byte[]>();
    private int sentCount;
    private int receivedCount;

    public void Send(byte[] bytes) {

        var frame = UnityEngine.Time.frameCount + UnityEngine.Random.Range(10, 100);
        while (this.buffers.ContainsKey(frame) == true) {

            ++frame;

        }
        
        this.buffers.Add(frame, bytes);
        ++this.sentCount;

    }

    public byte[] Receive() {

        foreach (var item in this.buffers) {

            if (UnityEngine.Time.frameCount >= item.Key) {

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