using ME.ECS;

namespace Warcraft.Views.UI {
    
    using ME.ECS.Views.Providers;
    using Warcraft.Markers;
    using Warcraft.Entities;
    using Warcraft.Components;
    using TEntity = Warcraft.Entities.PlayerEntity;
    using UnityEngine;
    
    public class PlayerSelectedUnitUI : MonoBehaviourView<TEntity> {

        public UnitInfo noUnitInfo;
        
        public UnityEngine.UI.Text unitCaption;
        public UnityEngine.UI.Image unitIcon;

        public UnityEngine.UI.Slider healthProgress;
        public UnityEngine.UI.Text healthValue;

        public UnityEngine.UI.Slider buildingProgress;
        public UnityEngine.UI.Text buildingValue;

        public Transform actionsContainer;
        public ButtonActionUI buttonSource;
        private System.Collections.Generic.List<ButtonActionUI> buttons = new System.Collections.Generic.List<ButtonActionUI>();

        public Transform queueContainer;
        public ButtonQueueActionUI queueButtonSource;
        private System.Collections.Generic.List<ButtonQueueActionUI> queueButtons = new System.Collections.Generic.List<ButtonQueueActionUI>();

        private ActionsGraphNode[] graphNodeSelected;
        private TEntity playerData;
        private Entity selectedUnit;
        private bool actionsIsDirty;
        
        public override void OnInitialize(in TEntity data) {

            this.playerData = data;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            this.selectedUnit = Entity.Empty;
            
        }

        private void UpdateActions() {

            if (this.actionsIsDirty == false) return;
            this.actionsIsDirty = false;
            
            var used = PoolList<ButtonActionUI>.Spawn(10);
            var i = 0;
            if (this.graphNodeSelected != null) {

                foreach (var item in this.graphNodeSelected) {

                    var icon = item.GetIcon();
                    if (icon != null) {

                        var isExists = (i < this.buttons.Count);
                        var actionInfo = item;
                        var button = (isExists == true ? this.buttons[i] : Object.Instantiate(this.buttonSource, this.actionsContainer));
                        button.gameObject.SetActive(true);
                        button.SetInfo(actionInfo.cost, icon);
                        button.button.onClick.RemoveAllListeners();
                        button.button.onClick.AddListener(() => { this.DoAction(actionInfo); });
                        button.button.interactable = actionInfo.IsEnabled(this.playerData);
                        if (isExists == false) this.buttons.Add(button);

                        used.Add(button);

                        ++i;

                    }

                }

            }

            for (i = 0; i < this.buttons.Count; ++i) {
                    
                var button = this.buttons[i];
                if (used.Contains(button) == false) {

                    Object.Destroy(button.gameObject);
                    this.buttons.RemoveAt(i);
                    --i;

                }
            }
            
            PoolList<ButtonActionUI>.Recycle(ref used);

        }

        private void UpdateQueue(Entity entity) {
            
            var world = Worlds<WarcraftState>.currentWorld;

            var queue = world.GetComponent<UnitEntity, UnitBuildingQueueComponent>(entity);
            if (queue != null) {

                this.queueContainer.gameObject.SetActive(queue.units.Count > 0);

                var used = new System.Collections.Generic.List<ButtonQueueActionUI>();
                var i = 0;
                foreach (var unitEntity in queue.units) {

                    var unit = unitEntity;
                    var unitInfo = world.GetComponent<UnitEntity, UnitInfoComponent>(unit);
                    var icon = unitInfo.unitInfo.icon;

                    var progress = world.GetComponent<UnitEntity, UnitBuildingProgress>(unit);

                    var isExists = (i < this.queueButtons.Count);
                    var button = (isExists == true ? this.queueButtons[i] : Object.Instantiate(this.queueButtonSource, this.queueContainer));
                    button.gameObject.SetActive(true);
                    button.SetInfo(icon, progress.progress / progress.time, i == 0);
                    button.button.onClick.RemoveAllListeners();
                    button.button.onClick.AddListener(() => { this.DoCancelQueue(entity, unit); });
                    if (isExists == false) this.queueButtons.Add(button);
                    
                    used.Add(button);

                    ++i;

                }

                for (i = 0; i < this.queueButtons.Count; ++i) {
                    
                    var button = this.queueButtons[i];
                    if (used.Contains(button) == false) {

                        Object.Destroy(button.gameObject);
                        this.queueButtons.RemoveAt(i);
                        --i;

                    }
                }

            }

        }

        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            var world = Worlds<WarcraftState>.currentWorld;

