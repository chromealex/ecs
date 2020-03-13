namespace ME.ECS {

    public interface IConstructParameters { }

    public struct ConstructParameters : IConstructParameters {

    }

    public struct ConstructParameters<T1> : IConstructParameters {

        public T1 p1;
        
        public ConstructParameters(T1 p1) {

            this.p1 = p1;

        }

    }

    public struct ConstructParameters<T1, T2> : IConstructParameters {

        public T1 p1;
        public T2 p2;
        
        public ConstructParameters(T1 p1, T2 p2) {

            this.p1 = p1;
            this.p2 = p2;

        }

    }

    public struct ConstructParameters<T1, T2, T3> : IConstructParameters {

        public T1 p1;
        public T2 p2;
        public T3 p3;
        
        public ConstructParameters(T1 p1, T2 p2, T3 p3) {

            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

        }

    }

    public struct ConstructParameters<T1, T2, T3, T4> : IConstructParameters {

        public T1 p1;
        public T2 p2;
        public T3 p3;
        public T4 p4;
        
        public ConstructParameters(T1 p1, T2 p2, T3 p3, T4 p4) {

            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
            this.p4 = p4;

        }

    }

    public abstract class Feature<TState> : Feature<TState, ConstructParameters> where TState : class, IState<TState>, new() { }

    public abstract class Feature<TState, TConstructParameters> : IFeature<TState, TConstructParameters> where TState : class, IState<TState>, new() where TConstructParameters : IConstructParameters {

        public IWorld<TState> world { get; set; }

        protected abstract void OnConstruct(ref TConstructParameters parameters);
        protected abstract void OnDeconstruct();
        
        void IFeature<TState, TConstructParameters>.OnConstruct(ref TConstructParameters parameters) {
            
            this.OnConstruct(ref parameters);
            
        }

        void IFeatureBase.OnDeconstruct() {
            
            this.OnDeconstruct();
            
        }

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