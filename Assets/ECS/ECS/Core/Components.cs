using System.Collections.Generic;
using System.Linq;
using EntityId = System.Int32;

namespace ME.ECS {

    public interface IComponents { }
    public class Components<TEntity, TState> : IComponents where TEntity : IEntity where TState : IStateBase {

        private Dictionary<EntityId, List<IComponent<TState, TEntity>>> dic;
        private bool freeze;

        public int Count {

            get {

                return this.dic.Count;

            }

        }

        public void Add(Entity entity, IComponent<TState, TEntity> data) {

            List<IComponent<TState, TEntity>> list;
            if (this.dic.TryGetValue(entity.id, out list) == true) {

                list.Add(data);

            } else {
                
                list = new List<IComponent<TState, TEntity>>();
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
            
            this.dic = new Dictionary<EntityId, List<IComponent<TState, TEntity>>>(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Components<TEntity, TState> other) {
            
            this.dic = new Dictionary<EntityId, List<IComponent<TState, TEntity>>>();
            foreach (var item in other.dic) {
                
                this.dic.Add(item.Key, item.Value.ToList());
                
            }

        }

        public void SetData(Dictionary<EntityId, List<IComponent<TState, TEntity>>> data) {

            if (this.freeze == false && data != null) {

                this.dic = data;

            }

        }

    }

}