using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;

namespace ME.ECSEditor {
    
    using ME.ECS;

    public abstract class CustomEditorAttribute : System.Attribute {

        public System.Type type;
        public int order;

        protected CustomEditorAttribute(System.Type type, int order = 0) {

            this.type = type;
            this.order = order;

        }

    }

    public class ComponentCustomEditorAttribute : CustomEditorAttribute {

	    public ComponentCustomEditorAttribute(System.Type type, int order = 0) : base(type, order) {}

    }

    public static class GUILayoutExt {

	    public static void DrawGradient(float height, Color from, Color to, string labelFrom, string labelTo) {
	        
		    var tex = new Texture2D(2, 1, TextureFormat.RGBA32, false);
		    tex.filterMode = FilterMode.Bilinear;
		    tex.wrapMode = TextureWrapMode.Clamp;
		    tex.SetPixel(0, 0, from);
		    tex.SetPixel(1, 0, to);
		    tex.Apply();
		    
		    Rect rect = EditorGUILayout.GetControlRect(false, height);
		    rect.height = height;
		    EditorGUI.DrawTextureTransparent(rect, tex, ScaleMode.StretchToFill);
		    
		    GUILayout.BeginHorizontal();
		    {
			    GUILayout.Label(labelFrom);
			    GUILayout.FlexibleSpace();
			    GUILayout.Label(labelTo);
		    }
		    GUILayout.EndHorizontal();
		    
	    }

	    public static void ProgressBar(float value, float max, bool drawLabel = false) {
		    
		    GUILayoutExt.ProgressBar(value, max, new Color(0f, 0f, 0f, 0.3f), new Color32(104, 148, 192, 255), drawLabel);
		    
	    }

	    public static void ProgressBar(float value, float max, Color back, Color fill, bool drawLabel = false) {

		    var progress = value / max;
		    var lineHeight = (drawLabel == true ? 8f : 4f);
		    Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
		    rect.height = lineHeight;
		    var fillRect = rect;
		    fillRect.width = progress * rect.width;
		    EditorGUI.DrawRect(rect, back);
		    EditorGUI.DrawRect(fillRect, fill);

		    if (drawLabel == true) {
			    
			    EditorGUI.LabelField(rect, string.Format("{0}/{1}", value, max), EditorStyles.centeredGreyMiniLabel);
			    
		    }

	    }

	    public static Entity DrawEntitySelection(World world, in Entity entity, bool checkAlive, bool drawSelectButton = true) {
		    
		    ref var currentEntity = ref world.GetEntityById(in entity.id);
		    if (checkAlive == true && entity.IsAlive() == false) {

			    EditorGUILayout.HelpBox("This entity version is already in pool, the list of components has been changed.", MessageType.Warning);
			    if (currentEntity.version > 0) {
                        
				    GUILayout.Label("New entity: " + currentEntity.ToSmallString());
                        
			    } else {
                        
				    GUILayout.Label("New entity is not active");
                        
			    }

		    }

		    if (drawSelectButton == true && currentEntity.version > 0) {

			    UnityEngine.GUILayout.BeginHorizontal();
			    UnityEngine.GUILayout.FlexibleSpace();
			    if (UnityEngine.GUILayout.Button("Select Entity", UnityEngine.GUILayout.Width(150f)) == true) {

				    WorldsViewerEditor.SelectEntity(currentEntity);

			    }
			    UnityEngine.GUILayout.FlexibleSpace();
			    UnityEngine.GUILayout.EndHorizontal();

		    }

		    return currentEntity;

	    }
	    
	    public static void DrawAddEntityMenu(EntityDebugComponent entityDebugComponent) {
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 12;
            style.fixedWidth = 230;
            style.fixedHeight = 23;
 
            var rect = GUILayoutUtility.GetLastRect();
 
            if (GUILayout.Button("Assign Entity", style)) {
                
                rect.y += 26f;
                rect.x += rect.width;
                rect.width = style.fixedWidth;
                //AddEquipmentBehaviourWindow.Show(rect, entity, usedComponents);

                //var lastRect = GUILayoutUtility.GetLastRect();
                //lastRect.height = 200f;
                var v2 = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = v2.x;
                rect.y = v2.y;
                rect.height = 320f;
                
                var popup = new Popup() {
	                title = "Entities",
	                autoHeight = false,
	                autoClose = true,
	                screenRect = rect,
	                searchText = string.Empty,
	                separator = '.',
	                
                };
                
                var worldEditor = new WorldsViewerEditor.WorldEditor();
                worldEditor.world = Worlds.currentWorld;
                var entities = worldEditor.GetEntitiesStorage();
                
                foreach (var idx in entities) {

	                if (entities.IsFree(idx) == true) continue;
	                var entity = entities[idx];
	                var name = entity.HasData<ME.ECS.Name.Name>() == true ? entity.GetData<ME.ECS.Name.Name>().value : "Unnamed";
	                popup.Item(string.Format("{0} ({1})", name, entity), () => {
		                
		                entityDebugComponent.world = worldEditor.world;
		                entityDebugComponent.entity = entity;
		                
	                });
	                
                }
                popup.Show();

            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
 
        }
	    
	    private static System.Type[] allStructComponents;
	    private static System.Type[] allComponents;

	    public static void DrawAddComponentMenu(System.Collections.Generic.HashSet<System.Type> usedComponents, System.Action<System.Type, bool> onAdd, bool drawRefComponents = false) {
		    
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 12;
            style.fixedWidth = 230;
            style.fixedHeight = 23;
 
            var rect = GUILayoutUtility.GetLastRect();
 
            if (GUILayout.Button(drawRefComponents == true ? "Edit Managed Components" : "Edit Components", style)) {
                
                rect.y += 26f;
                rect.x += rect.width;
                rect.width = style.fixedWidth;
                //AddEquipmentBehaviourWindow.Show(rect, entity, usedComponents);

                if (drawRefComponents == true && GUILayoutExt.allStructComponents == null) {

	                GUILayoutExt.allStructComponents = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsValueType == true && typeof(IStructComponent).IsAssignableFrom(x)).ToArray();

                }

                if (drawRefComponents == true && GUILayoutExt.allComponents == null) {

	                GUILayoutExt.allComponents = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => x.IsInterface == false && x.IsValueType == false && typeof(IComponent).IsAssignableFrom(x)).ToArray();

                }

                //var lastRect = GUILayoutUtility.GetLastRect();
                //lastRect.height = 200f;
                var v2 = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = v2.x;
                rect.y = v2.y;
                rect.height = 320f;
                
                var popup = new Popup() {
	                title = "Components",
	                autoHeight = false,
	                screenRect = rect,
	                searchText = string.Empty,
	                separator = '.',
	                
                };
                var arr = GUILayoutExt.allStructComponents;
                if (drawRefComponents == true) arr = GUILayoutExt.allComponents;
                foreach (var type in arr) {

	                var isUsed = usedComponents.Contains(type);

	                var addType = type;
	                var name = type.FullName;
	                var fixName = string.Empty;

	                if (name.StartsWith("ME.ECS") == true) {
		                
		                var spName = name.Split('.');
		                var p1 = spName[spName.Length - 2];
		                var p2 = spName[spName.Length - 1];
		                if (p1 == p2) {
			                
			                fixName = "ECS." + p2;

		                } else {

			                fixName = "ECS." + p1 + "." + p2;

		                }

	                } else {

		                var spName = name.Split('.');
		                var component = spName[spName.Length - 1];
		                spName = name.Split(new[] { ".Features." }, StringSplitOptions.RemoveEmptyEntries);
		                var rootName = spName[0];
		                name = spName[spName.Length - 1];
		                spName = name.Split(new[] { ".Components." }, StringSplitOptions.RemoveEmptyEntries);
		                var feature = spName[0];
						fixName = rootName + "." + feature + "." + component;

	                }

	                System.Action<PopupWindowAnim.PopupItem> onItemSelect = (item) => {
		                
		                isUsed = usedComponents.Contains(type);
		                onAdd.Invoke(addType, isUsed);
		                
		                isUsed = usedComponents.Contains(type);
		                var tex = isUsed == true ? EditorStyles.toggle.onNormal.scaledBackgrounds[0] : EditorStyles.toggle.normal.scaledBackgrounds[0];
		                item.image = tex;
		                
	                };
	                
	                if (isUsed == true) popup.Item("Used." + type.Name, isUsed == true ? EditorStyles.toggle.onNormal.scaledBackgrounds[0] : EditorStyles.toggle.normal.scaledBackgrounds[0], onItemSelect, searchable: false);
	                popup.Item(fixName, isUsed == true ? EditorStyles.toggle.onNormal.scaledBackgrounds[0] : EditorStyles.toggle.normal.scaledBackgrounds[0], onItemSelect);

                }
                popup.Show();

            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
 
	    }

