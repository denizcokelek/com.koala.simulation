using System;
using System.Collections.Generic;
using Koala.Simulation.Input;
using UnityEngine.InputSystem;

namespace Koala.Simulation.Interaction.Core
{
    /// <summary>
    /// Resolves available interactions from nearby interactable objects and triggers callbacks on input.
    /// </summary>
    public class InteractionSolver : IDisposable
    {
        private readonly Dictionary<string, InteractionContext> _currentInteractions = new(8);
        private readonly List<InteractableObject> _lastSerie = new();
        private readonly bool _generateInteractionArgs;
        private readonly int _id;

        /// <summary>
        /// Invoked when the set of available interactions changes.
        /// Subscribe to this event to get the current interactions belongs this solver.
        /// </summary>
        public event Action<IReadOnlyDictionary<string, InteractionContext>> OnNewInteraction;

        /// <summary>
        /// Creates a new interaction solver.
        /// </summary>
        /// <param name="generateInteractionArgs">Whether interaction arguments should be generated.</param>
        /// <param name="id">The solver ID used for filtering interactions.</param>
        public InteractionSolver(bool generateInteractionArgs, int id)
        {
            _generateInteractionArgs = generateInteractionArgs;
            _id = id;
            InputService.OnInputReceive += OnInputReceive;
        }

        /// <summary>
        /// Unsubscribes from input events and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            InputService.OnInputReceive -= OnInputReceive;
        }

        /// <summary>
        /// Updates the solver with a new set of interactable objects and resolves their interactions.
        /// Call this method when you handle set of <see cref="InteractableObject"/> to solve their interactions.
        /// </summary>
        /// <param name="interactableObjects">The list of interactable objects in range.</param>
        public void Solve(List<InteractableObject> interactableObjects)
        {
            _currentInteractions.Clear();

            for (int i = 0; i < interactableObjects.Count; i++)
            {
                var interactable = interactableObjects[i];
                var interactions = _generateInteractionArgs
                    ? interactable.GetInteractions(_id)
                    : interactable.GetInteractions();

                foreach (var interaction in interactions)
                    _currentInteractions[interaction.ActionName] = interaction;
            }

            OnNewInteraction?.Invoke(_currentInteractions);

            for (int i = 0; i < _lastSerie.Count; i++)
            {
                if (!interactableObjects.Contains(_lastSerie[i]))
                    _lastSerie[i].OnInteractionEnded();
            }

            _lastSerie.Clear();
            _lastSerie.AddRange(interactableObjects);
        }

        /// <summary>
        /// Clears all interactions and signals the end of any active ones.
        /// To ensure no data remains on the solver, call this method when there are no interactable objects left to process.
        /// </summary>
        public void Reset()
        {
            _currentInteractions.Clear();
            foreach (var interactable in _lastSerie)
                interactable.OnInteractionEnded();
            _lastSerie.Clear();
            OnNewInteraction?.Invoke(_currentInteractions);
        }

        private void OnInputReceive(InputAction.CallbackContext ctx)
        {
            if (_currentInteractions.TryGetValue(ctx.action.name, out var interaction))
                interaction.OnInteract?.Invoke();
        }
    }
}