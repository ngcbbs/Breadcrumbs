using UnityEngine;

namespace GamePortfolio.Gameplay.AI {
    /// <summary>
    /// Interface for enemy AI states in the state machine pattern
    /// </summary>
    public interface IEnemyState {
        /// <summary>
        /// Called when entering this state
        /// </summary>
        void EnterState();

        /// <summary>
        /// Called every frame to update the state
        /// </summary>
        void UpdateState();

        /// <summary>
        /// Called when exiting this state
        /// </summary>
        void ExitState();

        /// <summary>
        /// Called to check if the state should transition to another state
        /// </summary>
        /// <returns>The next state type, or null if no transition should occur</returns>
        EnemyStateType? CheckTransitions();
    }
}