	    public static void DrawAddComponentMenu(Entity entity, System.Collections.Generic.HashSet<System.Type> usedComponents, IStructComponentsContainer componentsStructStorage) {
            
		    GUILayoutExt.DrawAddComponentMenu(usedComponents, (addType, isUsed) => {
			    
			    var registries = componentsStructStorage.GetAllRegistries();
			    for (int i = 0; i < registries.Length; ++i) {
		                
				    var registry = registries.arr[i];
				    if (registry == null) continue;
				    if (registry.HasType(addType) == true) {

					    if (isUsed == true) {

						    usedComponents.Remove(addType);
						    registry.RemoveObject(entity);
						    Worlds.currentWorld.RemoveComponentFromFilter(entity);

					    } else {
				                
						    usedComponents.Add(addType);
						    registry.SetObject(entity, (IStructComponent)System.Activator.CreateInstance(addType));
						    Worlds.currentWorld.AddComponentToFilter(entity);

					    }

					    break;

				    }
		                
			    }

		    });
		    
        }
        
        public static void CollectEditors<TEditor, TAttribute>(ref System.Collections.Generic.Dictionary<System.Type, TEditor> dic, System.Reflection.Assembly searchAssembly = null) where TEditor : IGUIEditorBase where TAttribute : CustomEditorAttribute {

            if (dic == null) {

                dic = new System.Collections.Generic.Dictionary<System.Type, TEditor>();
                
                var assembly = (searchAssembly == null ? System.Reflection.Assembly.GetExecutingAssembly() : searchAssembly);
                var types = assembly.GetTypes();
                foreach (var type in types) {

                    var attrs = type.GetCustomAttributes(typeof(TAttribute), inherit: true);
                    if (attrs.Length > 0) {

                        if (attrs[0] is TAttribute attr) {

                            if (typeof(TEditor).IsAssignableFrom(type) == true) {
                            
                                var editor = (TEditor)System.Activator.CreateInstance(type);
                                if (dic.ContainsKey(attr.type) == false) {
	                                
	                                dic.Add(attr.type, editor);
	                                
                                }

                            }

                        }

                    }

                }

            }

        }
        
        public static bool ToggleLeft(ref bool state, ref bool isDirty, string caption, string text) {

            var labelRich = new GUIStyle(EditorStyles.label);
            labelRich.richText = true;

            var isLocalDirty = false;
            var flag = EditorGUILayout.ToggleLeft(caption, state, labelRich);
            if (flag != state) {

                isLocalDirty = true;
                isDirty = true;
                state = flag;
                        
            }
            if (string.IsNullOrEmpty(text) == false) GUILayoutExt.SmallLabel(text);
            EditorGUILayout.Space();

            return isLocalDirty;

        }

        public static LayerMask DrawLayerMaskField(string label, LayerMask layerMask) {

	        System.Collections.Generic.List<string> layers = new System.Collections.Generic.List<string>();
	        System.Collections.Generic.List<int> layerNumbers = new System.Collections.Generic.List<int>();

	        for (int i = 0; i < 32; i++) {
		        string layerName = LayerMask.LayerToName(i);
		        if (layerName != "") {
			        layers.Add(layerName);
			        layerNumbers.Add(i);
		        }
	        }
	        int maskWithoutEmpty = 0;
	        for (int i = 0; i < layerNumbers.Count; i++) {
		        if (((1 << layerNumbers[i]) & layerMask.value) > 0)
			        maskWithoutEmpty |= (1 << i);
	        }
	        maskWithoutEmpty = EditorGUILayout.MaskField( label, maskWithoutEmpty, layers.ToArray());
	        int mask = 0;
	        for (int i = 0; i < layerNumbers.Count; i++) {
		        if ((maskWithoutEmpty & (1 << i)) > 0)
			        mask |= (1 << layerNumbers[i]);
	        }
	        layerMask.value = mask;
	        return layerMask;

        }
        
        public static void DrawHeader(string caption) {

            var style = GUIStyle.none;//new GUIStyle("In BigTitle");
            //new Editor().DrawHeader();
            
            GUILayout.Space(4f);
            GUILayoutExt.Separator();
            GUILayoutExt.Padding(
                16f, 4f,
                () => {
                    
                    GUILayout.Label(caption, EditorStyles.boldLabel);
                    
                }, style);
            GUILayoutExt.Separator(new Color(0.2f, 0.2f, 0.2f, 1f));
            
        }

        public static void SmallLabel(string text) {

            var labelRich = new GUIStyle(EditorStyles.miniLabel);
            labelRich.richText = true;
            labelRich.wordWrap = true;

            var oldColor = GUI.color;
            var c = oldColor;
            c.a = 0.5f;
            GUI.color = c;
            
            EditorGUILayout.LabelField(text, labelRich);

            GUI.color = oldColor;

        }

