using UnityEngine;
using UnityEditor;

namespace ME.ECSEditor {

    public static class GUILayoutExt {

        public static void DrawFields(object instance, float fieldWidth) {

            var padding = 2f;
            var margin = 1f;
            var cellHeight = 24f;
            var tableStyle = new GUIStyle("Box");

            GUILayout.BeginHorizontal();
            {
                GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Field", EditorStyles.miniBoldLabel); },
                                 tableStyle, GUILayout.Width(fieldWidth),
                                 GUILayout.Height(cellHeight));
                GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.TableCaption("Value", EditorStyles.miniBoldLabel); },
                                 tableStyle, GUILayout.ExpandWidth(true),
                                 GUILayout.Height(cellHeight));
            }
            GUILayout.EndHorizontal();

            var fields = instance.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var field in fields) {

                GUILayout.BeginHorizontal();
                {
                    GUILayoutExt.Box(padding, margin, () => { GUILayoutExt.DataLabel(field.Name); }, tableStyle,
                                     GUILayout.Width(fieldWidth), GUILayout.Height(cellHeight));
                    GUILayoutExt.Box(padding, margin, () => {

                        var value = field.GetValue(instance);
                        if (GUILayoutExt.PropertyField(field, ref value) == true) {

                            field.SetValue(instance, value);

                        }

                    }, tableStyle, GUILayout.ExpandWidth(true), GUILayout.Height(cellHeight));
                }
                GUILayout.EndHorizontal();

            }

        }

        public static bool PropertyField(System.Reflection.FieldInfo fieldInfo, ref object value) {

            if (fieldInfo.FieldType == typeof(Color)) {

                value = EditorGUILayout.ColorField((Color)value);
                GUILayout.Label(value.ToString());

            } else if (fieldInfo.FieldType == typeof(Color32)) {

                value = EditorGUILayout.ColorField((Color32)value);
                GUILayout.Label(value.ToString());

            } else if (fieldInfo.FieldType == typeof(Vector2)) {

                value = EditorGUILayout.Vector3Field(string.Empty, (Vector2)value);

            } else if (fieldInfo.FieldType == typeof(Vector3)) {

                value = EditorGUILayout.Vector3Field(string.Empty, (Vector3)value);

            } else if (fieldInfo.FieldType == typeof(Vector4)) {

                value = EditorGUILayout.Vector4Field(string.Empty, (Vector4)value);

            } else if (fieldInfo.FieldType == typeof(Quaternion)) {

                value = Quaternion.Euler((Vector3)EditorGUILayout.Vector3Field(string.Empty, ((Quaternion)value).eulerAngles));

            } else if (fieldInfo.FieldType == typeof(int)) {

                value = EditorGUILayout.IntField((int)value);

            } else if (fieldInfo.FieldType == typeof(float)) {

                value = EditorGUILayout.FloatField((float)value);

            } else if (fieldInfo.FieldType == typeof(double)) {

                value = EditorGUILayout.DoubleField((double)value);

            } else if (fieldInfo.FieldType == typeof(long)) {

                value = EditorGUILayout.LongField((long)value);

            } else {

                var str = value.ToString();
                if (str.Contains("\n") == true) {

                    value = EditorGUILayout.TextArea(str);

                } else {

                    value = EditorGUILayout.TextField(str);

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

            var lineHeight = 1f;
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            rect.height = lineHeight;
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));

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

            GUILayout.BeginVertical(options);
            {
                GUILayout.Space(padding);
                GUILayout.BeginHorizontal(options);
                {
                    GUILayout.Space(padding);
                    {
                        GUILayout.BeginVertical(options);
                        onContent.Invoke();
                        GUILayout.EndVertical();
                    }
                    GUILayout.Space(padding);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(padding);
            }
            GUILayout.EndVertical();

        }

    }

}