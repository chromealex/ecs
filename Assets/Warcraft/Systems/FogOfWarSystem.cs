using ME.ECS;

namespace Warcraft.Systems {

    using TState = WarcraftState;
    using Warcraft.Components;
    using Warcraft.Entities;
    
    public class FogOfWarSystem : ISystem<TState>, ISystemAdvanceTick<TState>, ISystemUpdate<TState> {

        private const float FOW_RANGE_FOR_BUILDINGS_IN_PROGRESS = 1f;
        
        private class Player {

            public PlayerFogOfWarComponent fow;
            public UnityEngine.Tilemaps.TileBase[] tiles;
            public UnityEngine.Tilemaps.TileBase[] tilesRevealed;

        }

        private IFilter<TState, Warcraft.Entities.UnitEntity> unitsFilter;
        private IFilter<TState, Warcraft.Entities.UnitEntity> unitsSightFilter;
        private IFilter<TState, Warcraft.Entities.UnitEntity> unitsSightBuildingsInProgressFilter;
        
        private Warcraft.Features.MapFeature mapFeature;
        private Warcraft.Features.PlayersFeature playersFeature;
        private UnityEngine.Tilemaps.TileBase[] tilesCacheClear;
        private System.Collections.Generic.Dictionary<Entity, Player> playersCache;
        
        public IWorld<TState> world { get; set; }

        void ISystemBase.OnConstruct() {

            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.unitsFilter, "unitsFilter").WithoutComponent<UnitGhosterComponent>().WithComponent<UnitCompleteComponent>().WithoutComponent<UnitHiddenView>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.unitsSightFilter, "unitsSightFilter").WithComponent<UnitSightRangeComponent>().WithComponent<UnitPlayerOwnerComponent>().WithoutComponent<UnitGhosterComponent>().WithComponent<UnitCompleteComponent>().WithoutComponent<UnitHiddenView>().WithoutComponent<UnitDeathState>().Push();
            Filter<TState, Warcraft.Entities.UnitEntity>.Create(ref this.unitsSightBuildingsInProgressFilter, "unitsSightFilterBuildingsInProgress").WithComponent<UnitSightRangeComponent>().WithComponent<UnitPlayerOwnerComponent>().WithoutComponent<UnitGhosterComponent>().WithoutComponent<UnitCompleteComponent>().WithoutComponent<CharacterComponent>().WithoutComponent<UnitHiddenView>().Push();

            this.mapFeature = this.world.GetFeature<Warcraft.Features.MapFeature>();
            this.playersFeature = this.world.GetFeature<Warcraft.Features.PlayersFeature>();
            
            this.tilesCacheClear = new UnityEngine.Tilemaps.TileBase[this.mapFeature.mapInfo.mapSize.x * this.mapFeature.mapInfo.mapSize.y];
            for (int i = 0; i < this.tilesCacheClear.Length; ++i) {

                this.tilesCacheClear[i] = this.mapFeature.mapInfo.fogOfWarTile;

            }

            this.playersCache = PoolDictionary<Entity, Player>.Spawn(1);

