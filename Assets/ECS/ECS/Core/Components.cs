using System.Collections.Generic;
using EntityId = System.Int32;

namespace ME.ECS {

    public interface IComponentsBase { }

    public interface IComponents<TState> : IComponentsBase, IPoolableRecycle where TState : class, IState<TState> {

        void RemoveAll(Entity entity);
        void RemoveAll<TComponent>(Entity entity) where TComponent : class, IComponentBase;
        void RemoveAll<TComponent>() where TComponent : class, IComponentBase;

        void RemoveAllPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent>;

    }

    public static class ComponentExtensions {

        public static bool GetEntityData<TState, TEntity, TEntitySource>(this IComponent<TState, TEntitySource> _, EntityId entityId, out TEntity data) where TEntity : struct, IEntity where TEntitySource : struct, IEntity where TState : class, IState<TState> {

            var world = Worlds<TState>.currentWorld;
            return world.GetEntityData(entityId, out data);

        }

    }

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class Components<TEntity, TState> : IComponents<TState> where TEntity : struct, IEntity where TState : class, IState<TState> {

        private Dictionary<EntityId, List<IComponent<TState, TEntity>>> dic;
        private bool freeze;
        private int capacity;

        void IPoolableRecycle.OnRecycle() {

            foreach (var item in this.dic) {
                
                PoolComponents.Recycle(item.Value);
                PoolList<IComponent<TState, TEntity>>.Recycle(item.Value);
                
            }
            PoolDictionary<EntityId, List<IComponent<TState, TEntity>>>.Recycle(ref this.dic);
            
            this.freeze = false;
            this.capacity = 0;

        }

        public int Count {

            get {

                var count = 0;
                foreach (var item in this.dic) {

                    count += item.Value.Count;

                }

                return count;

            }

        }

        public void RemoveAllPredicate<TComponent, TComponentPredicate>(Entity entity, TComponentPredicate predicate) where TComponent : class, IComponentBase where TComponentPredicate : IComponentPredicate<TComponent> {
            
            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                for (int i = 0, count = list.Count; i < count; ++i) {

                    var listItem = list[i];
                    if (listItem is TComponent listItemComponent) {

                        if (predicate.Execute(listItemComponent) == true) {

                            PoolComponents.Recycle(listItemComponent);
                            list.RemoveAt(i);
                            --i;
                            --count;

                        }

                    }
                    
                }

            }
            
        }

        public void RemoveAll<TComponent>(Entity entity) where TComponent : class, IComponentBase {
            
            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                for (int i = 0, count = list.Count; i < count; ++i) {

                    var listItem = list[i];
                    if (listItem is TComponent listItemComponent) {
                        
                        PoolComponents.Recycle(listItemComponent);
                        list.RemoveAt(i);
                        --i;
                        --count;

                    }
                    
                }

            }
            
        }

        public void RemoveAll<TComponent>() where TComponent : class, IComponentBase {

            foreach (var item in this.dic) {

                var list = item.Value;
                for (int i = 0, count = list.Count; i < count; ++i) {

                    var listItem = list[i];
                    if (listItem is TComponent listItemComponent) {
                        
                        PoolComponents.Recycle(listItemComponent);
                        list.RemoveAt(i);
                        --i;
                        --count;

                    }
                    
                }

            }

        }

        public void RemoveAll(Entity entity) {
            
            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                PoolComponents.Recycle(list);
                list.Clear();

            }

        }

        public void Add(Entity entity, IComponent<TState, TEntity> data) {

            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                list.Add(data);

            } else {

                list = PoolList<IComponent<TState, TEntity>>.Spawn(this.capacity);
                list.Add(data);
                this.dic.Add(entity.id, list);
                
            }

        }

        public TComponent GetFirst<TComponent>(Entity entity) where TComponent : class, IComponent<TState, TEntity> {
            
            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                for (int i = 0, count = list.Count; i < count; ++i) {

                    if (list[i] is TComponent item) return item;

                }

            }

            return null;

        }

        public void ForEach<TComponent>(Entity entity, List<TComponent> output) where TComponent : class, IComponent<TState, TEntity> {
            
            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                for (int i = 0, count = list.Count; i < count; ++i) {

                    if (list[i] is TComponent item) output.Add(item);

                }

            }

        }

        public bool Contains<TComponent>(Entity entity) where TComponent : IComponent<TState, TEntity> {

            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                for (int i = 0, count = list.Count; i < count; ++i) {

                    if (list[i] is TComponent) return true;

                }

            }
            
            return false;

        }

        public bool Contains(Entity entity, IComponent<TState, TEntity> data) {
            
            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                return list.Contains(data);

            }

            return false;

        }

        public Dictionary<EntityId, List<IComponent<TState, TEntity>>> GetData() {

            return this.dic;

        }

        public void Initialize(int capacity) {

            this.capacity = capacity;
            this.dic = PoolDictionary<EntityId, List<IComponent<TState, TEntity>>>.Spawn(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Components<TEntity, TState> other) {

            if (this.dic != null) {

                foreach (var item in this.dic) {

                    PoolComponents.Recycle(item.Value);
                    PoolList<IComponent<TState, TEntity>>.Recycle(item.Value);

                }
                PoolDictionary<EntityId, List<IComponent<TState, TEntity>>>.Recycle(ref this.dic);
                
            }
            
            this.dic = PoolDictionary<EntityId, List<IComponent<TState, TEntity>>>.Spawn(this.capacity);
            foreach (var item in other.dic) {
                
                var newList = PoolList<IComponent<TState, TEntity>>.Spawn(item.Value.Capacity);
                //UnityEngine.Debug.Log("CopyState for " + typeof(TEntity) + ", list: " + newList.Count + " << " + item.Value.Count);
                for (int i = 0, count = item.Value.Count; i < count; ++i) {

                    var element = item.Value[i];
                    var comp = (IComponent<TState, TEntity>)PoolComponents.Spawn(element.GetType());
                    if (comp == null) comp = (IComponent<TState, TEntity>)System.Activator.CreateInstance(element.GetType());
                    comp.CopyFrom(element);
                    newList.Add(comp);

                }
                //UnityEngine.Debug.Log("Result: " + newList.Count);

                this.dic.Add(item.Key, newList);
                
            }

        }

    }

}