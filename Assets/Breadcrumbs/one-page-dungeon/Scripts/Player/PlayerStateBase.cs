using UnityEngine;

namespace Breadcrumbs.Player {
    public abstract class PlayerStateBase
    {
        protected PlayerController CurrentController { get; private set; }

        public void SetController(PlayerController controller)
        {
            CurrentController = controller;
        }

        public abstract void OnEnterState();
        public abstract void OnExitState();
        public abstract void UpdateState();
        public abstract void HandleInput(InputData input);
    }
}