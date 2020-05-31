namespace ME.ECS {

    [System.Serializable]
    public class FeaturesList {

        [System.Serializable]
        public class FeatureData {

            public bool enabled;
            public FeatureBase feature;

        }

        public System.Collections.Generic.List<FeatureData> features = new System.Collections.Generic.List<FeatureData>();

        internal void Initialize(World world) {

            for (int i = 0; i < this.features.Count; ++i) {

                var item = this.features[i];
                if (item.enabled == true) world.AddFeature(item.feature);
                
            }

        }

        internal void DeInitialize(World world) {
            
            for (int i = 0; i < this.features.Count; ++i) {
                
                var item = this.features[i];
                if (item.enabled == true) world.RemoveFeature(item.feature);
                
            }

        }

    }

    public abstract class FeatureBase : UnityEngine.ScriptableObject, IFeatureBase {

        public World world { get; set; }

        internal void DoConstruct() {
            
            this.OnConstruct();
            
        }

        internal void DoDeconstruct() {
            
            this.OnDeconstruct();
            
        }
        
        protected abstract void OnConstruct();
        protected abstract void OnDeconstruct();

        protected bool AddSystem<TSystem>() where TSystem : class, ISystemBase, new() {

            if (this.world.HasSystem<TSystem>() == false) {
                
                return this.world.AddSystem<TSystem>();
                
            }

            return false;

        }

        protected bool AddModule<TModule>() where TModule : class, IModuleBase, new() {

            if (this.world.HasModule<TModule>() == false) {
                
                return this.world.AddModule<TModule>();
                
            }

            return false;

        }

    }

    public abstract class Feature : FeatureBase, IFeature {

        new protected bool AddSystem<TSystem>() where TSystem : class, ISystem, new() {

            if (this.world.HasSystem<TSystem>() == false) {
                
                return this.world.AddSystem<TSystem>();
                
            }

            return false;

        }

        new protected bool AddModule<TModule>() where TModule : class, IModule, new() {

            if (this.world.HasModule<TModule>() == false) {
                
                return this.world.AddModule<TModule>();
                
            }

            return false;

        }

    }

}