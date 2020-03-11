using ME.ECS;
using UnityEngine;
using Warcraft.Features;

namespace Warcraft.Views {
    
    using ME.ECS.Views.Providers;
    using Warcraft.Entities;
    using Warcraft.Components;
    using Warcraft.Components.CharacterStates;

    [System.Serializable]
    public struct DirectionSprites {

        [System.Serializable]
        public struct DirectionSprite {

            public Sprite sprite;
            public bool flip;

        }

        [System.Serializable]
        public struct Frame {

            public DirectionSprite[] directions;

        }

        public Frame[] frames;
        public float animationTime;
        public bool cyclic;
        public bool animate;
        
        private float animationTimer;
        private int frameIndex;
        private int orientation;

        public void ApplySprite(SpriteRenderer spriteRenderer, Vector2 from, Vector2 to, float deltaTime, bool reset) {

            if (this.animate == true) {

                if (reset == true) {
                    
                    this.animationTimer = 0f;
                    this.frameIndex = 0;

                }
                
                this.animationTimer += deltaTime;
                if (this.animationTimer >= this.animationTime) {

                    this.animationTimer -= this.animationTime;
                    if (this.animationTimer > this.animationTime) {

                        this.animationTimer = 0f;

                    }

                    ++this.frameIndex;
                    if (this.frameIndex >= this.frames.Length) {

                        if (this.cyclic == false) {

                            this.frameIndex = this.frames.Length - 1;

                        } else {

                            this.frameIndex = 0;

                        }

                    }

                }

            }

            if (from != to) {
                
                MathUtils.GetOrientation(out this.orientation, from.XY(), to);
                
            }
            var dir = this.frames[this.frameIndex].directions[this.orientation];

            spriteRenderer.sprite = dir.sprite;
            spriteRenderer.flipX = dir.flip;

        }

    }

    public class CharacterView : UnitView {

        public DirectionSprites idleSprites;
        public DirectionSprites walkSprites;
        public DirectionSprites deathSprites;
        public DirectionSprites attackSprites;
        
        private int lastIndex;
        
        [ContextMenu("Setup")]
        public void SetupFromSpriteRenderer() {

            this.ApplyStateSprites(ref this.idleSprites);
            this.ApplyStateSprites(ref this.walkSprites);
            this.ApplyStateSprites(ref this.deathSprites);
            this.ApplyStateSprites(ref this.attackSprites);
            
        }

        private void ApplyStateSprites(ref DirectionSprites state) {
            
            var sprite = this.spriteRenderer.sprite;
            var path = UnityEditor.AssetDatabase.GetAssetPath(sprite.texture);
            var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);

            for (var i = 0; i < state.frames.Length; ++i) {
                
                var frame = state.frames[i];
                for (var j = 0; j < frame.directions.Length; ++j) {
                    
                    var dir = frame.directions[j];
                    var n1 = dir.sprite.name.Split('_')[1];
                    foreach (var spr in sprites) {

                        var name = spr.name.Split('_');
                        if (spr is Sprite) {

                            var n2 = name[1];
                            if (n1 == n2) {

                                frame.directions[j].sprite = spr as Sprite;
                                break;

                            }

                        }

                    }
                    
                }
                
            }
            
        }

        public override void ApplyState(in UnitEntity data, float deltaTime, bool immediately) {

            var reset = false;
            ref var spriteGroup = ref this.GetIdleState(in data);
            if (this.IsDead(in data) == true) {

                reset = (this.lastIndex != 1);
                this.lastIndex = 1;
                spriteGroup = ref this.deathSprites;

            } else if (this.IsWalk(in data) == true) {

                reset = (this.lastIndex != 2);
                this.lastIndex = 2;
                spriteGroup = ref this.GetWalkState(in data);

            } else if (this.IsAttack(in data) == true) {

                reset = (this.lastIndex != 3);
                this.lastIndex = 3;
                spriteGroup = ref this.attackSprites;

            } else {
                
                reset = (this.lastIndex != 0);
                this.lastIndex = 0;
                
            }

            spriteGroup.ApplySprite(this.spriteRenderer, this.tr.position, data.position, deltaTime, reset);

            base.ApplyState(in data, deltaTime, immediately);

        }

        protected virtual ref DirectionSprites GetIdleState(in UnitEntity data) {

            return ref this.idleSprites;

        }

        protected virtual ref DirectionSprites GetWalkState(in UnitEntity data) {

            return ref this.walkSprites;

        }

        protected virtual bool IsIdle(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, CharacterIdleState>(data.entity);
            
        }

        protected virtual bool IsWalk(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, CharacterMoveState>(data.entity);

        }

        protected virtual bool IsDead(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, UnitDeathState>(data.entity);

        }

        protected virtual bool IsAttack(in UnitEntity data) {

            var world = Worlds<WarcraftState>.currentWorld;
            return world.HasComponent<UnitEntity, CharacterAttackState>(data.entity);

        }

    }
    
}