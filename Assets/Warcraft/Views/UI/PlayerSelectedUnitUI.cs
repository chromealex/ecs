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

        public Transform actionsContainer;
        public ButtonActionUI buttonSource;
        private System.Collections.Generic.List<ButtonActionUI> buttons = new System.Collections.Generic.List<ButtonActionUI>();
        
        private ActionsGraphNode[] graphNodeSelected;
        private TEntity playerData;
        private Entity selectedUnit;
        
        public override void OnInitialize(in TEntity data) {

            this.playerData = data;

        }
        
        public override void OnDeInitialize(in TEntity data) {

            this.selectedUnit = Entity.Empty;
            
        }

        private void ClearButtons() {
            
            foreach (var button in this.buttons) {

                Object.Destroy(button.gameObject);

            }

            this.buttons.Clear();

        }

        private void UpdateActions() {
            
            this.ClearButtons();
            
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

        public override void ApplyState(in TEntity data, float deltaTime, bool immediately) {
            
            var comp = Worlds<WarcraftState>.currentWorld.GetComponent<TEntity, Warcraft.Components.PlayerSelectedUnitComponent>(data.entity);
            if (comp != null) {

                if (this.selectedUnit != comp.unit) this.graphNodeSelected = null;
                this.selectedUnit = comp.unit;
                
                if (Worlds<WarcraftState>.currentWorld.GetEntityData(comp.unit, out UnitEntity unitEntity) == true) {

                    var unitInfoComponent = Worlds<WarcraftState>.currentWorld.GetComponent<UnitEntity, UnitInfoComponent>(unitEntity.entity);
                    this.SetInfo(unitInfoComponent.unitInfo);
                
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