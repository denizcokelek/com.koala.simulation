using System;
using System.Collections.Generic;
using Koala.Simulation.Interaction.Core;
using UnityEngine;

namespace Koala.Simulation.Interaction.Components
{
    /// <summary>
    /// Casts a ray from a camera to detect nearby interactable objects and forwards them to the solver.
    /// </summary>
    [AddComponentMenu("Koala/Simulation/Interaction Raycaster")]
    [Icon("Packages/com.koala.simulation/Editor/Icons/koala.png")]
    public sealed class InteractionRaycaster : Interactor
    {
        /// <summary>
        /// The camera used for raycasting. Defaults to <c>Camera.main</c> if not set.
        /// </summary>
        [Tooltip("Defaults to Camera.main if empty.")]
        [SerializeField] private Camera _cam;

        /// <summary>
        /// Maximum distance to search for interactable objects.
        /// </summary>
        [SerializeField] private float _range = 3f;

        /// <summary>
        /// Layers considered for interaction raycasts.
        /// </summary>
        [SerializeField] private LayerMask _interactionLayerMask;

        /// <summary>
        /// Maximum number of interactables to detect per scan.
        /// </summary>
        [SerializeField] private int _maxInteractions = 3;

        /// <summary>
        /// Interval between interaction scans in seconds. Zero means scan every frame.
        /// </summary>
        [Tooltip("How often to scan for interactions (seconds). 0 = every frame. 1 = every second.")]
        [SerializeField] private float _scanInterval;

        private readonly List<InteractableObject> _interactableObjects = new();
        private float _lastScanTime;

        /// <summary>
        /// Ensures the raycasting camera is assigned, defaulting to <c>Camera.main</c> if missing.
        /// </summary>
        protected override void OnAwake()
        {
            if (_cam == null)
                _cam = Camera.main;
        }

        private void Update()
        {
            if (_scanInterval > 0f && Time.time - _lastScanTime < _scanInterval)
                return;

            _interactableObjects.Clear();

            var ray = new Ray(_cam.transform.position, _cam.transform.forward);
            var hits = Physics.RaycastAll(ray, _range, _interactionLayerMask);

            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            int max = Mathf.Min(hits.Length, _maxInteractions);
            for (int i = 0; i < max; i++)
            {
                var hit = hits[i];
                if (hit.collider.TryGetComponent<InteractableObject>(out var interactable))
                {
                    _interactableObjects.Add(interactable);
                }
            }

            if (_interactableObjects.Count == 0)
                Solver.Reset();
            else
                Solver.Solve(_interactableObjects);
        }
    }
}