using UnityEngine;
using ME.ECS;
using ME.ECS.Views.Providers;

namespace ME.Example.Game {
    
    using ME.Example.Game.Modules;
    using ME.Example.Game.Systems;
    using ME.Example.Game.Entities;

    public class GameGameObject : Game {

        public MonoBehaviourViewBase pointSourceGameObject;
        public MonoBehaviourViewBase unitSource;
        public MonoBehaviourViewBase unitSource2;

        public override void RegisterViewSources() {
            
            this.pointViewSourceId = this.world.RegisterViewSource<Point>(this.pointSourceGameObject);
            this.unitViewSourceId = this.world.RegisterViewSource<Unit>(this.unitSource);
            this.unitViewSourceId2 = this.world.RegisterViewSource<Unit>(this.unitSource2);

        }

    }

}
