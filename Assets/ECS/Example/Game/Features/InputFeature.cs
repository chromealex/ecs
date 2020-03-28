using ME.ECS;

namespace ME.Example.Game.Features {
    
    using ME.Example.Game.Systems;
    
    public class InputFeature : Feature<State, ConstructParameters<Entity, Entity>> {

        protected override void OnConstruct(ref ConstructParameters<Entity, Entity> parameters) {
            
            this.AddSystem<InputSystem>();
            {
                var inputSystem = this.world.GetSystem<InputSystem>();
                inputSystem.p1 = parameters.p1;
                inputSystem.p2 = parameters.p2;
            }
            
            this.AddSystem<PointsMoveSystem>();
            {
                var inputSystem = this.world.GetSystem<PointsMoveSystem>();
                inputSystem.p1 = parameters.p1;
                inputSystem.p2 = parameters.p2;
            }
            
        }

        protected override void OnDeconstruct() {
            
        }

    }

}