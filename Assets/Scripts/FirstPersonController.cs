using UnityEngine;
using UnityEngine.InputSystem;

namespace ChartsExample
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        public Transform CameraTransform;
        public float MoveSpeed = 3;
        public float LookSensitivity = 0.1f;

        private CharacterController _controller;
        private float _pitch;
        private float _verticalVelocity;
        private bool _interacting;
        private GUIStyle _hintStyle;

        public bool IsInteracting => _interacting;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _pitch = CameraTransform.localEulerAngles.x;
            if (_pitch > 180)
            {
                _pitch -= 360;
            }
        }

        private void Start()
        {
            SetInteraction(false);
        }

        private void Update()
        {
            if (!Application.isPlaying || _controller == null || CameraTransform == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null)
            {
                return;
            }

            if (keyboard.eKey.wasPressedThisFrame && !_interacting)
            {
                SetInteraction(true);
            }

            if (keyboard.escapeKey.wasPressedThisFrame && _interacting)
            {
                SetInteraction(false);
            }

            if (_interacting)
            {
                return;
            }

            var look = mouse.delta.ReadValue() * LookSensitivity;
            transform.Rotate(0, look.x, 0);
            _pitch = Mathf.Clamp(_pitch - look.y, -75, 75);
            CameraTransform.localEulerAngles = new Vector3(_pitch, 0, 0);

            var horizontal = (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            var vertical = (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0);
            var direction = Vector3.ClampMagnitude(transform.right * horizontal + transform.forward * vertical, 1);
            _verticalVelocity = _controller.isGrounded ? -2 : _verticalVelocity + Physics.gravity.y * Time.deltaTime;
            _controller.Move((direction * MoveSpeed + Vector3.up * _verticalVelocity) * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (!_interacting)
            {
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void SetInteraction(bool interacting)
        {
            _interacting = interacting;
            Cursor.lockState = interacting ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = interacting;
        }

        private void OnGUI()
        {
            if (_hintStyle == null)
            {
                _hintStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            var message = _interacting
                ? "Click the monitor controls  •  Esc: return to walking"
                : "WASD: walk  •  Mouse: look  •  E: use monitor";
            GUI.Box(new Rect((Screen.width - 720) / 2, Screen.height - 60, 720, 40), message, _hintStyle);
        }
    }
}
