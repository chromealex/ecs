using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.ECS {

    public abstract class InitializerBase : MonoBehaviour {

        public FeaturesList featuresList = new FeaturesList();

        protected void InitializeFeatures<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            this.featuresList.Initialize(world);

        }

        protected void DeInitializeFeatures<TState>(World<TState> world) where TState : class, IState<TState>, new() {

            this.featuresList.DeInitialize(world);

        }

    }

}
