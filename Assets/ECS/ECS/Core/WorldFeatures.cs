namespace ME.ECS {

    [System.Serializable]
    public class FeaturesList {

        [System.Serializable]
        public class FeatureData {

            public bool enabled;
            public FeatureBase feature;

        }

        public System.Collections.Generic.List<FeatureData> features = new System.Collections.Generic.List<FeatureData>();

        internal void Initialize<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            for (int i = 0; i < this.features.Count; ++i) {

                var item = this.features[i];
                if (item.enabled == true) world.AddFeature((IFeature<TState>)item.feature);
                
            }

        }

        internal void DeInitialize<TState>(World<TState> world) where TState : class, IState<TState>, new() {
            
            for (int i = 0; i < this.features.Count; ++i) {
                
                var item = this.features[i];
                if (item.enabled == true) world.RemoveFeature((IFeature<TState>)item.feature);
                
            }

        }

    }

    public abstract class FeatureBase : UnityEngine.ScriptableObject, IFeatureBase {

        internal void DoConstruct() {
            
            this.OnConstruct();
            
        }

        internal void DoDeconstruct() {
            
            this.OnDeconstruct();
            
        }
        
        protected abstract void OnConstruct();
        protected abstract void OnDeconstruct();

    }

    public abstract class Feature<TState> : FeatureBase, IFeature<TState> where TState : class, IState<TState>, new() {

        public IWorld<TState> world { get; set; }

        protected bool AddSystem<TSystem>() where TSystem : class, ISystem<TState>, new() {

            if (this.world.HasSystem<TSystem>() == false) {
                
                return this.world.AddSystem<TSystem>();
                
            }

            return false;

        }

        protected bool AddModule<TModule>() where TModule : class, IModule<TState>, new() {

            if (this.world.HasModule<TModule>() == false) {
                
                return this.world.AddModule<TModule>();
                
            }

            return false;

        }

    }

}