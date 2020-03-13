using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Entities;
    using Warcraft.Components;
    
    public class UnitsAttackSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {

        private IFilter<TState, UnitEntity> unitsFilter;
        private IFilter<TState, UnitEntity> unitTargetsFilter;

        private Warcraft.Features.PlayersFeature playersFeature;
        private Warcraft.Features.UnitsFeature unitsFeature;
        private Warcraft.Features.PathfindingFeature pathfindingFeature;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            this.unitsFeature = this.world.GetFeature<Warcraft.Features.UnitsFeature>();
            this.pathfindingFeature = this.world.GetFeature<Warcraft.Features.PathfindingFeature>();

            Filter<TState, UnitEntity>.Create(ref this.unitsFilter).WithComponent<UnitInfoComponent>().WithComponent<UnitPlayerOwnerComponent>().WithComponent<UnitCompleteComponent>().WithoutComponent<UnitDeathState>().WithoutComponent<UnitHiddenView>().WithComponent<CharacterAutoTarget>().Push();
            Filter<TState, UnitEntity>.Create(ref this.unitTargetsFilter).WithComponent<UnitInfoComponent>().WithComponent<UnitPlayerOwnerComponent>().WithComponent<UnitHealthComponent>().WithoutComponent<UnitDeathState>().WithoutComponent<UnitHiddenView>().Push();

        }
        
        void ISystemBase.OnDeconstruct() {}

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {

            foreach (var index in state.units) {

                ref var unit = ref state.units[index];
                if (this.unitsFilter.Contains(unit) == true) {

                    var found = false;
                    var rangeToTargetSqr = 0f;
                    UnitEntity targetUnit = default;
                    var dist = float.MaxValue;
                    var unitInfo = this.world.GetComponent<UnitEntity, UnitInfoComponent>(unit.entity);
                    var playerOwnerComponent = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                    var ownerEntity = playerOwnerComponent.player;
                    foreach (var target in this.unitTargetsFilter.GetData()) {
                        
                        this.world.GetEntityData(target, out UnitEntity targetData);

                        var distance = (targetData.position - unit.position).sqrMagnitude;
                        if (distance < dist && distance <= unitInfo.unitInfo.sightRange * unitInfo.unitInfo.sightRange) {

                            var targetPlayerOwnerComponent = this.world.GetComponent<UnitEntity, UnitPlayerOwnerComponent>(target);
                            var targetOwnerEntity = targetPlayerOwnerComponent.player;
                            if (ownerEntity == targetOwnerEntity || this.playersFeature.IsNeutralPlayer(targetOwnerEntity) == true) continue;
                        
                            this.world.GetEntityData(ownerEntity, out PlayerEntity ownerData);
                            var fow = this.world.GetComponent<UnitEntity, UnitFogOfWarComponent>(target);
                            if (fow == null || fow.IsVisible(ownerData.index) == false) continue;

                            dist = distance;
                            rangeToTargetSqr = distance;
                            targetUnit = targetData;
                            found = true;

                        }

                    }

                    if (found == true) {

                        if (rangeToTargetSqr <= unitInfo.unitInfo.attackRange * unitInfo.unitInfo.attackRange) {
                            
                            // Attack target
                            var manualTarget = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                            manualTarget.target = unit.position;
                            manualTarget.targetRange = unitInfo.unitInfo.attackRange;
                            
                            this.pathfindingFeature.StopMovement(unit.entity, false);
                            this.world.AddOrGetComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterStopState>(unit.entity);

                            var target = this.world.AddOrGetComponent<UnitEntity, CharacterTarget>(unit.entity);
                            if (target.target.entity != targetUnit.entity) {

                                target.target = targetUnit;
                                target.timer = 0f;
                                target.delay = 0f;

                            }

                            if (unitInfo.unitInfo.attackTime > 0f) {

                                if (target.timer <= 0f) {

                                    target.delay += deltaTime;
                                    if (target.delay >= unitInfo.unitInfo.attackDelay) {

                                        target.delay -= unitInfo.unitInfo.attackDelay;
                                        target.timer = deltaTime;

                                    }

                                } else {
                                    
                                    this.world.AddComponent<UnitEntity, Warcraft.Components.CharacterStates.CharacterAttackState>(unit.entity);

                                    target.timer += deltaTime;
                                    if (target.timer >= unitInfo.unitInfo.attackTime) {

                                        target.timer = 0f;
                                        
                                        this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterAttackState>(unit.entity);

                                        var health = this.world.GetComponent<UnitEntity, UnitHealthComponent>(targetUnit.entity);
                                        var damage = this.world.GetRandomRange(unitInfo.unitInfo.attackDamageRange.x, unitInfo.unitInfo.attackDamageRange.y);
                                        health.value -= damage;
                                        //UnityEngine.Debug.Log("H: " + health.value + ", D: " + damage + ", timer: " + target.timer + ", delay: " + target.delay);
                                        if (health.value <= 0) {

                                            this.world.RemoveComponents<UnitEntity, CharacterTarget>(unit.entity);
                                            this.unitsFeature.UnitDie(targetUnit.entity);
                                            this.pathfindingFeature.StopMovement(unit.entity, repath: false);

                                        }

                                    }

                                }

                            }

                        } else {

                            // Move to selected unit
                            var manualTarget = this.world.AddOrGetComponent<UnitEntity, CharacterManualTarget>(unit.entity);
                            manualTarget.target = targetUnit.position;
                            manualTarget.targetRange = unitInfo.unitInfo.attackRange;

                        }

                    } else {

                        this.world.RemoveComponents<UnitEntity, Warcraft.Components.CharacterStates.CharacterAttackState>(unit.entity);

                    }

                }

            }

        }
        
        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {}
        
    }
    
}