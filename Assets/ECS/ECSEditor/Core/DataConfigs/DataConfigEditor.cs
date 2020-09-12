using System.Linq;

namespace ME.ECSEditor {

    using UnityEngine;
    using UnityEditor;
    using ME.ECS;

    [UnityEditor.CustomEditor(typeof(ME.ECS.DataConfigs.DataConfig), true)]
    public class DataConfigEditor : Editor {

        public struct Registry {

            public IStructComponent data;
            public int index;

        }

        public struct RegistryComponent {

            public IComponent data;
            public int index;

        }

        private static readonly System.Collections.Generic.Dictionary<Object, WorldsViewerEditor.WorldEditor> worldEditors = new System.Collections.Generic.Dictionary<Object, WorldsViewerEditor.WorldEditor>();

        public override void OnInspectorGUI() {

            var style = new GUIStyle(EditorStyles.toolbar);
            style.fixedHeight = 0f;
            style.stretchHeight = true;

            var backStyle = new GUIStyle(EditorStyles.label);
            backStyle.normal.background = Texture2D.whiteTexture;
            
            var dataConfig = (ME.ECS.DataConfigs.DataConfig)this.target;
            if (DataConfigEditor.worldEditors.TryGetValue(this.target, out var worldEditor) == false) {

                worldEditor = new WorldsViewerEditor.WorldEditor();
                DataConfigEditor.worldEditors.Add(this.target, worldEditor);

            }

            GUILayoutExt.Padding(8f, () => {
                
                var usedComponents = new System.Collections.Generic.HashSet<System.Type>();

                var kz = 0;
                var registries = dataConfig.structComponents;
                var sortedRegistries = new System.Collections.Generic.SortedDictionary<int, Registry>(new WorldsViewerEditor.DuplicateKeyComparer<int>());
                for (int i = 0; i < registries.Length; ++i) {

                    var registry = registries[i];
                    if (registry == null) {
                        continue;
                    }

                    var component = registry;
                    usedComponents.Add(component.GetType());

                    var editor = WorldsViewerEditor.GetEditor(component, out var order);
                    if (editor != null) {

                        sortedRegistries.Add(order, new Registry() {
                            index = i,
                            data = component
                        });

                    } else {

                        sortedRegistries.Add(0, new Registry() {
                            index = i,
                            data = component
                        });

                    }

                }

                foreach (var registryKv in sortedRegistries) {

                    var registry = registryKv.Value;
                    var component = registry.data;

                    var backColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                    GUILayout.BeginVertical(backStyle);
                    {
                        GUI.backgroundColor = backColor;
                        var editor = WorldsViewerEditor.GetEditor(component);
                        if (editor != null) {

                            EditorGUI.BeginChangeCheck();
                            editor.OnDrawGUI();
                            if (EditorGUI.EndChangeCheck() == true) {

                                component = editor.GetTarget<IStructComponent>();
                                dataConfig.structComponents[registry.index] = component;

                            }

                        } else {

                            var componentName = component.GetType().Name;
                            var fieldsCount = GUILayoutExt.GetFieldsCount(component);
                            if (fieldsCount == 0) {

                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.Toggle(componentName, true);
                                EditorGUI.EndDisabledGroup();

                            } else if (fieldsCount == 1) {

                                var changed = GUILayoutExt.DrawFields(worldEditor, component, componentName);
                                if (changed == true) {

                                    dataConfig.structComponents[registry.index] = component;

                                }

                            } else {

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(18f);
                                    GUILayout.BeginVertical();
                                    {

                                        var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                        var foldout = EditorPrefs.GetBool(key, true);
                                        GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                            var changed = GUILayoutExt.DrawFields(worldEditor, component);
                                            if (changed == true) {

                                                dataConfig.structComponents[registry.index] = component;

                                            }

                                        });
                                        EditorPrefs.SetBool(key, foldout);

                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();

                            }

                        }
                    }
                    GUILayout.EndVertical();

                    GUILayoutExt.Separator();

                }

                GUILayoutExt.DrawAddComponentMenu(usedComponents, (addType, isUsed) => {

                    if (isUsed == true) {

                        usedComponents.Remove(addType);
                        for (int i = 0; i < dataConfig.structComponents.Length; ++i) {

                            if (dataConfig.structComponents[i].GetType() == addType) {

                                var list = dataConfig.structComponents.ToList();
                                list.RemoveAt(i);
                                dataConfig.structComponents = list.ToArray();
                                dataConfig.OnScriptLoad();
                                break;

                            }

                        }

                    } else {

                        usedComponents.Add(addType);
                        System.Array.Resize(ref dataConfig.structComponents, dataConfig.structComponents.Length + 1);
                        dataConfig.structComponents[dataConfig.structComponents.Length - 1] = (IStructComponent)System.Activator.CreateInstance(addType);
                        dataConfig.OnScriptLoad();

                    }

                });

            });

