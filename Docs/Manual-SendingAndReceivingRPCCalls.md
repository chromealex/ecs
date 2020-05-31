# Sending and receiving RPC calls [![](Logo-Tiny.png)](/../../#glossary)

NetworkModule implementations:
| Link         | Description |
| ------------ | --- |
| [Fake](FakeNetworkModule.md) | You can use it for faster set up demo project. |
| [Photon](PhotonNetworkModule.md) | Photon transport layer implementation. |
| [Manual Implementation](ManualNetworkModule.md) | Manual implementation of network module. |

After you have got a marker, you can easily initiate RPC call with marker data.
```csharp
public class UserInputReceiveSystem : ISystem, IUpdate {
    
    // This number must be determined directly
    private const int GLOBAL_RPC_ID = 1;
            
    private RPCId rpcId;
            
    void ISystemBase.OnConstruct() {
            
        // Get registered Network Module
        var networkModule = this.world.GetModule<NetworkModule>();
        // Registering this system as an RPC receiver
        networkModule.RegisterObject(this, UserInputReceiveSystem.GLOBAL_RPC_ID);

        // Register RPC call. This method returns RPCId which determines your method.
        this.rpcId = networkModule.RegisterRPC(new System.Action<WorldClick>(this.WorldClick_RPC).Method);

    }

    void ISystemBase.OnDeconstruct() {

        // Unregister object on deconstruction
        var networkModule = this.world.GetModule<NetworkModule>();
        networkModule.UnRegisterObject(this, UserInputReceiveSystem.GLOBAL_RPC_ID);

    }
    
    void IUpdate.Update(in float deltaTime) {
    
        if (this.world.GetMarker(out WorldClick markerClick) == true) {
            
            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.rpcId, markerClick);
            
        }
        
    }
    
    private void WorldClick_RPC(WorldClick worldClick) {
    
        // You can use worldClick data here
        // For example set to some entity or create the new entity here
        var networkEntity = this.world.AddEntity();
        networkEntity.SetPosition(worldClick.worldPos);
    
    }
    
}
```

[![](Footer.png)](/../../#glossary)
