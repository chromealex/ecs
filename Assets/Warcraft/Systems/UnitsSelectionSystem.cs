using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Markers;
    using Warcraft.Components;
    
    public class UnitsSelectionSystem : ISystem<TState> {

        private const int MAX_SELECTION_COUNT = 12;
        
        private InputUnitUpgrade upgradeMarker;
        private bool upgradeMarkerExists;

        private InputLeftClick lastInputLeftClick;
        private bool lastInputClickUsed;

        private InputDragBegin lastInputDragBegin;
        private bool lastInputDragBeginUsed;

        private InputDragMove lastInputDragMove;
        private bool lastInputDragMoveUsed;

        private InputDragEnd lastInputDragEnd;
        private bool lastInputDragEndUsed;

        private Entity selectionRectEntity;
        
        private IFilter<Warcraft.WarcraftState, Warcraft.Entities.UnitEntity> unitsFilter;

        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            this.lastInputClickUsed = true;
            this.lastInputDragBeginUsed = true;
            this.lastInputDragMoveUsed = true;
            this.lastInputDragEndUsed = true;
            this.upgradeMarkerExists = false;

            Filter<Warcraft.WarcraftState, Warcraft.Entities.UnitEntity>.Create(ref this.unitsFilter, "unitsFilter").WithoutComponent<UnitGhosterComponent>().WithoutComponent<UnitHiddenView>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystem<TState>.AdvanceTick(TState state, float deltaTime) {
            
            var mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            var playerFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            var unitsSelectionFeature = this.world.GetFeature<Warcraft.Features.UnitsSelectionFeature>();

            if (this.upgradeMarkerExists == true) {

                this.upgradeMarkerExists = false;
                
                this.ClearSelections(state);

            }

            if (this.lastInputClickUsed == false) {

                this.lastInputClickUsed = true;
                
                this.ClearSelections(state);
                
                var clickPos = this.lastInputLeftClick.worldPosition;
                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.unitsFilter.Contains(unit) == false) continue;
                        
                    if (mapFeature.IsInBounds(unit.position, unit.size, clickPos) == true) {

                        var ownerComp = this.world.GetComponent<Warcraft.Entities.UnitEntity, Warcraft.Components.UnitPlayerOwnerComponent>(unit.entity);
                        if (ownerComp.player == playerFeature.GetActivePlayer()) {

                            unitsSelectionFeature.AddSelection(unit);
                            break;

                        }

                    }

                }

            }

            if (this.lastInputDragBeginUsed == false) {

                this.lastInputDragBeginUsed = true;

                this.ClearSelections(state);

                this.selectionRectEntity = unitsSelectionFeature.AddSelectionRect(this.lastInputDragBegin.worldPosition);

            }

            if (this.lastInputDragMoveUsed == false) {

                this.lastInputDragMoveUsed = true;

                var comp = this.world.GetComponent<Warcraft.Entities.SelectionRectEntity, Warcraft.Components.SelectionRectComponent>(this.selectionRectEntity);
                comp.size = VectorExt.Abs(this.lastInputDragMove.worldPosition - this.lastInputDragMove.beginWorldPosition);
                comp.worldPosition = (this.lastInputDragMove.worldPosition + this.lastInputDragMove.beginWorldPosition) * 0.5f;

            }

            if (this.lastInputDragEndUsed == false) {

                this.lastInputDragEndUsed = true;

                var count = 0;
                var comp = this.world.GetComponent<Warcraft.Entities.SelectionRectEntity, Warcraft.Components.SelectionRectComponent>(this.selectionRectEntity);
                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.unitsFilter.Contains(unit) == false) continue;
                    
                    if (mapFeature.IsInBounds(unit.position, unit.size, comp.worldPosition, comp.size) == true) {

                        ++count;
                        if (count >= UnitsSelectionSystem.MAX_SELECTION_COUNT) {
                            
                            break;
                            
                        }
                        
                    }
                    
                }

                var allCount = count;
                count = 0;
                foreach (var index in state.units) {

                    ref var unit = ref state.units[index];
                    if (this.unitsFilter.Contains(unit) == false) continue;
                    
                    if (mapFeature.IsInBounds(unit.position, unit.size, comp.worldPosition, comp.size) == true) {

                        unitsSelectionFeature.AddSelection(unit, allCount);
                        ++count;
                        if (count >= UnitsSelectionSystem.MAX_SELECTION_COUNT) {
                            
                            break;
                            
                        }

                    }

                }

                this.world.RemoveEntity<Warcraft.Entities.SelectionRectEntity>(this.selectionRectEntity);

            }

        }

        void ISystem<TState>.Update(TState state, float deltaTime) {

            if (this.world.GetMarker(out InputUnitUpgrade marker) == true) {

                this.upgradeMarker = marker;
                this.upgradeMarkerExists = true;

            }

            if (this.world.GetMarker(out InputLeftClick inputClick) == true) {

                this.lastInputClickUsed = false;
                this.lastInputLeftClick = inputClick;

            }

            if (this.world.GetMarker(out InputDragBegin inputDragBegin) == true) {

                this.lastInputDragBeginUsed = false;
                this.lastInputDragBegin = inputDragBegin;

            }

            if (this.world.GetMarker(out InputDragMove inputDragMove) == true) {

                this.lastInputDragMoveUsed = false;
                this.lastInputDragMove = inputDragMove;

            }

            if (this.world.GetMarker(out InputDragEnd inputDragEnd) == true) {

                this.lastInputDragEndUsed = false;
                this.lastInputDragEnd = inputDragEnd;

            }

        }
        
        private void ClearSelections(TState state) {
            
            var playerFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            var unitsSelectionFeature = this.world.GetFeature<Warcraft.Features.UnitsSelectionFeature>();
            
            foreach (var index in state.selections) {

                ref var selection = ref state.selections[index];
                    
                unitsSelectionFeature.ClearSelection(selection.unitEntity);
                this.world.RemoveEntity<Warcraft.Entities.SelectionEntity>(selection.entity);
                this.world.RemoveComponents<Warcraft.Entities.PlayerEntity, Warcraft.Components.PlayerSelectedUnitComponent>(playerFeature.GetActivePlayer());

            }

        }

    }
    
}