            var comp = world.GetComponent<TEntity, Warcraft.Components.PlayerSelectedUnitComponent>(data.entity);
            if (comp != null) {

                if (this.selectedUnit != comp.unit) {
                    
                    if (this.graphNodeSelected != null) this.actionsIsDirty = true;
                    this.graphNodeSelected = null;
                    
                }
                this.selectedUnit = comp.unit;
                
                if (world.GetEntityData(comp.unit, out UnitEntity unitEntity) == true) {

                    var unitInfoComponent = world.GetComponent<UnitEntity, UnitInfoComponent>(unitEntity.entity);
                    this.SetInfo(unitInfoComponent.unitInfo);
                
                } else {

                    if (this.graphNodeSelected != null) this.actionsIsDirty = true;
                    this.graphNodeSelected = null;
                    this.selectedUnit = Entity.Empty;

                }

            } else {

                if (this.selectedUnit != Entity.Empty) {

                    if (this.graphNodeSelected != null) this.actionsIsDirty = true;
                    this.graphNodeSelected = null;
                    
                }
                this.selectedUnit = Entity.Empty;
                this.SetInfo(this.noUnitInfo);
                
            }

        }

        private void SetInfo(UnitInfo unitInfo) {
            
            var world = Worlds<WarcraftState>.currentWorld;

            this.unitCaption.text = unitInfo.title;
            this.unitIcon.sprite = unitInfo.icon;

            var inProgress = world.GetComponent<UnitEntity, UnitBuildingProgress>(this.selectedUnit);
            if (inProgress != null) {

                this.healthProgress.gameObject.SetActive(false);
                this.actionsContainer.gameObject.SetActive(false);
                this.queueContainer.gameObject.SetActive(false);
                this.buildingProgress.gameObject.SetActive(true);
                var progress = inProgress.progress / inProgress.time;
                this.buildingProgress.value = progress;
                this.buildingValue.text = Mathf.FloorToInt(progress * 100f).ToString() + "%";
                return;

            }
            
            var health = world.GetComponent<UnitEntity, UnitHealthComponent>(this.selectedUnit);
            if (health != null) {

                var healthProgress = health.value / (float)health.maxValue;
                this.healthProgress.value = healthProgress;
                this.healthValue.text = Mathf.FloorToInt(healthProgress * 100f).ToString() + "%";
                this.healthProgress.gameObject.SetActive(true);

            } else {
                
                this.healthProgress.gameObject.SetActive(false);
                
            }

            this.actionsContainer.gameObject.SetActive(true);
            this.queueContainer.gameObject.SetActive(false);
            this.buildingProgress.gameObject.SetActive(false);

            if (unitInfo.actions != null) {

                var actions = unitInfo.actions.roots;
                if (this.graphNodeSelected == null) {

                    if (this.graphNodeSelected != actions) this.actionsIsDirty = true;
                    this.graphNodeSelected = actions;

                }

            }
            
            this.UpdateQueue(this.selectedUnit);
            this.UpdateActions();

        }

        private void DoCancelQueue(Entity building, Entity unit) {
            
            Worlds<WarcraftState>.currentWorld.AddMarker(new InputUnitQueueCancel() {
                selectedUnit = building,
                unitInQueue = unit
            });
            
        }

        private void DoAction(ActionsGraphNode actionInfo) {

            if (actionInfo.GetNext().Length > 0) {

                if (this.graphNodeSelected != actionInfo.GetNext()) this.actionsIsDirty = true;
                this.graphNodeSelected = actionInfo.GetNext();

            } else {

                if (actionInfo is ActionBuildingNode actionBuildingInfo) {

                    if (actionBuildingInfo.isUpgrade == true) {
                        
                        Worlds<WarcraftState>.currentWorld.AddMarker(new InputUnitUpgrade() {
                            selectedUnit = this.selectedUnit,
                            unitInfo = actionBuildingInfo.building,
                            actionInfo = actionBuildingInfo,
                        });
                        
                    } else {

                        if (actionBuildingInfo.building is CharacterUnitInfo) {
                            
                            // Place into the building queue
                            Worlds<WarcraftState>.currentWorld.AddMarker(new InputUnitQueue() {
                                selectedUnit = this.selectedUnit,
                                unitInfo = actionBuildingInfo.building,
                                actionInfo = actionBuildingInfo,
                            });
                            
                        } else {

                            Worlds<WarcraftState>.currentWorld.AddMarker(new InputUnitPlacement() {
                                selectedUnit = this.selectedUnit,
                                unitInfo = actionBuildingInfo.building,
                                actionInfo = actionBuildingInfo,
                            });

                        }

                    }

                }

            }

        }

    }
    
}