using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Koala.Simulation.Input
{
    /// <summary>
    /// Provides a service for managing input bindings and broadcasting input events.
    /// </summary>
    public sealed class InputService
    {
        /// <summary>
        /// Triggered whenever an input action is received.
        /// </summary>
        public static event Action<InputAction.CallbackContext> OnInputReceive;

        private static Dictionary<string, string> _bindings = new();
        private static InputSpriteAsset _inputSpriteAsset;

        /// <summary>
        /// Creates a new input service and initializes input bindings from the given player input.
        /// </summary>
        /// <param name="playerInput">The <see cref="PlayerInput"/> used to set up bindings.</param>
        public InputService(PlayerInput playerInput, InputSpriteAsset inputSpriteAsset)
        {
            SetInputMap(playerInput);
            _inputSpriteAsset = inputSpriteAsset;
        }

        internal void SetInputMap(PlayerInput playerInput)
        {
            _bindings = new Dictionary<string, string>();
            _bindings.Clear();

            var map = playerInput.currentActionMap;
            if (map == null)
                return;

            foreach (var action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || binding.isPartOfComposite)
                        continue;

                    var display = action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                    _bindings[action.name] = display;
                    break;
                }
            }
        }

        internal void OnAnyAction(InputAction.CallbackContext ctx)
        {
            OnInputReceive?.Invoke(ctx);
        }

        /// <summary>
        /// Gets the display key string for the specified action name.
        /// </summary>
        /// <param name="actionName">The name of the input action.</param>
        /// <returns>The display string of the key if found, otherwise "N/A".</returns>
        public static string GetKeyByActionName(string actionName)
        {
            return _bindings.TryGetValue(actionName, out var display) ? display : "N/A";
        }

        /// <summary>
        /// Tries to get the sprite for the specified action name.
        /// </summary>
        /// <param name="actionName">The action name to get the sprite for.</param>
        /// <param name="sprite">The sprite if found, otherwise null.</param>
        /// <returns>True if the sprite is found, otherwise false.</returns>
        public static bool TryGetSprite(string actionName, out Sprite sprite)
        {
            return _inputSpriteAsset.TryGetSprite(actionName, out sprite);
        }
    }
}