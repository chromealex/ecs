#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using UnityEngine;
using ViewId = System.UInt64;

namespace ME.ECS.Views {
    
    public static class PoolGameObject {

        private static Dictionary<ViewId, Stack<Component>> prefabToInstances = new Dictionary<ViewId, Stack<Component>>();
        
        public static T Spawn<T>(T source, ViewId sourceId) where T : Component, IViewBase {

            T instance = default;
            var found = false;
            //var key = (ViewId)(int.MaxValue + source.gameObject.GetInstanceID());
            var key = sourceId;
            Stack<Component> list;
            if (PoolGameObject.prefabToInstances.TryGetValue(key, out list) == true) {

                if (list.Count > 0) {

                    instance = (T)list.Pop();
                    found = true;

                }

            } else {
                
                list = new Stack<Component>();
                PoolGameObject.prefabToInstances.Add(key, list);
                
            }

            if (found == false) {

                var go = GameObject.Instantiate(source);
                instance = go.GetComponent<T>();

            }

            instance.prefabSourceId = key;
            instance.gameObject.SetActive(true);
            return instance;

        }

        public static void Recycle<T>(ref T instance) where T : Component, IViewBase {
            
            var key = instance.prefabSourceId;
            Stack<Component> list;
            if (PoolGameObject.prefabToInstances.TryGetValue(key, out list) == true) {
                
                instance.gameObject.SetActive(false);
                list.Push(instance);
                
            } else {

                GameObject.Destroy(instance.gameObject);

            }

            instance = null;

        }

    }

}
#endif