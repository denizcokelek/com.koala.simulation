using UnityEngine;
using UnityEngine.InputSystem;

namespace Koala.Simulation.Input
{
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("Koala/Simulation/Input Handler")]
    [Icon("Packages/com.koala.simulation/Editor/Icons/koala.png")]
    public sealed class InputHandler : MonoBehaviour
    {
        [SerializeField] private InputSpriteAsset _inputSpriteAsset;
        [SerializeField] private bool _debug;
        private PlayerInput _playerInput;
        private InputService _inputService;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            _playerInput.onActionTriggered += OnAnyAction;
        }

        private void OnDisable()
        {
            _playerInput.onActionTriggered -= OnAnyAction;
        }
        
        private void Start()
        {
            _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            _playerInput.SwitchCurrentActionMap(_playerInput.defaultActionMap);
            _inputService = new InputService(_playerInput, _inputSpriteAsset);
        }

        private void OnAnyAction(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (_debug)
                    Debug.Log($"Action: {ctx.action.name} | Value={ctx.ReadValueAsObject()}");
                
                _inputService.OnAnyAction(ctx);
            }
        }

        internal string GetBindingDisplay(string actionName)
        {
            var action = _playerInput.currentActionMap.FindAction(actionName);
            if (action == null)
                return $"Action '{actionName}' not found in map '{_playerInput.currentActionMap.name}'";

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    return action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontIncludeInteractions);
                }
            }

            return "No binding found";
        }
    }
}