using System.Collections.Generic;
using System.Linq;
using EntityId = System.Int32;

namespace ME.ECS {

    public interface IComponents : IPoolableRecycle {

        void RemoveAll(Entity entity);
        void RemoveAll<TComponent>() where TComponent : class, IComponentBase;

    }
    public class Components<TEntity, TState> : IComponents where TEntity : IEntity where TState : IStateBase {

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

        public void RemoveAll<TComponent>() where TComponent : class, IComponentBase {

            foreach (var item in this.dic) {

                var list = item.Value;
                for (int i = 0; i < list.Count; ++i) {

                    var listItem = list[i];
                    if (listItem is TComponent listItemComponent) {
                        
                        PoolComponents.Recycle(listItemComponent);
                        list.RemoveAt(i);
                        --i;

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
            
            if (this.dic != null) PoolDictionary<EntityId, List<IComponent<TState, TEntity>>>.Recycle(ref this.dic);
            this.dic = PoolDictionary<EntityId, List<IComponent<TState, TEntity>>>.Spawn(this.capacity);
            foreach (var item in other.dic) {
                
                var newList = PoolList<IComponent<TState, TEntity>>.Spawn(item.Value.Capacity);
                newList.AddRange(item.Value);
                this.dic.Add(item.Key, newList);
                PoolList<IComponent<TState, TEntity>>.Recycle(item.Value);

            }

        }

    }

}