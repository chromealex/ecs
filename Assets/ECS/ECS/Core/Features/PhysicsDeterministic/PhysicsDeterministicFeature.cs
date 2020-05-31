namespace ME.ECS.Features {

    using PhysicsDeterministic.Components; using PhysicsDeterministic.Modules; using PhysicsDeterministic.Systems; using PhysicsDeterministic.Markers;
    
    namespace PhysicsDeterministic.Components {}
    namespace PhysicsDeterministic.Modules {}
    namespace PhysicsDeterministic.Systems {}
    namespace PhysicsDeterministic.Markers {}
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class PhysicsDeterministicFeature : FeatureBase {

        protected override void OnConstruct() {
            
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics.autoSyncTransforms = false;
            
            this.AddSystem<PhysicsRemoveOnceSystem>();
            this.AddSystem<PhysicsPlayTick>();

        }

        protected override void OnDeconstruct() {
            
        }

    }

}