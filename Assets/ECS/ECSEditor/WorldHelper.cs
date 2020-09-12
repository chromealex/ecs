using System.Linq;

namespace ME.ECSEditor {
    
    using ME.ECS;
    using System.Collections;
    using System.Collections.Generic;

    public static class WorldHelper {

        public static FiltersStorage GetFilters(World world) {

            //var field = world.GetType().GetField("filtersStorage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //return (FiltersStorage)field.GetValue(world);
            return world.currentState.filters;

        }

        public static ME.ECS.Components GetComponentsStorage(World world) {

            return world.currentState.components;
            
            /*
            var field = world.GetType().GetField("componentsCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var dic = (ME.ECS.IComponentsBase)field.GetValue(world);
            return dic;*/

        }

        public static IStructComponentsContainer GetStructComponentsStorage(World world) {

            var field = world.currentState.structComponents;//.GetType().GetField("componentsStructCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //var dic = (IStructComponentsContainer)field.GetValue(world);
            return field;

        }
        
        public static Storage GetEntitiesStorage(World world) {

            //var field = world.GetType().GetField("storagesCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            //return (Storage)field.GetValue(world);

            return world.currentState.storage;

        }

        public static ME.ECS.Collections.BufferArray<SystemGroup> GetSystems(World world) {

            return world.systemGroups;
            
            /*
            var field = world.GetType().GetField("systems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var val = field.GetValue(world);
            if (val == null) return new System.Collections.ObjectModel.Collection<ISystemBase>();
            return ((IList)val).Cast<ME.ECS.ISystemBase>().ToList();*/

        }

        public static ME.ECS.Collections.ListCopyable<ME.ECS.IModuleBase> GetModules(World world) {

            return world.modules;
            
            /*
            var field = world.GetType().GetField("modules", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var val = field.GetValue(world);
            if (val == null) return new System.Collections.ObjectModel.Collection<IModuleBase>();
            return ((IList)val).Cast<ME.ECS.IModuleBase>().ToList();*/

        }

        public static bool HasMethod(object instance, string methodName) {

            if (instance == null) return false;
            
            var hasAny = false;
            var targetType = instance.GetType();
            foreach (var @interface in targetType.GetInterfaces()) {

                var map = targetType.GetInterfaceMap(@interface);
                var interfaceMethod = @interface.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (interfaceMethod == null) continue;

                var index = System.Array.IndexOf(map.InterfaceMethods, interfaceMethod);
                var methodInfo = map.TargetMethods[index];
                var bodyInfo = (methodInfo == null ? null : methodInfo.GetMethodBody());
                if (bodyInfo == null || (bodyInfo.GetILAsByteArray().Length <= 2 && bodyInfo.LocalVariables.Count == 0)) {

                    return false;

                }

                hasAny = true;

            }

            return hasAny;

        }


    }

}