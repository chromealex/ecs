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
        
        public override void OnInitialize(in TEntity data) {

            this.playerData = data;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            this.selectedUnit = Entity.Empty;
            
        }

        private void UpdateActions() {
            
            foreach (var button in this.buttons) {

                Object.Destroy(button.gameObject);

            }

            this.buttons.Clear();

            foreach (var item in this.graphNodeSelected) {

                var icon = item.GetIcon();
                if (icon != null) {

                    var actionInfo = item;
                    var button = Object.Instantiate(this.buttonSource, this.actionsContainer);
                    button.gameObject.SetActive(true);
                    button.SetInfo(actionInfo.cost, icon);
                    button.button.onClick.RemoveAllListeners();
                    button.button.onClick.AddListener(() => { this.DoAction(actionInfo); });
                    button.button.interactable = actionInfo.IsEnabled(this.playerData);
                    this.buttons.Add(button);

                }

            }

        }

        private void UpdateQueue(Entity entity) {
            
            var world = Worlds<WarcraftState>.currentWorld;

            foreach (var button in this.queueButtons) {

                Object.Destroy(button.gameObject);

            }

            this.queueButtons.Clear();

            var queue = world.GetComponent<UnitEntity, UnitBuildingQueueComponent>(entity);
            if (queue != null) {

                var i = 0;
                foreach (var unitEntity in queue.units) {

                    var unit = unitEntity;
                    var unitInfo = world.GetComponent<UnitEntity, UnitInfoComponent>(unit);
                    var icon = unitInfo.unitInfo.icon;

                    var progress = world.GetComponent<UnitEntity, UnitBuildingProgress>(unit);

                    var button = Object.Instantiate(this.queueButtonSource, this.actionsContainer);
                    button.gameObject.SetActive(true);
                    button.SetInfo(icon, progress.progress / progress.time, i == 0);
                    button.button.onClick.RemoveAllListeners();
                    button.button.onClick.AddListener(() => { this.DoCancelQueue(entity, unit); });
                    this.queueButtons.Add(button);

                    ++i;

                }

            }

        }

        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {

            var world = Worlds<WarcraftState>.currentWorld;

            var inProgress = world.GetComponent<UnitEntity, UnitBuildingProgress>(data.entity);
            if (inProgress != null) {

                this.actionsContainer.gameObject.SetActive(false);
                this.queueContainer.gameObject.SetActive(false);
                this.buildingProgress.gameObject.SetActive(true);
                var progress = inProgress.progress / inProgress.time;
                this.buildingProgress.value = progress;
                this.buildingValue.text = Mathf.FloorToInt(progress * 100f).ToString() + "%";
                return;

            }
            
            this.actionsContainer.gameObject.SetActive(true);
            this.queueContainer.gameObject.SetActive(false);
            this.buildingProgress.gameObject.SetActive(false);

            var comp = world.GetComponent<TEntity, Warcraft.Components.PlayerSelectedUnitComponent>(data.entity);
            if (comp != null) {

                if (this.selectedUnit != comp.unit) this.graphNodeSelected = null;
                this.selectedUnit = comp.unit;
                
                if (world.GetEntityData(comp.unit, out UnitEntity unitEntity) == true) {

                    var unitInfoComponent = world.GetComponent<UnitEntity, UnitInfoComponent>(unitEntity.entity);
                    this.SetInfo(unitInfoComponent.unitInfo);
                
                    this.UpdateQueue(data.entity);

                } else {

                    this.graphNodeSelected = null;
                    this.selectedUnit = Entity.Empty;

                }

            } else {

                if (this.selectedUnit != Entity.Empty) this.graphNodeSelected = null;
                this.selectedUnit = Entity.Empty;
                this.SetInfo(this.noUnitInfo);
                
            }

        }

        private void SetInfo(UnitInfo unitInfo) {
            
            this.unitCaption.text = unitInfo.name;
            this.unitIcon.sprite = unitInfo.icon;

            if (unitInfo.actions != null) {

                var actions = unitInfo.actions.roots;
                if (this.graphNodeSelected == null) {

                    this.graphNodeSelected = actions;
                    this.UpdateActions();

                }

            }
            
        }

        private void DoCancelQueue(Entity building, Entity unit) {
            
            Worlds<WarcraftState>.currentWorld.AddMarker(new InputUnitQueueCancel() {
                selectedUnit = building,
                unitInQueue = unit
            });
            
        }

        private void DoAction(ActionsGraphNode actionInfo) {

            if (actionInfo.GetNext().Length > 0) {

                this.graphNodeSelected = actionInfo.GetNext();
                this.UpdateActions();

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