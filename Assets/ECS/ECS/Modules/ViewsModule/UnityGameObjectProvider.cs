#if GAMEOBJECT_VIEWS_MODULE_SUPPORT
using EntityId = System.Int32;
using ViewId = System.UInt64;
using Tick = System.UInt64;

namespace ME.ECS {

    public partial interface IWorld<TState> where TState : class, IState<TState> {

        void InstantiateView<TEntity>(IView<TEntity> prefab, Entity entity) where TEntity : struct, IEntity;
        void InstantiateView<TEntity>(UnityEngine.GameObject prefab, Entity entity) where TEntity : struct, IEntity;
        void DestroyView<TEntity>(ref IView<TEntity> instance) where TEntity : struct, IEntity;

    }

    public partial class World<TState> where TState : class, IState<TState>, new() {

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool UnRegisterViewSource<TEntity>(IView<TEntity> prefab) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.UnRegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(IView<TEntity> prefab) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            return viewsModule.RegisterViewSource(prefab);

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource<TEntity>(UnityEngine.GameObject prefab) where TEntity : struct, IEntity {

            return this.RegisterViewSource(prefab.GetComponent<IView<TEntity>>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(IView<TEntity> prefab, Entity entity) where TEntity : struct, IEntity {

            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.InstantiateView(prefab, entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView<TEntity>(UnityEngine.GameObject prefab, Entity entity) where TEntity : struct, IEntity {

            this.InstantiateView(prefab.GetComponent<IView<TEntity>>(), entity);
            
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void DestroyView<TEntity>(ref IView<TEntity> instance) where TEntity : struct, IEntity {
            
            var viewsModule = this.GetModule<ViewsModule<TState, TEntity>>();
            viewsModule.DestroyView(ref instance);
            
        }

    }

    public abstract class View<TEntity> : UnityEngine.MonoBehaviour, IView<TEntity> where TEntity : struct, IEntity {

        public Entity entity { get; set; }
        public ViewId prefabSourceId { get; set; }

        public virtual void OnInitialize(TEntity data) { }
        public virtual void OnDeInitialize(TEntity data) { }
        public abstract void ApplyState(TEntity data, float deltaTime, bool immediately);

    }
    
    public partial interface IViewModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {

        ViewId RegisterViewSource(UnityEngine.GameObject prefab);
        void InstantiateView(UnityEngine.GameObject prefab, Entity entity);

    }

    public partial class ViewsModule<TState, TEntity> where TState : class, IState<TState> where TEntity : struct, IEntity {
        
        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ViewId RegisterViewSource(UnityEngine.GameObject prefab) {

            return this.RegisterViewSource(prefab.GetComponent<View<TEntity>>());

        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void InstantiateView(UnityEngine.GameObject prefab, Entity entity) {
            
            var viewSource = prefab.GetComponent<View<TEntity>>();
            this.InstantiateView(this.GetViewSourceId(viewSource), entity);
            
        }

    }

    public class UnityGameObjectProvider<TEntity> : ViewsProvider<TEntity> where TEntity : struct, IEntity {

        public override IView<TEntity> Spawn(IView<TEntity> prefab, ViewId prefabSourceId) {

            return PoolGameObject.Spawn((View<TEntity>)prefab, prefabSourceId);;

        }

        public override void Destroy(ref IView<TEntity> instance) {

            var instanceTyped = (View<TEntity>)instance;
            PoolGameObject.Recycle(ref instanceTyped);
            instance = null;

        }

    }

    public class UnityGameObjectProvider : IViewsProvider {

        public void RegisterEntityType<TState, TEntity>(IViewModule<TState, TEntity> module) where TState : class, IState<TState> where TEntity : struct, IEntity {

            module.RegisterProvider<UnityGameObjectProvider<TEntity>>();

        }

    }

}
#endif