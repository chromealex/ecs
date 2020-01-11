using ME.ECS;
using UnityEngine;
using RPCId = System.Int32;

public class InputSystem : ISystem<State> {

    public IWorld<State> world { get; set; }
    private RPCId testCallId;

    void ISystem<State>.OnConstruct() {
        
        var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
        this.testCallId = network.RegisterRPC(new System.Action<int, Vector3>(this.TestCall_RPC).Method);
        network.RegisterObject(this, 1);

    }

    void ISystem<State>.OnDeconstruct() {
        
        var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
        network.UnRegisterObject(this, 1);
        network.UnRegisterRPC(this.testCallId);

    }

    void ISystem<State>.AdvanceTick(State state, float deltaTime) {
        
    }
    
    void ISystem<State>.Update(State state, float deltaTime) {
        
        if (Input.GetKeyDown(KeyCode.Q) == true) {

            this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(1));
            this.world.AddComponent<Point, IncreaseUnits>(Entity.Create<Point>(2));

        }

        if (Input.GetKey(KeyCode.F) == true) {

            var networkModule = this.world.GetModule<NetworkModule>();
            networkModule.RPC(this, this.testCallId, 1, Vector3.one);
            
        }
        
    }

    public void AddEventUIButtonClick() {
        
        var networkModule = this.world.GetModule<NetworkModule>();
        networkModule.RPC(this, this.testCallId, 1, Vector3.one);
        
    }

    private void TestCall_RPC(int p1, Vector3 p2) {

        var count = 1;//this.world.GetRandomRange(1, 5);
        for (int i = 0; i < count; ++i) {

            this.world.AddComponent<Point, IncreaseUnitsOnce>(Entity.Create<Point>(1));

        }

    }

}
