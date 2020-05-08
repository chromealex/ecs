using ME.ECS;

namespace Prototype.Features {
    
    using TState = PrototypeState;
    using Entities; using Components; using Modules; using Systems; using Features; using Markers;
    
    public class InputFeature : Feature<TState> {

        public Prototype.Features.Input.Data.FeatureInputData resourcesData { get; private set; }

        protected override void OnConstruct(ref ConstructParameters parameters) {

            this.resourcesData = UnityEngine.Resources.Load<Prototype.Features.Input.Data.FeatureInputData>("FeatureInputData");
            
            //#if UNITY_ANDROID || UNITY_IOS
            this.AddModule<Prototype.Features.Input.Modules.TouchInputModule>();
            //#else
            this.AddModule<Prototype.Features.Input.Modules.MouseInputModule>();
            //#endif

        }

        protected override void OnDeconstruct() {
            
        }

    }

}