            var allPlayers = this.playersFeature.GetAllPlayers();
            foreach (var playerItem in allPlayers) {

                var playerEntity = playerItem.Value;
                
                var comp = this.world.AddComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
                comp.revealed = PoolArray<byte>.Spawn(this.mapFeature.mapInfo.mapSize.x * this.mapFeature.mapInfo.mapSize.y);

                if (this.playersFeature.IsNeutralPlayer(playerEntity) == true) this.world.AddComponent<PlayerEntity, IsNeutral>(playerEntity);
                
                var arr = new UnityEngine.Tilemaps.TileBase[this.tilesCacheClear.Length];
                System.Array.Copy(this.tilesCacheClear, arr, this.tilesCacheClear.Length);
                var arrRevealed = new UnityEngine.Tilemaps.TileBase[this.tilesCacheClear.Length];
                System.Array.Copy(this.tilesCacheClear, arrRevealed, this.tilesCacheClear.Length);
                this.playersCache.Add(playerEntity, new Player() {
                    tiles = arr,
                    tilesRevealed = arrRevealed,
                    fow = comp
                });

            }

        }

        void ISystemBase.OnDeconstruct() {
            
            PoolDictionary<Entity, Player>.Recycle(ref this.playersCache);
            
        }

        void ISystemAdvanceTick<TState>.AdvanceTick(TState state, float deltaTime) {

            foreach (var index in state.players) {
                
                ref var player = ref state.players[index];
                this.Clear(player.entity);

            }

            foreach (var unitEntity in this.unitsSightFilter) {

                ref var unit = ref this.world.GetEntityDataRef<UnitEntity>(unitEntity);
                var playerOwner = this.world.GetComponent<Warcraft.Entities.UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                if (this.playersFeature.IsNeutralPlayer(playerOwner.player) == false) {

                    var sightRange = this.world.GetComponent<Warcraft.Entities.UnitEntity, UnitSightRangeComponent>(unit.entity);
                    this.RevealRange(playerOwner.player, unit.position, sightRange.range);

                }

            }

            foreach (var unitEntity in this.unitsSightBuildingsInProgressFilter) {

                ref var unit = ref this.world.GetEntityDataRef<UnitEntity>(unitEntity);
                var playerOwner = this.world.GetComponent<Warcraft.Entities.UnitEntity, UnitPlayerOwnerComponent>(unit.entity);
                if (this.playersFeature.IsNeutralPlayer(playerOwner.player) == false) {

                    this.RevealRange(playerOwner.player, unit.position, FogOfWarSystem.FOW_RANGE_FOR_BUILDINGS_IN_PROGRESS);

                }

            }

            foreach (var unitEntity in this.unitsFilter) {

                ref var unit = ref this.world.GetEntityDataRef<UnitEntity>(unitEntity);
                foreach (var pIndex in state.players) {

                    ref var player = ref state.players[pIndex];
                    if (this.playersFeature.IsNeutralPlayer(player.entity) == true) continue;

                    var comp = this.world.AddOrGetComponent<UnitEntity, UnitFogOfWarComponent>(unit.entity);
                    this.GetAnyRevealedAndVisible(player.entity, unit.position, unit.size, out var isRev, out var isVis);
                    if (isRev == true) {

                        comp.playersRevealed |= (ulong)(long)(1 << player.index);

                    } else {

                        comp.playersRevealed &= ~(ulong)(1 << player.index);

                    }

                    if (isVis == true) {

                        comp.playersVisible |= (ulong)(long)(1 << player.index);

                    } else {

                        comp.playersVisible &= ~(ulong)(1 << player.index);

                    }

                }

            }

        }

        void ISystemUpdate<TState>.Update(TState state, float deltaTime) {
            
            var tilemap = this.mapFeature.grid.fowTilemap;
            var tilemapRevealed = this.mapFeature.grid.fowRevealedTilemap;
            var activePlayer = this.playersFeature.GetActivePlayer();
            
            if (this.playersCache.TryGetValue(activePlayer, out var playerCache) == true) {

                tilemap.SetTilesBlock(new UnityEngine.BoundsInt(0, 0, 0, this.mapFeature.mapInfo.mapSize.x, this.mapFeature.mapInfo.mapSize.y, 1), playerCache.tiles);
                tilemapRevealed.SetTilesBlock(new UnityEngine.BoundsInt(0, 0, 0, this.mapFeature.mapInfo.mapSize.x, this.mapFeature.mapInfo.mapSize.y, 1), playerCache.tilesRevealed);

            }
            
        }

        private void GetAnyRevealedAndVisible(Entity playerEntity, UnityEngine.Vector2 worldPos, UnityEngine.Vector2Int size, out bool isRev, out bool isVis) {

            isRev = false;
            isVis = false;

            if (this.playersCache.TryGetValue(playerEntity, out var playerCache) == true) {

                var leftBottom = this.mapFeature.GetCellLeftBottomPosition(worldPos, size);
                var playerFogOfWar = playerCache.fow;
                var mapSize = this.mapFeature.mapInfo.mapSize;
                for (int x = 0; x < size.x; ++x) {

                    for (int y = 0; y < size.y; ++y) {

                        var pos = leftBottom + new UnityEngine.Vector2Int(x, y);
                        var idx = pos.y * mapSize.x + pos.x;

                        isRev = playerFogOfWar.revealed[idx] >= 1;
                        isVis = playerFogOfWar.revealed[idx] == 1;

                        if (isRev == true) return;

                    }

                }

            }

        }

        private bool IsRevealed(Entity playerEntity, UnityEngine.Vector2Int cellPos) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
            return this.IsRevealed_INTERNAL(playerFogOfWar, cellPos);

        }

        private bool IsVisible(Entity playerEntity, UnityEngine.Vector2Int cellPos) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
            return this.IsVisible_INTERNAL(playerFogOfWar, cellPos);

        }

        private bool IsRevealed_INTERNAL(PlayerFogOfWarComponent playerFogOfWarComponent, UnityEngine.Vector2Int cellPos) {

            var mapSize = this.mapFeature.mapInfo.mapSize;
            var idx = cellPos.y * mapSize.x + cellPos.x;
            if (idx < 0 || idx >= playerFogOfWarComponent.revealed.Length) return false;
            return playerFogOfWarComponent.revealed[idx] >= 1;

        }

        private bool IsVisible_INTERNAL(PlayerFogOfWarComponent playerFogOfWarComponent, UnityEngine.Vector2Int cellPos) {

            var mapSize = this.mapFeature.mapInfo.mapSize;
            var idx = cellPos.y * mapSize.x + cellPos.x;
            if (idx < 0 || idx >= playerFogOfWarComponent.revealed.Length) return false;
            return playerFogOfWarComponent.revealed[idx] == 1;

        }

        public bool IsRevealedAny(Entity playerEntity, UnityEngine.Vector2Int leftBottom, UnityEngine.Vector2Int size) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {

                    if (this.IsRevealed_INTERNAL(playerFogOfWar, leftBottom + new UnityEngine.Vector2Int(x, y)) == true) {

                        return true;

                    }

                }
                
            }

            return false;

        }

        public bool IsRevealedAll(Entity playerEntity, UnityEngine.Vector2Int leftBottom, UnityEngine.Vector2Int size) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {

                    if (this.IsRevealed_INTERNAL(playerFogOfWar, leftBottom + new UnityEngine.Vector2Int(x, y)) == false) {

                        return false;

                    }

                }
                
            }

            return true;

        }

        public bool IsVisibleAny(Entity playerEntity, UnityEngine.Vector2Int leftBottom, UnityEngine.Vector2Int size) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {

                    if (this.IsVisible_INTERNAL(playerFogOfWar, leftBottom + new UnityEngine.Vector2Int(x, y)) == true) {

                        return true;

                    }

                }
                
            }

            return false;

        }

        public bool IsVisibleAll(Entity playerEntity, UnityEngine.Vector2Int leftBottom, UnityEngine.Vector2Int size) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
            for (int x = 0; x < size.x; ++x) {
             
                for (int y = 0; y < size.y; ++y) {

                    if (this.IsVisible_INTERNAL(playerFogOfWar, leftBottom + new UnityEngine.Vector2Int(x, y)) == false) {

                        return false;

                    }

                }
                
            }

            return true;

        }

        private void Clear(Entity playerEntity) {
            
            if (this.playersCache.TryGetValue(playerEntity, out var playerCache) == true) {

                //System.Array.Copy(this.tilesCacheClear, playerCache.tiles, this.tilesCacheClear.Length);
                System.Array.Copy(this.tilesCacheClear, playerCache.tilesRevealed, this.tilesCacheClear.Length);

                var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);
                //System.Array.Clear(playerFogOfWar.revealed, 0, playerFogOfWar.revealed.Length);
                for (int i = 0; i < playerFogOfWar.revealed.Length; ++i) {

                    ref var rev = ref playerFogOfWar.revealed[i];
                    if (rev >= 1) {

                        //playerCache.tilesRevealed[i] = this.mapFeature.mapInfo.fogOfWarTile;
                        rev = 2;

                    } else {

                        rev = 0;

                    }

                }

            }

        }
        
        private void RevealCell(PlayerFogOfWarComponent fogOfWar, Player playerCache, Entity playerEntity, UnityEngine.Vector2Int cellPos) {

            var mapSize = this.mapFeature.mapInfo.mapSize;
            var idx = cellPos.y * mapSize.x + cellPos.x;
            fogOfWar.revealed[idx] = 1;
            
            playerCache.tiles[idx] = null;
            playerCache.tilesRevealed[idx] = null;

        }

        private void RevealRange(Entity playerEntity, UnityEngine.Vector2 position, float range) {

            var playerFogOfWar = this.world.GetComponent<PlayerEntity, PlayerFogOfWarComponent>(playerEntity);

            Player playerCache;
            this.playersCache.TryGetValue(playerEntity, out playerCache);

            var cellPosition = this.mapFeature.GetMapPositionFromWorld(position);
            
            var rangeSize = this.mapFeature.GetMapPositionFromWorld(new UnityEngine.Vector2(range, range));
            var cellRange = rangeSize.x;
            var cellRangeSqr = cellRange * cellRange;
            var bottomLeft = cellPosition - rangeSize;
            var topRight = cellPosition + rangeSize;
            bottomLeft = this.mapFeature.ClampToMap(bottomLeft);
            topRight = this.mapFeature.ClampToMap(topRight);
            for (int x = bottomLeft.x; x < topRight.x; ++x) {
             
                for (int y = bottomLeft.y; y < topRight.y; ++y) {
                    
                    var cellPos = new UnityEngine.Vector2Int(x, y);
                    if ((cellPosition - cellPos).sqrMagnitude <= cellRangeSqr) {

                        this.RevealCell(playerFogOfWar, playerCache, playerEntity, cellPos);

                    }

                }
                
            }
            
        }

    }
    
}