        public static int Pages(int count, int page, int elementsOnPage, System.Action<int, int> onDraw, System.Action<int> onPageElementsChanged, System.Action onDrawHeader = null) {

            var from = page * elementsOnPage;
            var to = from + elementsOnPage;
            if (from < 0) from = 0;
            if (to > count) to = count;
            var pages = Mathf.CeilToInt(count / (float)elementsOnPage) - 1;
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (onDrawHeader != null) onDrawHeader.Invoke();
                
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginHorizontal();
                {

                    GUILayout.Label("On page:", EditorStyles.toolbarButton);
                    if (GUILayout.Button(elementsOnPage.ToString(), EditorStyles.toolbarDropDown, GUILayout.MinWidth(30f)) == true) {

                        var items = new[] { 10, 20, 30, 40, 50, 100 };
                        var menu = new GenericMenu();
                        for (int i = 0; i < items.Length; ++i) {

                            var idx = i;
                            menu.AddItem(new GUIContent(items[i].ToString()), items[i] == elementsOnPage, () => { onPageElementsChanged.Invoke(items[idx]); });

                        }

                        //menu.DropDown(GUILayoutUtility.GetLastRect());
                        menu.ShowAsContext();

                    }

                    EditorGUI.BeginDisabledGroup(page <= 0);
                    if (GUILayout.Button("◄", EditorStyles.toolbarButton) == true) {

                        --page;

                    }

                    EditorGUI.EndDisabledGroup();

                    var pageStr = GUILayout.TextField((page + 1).ToString(), EditorStyles.toolbarTextField, GUILayout.MinWidth(20f));
                    if (int.TryParse(pageStr, out var res) == true) {

                        page = res - 1;

                    }
                    GUILayout.Label("/", EditorStyles.toolbarButton);
                    GUILayout.Label(string.Format("{0}", pages + 1), EditorStyles.toolbarButton, GUILayout.MinWidth(20f));

                    EditorGUI.BeginDisabledGroup(page >= pages);
                    if (GUILayout.Button("►", EditorStyles.toolbarButton) == true) {

                        ++page;

                    }

                    EditorGUI.EndDisabledGroup();

                }
                GUILayout.EndHorizontal();
                
                if (page < 0) page = 0;
                if (page > pages) page = pages;

            }
            GUILayout.EndHorizontal();
            
            onDraw.Invoke(from, to);

            return page;

        }

        public static int GetFieldsCount(object instance) {
            
            var fields = instance.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return fields.Length;

        }