            GUILayoutExt.Padding(8f, () => {

                var usedComponents = new System.Collections.Generic.HashSet<System.Type>();

                var kz = 0;
                var registries = dataConfig.components;
                var sortedRegistries = new System.Collections.Generic.SortedDictionary<int, RegistryComponent>(new WorldsViewerEditor.DuplicateKeyComparer<int>());
                for (int i = 0; i < registries.Length; ++i) {

                    var registry = registries[i];
                    if (registry == null) {
                        continue;
                    }

                    var component = registry;
                    usedComponents.Add(component.GetType());

                    var editor = WorldsViewerEditor.GetEditor(component, out var order);
                    if (editor != null) {

                        sortedRegistries.Add(order, new RegistryComponent() {
                            index = i,
                            data = component
                        });

                    } else {

                        sortedRegistries.Add(0, new RegistryComponent() {
                            index = i,
                            data = component
                        });

                    }

                }

                foreach (var registryKv in sortedRegistries) {

                    var registry = registryKv.Value;
                    var component = registry.data;

                    var backColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(1f, 1f, 1f, kz++ % 2 == 0 ? 0f : 0.05f);

                    GUILayout.BeginVertical(backStyle);
                    {
                        GUI.backgroundColor = backColor;
                        var editor = WorldsViewerEditor.GetEditor(component);
                        if (editor != null) {

                            EditorGUI.BeginChangeCheck();
                            editor.OnDrawGUI();
                            if (EditorGUI.EndChangeCheck() == true) {

                                component = editor.GetTarget<IComponent>();
                                dataConfig.components[registry.index] = component;

                            }

                        } else {

                            var componentName = component.GetType().Name;
                            var fieldsCount = GUILayoutExt.GetFieldsCount(component);
                            if (fieldsCount == 0) {

                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.Toggle(componentName, true);
                                EditorGUI.EndDisabledGroup();

                            } else if (fieldsCount == 1) {

                                var changed = GUILayoutExt.DrawFields(worldEditor, component, componentName);
                                if (changed == true) {

                                    dataConfig.components[registry.index] = component;

                                }

                            } else {

                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(18f);
                                    GUILayout.BeginVertical();
                                    {

                                        var key = "ME.ECS.WorldsViewerEditor.FoldoutTypes." + component.GetType().FullName;
                                        var foldout = EditorPrefs.GetBool(key, true);
                                        GUILayoutExt.FoldOut(ref foldout, componentName, () => {

                                            var changed = GUILayoutExt.DrawFields(worldEditor, component);
                                            if (changed == true) {

                                                dataConfig.components[registry.index] = component;

                                            }

                                        });
                                        EditorPrefs.SetBool(key, foldout);

                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();

                            }

                        }
                    }
                    GUILayout.EndVertical();

                    GUILayoutExt.Separator();

                }

                GUILayoutExt.DrawAddComponentMenu(usedComponents, (addType, isUsed) => {

                    if (isUsed == true) {

                        usedComponents.Remove(addType);
                        for (int i = 0; i < dataConfig.components.Length; ++i) {

                            if (dataConfig.components[i].GetType() == addType) {

                                var list = dataConfig.components.ToList();
                                list.RemoveAt(i);
                                dataConfig.components = list.ToArray();
                                dataConfig.OnScriptLoad();
                                break;

                            }

                        }

                    } else {

                        usedComponents.Add(addType);
                        System.Array.Resize(ref dataConfig.components, dataConfig.components.Length + 1);
                        dataConfig.components[dataConfig.components.Length - 1] = (IComponent)System.Activator.CreateInstance(addType);
                        dataConfig.OnScriptLoad();

                    }

                }, drawRefComponents: true);

            });

        }

    }

}