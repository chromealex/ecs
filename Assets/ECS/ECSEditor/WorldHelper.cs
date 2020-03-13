using System.Linq;

namespace ME.ECSEditor {
    
    using ME.ECS;
    using System.Collections;
    using System.Collections.Generic;

    public static class WorldHelper {

        private static Dictionary<int, ME.ECS.IComponentsBase> componentsCache = new Dictionary<int, ME.ECS.IComponentsBase>();

        public static FiltersStorage GetFilters(IWorldBase world) {

            var field = world.GetType().GetField("filtersStorage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (FiltersStorage)field.GetValue(world);

        }

        /*public static Dictionary<int, ME.ECS.IComponentsBase> GetComponentsStorage(IWorldBase world) {

            var field = world.GetType().GetField("componentsCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var dic = (IList)field.GetValue(world);
            WorldHelper.componentsCache.Clear();
            foreach (var item in dic) {

                WorldHelper.componentsCache.Add(, (ME.ECS.IComponentsBase)item);

            }

            return WorldHelper.componentsCache;

        }*/

        public static Dictionary<int, IList> GetEntitiesStorage(IWorldBase world) {

            var field = world.GetType().GetField("storagesCache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (Dictionary<int, IList>)field.GetValue(world);

        }

        public static IList<ME.ECS.ISystemBase> GetSystems(IWorldBase world) {

            var field = world.GetType().GetField("systems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return ((IList)field.GetValue(world)).Cast<ME.ECS.ISystemBase>().ToList();

        }

        public static IList<ME.ECS.IModuleBase> GetModules(IWorldBase world) {

            var field = world.GetType().GetField("modules", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return ((IList)field.GetValue(world)).Cast<ME.ECS.IModuleBase>().ToList();

        }

        public static bool HasMethod(object instance, string methodName) {

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

            }

            return true;

        }


    }

}