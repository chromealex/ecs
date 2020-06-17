using System.Collections.Generic;

namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public static class Worlds {

        public static World currentWorld;
        public static State currentState;
        
        public static readonly List<World> registeredWorlds = new List<World>();
        private static Dictionary<int, World> cache = new Dictionary<int, World>(1);

        internal static bool isInDeInitialization;
        public static void DeInitializeBegin() {

            Worlds.isInDeInitialization = true;

        }

        public static void DeInitializeEnd() {
            
            Worlds.isInDeInitialization = false;
            
        }

        public static World GetWorld(int id) {

            World world;
            if (Worlds.cache.TryGetValue(id, out world) == true) {

                return world;
                
            }

            return null;

        }

        public static void Register(World world) {
            
            Worlds.registeredWorlds.Add(world);
            Worlds.cache.Add(world.id, world);
            
        }
        
        public static void UnRegister(World world) {
            
            if (Worlds.registeredWorlds != null) Worlds.registeredWorlds.Remove(world);
            if (Worlds.cache != null) Worlds.cache.Remove(world.id);

            if (world == Worlds.currentWorld) {

                Worlds.currentWorld = null;
                Worlds.currentState = null;

            }
            
        }

    }

}