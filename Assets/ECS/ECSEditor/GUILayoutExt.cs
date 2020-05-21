using UnityEngine;
using UnityEditor;

namespace ME.ECSEditor {

    public static class GUILayoutExt {

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

        public static bool DrawFields(object instance, string customName = null) {

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
                        var isEditable = GUILayoutExt.PropertyField(field.Name, field, ref value, typeCheckOnly: true);
                        EditorGUI.BeginDisabledGroup(disabled: (isEditable == false));
                        if (GUILayoutExt.PropertyField(customName != null ? customName : field.Name, field, ref value, typeCheckOnly: false) == true) {

                            if (oldValue.ToString() != value.ToString()) {

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

        public static bool PropertyField(System.Reflection.FieldInfo fieldInfo, ref object value, bool typeCheckOnly) {

            return GUILayoutExt.PropertyField(null, fieldInfo, ref value, typeCheckOnly);

        }

        public static bool PropertyField(string caption, System.Reflection.FieldInfo fieldInfo, ref object value, bool typeCheckOnly) {

            if (value == null) {

                if (typeCheckOnly == false) EditorGUILayout.LabelField("Null");
                return false;

            }

            if (fieldInfo.FieldType == typeof(Color)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.ColorField(caption, (Color)value);
                    GUILayout.Label(value.ToString());

                }
                
            } else if (fieldInfo.FieldType == typeof(Color32)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.ColorField(caption, (Color32)value);
                    GUILayout.Label(value.ToString());

                }
                
            } else if (fieldInfo.FieldType == typeof(Vector2)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.Vector2Field(caption, (Vector2)value);

                }

            } else if (fieldInfo.FieldType == typeof(Vector3)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.Vector3Field(caption, (Vector3)value);

                }

            } else if (fieldInfo.FieldType == typeof(Vector4)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.Vector4Field(caption, (Vector4)value);

                }

            } else if (fieldInfo.FieldType == typeof(Quaternion)) {

                if (typeCheckOnly == false) {

                    value = Quaternion.Euler(EditorGUILayout.Vector3Field(caption, ((Quaternion)value).eulerAngles));

                }

            } else if (fieldInfo.FieldType == typeof(int)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.IntField(caption, (int)value);

                }

            } else if (fieldInfo.FieldType == typeof(float)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.FloatField(caption, (float)value);

                }

            } else if (fieldInfo.FieldType == typeof(double)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.DoubleField(caption, (double)value);

                }

            } else if (fieldInfo.FieldType == typeof(long)) {

                if (typeCheckOnly == false) {

                    value = EditorGUILayout.LongField(caption, (long)value);

                }

            } else {

                if (typeCheckOnly == false) {

                    var str = value.ToString();
                    if (str.Contains("\n") == true) {

                        value = EditorGUILayout.TextArea(str);

                    } else {

                        value = EditorGUILayout.TextField(caption, str);

                    }

                }

                return false;

            }

            return true;

        }

        public static void DataLabel(string content, params GUILayoutOption[] options) {

            var style = new GUIStyle(EditorStyles.label);
            var rect = GUILayoutUtility.GetRect(new GUIContent(content), style, options);
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
            
            GUILayoutExt.Separator(new Color(0.1f, 0.1f, 0.1f, 1f));
            
        }

        public static void Separator(Color color) {

            var lineHeight = 1f;
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            rect.height = lineHeight;
            rect.width += 4f;
            rect.x -= 2f;
            EditorGUI.DrawRect(rect, color);
            GUILayout.Space(-lineHeight);

        }

        public static void TableCaption(string content, GUIStyle style) {

            style = new GUIStyle(style);
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = true;
            style.stretchHeight = true;

            GUILayout.Label(content, style);

        }

        private static int foldOutLevel;

        public static void FoldOut(ref bool state, string content, System.Action onContent, GUIStyle style = null) {

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
            state = GUILayoutExt.BeginFoldoutHeaderGroup(state, new GUIContent(content), style);
            if (state == true) {

                onContent.Invoke();

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
            if (EditorGUIUtility.hierarchyMode) position.xMin -= (float)(EditorStyles.inspectorDefaultMargins.padding.left - EditorStyles.inspectorDefaultMargins.padding.right);
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

}