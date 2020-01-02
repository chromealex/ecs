using System.Collections.Generic;
using System.Linq;

namespace ME.ECS {
    
    public interface IFilter { }
    public class Filter<T> : IFilter where T : IEntity {

        private List<T> list;
        private bool freeze;

        public int Count {

            get {

                return this.list.Count;

            }

        }
        
        public T this[int index] {
            get {
                return this.list[index];
            }
            set {
                this.list[index] = value;
            }
        }

        public void Initialize(int capacity) {
            
            this.list = new List<T>(capacity);

        }

        public void SetFreeze(bool freeze) {

            this.freeze = freeze;

        }

        public void CopyFrom(Filter<T> other) {
            
            this.list = other.list.ToList();
            
        }

        public List<T> GetData() {

            return this.list;

        }

        public void SetData(List<T> data) {

            if (this.freeze == false && data != null) {

                this.list = data;

            }

        }

    }

}