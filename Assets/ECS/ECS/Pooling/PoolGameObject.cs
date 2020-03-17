#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Views {
    
    public class PoolGameObject<T> where T : Component, IViewBase {

        private Dictionary<ViewId, Stack<T>> prefabToInstances = new Dictionary<ViewId, Stack<T>>();
        
        public T Spawn(T source, ViewId sourceId) {

            T instance = default;
            var found = false;
            //var key = (ViewId)(int.MaxValue + source.gameObject.GetInstanceID());
            var key = sourceId;
            Stack<T> list;
            if (this.prefabToInstances.TryGetValue(key, out list) == true) {

                if (list.Count > 0) {

                    instance = (T)list.Pop();
                    found = true;

                }

            } else {
                
                list = new Stack<T>();
                this.prefabToInstances.Add(key, list);
                
            }

            if (found == false) {

                var go = GameObject.Instantiate(source);
                instance = go.GetComponent<T>();

            }

            instance.prefabSourceId = key;
            instance.gameObject.SetActive(true);
            return instance;

        }

        public void Recycle(ref T instance) {
            
            var key = instance.prefabSourceId;
            Stack<T> list;
            if (this.prefabToInstances.TryGetValue(key, out list) == true) {
                
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