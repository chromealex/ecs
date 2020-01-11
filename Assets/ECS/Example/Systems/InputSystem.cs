using ME.ECS;
using UnityEngine;
using RPCId = System.Int32;

public class InputSystem : ISystem<State> {

    public IWorld<State> world { get; set; }
    private RPCId testEventCallId;

    void ISystem<State>.OnConstruct() {
        
        var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
        this.testEventCallId = network.RegisterRPC(new System.Action<int, Color, float>(this.TestEvent_RPC).Method);
        network.RegisterObject(this, 1);

    }

    void ISystem<State>.OnDeconstruct() {
        
        var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
        network.UnRegisterObject(this, 1);
        network.UnRegisterRPC(this.testEventCallId);

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
            networkModule.RPC(this, this.testEventCallId, 1, Color.white);
            
        }
        
    }

    public void AddEventUIButtonClick(int pointId, Color color, float moveSide) {
        
        var networkModule = this.world.GetModule<NetworkModule>();
        networkModule.RPC(this, this.testEventCallId, pointId, color, moveSide);
        
    }

    private void TestEvent_RPC(int pointId, Color color, float moveSide) {

        var componentColor = this.world.AddComponent<Point, SetColor>(Entity.Create<Point>(pointId));
        componentColor.color = color;

        var componentPos = this.world.AddComponent<Point, SetPosition>(Entity.Create<Point>(pointId));
        componentPos.position = Vector3.left * moveSide;

    }

}
