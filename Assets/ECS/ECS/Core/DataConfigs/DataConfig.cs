using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS.DataConfigs {

    [CreateAssetMenu(menuName = "ME.ECS/Data Config")]
    public class DataConfig : ScriptableObject {

        [SerializeReference]
        public IStructComponent[] structComponents = new IStructComponent[0];
        [SerializeReference]
        public IComponent[] components = new IComponent[0];

        public int[] structComponentsTypeIds = new int[0];
        public int[] componentsTypeIds = new int[0];
        
        public void Apply(in Entity entity) {

            for (int i = 0; i < this.structComponents.Length; ++i) {

                Worlds.currentWorld.SetData(in entity, in this.structComponents[i], in this.structComponentsTypeIds[i]);

            }

            for (int i = 0; i < this.components.Length; ++i) {

                Worlds.currentWorld.AddComponent(entity, this.components[i], this.componentsTypeIds[i]);

            }

        }

        public void OnValidate() {

            if (Application.isPlaying == true) return;
            
            this.OnScriptLoad();
            
        }

        public void OnScriptLoad() {

            if (Application.isPlaying == true) return;

            var allAsms = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in allAsms) {

                var asmType = asm.GetType("ME.ECS.ComponentsInitializer");
                if (asmType != null) {

                    var m = asmType.GetMethod("InitTypeId", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    m.Invoke(null, null);
                    
                    {
                        
                        this.structComponentsTypeIds = new int[this.structComponents.Length];
                        for (int i = 0; i < this.structComponents.Length; ++i) {

                            var obj = this.structComponents[i];
                            if (obj == null) {

                                this.structComponentsTypeIds[i] = -1;
                                continue;
                                
                            }
                            
                            var type = obj.GetType();
                            var id = ComponentTypesRegistry.typeId[type];
                            this.structComponentsTypeIds[i] = id;

                        }
                        
                    }
                    
                    {
                        
                        this.componentsTypeIds = new int[this.components.Length];
                        for (int i = 0; i < this.components.Length; ++i) {

                            var obj = this.components[i];
                            if (obj == null) {

                                this.componentsTypeIds[i] = -1;
                                continue;
                                
                            }
                            
                            var type = obj.GetType();
                            var id = ComponentTypesRegistry.typeId[type];
                            this.componentsTypeIds[i] = id;

                        }
                        
                    }
                    break;

                }

            }

        }
        
        #if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnScriptsReloaded() {

            var configs = Resources.FindObjectsOfTypeAll<DataConfig>();
            foreach (var config in configs) config.OnScriptLoad();

        }
        #endif

    }

}
