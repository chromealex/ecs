namespace ME.ECSEditor {

    using UnityEngine;
    using UnityEditor;
    using ME.ECS;
    
    [UnityEditor.CustomEditor(typeof(InitializerBase), true)]
    public class InitializerEditor : Editor {

        private const float ONE_LINE_HEIGHT = 22f;
        
        private UnityEditorInternal.ReorderableList list;
        
        public override void OnInspectorGUI() {

            var target = this.target as InitializerBase;
            if (target.featuresList == null) target.featuresList = new FeaturesList();
            if (target.featuresList.features == null) target.featuresList.features = new System.Collections.Generic.List<FeaturesList.FeatureData>();

            if (this.list == null) {
                
                this.list = new UnityEditorInternal.ReorderableList(target.featuresList.features, typeof(FeaturesList.FeatureData), true, true, true, true);
                this.list.drawElementCallback = this.OnDrawListItem;
                this.list.drawHeaderCallback = this.OnDrawHeader;
                this.list.onChangedCallback = this.OnChanged;
                this.list.elementHeightCallback = this.GetElementHeight;

            }

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            this.list.DoLayoutList();
            EditorGUI.EndDisabledGroup();

        }

        private System.Collections.Generic.List<string> GetSystems(FeatureBase feature) {

            var list = new System.Collections.Generic.List<string>();
            var script = MonoScript.FromScriptableObject(feature);
            var text = script.text;
                
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"AddSystem\s*\<(.*?)\>");
            foreach (System.Text.RegularExpressions.Match match in matches) {

                if (match.Groups.Count > 0) {

                    var systemType = match.Groups[1].Value;
                    list.Add(systemType);
                    
                }
                    
            }

            return list;

        }
        
        private float GetElementHeight(int index) {
            
            var featureData = (FeaturesList.FeatureData)this.list.list[index];
            var height = InitializerEditor.ONE_LINE_HEIGHT;

            if (featureData.feature != null) { // Draw systems

                var systems = this.GetSystems(featureData.feature);
                height += InitializerEditor.ONE_LINE_HEIGHT * systems.Count;

            }

            return height;

        }

        private void OnDrawHeader(Rect rect) {
            
            GUI.Label(rect, "Features");
            
        }

        private void OnChanged(UnityEditorInternal.ReorderableList reorderableList) {
            
            EditorUtility.SetDirty(this.target);
            
        }

        private void OnDrawListItem(Rect rect, int index, bool isActive, bool isFocused) {

            var featureData = (FeaturesList.FeatureData)this.list.list[index];

            rect.height = InitializerEditor.ONE_LINE_HEIGHT;
            
            var rectCheckBox = new Rect(rect);
            rectCheckBox.width = 20f;

            var isDirty = false;
            {

                rect.y += 1f;
                rect.height -= 2f;
                
                var rectObjectField = new Rect(rect);
                rectObjectField.x += rectCheckBox.width;
                rectObjectField.width -= rectCheckBox.width;
            
                var oldColor = GUI.color;
                if (featureData.enabled == false) {

                    GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.5f);

                }

                var obj = (FeatureBase)EditorGUI.ObjectField(rectObjectField, featureData.feature, typeof(FeatureBase), allowSceneObjects: false);
                if (obj != featureData.feature) {

                    featureData.feature = obj;

                    var count = 0;
                    for (int i = 0; i < this.list.count; ++i) {
                        
                        var data = (FeaturesList.FeatureData)this.list.list[i];
                        if (data.feature != null && featureData.feature != null && featureData.feature == data.feature) {

                            ++count;

                        }
                        
                    }

                    if (count > 1) {

                        featureData.feature = null;

                    }
                    
                    isDirty = true;

                }

                GUI.color = oldColor;

                if (featureData.feature == null) {
                    
                    featureData.enabled = false;
                    
                }
                
                EditorGUI.BeginDisabledGroup(featureData.feature == null);
                var flag = GUI.Toggle(rectCheckBox, featureData.enabled, string.Empty);
                if (flag != featureData.enabled) {

                    featureData.enabled = flag;
                    isDirty = true;

                }
                EditorGUI.EndDisabledGroup();

            }

            if (featureData.feature != null) { // Draw systems

                rect.x += rectCheckBox.width;

                var systems = this.GetSystems(featureData.feature);
                foreach (var system in systems) {

                    rect.y += InitializerEditor.ONE_LINE_HEIGHT;
                    GUI.Label(rect, system, EditorStyles.label);
                    
                }

            }
            
            if (isDirty == true) {
                
                EditorUtility.SetDirty(this.target);
                
            }
            
        }

    }

}
