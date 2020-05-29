# Manual NetworkModule Implementation
First you need to set up **NetworkModule** submodules. Go to **Modules/NetworkModule.cs** and in **OnIntiailize** method you need to set up your Transport (**ITransporter**) and Serializer (**ISerializer**). If you don't set up them - none of RPC calls would work.

1. Choose the right server-side. It could be your own implementation or any exist engine.
2. Implement ITransporter interface (Here you need to set up connection to your server-side, send and receive byte arrays).
3. Implement ISerializer interface (You can use default FakeSerializer version or your own).
4. Set up ITransporter and ISerializer in **NetworkModule.OnInitialize** method.
5. All is done ;)


```csharp
public class NetworkModule : ME.ECS.Network.NetworkModule<State> {

    protected override int GetRPCOrder() {

        // TODO: Place here your Network Player Id
        return this.world.id;

    }

    protected override ME.ECS.Network.NetworkType GetNetworkType() {

        // You can remove RunLocal parameter to avoid run events on local machine immediately.
        // This behaviour depends on your needs, because in some projects you need to run all user events smoothly without any delay like a ping or awaiting of server logic.
        return ME.ECS.Network.NetworkType.SendToNet | ME.ECS.Network.NetworkType.RunLocal;

    }

    protected override void OnInitialize() {

        // Here you need to set up transporter and serializer classes
        var instance = (ME.ECS.Network.INetworkModuleBase)this;
        instance.SetTransporter(new CustomTransporter(this.GetNetworkType()));
        instance.SetSerializer(new CustomSerializer());

    }

}
```

[![](Footer.png)](/../../#glossary)
