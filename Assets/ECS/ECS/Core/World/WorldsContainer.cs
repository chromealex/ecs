using System.Collections.Generic;

namespace ME.ECS {

    public static class Worlds {

        public static IWorldBase currentWorld;

    }

    public static class Worlds<TState> where TState : class, IState<TState> {

        public static IWorld<TState> currentWorld;
        public static TState currentState;
        
        private static Dictionary<int, IWorld<TState>> cache = new Dictionary<int, IWorld<TState>>(1);

        public static IWorld<TState> GetWorld(int id) {

            IWorld<TState> world;
            if (Worlds<TState>.cache.TryGetValue(id, out world) == true) {

                return world;
                
            }

            return null;

        }

        public static void Register(IWorld<TState> world) {
            
            Worlds<TState>.cache.Add(world.id, world);
            
        }
        
        public static void UnRegister(IWorld<TState> world) {
            
            Worlds<TState>.cache.Remove(world.id);
            
        }

    }

}