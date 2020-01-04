
namespace ME.ECS {

    public static class PoolArray {

        public static T[] Spawn<T>(int length) {
            
            return new T[length];
            
        }

        public static void Recycle<T>(ref T[] buffer) {

            buffer = null;

        }

    }

}