        public static bool DrawFields(WorldsViewerEditor.WorldEditor world, object instance, string customName = null) {

            //var padding = 2f;
            //var margin = 1f;
            //var cellHeight = 24f;
            //var tableStyle = new GUIStyle("Box");

            var changed = false;
            var fields = instance.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (fields.Length > 0) {

                /*GUILayout.BeginHorizontal();
                {
                    GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Field", EditorStyles.miniBoldLabel); },
                                     tableStyle, GUILayout.Width(fieldWidth),
                                     GUILayout.Height(cellHeight));
                    GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Value", EditorStyles.miniBoldLabel); },
                                     tableStyle, GUILayout.ExpandWidth(true),
                                     GUILayout.Height(cellHeight));
                }
                GUILayout.EndHorizontal();*/

                foreach (var field in fields) {

                    //GUILayout.BeginHorizontal();
                    {
                        //GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.DataLabel(field.Name); }, tableStyle,
                        //                 GUILayout.Width(fieldWidth), GUILayout.Height(cellHeight));
                        //GUILayoutExt.Box(padding, margin, () => {

                        //GUILayoutExt.DataLabel(field.Name);
                        //var lastRect = GUILayoutUtility.GetLastRect();
                        var value = field.GetValue(instance);
                        var oldValue = value;
                        var isEditable = GUILayoutExt.PropertyField(world, field.Name, field, field.FieldType, ref value, typeCheckOnly: true);
                        EditorGUI.BeginDisabledGroup(disabled: (isEditable == false));
                        if (GUILayoutExt.PropertyField(world, customName != null ? customName : field.Name, field, field.FieldType, ref value, typeCheckOnly: false) == true) {

                            if (oldValue != value) {

                                field.SetValue(instance, value);
                                changed = true;

                            }

                        }
                        EditorGUI.EndDisabledGroup();
                        //GUILayout.EndHorizontal();

                        //}, tableStyle, GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                    }
                    //GUILayout.EndHorizontal();

                }

            }

            return changed;

        }

        public static void Icon(string path, float width = 32f, float height = 32f) {

	        var icon = new GUIStyle();
	        icon.normal.background = UnityEditor.Experimental.EditorResources.Load<Texture2D>(path);
	        EditorGUILayout.LabelField(string.Empty, icon, GUILayout.Width(width), GUILayout.Height(height));

        }

        private static bool HasBaseType(this System.Type type, System.Type baseType) {

	        return baseType.IsAssignableFrom(type);

        }

        private static bool HasInterface(this System.Type type, System.Type interfaceType) {

	        return interfaceType.IsAssignableFrom(type);
	        
        }

        public static bool PropertyField(WorldsViewerEditor.WorldEditor world, string caption, System.Reflection.FieldInfo fieldInfo, System.Type type, ref object value, bool typeCheckOnly) {

            if (typeCheckOnly == false && value == null && type.HasBaseType(typeof(UnityEngine.Object)) == false) {

                EditorGUILayout.LabelField("Null");
                return false;

            }

            if (type.IsEnum == true) {

	            if (typeCheckOnly == false) {

		            var attrs = fieldInfo.GetCustomAttributes(typeof(IsBitmask), true);
		            if (attrs.Length == 0) {
				        
			            value = EditorGUILayout.EnumPopup(caption, (Enum)value);
    
		            } else {
			            
			            value = EditorGUILayout.EnumFlagsField(caption, (Enum)value);

		            }
		            
	            }

            } else if (type.HasInterface(typeof(ME.ECS.Collections.IBufferArray)) == true ||
                       type.IsArray == true ||
                       type.HasInterface(typeof(ME.ECS.Collections.IStackArray)) == true)
            {

	            if (typeCheckOnly == false) {

		            if (type.HasInterface(typeof(ME.ECS.Collections.IStackArray)) == true) {
			            
			            var arr = (ME.ECS.Collections.IStackArray)value;
			            var state = world.IsFoldOutCustom(value);
			            GUILayoutExt.FoldOut(ref state, string.Format("{0} [{1}]", caption, arr.Length), () => {

				            for (int i = 0; i < arr.Length; ++i) {

					            if (i > 0) GUILayoutExt.Separator();
					            var arrValue = arr[i];
					            object v = default;
					            var isEditable = GUILayoutExt.PropertyField(world, null, fieldInfo, arrValue.GetType(), ref v, typeCheckOnly: true);
					            EditorGUI.BeginDisabledGroup(disabled: (isEditable == false));
					            GUILayoutExt.PropertyField(world, "Element [" + i.ToString() + "]", fieldInfo, arrValue.GetType(), ref arrValue, typeCheckOnly: false);
					            EditorGUI.EndDisabledGroup();
					            arr[i] = arrValue;

				            }
			            
			            });
			            world.SetFoldOutCustom(value, state);
			            value = arr;

		            } else if (type.HasInterface(typeof(ME.ECS.Collections.IBufferArray)) == true) {
			            
			            var arr = (ME.ECS.Collections.IBufferArray)value;
			            var state = true;
			            GUILayoutExt.FoldOut(ref state, string.Format("{0} [{1}]", caption, arr.Count), () => {

				            var array = arr.GetArray();
				            for (int i = 0; i < arr.Count; ++i) {

					            if (i > 0) GUILayoutExt.Separator();
					            var arrValue = array.GetValue(i);
					            object v = default;
					            var isEditable = GUILayoutExt.PropertyField(world, null, fieldInfo, arrValue.GetType(), ref v, typeCheckOnly: true);
					            EditorGUI.BeginDisabledGroup(disabled: (isEditable == false));
					            GUILayoutExt.PropertyField(world, "Element [" + i.ToString() + "]", fieldInfo, arrValue.GetType(), ref arrValue, typeCheckOnly: false);
					            EditorGUI.EndDisabledGroup();
					            array.SetValue(arrValue, i);

				            }
			            
			            });
			            value = arr;

		            } else if (type.IsArray == true) {

			            var arr = (System.Array)value;
			            var state = true;
			            GUILayoutExt.FoldOut(ref state, string.Format("{0} [{1}]", caption, arr.Length), () => {

				            var array = arr;
				            for (int i = 0; i < arr.Length; ++i) {

					            if (i > 0) GUILayoutExt.Separator();
					            var arrValue = array.GetValue(i);
					            object v = default;
					            var isEditable = GUILayoutExt.PropertyField(world, null, fieldInfo, arrValue.GetType(), ref v, typeCheckOnly: true);
					            EditorGUI.BeginDisabledGroup(disabled: (isEditable == false));
					            GUILayoutExt.PropertyField(world, "Element [" + i.ToString() + "]", fieldInfo, arrValue.GetType(), ref arrValue, typeCheckOnly: false);
					            EditorGUI.EndDisabledGroup();
					            array.SetValue(arrValue, i);

				            }

			            });

		            }

	            }
	            
            } else if (type == typeof(RefEntity)) {

	            if (typeCheckOnly == false) {

		            var entity = (Worlds.currentWorld != null) ? ((RefEntity)value).entity : Entity.Empty;
		            GUILayout.BeginHorizontal();
		            var buttonWidth = 50f;
		            EditorGUILayout.LabelField(caption, GUILayout.Width(EditorGUIUtility.labelWidth));
		            GUILayoutExt.Icon("Assets/ECS/ECSEditor/EditorResources/icon-link.png", 16f, 16f);
		            if (entity == Entity.Empty) {
						GUILayout.Label("Empty");   
		            } else {
			            var customName = entity.GetData<ME.ECS.Name.Name>(createIfNotExists: false).value;
			            GUILayout.BeginVertical();
			            GUILayout.Label(string.IsNullOrEmpty(customName) == false ? customName : "Unnamed");
			            GUILayout.Label(entity.ToSmallString(), EditorStyles.miniLabel);
			            GUILayout.EndVertical();
		            }

		            GUILayout.FlexibleSpace();
		            EditorGUI.BeginDisabledGroup(entity == Entity.Empty);
		            if (GUILayout.Button("Select", GUILayout.Width(buttonWidth)) == true) {

			            WorldsViewerEditor.SelectEntity(entity);

		            }
		            EditorGUI.EndDisabledGroup();
		            GUILayout.EndHorizontal();
		            
	            }

            } else if (type == typeof(Entity)) {

	            if (typeCheckOnly == false) {

		            var entity = (Entity)value;
		            GUILayout.BeginHorizontal();
		            var buttonWidth = 50f;
		            EditorGUILayout.LabelField(caption, GUILayout.Width(EditorGUIUtility.labelWidth));
		            if (entity == Entity.Empty) {
			            GUILayout.Label("Empty");   
		            } else {
			            var customName = entity.GetData<ME.ECS.Name.Name>(createIfNotExists: false).value;
			            GUILayout.BeginVertical();
			            GUILayout.Label(string.IsNullOrEmpty(customName) == false ? customName : "Unnamed");
			            GUILayout.Label(entity.ToSmallString(), EditorStyles.miniLabel);
			            GUILayout.EndVertical();
		            }

		            GUILayout.FlexibleSpace();
		            EditorGUI.BeginDisabledGroup(entity == Entity.Empty);
		            if (GUILayout.Button("Select", GUILayout.Width(buttonWidth)) == true) {

			            WorldsViewerEditor.SelectEntity(entity);

		            }
		            EditorGUI.EndDisabledGroup();
		            GUILayout.EndHorizontal();

	            }

            } else if (type == typeof(Color)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.ColorField(caption, (Color)value);
                    GUILayout.Label(value.ToString());

                }
                
            } else if (type == typeof(Color32)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.ColorField(caption, (Color32)value);
                    GUILayout.Label(value.ToString());

                }
                
            } else if (type == typeof(Vector2)) {

	            if (typeCheckOnly == false) {

		            value = EditorGUILayout.Vector2Field(caption, (Vector2)value);

	            }

            } else if (type == typeof(Vector3)) {

	            if (typeCheckOnly == false) {

		            value = EditorGUILayout.Vector3Field(caption, (Vector3)value);

	            }

            }  else if (type == typeof(FPVector2)) {

	            if (typeCheckOnly == false) {

		            value = (FPVector2)EditorGUILayout.Vector2Field(caption, (FPVector2)value);

	            }

            } else if (type == typeof(FPVector3)) {

	            if (typeCheckOnly == false) {

		            value = (FPVector3)EditorGUILayout.Vector3Field(caption, (FPVector3)value);

	            }

            } else if (type == typeof(Vector4)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.Vector4Field(caption, (Vector4)value);

                }

            } else if (type == typeof(Quaternion)) {

	            if (typeCheckOnly == false) {

		            value = Quaternion.Euler(EditorGUILayout.Vector3Field(caption, ((Quaternion)value).eulerAngles));

	            }

            } else if (type == typeof(FPQuaternion)) {

	            if (typeCheckOnly == false) {

		            value = (FPQuaternion)Quaternion.Euler(EditorGUILayout.Vector3Field(caption, ((FPQuaternion)value).eulerAngles));

	            }

            } else if (type == typeof(pfloat)) {

	            if (typeCheckOnly == false) {

		            value = (pfloat)EditorGUILayout.FloatField(caption, (float)(pfloat)value);

	            }

            } else if (type == typeof(int)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.IntField(caption, (int)value);

                }

            } else if (type == typeof(float)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.FloatField(caption, (float)value);

                }

            } else if (type == typeof(double)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.DoubleField(caption, (double)value);

                }

            } else if (type == typeof(long)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.LongField(caption, (long)value);

                }

            } else if (type.HasBaseType(typeof(UnityEngine.Object)) == true) {

	            if (typeCheckOnly == false) {

		            var obj = (UnityEngine.Object)value;
		            obj = EditorGUILayout.ObjectField(caption, obj, type, allowSceneObjects: true);
		            value = obj;

	            }

            } else if (type == typeof(string)) {

	            if (typeCheckOnly == false) {

		            var str = value.ToString();
		            if (str.Contains("\n") == true) {

			            value = EditorGUILayout.TextArea(str);

		            } else {

			            value = EditorGUILayout.TextField(caption, str);

		            }

	            }

	            return false;

            } else {

	            if (typeCheckOnly == false) {

		            var str = value.ToString();
		            if (str.Contains("\n") == true) {

			            EditorGUILayout.TextArea(str);

		            } else {

			            EditorGUILayout.TextField(caption, str);

		            }

	            }

	            return false;

            }

            return true;

        }

        public static void DataLabel(string content, params GUILayoutOption[] options) {

            var style = new GUIStyle(EditorStyles.label);
            var rect = GUILayoutUtility.GetRect(new GUIContent(content), style, options);
            style.richText = true;
            style.stretchHeight = false;
            style.fixedHeight = 0f;
            EditorGUI.SelectableLabel(rect, content, style);

        }

        public static string GetTypeLabel(System.Type type) {

            var output = type.Name;
            var sOutput = output.Split('`');
            if (sOutput.Length > 0) {

                output = sOutput[0];

            }

            var genericTypes = type.GenericTypeArguments;
            if (genericTypes != null && genericTypes.Length > 0) {

                var sTypes = string.Empty;
                for (int i = 0; i < genericTypes.Length; ++i) {

                    sTypes += (i > 0 ? ", " : string.Empty) + genericTypes[i].Name;

                }

                output += "<" + sTypes + ">";

            }

            return output;

        }

        public static void TypeLabel(System.Type type, params GUILayoutOption[] options) {

            GUILayoutExt.DataLabel(GUILayoutExt.GetTypeLabel(type), options);

        }

        public static void Separator() {
            
            GUILayoutExt.Separator(new Color(0.1f, 0.1f, 0.1f, 0.2f));
            
        }

        public static void Separator(Color color) {

            var lineHeight = 0.5f;
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            rect.height = lineHeight;
            rect.width += 4f;
            rect.x -= 2f;
            rect.y -= lineHeight * 2f;
            EditorGUI.DrawRect(rect, color);

        }

        public static void TableCaption(string content, GUIStyle style) {

            style = new GUIStyle(style);
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = true;
            style.stretchHeight = true;

            GUILayout.Label(content, style);

        }

        private static int foldOutLevel;

        public static void FoldOut(ref bool state, string content, System.Action onContent, GUIStyle style = null, System.Action<Rect> onHeader = null) {

            if (style == null) {

                style = new GUIStyle(EditorStyles.foldoutHeader);
                style.fixedWidth = 0f;
                style.stretchWidth = true;

                if (GUILayoutExt.foldOutLevel == 0) {

                    style.fixedHeight = 24f;
                    style.richText = true;
                    content = "<b>" + content + "</b>";

                } else {

                    style.fixedHeight = 16f;
                    style.richText = true;

                }

            }

            ++GUILayoutExt.foldOutLevel;
            state = GUILayoutExt.BeginFoldoutHeaderGroup(state, new GUIContent(content), style, menuAction: onHeader);
            if (state == true) {

	            GUILayout.BeginHorizontal();
	            {
		            GUILayout.Space(10f);
		            GUILayout.BeginVertical();
		            onContent.Invoke();
		            GUILayout.EndVertical();
	            }
	            GUILayout.EndHorizontal();

            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            --GUILayoutExt.foldOutLevel;

        }

        public static bool BeginFoldoutHeaderGroup(
            bool foldout,
            GUIContent content,
            GUIStyle style = null,
            System.Action<Rect> menuAction = null,
            GUIStyle menuIcon = null) {

            return GUILayoutExt.BeginFoldoutHeaderGroup(GUILayoutUtility.GetRect(content, style), foldout, content, style, menuAction, menuIcon);

        }

        public static bool BeginFoldoutHeaderGroup(
            Rect position,
            bool foldout,
            GUIContent content,
            GUIStyle style = null,
            System.Action<Rect> menuAction = null,
            GUIStyle menuIcon = null) {
            //if (EditorGUIUtility.hierarchyMode) position.xMin -= (float)(EditorStyles.inspectorDefaultMargins.padding.left - EditorStyles.inspectorDefaultMargins.padding.right);
            if (style == null) style = EditorStyles.foldoutHeader;
            Rect position1 = new Rect() {
                x = (float)((double)position.xMax - (double)style.padding.right - 16.0),
                y = position.y + (float)style.padding.top,
                size = Vector2.one * 16f
            };
            bool isHover = position1.Contains(Event.current.mousePosition);
            bool isActive = isHover && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            if (menuAction != null && isActive) {
                menuAction(position1);
                Event.current.Use();
            }

            foldout = GUI.Toggle(position, foldout, content, style);
            if (menuAction != null && Event.current.type == EventType.Repaint) {
                if (menuIcon == null) menuIcon = EditorStyles.foldoutHeaderIcon;
                menuIcon.Draw(position1, isHover, isActive, false, false);
            }

            return foldout;
        }

        public static void Box(float padding, float margin, System.Action onContent, GUIStyle style = null, params GUILayoutOption[] options) {

            GUILayoutExt.Padding(margin, () => {

                if (style == null) {

                    style = "GroupBox";

                } else {

                    style = new GUIStyle(style);

                }

                style.padding = new RectOffset();
                style.margin = new RectOffset();

                GUILayout.BeginVertical(style, options);
                {

                    GUILayoutExt.Padding(padding, onContent);

                }
                GUILayout.EndVertical();

            }, options);

        }

        public static void Padding(float padding, System.Action onContent, params GUILayoutOption[] options) {

            GUILayoutExt.Padding(padding, padding, onContent, options);

        }

        public static void Padding(float paddingX, float paddingY, System.Action onContent, params GUILayoutOption[] options) {

            GUILayoutExt.Padding(paddingX, paddingY, onContent, GUIStyle.none, options);

        }

        public static void Padding(float paddingX, float paddingY, System.Action onContent, GUIStyle style, params GUILayoutOption[] options) {

            GUILayout.BeginVertical(style, options);
            {
                GUILayout.Space(paddingY);
                GUILayout.BeginHorizontal(options);
                {
                    GUILayout.Space(paddingX);
                    {
                        GUILayout.BeginVertical(options);
                        onContent.Invoke();
                        GUILayout.EndVertical();
                    }
                    GUILayout.Space(paddingX);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(paddingY);
            }
            GUILayout.EndVertical();

        }

    }

    public class PopupWindowAnim : EditorWindow {

		private const float defaultWidth = 150;
		private const float defaultHeight = 250;
		private const float elementHeight = 20;
		
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect;
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator = '/';
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch = true;
		
		/// <summary> Название рута </summary>
		public new string title = "Menu";

		public new string name { get { return title; } set { title = value; } }
		
		/// <summary> Стили, используемые для визуализации попапа </summary>
		private static Styles styles;
		
		//Поиск
		/// <summary> Строка поиска </summary>
		public string searchText = "";

		/// <summary> Активен ли поиск? </summary>
		private bool hasSearch { get { return useSearch && !string.IsNullOrEmpty(searchText); } }
		
		//Анимация
		private float _anim;
		private int _animTarget = 1;
		private long _lastTime;
		
		//Элементы
		/// <summary> Список конечных элементов (до вызова Show) </summary>
		private System.Collections.Generic.List<PopupItem> submenu = new System.Collections.Generic.List<PopupItem>();
		/// <summary> Хранит контекст элементов (нужно при заполнении попапа) </summary>
		private System.Collections.Generic.List<string> folderStack = new System.Collections.Generic.List<string>();
		/// <summary> Список элементов (после вызова Show) </summary>
		private Element[] _tree;
		/// <summary> Список элементов, подходящих под условия поиска </summary>
		private Element[] _treeSearch;
		/// <summary> Хранит контексты элементов (после вызова Show) </summary>
		private System.Collections.Generic.List<GroupElement> _stack = new System.Collections.Generic.List<GroupElement>();
		/// <summary> Указывает, нуждается ли выбор нового элемента в прокрутке </summary>
		private bool scrollToSelected;
		
		private Element[] activeTree { get { return (!hasSearch ? _tree : _treeSearch); } }

		private GroupElement activeParent { get { return _stack[(_stack.Count - 2) + _animTarget]; } }

		private Element activeElement {
			get {
				if (activeTree == null)
					return null;
				var childs = GetChildren(activeTree, activeParent);
				if (childs.Count == 0)
					return null;
				return childs[activeParent.selectedIndex];
			}
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim Create(Rect screenRect, bool useSearch = true) {
			var popup = CreateInstance<PopupWindowAnim>();
			popup.screenRect = screenRect;
			popup.useSearch = useSearch;
			return popup;
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim CreateByPos(Vector2 pos, bool useSearch = true) {
			return Create(new Rect(pos.x, pos.y, defaultWidth, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim CreateByPos(Vector2 pos, float width, bool useSearch = true) {
			return Create(new Rect(pos.x, pos.y, width, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim CreateBySize(Vector2 size, bool useSearch = true) {
			var screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			return Create(new Rect(screenPos.x, screenPos.y, size.x, size.y), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim Create(float width, bool useSearch = true) {
			return CreateBySize(new Vector2(width, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim Create(bool useSearch = true) {
			return CreateBySize(new Vector2(defaultWidth, defaultHeight), useSearch);
		}
		
		/// <summary> Отображает попап </summary>
		public new void Show() {
			if (submenu.Count == 0)
				DestroyImmediate(this);
			else
				Init();
		}
		
		/// <summary> Отображает попап </summary>
		public void ShowAsDropDown() {
			Show();
		}
		
		public void SetHeightByElementCount(int elementCount) {
			screenRect.height = elementCount * elementHeight + (useSearch ? 30f : 0f) + 26f;
		}
		
		public void SetHeightByElementCount() {
			SetHeightByElementCount(maxElementCount);
		}
		
		public bool autoHeight;
		public bool autoClose;
		
		public void BeginRoot(string folderName) {
			var previous = folderStack.Count != 0 ? folderStack[folderStack.Count - 1] : "";
			if (string.IsNullOrEmpty(folderName))
				folderName = "<Noname>";
			if (!string.IsNullOrEmpty(previous))
				folderStack.Add(previous + separator + folderName);
			else
				folderStack.Add(folderName);
		}
		
		public void EndRoot() {
			if (folderStack.Count > 0)
				folderStack.RemoveAt(folderStack.Count - 1);
			else
				throw new Exception("Excess call EndFolder()");
		}
		
		public void EndRootAll() {
			while (folderStack.Count > 0)
				folderStack.RemoveAt(folderStack.Count - 1);
		}
		
		public void Item(string title, Texture2D image, Action action) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image });
		}

		public void Item(string title, Texture2D image, Action<PopupItem> action, bool searchable) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, action) { image = image, searchable = searchable }
			: new PopupItem(this.title + separator + folder + separator + title, action) { image = image, searchable = searchable });
		}
		
		public void Item(string title, Action action) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, action)
			            : new PopupItem(this.title + separator + folder + separator + title, action));
		}
		
		public void Item(string title) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, () => { })
			            : new PopupItem(this.title + separator + folder + separator + title, () => { }));
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, action) { image = image });
		}
		
		public void ItemByPath(string path, Action action) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, action));
		}
		
		public void ItemByPath(string path) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, () => { }));
		}
		
		private void Init() {
			CreateComponentTree();
			if (autoHeight)
				SetHeightByElementCount();
			ShowAsDropDown(new Rect(screenRect.x, screenRect.y, 1, 1), new Vector2(screenRect.width, screenRect.height));
			Focus();
			wantsMouseMove = true;
		}
		
		private void CreateComponentTree() {

			var list = new System.Collections.Generic.List<string>();
			var elements = new System.Collections.Generic.List<Element>();

			this.submenu = this.submenu.OrderBy(x => x.path).ToList();
			
			for (int i = 0; i < submenu.Count; i++) {

				var submenuItem = submenu[i];
				string menuPath = submenuItem.path;
				var separators = new[] { separator };
				var pathParts = menuPath.Split(separators);

				while (pathParts.Length - 1 < list.Count) {

					list.RemoveAt(list.Count - 1);

				}

				while (list.Count > 0 && pathParts[list.Count - 1] != list[list.Count - 1]) {

					list.RemoveAt(list.Count - 1);

				}

				while (pathParts.Length - 1 > list.Count) {

					elements.Add(new GroupElement(list.Count, pathParts[list.Count]));
					list.Add(pathParts[list.Count]);

				}

				elements.Add(new CallElement(list.Count, pathParts[pathParts.Length - 1], submenuItem));

			}

			_tree = elements.ToArray();
			for (int i = 0; i < _tree.Length; i++) {
				var elChilds = GetChildren(_tree, _tree[i]);
				if (elChilds.Count > maxElementCount)
					maxElementCount = elChilds.Count;
			}
			if (_stack.Count == 0) {
				_stack.Add(_tree[0] as GroupElement);
				goto to_research;
			}
			var parent = _tree[0] as GroupElement;
			var level = 0;
			to_startCycle:
			var stackElement = _stack[level];
			_stack[level] = parent;
			if (_stack[level] != null) {
				_stack[level].selectedIndex = stackElement.selectedIndex;
				_stack[level].scroll = stackElement.scroll;
			}
			level++;
			if (level != _stack.Count) {
				var childs = GetChildren(activeTree, parent);
				var child = childs.FirstOrDefault(x => _stack[level].name == x.name);
				if (child is GroupElement)
					parent = child as GroupElement;
				else
					while (_stack.Count > level)
						_stack.RemoveAt(level);
				goto to_startCycle;
			}
			to_research:
			s_DirtyList = false;
			RebuildSearch();
		}
		
		private int maxElementCount = 1;
		private static bool s_DirtyList = true;

		private void RebuildSearch() {
			if (!hasSearch) {
				_treeSearch = null;
				if (_stack[_stack.Count - 1].name == "Search") {
					_stack.Clear();
					_stack.Add(_tree[0] as GroupElement);
				}
				_animTarget = 1;
				_lastTime = DateTime.Now.Ticks;
			}
			else {
				var separatorSearch = new[] { ' ', separator };
				var searchLowerWords = searchText.ToLower().Split(separatorSearch);
				var firstElements = new System.Collections.Generic.List<Element>();
				var otherElements = new System.Collections.Generic.List<Element>();
				foreach (var element in _tree) {
					if (!(element is CallElement))
						continue;
					if (element.searchable == false) continue;
					var elementNameShortLower = element.name.ToLower().Replace(" ", string.Empty);
					var itsSearchableItem = true;
					var firstContainsFlag = false;
					for (int i = 0; i < searchLowerWords.Length; i++) {
						var searchLowerWord = searchLowerWords[i];
						if (elementNameShortLower.Contains(searchLowerWord)) {
							if (i == 0 && elementNameShortLower.StartsWith(searchLowerWord))
								firstContainsFlag = true;
						}
						else {
							itsSearchableItem = false;
							break;
						}
					}
					if (itsSearchableItem) {
						if (firstContainsFlag)
							firstElements.Add(element);
						else
							otherElements.Add(element);
					}
				}
				firstElements.Sort();
				otherElements.Sort();
				
				var searchElements = new System.Collections.Generic.List<Element>
				{ new GroupElement(0, "Search") };
				searchElements.AddRange(firstElements);
				searchElements.AddRange(otherElements);
				//            searchElements.Add(_tree[_tree.Length - 1]);
				_treeSearch = searchElements.ToArray();
				_stack.Clear();
				_stack.Add(_treeSearch[0] as GroupElement);
				if (GetChildren(activeTree, activeParent).Count >= 1)
					activeParent.selectedIndex = 0;
				else
					activeParent.selectedIndex = -1;
			}
		}
		
		public void OnGUI() {
			if (_tree == null) {
				Close();
				return; 
			}
			//Создание стиля
			if (styles == null)
				styles = new Styles();
			//Фон
			if (s_DirtyList)
				CreateComponentTree();
			HandleKeyboard();
			GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, styles.background);
			
			//Поиск
			if (useSearch) {
				GUILayout.Space(7f);
				var rectSearch = GUILayoutUtility.GetRect(10f, 20f);
				rectSearch.x += 8f;
				rectSearch.width -= 16f;
				EditorGUI.FocusTextInControl("ComponentSearch");
				GUI.SetNextControlName("ComponentSearch");
				if (SearchField(rectSearch, ref searchText))
					RebuildSearch();
			}
			
			//Элементы
			ListGUI(activeTree, _anim, GetElementRelative(0), GetElementRelative(-1));
			if (_anim < 1f && _stack.Count > 1)
				ListGUI(activeTree, _anim + 1f, GetElementRelative(-1), GetElementRelative(-2));
			if (_anim != _animTarget && Event.current.type == EventType.Repaint) {
				var ticks = DateTime.Now.Ticks;
				var coef = (ticks - _lastTime) / 1E+07f;
				_lastTime = ticks;
				_anim = Mathf.MoveTowards(_anim, _animTarget, coef * 4f);
				if (_animTarget == 0 && _anim == 0f) {
					_anim = 1f;
					_animTarget = 1;
					_stack.RemoveAt(_stack.Count - 1);
				}
				Repaint();
			}
		}
		
		private void HandleKeyboard() {
			Event current = Event.current;
			if (current.type == EventType.KeyDown) {
				if (current.keyCode == KeyCode.DownArrow) {
					activeParent.selectedIndex++;
					activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex,
					                                       GetChildren(activeTree, activeParent).Count - 1);
					scrollToSelected = true;
					current.Use();
				}
				if (current.keyCode == KeyCode.UpArrow) {
					GroupElement element2 = activeParent;
					element2.selectedIndex--;
					activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
					scrollToSelected = true;
					current.Use();
				}
				if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) {
					GoToChild(activeElement, true);
					current.Use();
				}
				if (!hasSearch) {
					if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace) {
						GoToParent();
						current.Use();
					}
					if (current.keyCode == KeyCode.RightArrow) {
						GoToChild(activeElement, false);
						current.Use();
					}
					if (current.keyCode == KeyCode.Escape) {
						Close();
						current.Use();
					}
				}
			}
		}
		
		private static bool SearchField(Rect position, ref string text) {
			var rectField = position;
			rectField.width -= 15f;
			var startText = text;
			text = GUI.TextField(rectField, startText ?? "", styles.searchTextField);
			
			var rectCancel = position;
			rectCancel.x += position.width - 15f;
			rectCancel.width = 15f;
			var styleCancel = text == "" ? styles.searchCancelButtonEmpty : styles.searchCancelButton;
			if (GUI.Button(rectCancel, GUIContent.none, styleCancel) && text != "") {
				text = "";
				GUIUtility.keyboardControl = 0;
			}
			return startText != text;
		}
		
		private void ListGUI(Element[] tree, float anim, GroupElement parent, GroupElement grandParent) {
			anim = Mathf.Floor(anim) + Mathf.SmoothStep(0f, 1f, Mathf.Repeat(anim, 1f));
			Rect rectArea = position;
			rectArea.x = position.width * (1f - anim) + 1f;
			rectArea.y = useSearch ? 30f : 0;
			rectArea.height -= useSearch ? 30f : 0;
			rectArea.width -= 2f;
			GUILayout.BeginArea(rectArea);
			{
				var rectHeader = GUILayoutUtility.GetRect(10f, 25f);
				var nameHeader = parent.name;
				GUI.Label(rectHeader, nameHeader, styles.header);
				if (grandParent != null) {
					var rectHeaderBackArrow = new Rect(rectHeader.x + 4f, rectHeader.y + 7f, 13f, 13f);
					if (Event.current.type == EventType.Repaint)
						styles.leftArrow.Draw(rectHeaderBackArrow, false, false, false, false);
					if (Event.current.type == EventType.MouseDown && rectHeader.Contains(Event.current.mousePosition)) {
						GoToParent();
						Event.current.Use();
					}
				}
				ListGUI(tree, parent);
			}
			GUILayout.EndArea();
		}
		
		private void ListGUI(Element[] tree, GroupElement parent) {
			parent.scroll = GUILayout.BeginScrollView(parent.scroll, new GUILayoutOption[0]);
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			var children = GetChildren(tree, parent);
			var rect = new Rect();
			for (int i = 0; i < children.Count; i++) {
				var e = children[i];
				var options = new[] { GUILayout.ExpandWidth(true) };
				var rectElement = GUILayoutUtility.GetRect(16f, elementHeight, options);
				if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) 
					&& parent.selectedIndex != i && rectElement.Contains(Event.current.mousePosition)) {
					parent.selectedIndex = i;
					Repaint();
				}
				bool on = false;
				if (i == parent.selectedIndex) {
					on = true;
					rect = rectElement;
				}
				if (Event.current.type == EventType.Repaint) {
					(e.content.image != null ? styles.componentItem : styles.groupItem).Draw(rectElement, e.content, false, false, on, on);
					if (!(e is CallElement)) {
						var rectElementForwardArrow = new Rect(rectElement.x + rectElement.width - 13f, rectElement.y + 4f, 13f, 13f);
						styles.rightArrow.Draw(rectElementForwardArrow, false, false, false, false);
					}
				}
				if (Event.current.type == EventType.MouseDown && rectElement.Contains(Event.current.mousePosition)) {
					Event.current.Use();
					parent.selectedIndex = i;
					GoToChild(e, true);
				}
			}
			EditorGUIUtility.SetIconSize(Vector2.zero);
			GUILayout.EndScrollView();
			if (scrollToSelected && Event.current.type == EventType.Repaint) {
				scrollToSelected = false;
				var lastRect = GUILayoutUtility.GetLastRect();
				if ((rect.yMax - lastRect.height) > parent.scroll.y) {
					parent.scroll.y = rect.yMax - lastRect.height;
					Repaint();
				}
				if (rect.y < parent.scroll.y) {
					parent.scroll.y = rect.y;
					Repaint();
				}
			}
		}
		
		private void GoToParent() {
			if (_stack.Count <= 1) 
				return;
			_animTarget = 0;
			_lastTime = DateTime.Now.Ticks;
		}
		
		private void GoToChild(Element e, bool addIfComponent) {
			var element = e as CallElement;
			if (element != null) {
				if (!addIfComponent) 
					return;
				element.action();
				if (this.autoClose == true) Close();
			}
			else if (!hasSearch) {
					_lastTime = DateTime.Now.Ticks;
					if (_animTarget == 0)
						_animTarget = 1;
					else if (_anim == 1f) {
							_anim = 0f;
							_stack.Add(e as GroupElement);
						}
				}
		}
		
		private System.Collections.Generic.List<Element> GetChildren(Element[] tree, Element parent) {
			var list = new System.Collections.Generic.List<Element>();
			var num = -1;
			var index = 0;
			while (index < tree.Length) {
				if (tree[index] == parent) {
					num = parent.level + 1;
					index++;
					break;
				}
				index++;
			}
			if (num == -1) 
				return list;
			while (index < tree.Length) {
				var item = tree[index];
				if (item.level < num)
					return list;
				if (item.level <= num || hasSearch)
					list.Add(item);
				index++;
			}
			return list;
		}
		
		private GroupElement GetElementRelative(int rel) {
			int num = (_stack.Count + rel) - 1;
			return num < 0 ? null : _stack[num];
		}
		
		
		private class CallElement : Element {
			public Action action;
			
			public CallElement(int level, string name, PopupItem item) {
				base.level = level;
				content = new GUIContent(name, item.image);
				action = () => {
					item.action();
					content = new GUIContent(name, item.image);
				};
				this.searchable = item.searchable;
			}
		}
		
		[Serializable]
		private class GroupElement : Element {
			public Vector2 scroll;
			public int selectedIndex;
			
			public GroupElement(int level, string name) {
				this.level = level;
				content = new GUIContent(name);
				this.searchable = true;
			}
		}
		
		private class Element : IComparable {
			public GUIContent content;
			public int level;
			public bool searchable;
			
			public string name { get { return content.text; } }
			
			public int CompareTo(object o) {
				return String.Compare(name, ((Element)o).name, StringComparison.Ordinal);
			}
		}
		
		private class Styles {
			public GUIStyle searchTextField = "SearchTextField";
			public GUIStyle searchCancelButton = "SearchCancelButton";
			public GUIStyle searchCancelButtonEmpty = "SearchCancelButtonEmpty";
			public GUIStyle background = "grey_border";
			public GUIStyle componentItem = new GUIStyle("PR Label");
			public GUIStyle groupItem;
			public GUIStyle header = new GUIStyle("In BigTitle");
			public GUIStyle leftArrow = "AC LeftArrow";
			public GUIStyle rightArrow = "AC RightArrow";
			
			public Styles() {
				header.font = EditorStyles.boldLabel.font;
				header.richText = true;
				componentItem.alignment = TextAnchor.MiddleLeft;
				componentItem.padding.left -= 15;
				componentItem.fixedHeight = 20f;
				componentItem.richText = true;
				groupItem = new GUIStyle(componentItem);
				groupItem.padding.left += 0x11;
				groupItem.richText = true;
			}
		}
		
		public class PopupItem {
			public PopupItem(string path, Action action) {
				this.path = path;
				this.action = action;
			}
			
			public PopupItem(string path, Action<PopupItem> action) {
				this.path = path;
				this.action = () => { action(this); };
			}

			public string path;
			public Texture2D image;
			public Action action;
			public bool searchable;

		}
	}
    
    public class Popup {
		/// <summary> Окно, которое связано с попапом </summary>
		internal PopupWindowAnim window;
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect { get { return window.screenRect; } set { window.screenRect = value; } }
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator { get { return window.separator; } set { window.separator = value; } }
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch { get { return window.useSearch; } set { window.useSearch = value; } }

		/// <summary> Название рута </summary>
		public string title { get { return window.title; } set { window.title = value; } }

		/// <summary> Название рута </summary>
		public string searchText { get { return window.searchText; } set { window.searchText = value; } }

		/// <summary> Автоматически установить размер по высоте, узнав максимальное количество видимых элементов </summary>
		public bool autoHeight { get { return window.autoHeight; } set { window.autoHeight = value; } }
		public bool autoClose { get { return window.autoClose; } set { window.autoClose = value; } }
		
		/// <summary> Создание окна </summary>
		public Popup(Rect screenRect, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.Create(screenRect, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(Vector2 size, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.CreateBySize(size, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(float width, bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(width, useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		public void BeginFolder(string folderName) {
			window.BeginRoot(folderName);
		}
		
		public void EndFolder() {
			window.EndRoot();
		}
		
		public void EndFolderAll() {
			window.EndRootAll();
		}
		
		public void Item(string name) {
			window.Item(name);
		}
		
		public void Item(string name, Action action) {
			window.Item(name, action);
		}
		
		public void Item(string name, Texture2D image, Action action) {
			window.Item(name, image, action);
		}

		public void Item(string name, Texture2D image, Action<PopupWindowAnim.PopupItem> action, bool searchable = true) {
			window.Item(name, image, action, searchable);
		}

		public void ItemByPath(string path) {
			window.ItemByPath(path);
		}
		
		public void ItemByPath(string path, Action action) {
			window.ItemByPath(path, action);
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			window.ItemByPath(path, image, action);
		}
		
		public void Show() {
			window.Show();
		}
		
		public static void DrawInt(GUIContent label, string selected, System.Action<int> onResult, GUIContent[] options, int[] keys) {
			
			DrawInt_INTERNAL(new Rect(), selected, label, onResult, options, keys, true);
			
		}

		public static void DrawInt(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys) {

			DrawInt_INTERNAL(rect, selected, label, onResult, options, keys, false);

		}

		private static void DrawInt_INTERNAL(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys, bool layout) {

			var state = false;
			if (layout == true) {

				GUILayout.BeginHorizontal();
				if (label != null) GUILayout.Label(label);
				if (GUILayout.Button(selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				GUILayout.EndHorizontal();

			} else {
				
				if (label != null) rect = EditorGUI.PrefixLabel(rect, label);
				if (GUI.Button(rect, selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				
			}
			
			if (state == true) {

				Popup popup = null;
				if (layout == true) {

					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				} else {
					
					Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
					rect.x = vector.x;
					rect.y = vector.y;
					
					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				}
				
				for (int i = 0; i < options.Length; ++i) {
					
					var option = options[i];
					var result = keys[i];
					popup.ItemByPath(option.text, () => {
						
						onResult(result);
						
					});
					
				}
				
				popup.Show();

			}

		}

	}
    
}