using ME.ECS;
using UnityEngine;
using RPCId = System.Int32;

public class InputSystem : ISystem<State> {

    public IWorld<State> world { get; set; }
    private RPCId testEventCallId;
    private RPCId createUnitCallId;

    void ISystem<State>.OnConstruct() {
        
        var network = this.world.GetModule<ME.ECS.Network.INetworkModuleBase>();
        this.testEventCallId = network.RegisterRPC(new System.Action<int, Color, float>(this.TestEvent_RPC).Method);
        this.createUnitCallId = network.RegisterRPC(new System.Action(this.CreateUnit_RPC).Method);
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

            this.world.AddComponent<Point, PointIncreaseUnits>(Entity.Create<Point>(1));
            this.world.AddComponent<Point, PointIncreaseUnits>(Entity.Create<Point>(2));

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

    public void AddUnitButtonClick() {
        
        var networkModule = this.world.GetModule<NetworkModule>();
        networkModule.RPC(this, this.createUnitCallId);

    }

    private void CreateUnit_RPC() {

        var p1 = Entity.Create<Point>(1);
        var p2 = Entity.Create<Point>(2);
            
        var unit = this.world.AddEntity(new Unit() { position = Vector3.zero, speed = this.world.GetRandomRange(0.5f, 1.5f), pointFrom = p1, pointTo = p2 });
        var followComponent = this.world.AddComponent<Unit, UnitFollowFromTo>(unit);
        followComponent.@from = p1;
        followComponent.to = p2;
        
    }

    private void TestEvent_RPC(int pointId, Color color, float moveSide) {

        var componentColor = this.world.AddComponent<Point, PointSetColor>(Entity.Create<Point>(pointId));
        componentColor.color = color;

        var componentPos = this.world.AddComponent<Point, PointAddPositionDelta>(Entity.Create<Point>(pointId));
        componentPos.positionDelta = Vector3.left * moveSide;

    }

}
