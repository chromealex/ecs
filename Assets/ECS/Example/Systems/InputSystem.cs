using ME.ECS;

public class InputSystem : ISystem<State> {

    public IWorld<State> world { get; set; }
    
    void ISystem<State>.OnConstruct() { }
    void ISystem<State>.OnDeconstruct() { }

    void ISystem<State>.AdvanceTick(State state, float deltaTime) {
        
    }

}
