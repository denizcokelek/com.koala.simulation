using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace Koala.Simulation.Interaction.Core
{
    /// <summary>
    /// Base component for objects that can be interacted with in the simulation.
    /// Holds a collection of <see cref="InteractionBehaviour"/> instances
    /// and exposes them as <see cref="InteractionContext"/> entries.
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        /// <summary>
        /// Interaction behaviours that define available interactions for this object.
        /// </summary>
        [SerializeField] private List<InteractionBehaviour> _interactionBehaviours = new();

        /// <summary>
        /// Event invoked when an interaction begins on this object.
        /// </summary>
        [SerializeField] private UnityEvent OnInteract;

        /// <summary>
        /// Event invoked when an interaction ends on this object.
        /// </summary>
        [SerializeField] private UnityEvent OnInteractEnd;

        // cache re-use için tek liste
        private readonly List<InteractionContext> _contextCache = new(12);

        private int _id;
        /// <summary>
        /// The unique ID of this interactable object.
        /// </summary>
        protected int Id => _id;

        /// <summary>
        /// Called when an interaction is successfully performed on any of the behaviours.
        /// </summary>
        public event Action OnBehaviourInteraction;

        private void Awake()
        {
            _id = InteractableRegistry.Register(this);
            OnAwake();
        }

        private void OnEnable()
        {
            foreach (var behaviour in _interactionBehaviours)
                behaviour.OnInteractionPerform += OnInteractionPerformed;

            OnEnabled();
        }

        private void OnDisable()
        {
            foreach (var behaviour in _interactionBehaviours)
                behaviour.OnInteractionPerform -= OnInteractionPerformed;

            OnDisabled();
        }

        private void OnInteractionPerformed()
        {
            OnBehaviourInteraction?.Invoke();
        }

        internal void OnInteractionEnded()
        {
            OnInteractEnd?.Invoke();
        }

        /// <summary>
        /// Retrieves all interaction contexts available for this object,
        /// without binding them to a specific interactor.
        /// </summary>
        /// <returns>A list of available interaction contexts.</returns>
        public IReadOnlyList<InteractionContext> GetInteractions()
        {
            _contextCache.Clear();
            OnInteract?.Invoke();

            for (int i = 0; i < _interactionBehaviours.Count; i++)
            {
                var behaviour = _interactionBehaviours[i];
                if (behaviour.IsInteractable())
                {
                    _contextCache.Add(new InteractionContext(
                        behaviour.actionMapName,
                        behaviour.prompt,
                        behaviour.Interact
                    ));
                }
            }

            return _contextCache;
        }

        /// <summary>
        /// Retrieves all interaction contexts available for this object,
        /// binding them to a specific interactor by ID.
        /// </summary>
        /// <param name="interactorId">The ID of the interactor requesting interactions.</param>
        /// <returns>A list of available interaction contexts bound to the interactor.</returns>
        public IReadOnlyList<InteractionContext> GetInteractions(int interactorId)
        {
            _contextCache.Clear();
            OnInteract?.Invoke();

            for (int i = 0; i < _interactionBehaviours.Count; i++)
            {
                var behaviour = _interactionBehaviours[i];
                if (behaviour.IsInteractable())
                {
                    _contextCache.Add(new InteractionContext(
                        behaviour.actionMapName,
                        behaviour.prompt,
                        behaviour.Interact,
                        new InteractionArgs(interactorId, _id)
                    ));
                }
            }

            return _contextCache; // yine aynı cache
        }

        /// <summary>
        /// Hook for subclasses to add extra initialization logic during <see cref="Awake"/>.
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// Hook for subclasses to add extra logic when the object is enabled.
        /// </summary>
        protected virtual void OnEnabled() { }

        /// <summary>
        /// Hook for subclasses to add extra logic when the object is disabled.
        /// </summary>
        protected virtual void OnDisabled() { }
    }
}