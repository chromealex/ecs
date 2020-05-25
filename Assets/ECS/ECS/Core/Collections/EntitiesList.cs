namespace ME.ECS.Collections {

    /*public interface IEntitiesList {

        int Count { get; }
        int Capacity { get; }
        void Clear();
        Entity Remove(Entity entity);
        Entity RemoveAt(int index);
        int GetNextIndex();

        Entity Get(int index);
        void Set(int index, Entity data);

    }

    public class EntitiesList : IEntitiesList, IPoolableRecycle, IPoolableSpawn {

        private Entity[] arr;
        private int count;
        private int capacity;
        private int initCapacity;

        public EntitiesList() : this(4) {}

        public EntitiesList(int capacity) {

            this.initCapacity = capacity;
            this.Init(capacity);
            
        }
        
        void IPoolableSpawn.OnSpawn() {

            var capacity = this.initCapacity;
            this.Init(capacity);
            
        }

        private void Init(int capacity) {
            
            if (capacity < 0) capacity = 0;

            this.count = 0;
            this.capacity = -1;
            this.Resize_INTERNAL(capacity);

        }

        void IPoolableRecycle.OnRecycle() {
            
            PoolArray<Entity>.Recycle(ref this.arr);
            
        }

        public int Count {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.count;
            }
        }

        public int Capacity {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return this.capacity;
            }
        }

        public ref Entity this[int index] {
            [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get {
                return ref this.arr[index];
            }
        }

        public Entity Get(int index) {

            return this.arr[index];

        }

        public void Set(int index, Entity data) {

            this.arr[index] = data;

        }

        public void Clear() {

            System.Array.Clear(this.arr, 0, this.arr.Length);
            this.count = 0;

        }

        public int GetNextIndex() {
            
            return this.count;

        }

        public Entity Remove(Entity entity) {
            
            return this.RemoveAt(entity.storageIdx);
            
        }

        public Entity RemoveAt(int index) {

            Entity changedEntity = default;
            
            var lastIdx = this.count - 1;
            if (index != lastIdx) {

                var entityData = this.arr[lastIdx];
                var ent = entityData;
                ent = new Entity(ent.id, index);
                entityData = ent;
                this.arr[index] = entityData;
                this.arr[lastIdx] = default;
                changedEntity = ent;

            } else {

                this.arr[index] = default;

            }

            --this.count;

            return changedEntity;

        }

        public void CopyFrom(EntitiesList other) {

            if (this.arr != null) PoolArray<Entity>.Recycle(ref this.arr);
            this.arr = PoolArray<Entity>.Spawn(other.arr.Length);
            System.Array.Copy(other.arr, this.arr, other.arr.Length);
            
            this.capacity = other.capacity;
            this.count = other.count;
            
        }

        private void Resize_INTERNAL(int newLength) {

            if (newLength > this.capacity) {
                
                var oldCapacity = this.capacity;
                if (this.capacity > 0) {
                    
                    this.capacity *= 2;
                    
                } else {

                    this.capacity = newLength;
                    oldCapacity = 0;

                }

                var arr = PoolArray<Entity>.Spawn(this.capacity);
                if (this.arr != null) {
                    
                    System.Array.Copy(this.arr, arr, this.arr.Length);
                    PoolArray<Entity>.Recycle(ref this.arr);
                    
                }
                this.arr = arr;

            }

        }

        public override string ToString() {
            
            return "EntitiesList(Count: " + this.Count.ToString() + ", Capacity: " + this.Capacity.ToString() + ") ";
            
        }

    }*/

}