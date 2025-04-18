using UnityEngine;

namespace Breadcrumbs.Player {
    public abstract class PlayerBehaviorBase {
        protected PlayerController controller;
        
        protected ContextData Context => controller.Context;
        protected PlayerSettings Settings => controller.Settings;
        
        public int Priority { get; set; } = 0;

        public void SetController(PlayerController newController) {
            controller = newController;
        }

        public abstract void Execute();
        public abstract void HandleInput(InputData input);
    }
}