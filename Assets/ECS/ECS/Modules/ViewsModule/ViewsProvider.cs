using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS.Views {

    [System.Serializable]
    public struct ParticleSimulationItem {

        public UnityEngine.ParticleSystem particleSystem;

        public void SimulateParticles(float time) {
                
            this.particleSystem.Simulate(time, withChildren: true);
            this.particleSystem.Play(withChildren: true);
                
        }

    }

    [System.Serializable]
    public struct ParticleSystemSimulation {

        public ParticleSimulationItem[] particleItems;

        public void OnValidate(UnityEngine.ParticleSystem[] particleSystems) {
            
            this.particleItems = new ParticleSimulationItem[particleSystems.Length];
            for (int i = 0; i < this.particleItems.Length; ++i) {
                
                this.particleItems[i] = new ParticleSimulationItem();
                this.particleItems[i].particleSystem = particleSystems[i];

            }

        }

        public void SimulateParticles(float time) {

            for (int i = 0; i < this.particleItems.Length; ++i) {

                this.particleItems[i].SimulateParticles(time);

            }

        }

        public override string ToString() {

            if (this.particleItems == null) return string.Empty;
            
            return "Particle Systems Count: " + this.particleItems.Length.ToString();
            
        }

    }

    public interface IDoValidate {

        void DoValidate();

    }

    public interface IViewsProvider {

        IViewsProvider<TEntity> Create<TEntity>() where TEntity : struct, IEntity;
        void Destroy<TEntity>(IViewsProvider<TEntity> instance) where TEntity : struct, IEntity;

    }

    public interface IViewsProviderBase { }

    public interface IViewsProvider<TEntity> : IViewsProviderBase where TEntity : struct, IEntity {

        void OnConstruct();
        void OnDeconstruct();

        IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId);
        void Destroy(ref IView<TEntity> instance);

        void Update(System.Collections.Generic.List<IView<TEntity>> list, float deltaTime);

    }

    public abstract class ViewsProvider<TEntity> : IViewsProvider<TEntity> where TEntity : struct, IEntity {

        public abstract void OnConstruct();
        public abstract void OnDeconstruct();

        public abstract IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId);
        public abstract void Destroy(ref IView<TEntity> instance);

        public virtual void Update(System.Collections.Generic.List<IView<TEntity>> list, float deltaTime) {}

    }

}
