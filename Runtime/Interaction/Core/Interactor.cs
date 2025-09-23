using UnityEngine;

namespace Koala.Simulation.Interaction.Core
{
    /// <summary>
    /// Manages interactions for a registered interactor and provides an interaction solver.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        private int _id;
        private InteractionSolver _interactionSolver;

        /// <summary>
        /// The unique identifier assigned to this interactor.
        /// </summary>
        protected int Id => _id;

        /// <summary>
        /// The solver that processes and resolves interactions for this interactor.
        /// </summary>
        public InteractionSolver Solver => _interactionSolver;

        /// <summary>
        /// Whether the solver should generate interaction arguments.
        /// </summary>
        [SerializeField] private bool _generateInteractionArgs;

        protected void Awake()
        {
            _id = InteractorRegistry.Register(this);
            _interactionSolver = new InteractionSolver(_generateInteractionArgs, Id);

            OnAwake();
        }

        protected void OnDestroy()
        {
            _interactionSolver.Dispose();
            OnDestroyed();
        }

        /// <summary>
        /// Called when the interactor is initialized and registered.
        /// Override to add custom setup logic.
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// Called when the interactor is destroyed and unregistered.
        /// Override to add custom cleanup logic.
        /// </summary>
        protected virtual void OnDestroyed() { }
    }
}