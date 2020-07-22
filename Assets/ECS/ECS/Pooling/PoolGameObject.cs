#if VIEWS_MODULE_SUPPORT
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.Views {
    
    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false),
     Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    public class PoolGameObject<T> where T : Component, IViewBase {

        private Dictionary<ViewId, Stack<T>> prefabToInstances = new Dictionary<ViewId, Stack<T>>();

        public void Clear() {

            foreach (var instance in this.prefabToInstances) {

                foreach (var view in instance.Value) {
                    
                    if (view != null) GameObject.Destroy(view.gameObject);
                    
                }
                
            }
            this.prefabToInstances.Clear();
            
        }
        
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

            var instanceInternal = (IViewBaseInternal)instance;
            instanceInternal.Setup(instance.world, new ViewInfo(instance.entity, key, instance.creationTick));
            instance.gameObject.SetActive(true);
            return instance;

        }

        public void Recycle(ref T instance) {
            
            var key = instance.prefabSourceId;
            Stack<T> list;
            if (this.prefabToInstances.TryGetValue(key, out list) == true) {

                if (instance != null) {

                    if (instance.gameObject != null) instance.gameObject.SetActive(false);
                    list.Push(instance);

                }

            } else {

                GameObject.Destroy(instance.gameObject);

            }

            instance = null;

        }

    }

}
#endif