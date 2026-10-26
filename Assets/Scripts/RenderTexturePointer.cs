using MouseButton = Noesis.MouseButton;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChartsExample
{
    [RequireComponent(typeof(NoesisView), typeof(MeshCollider))]
    public sealed class RenderTexturePointer : MonoBehaviour
    {
        private NoesisView _view;
        private Camera _camera;
        private MeshCollider _collider;
        private bool _previousMouseInput;
        private bool _pressed;

        private void OnEnable()
        {
            _view = GetComponent<NoesisView>();
            _collider = GetComponent<MeshCollider>();
            _camera = Camera.main;
            _previousMouseInput = _view.EnableMouse;

            // The SDK's OnGUI projection uses the texture height to flip screen coordinates.
            _view.EnableMouse = false;
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || _view.Texture == null || _camera == null)
            {
                return;
            }

            if (!Cursor.visible || Cursor.lockState == CursorLockMode.Locked)
            {
                ReleasePointer();
                return;
            }

            var point = ProjectPointer(mouse.position.ReadValue());
            _view.MouseMove(point.x, point.y);

            if (mouse.leftButton.wasPressedThisFrame && point.x >= 0)
            {
                _pressed = true;
                _view.MouseButtonDown(point.x, point.y, MouseButton.Left);
            }

            if (mouse.leftButton.wasReleasedThisFrame && _pressed)
            {
                _pressed = false;
                _view.MouseButtonUp(point.x, point.y, MouseButton.Left);
            }

            var wheel = Mathf.RoundToInt(mouse.scroll.ReadValue().y);
            if (wheel != 0 && point.x >= 0)
            {
                _view.MouseWheel(point.x, point.y, wheel);
            }
        }

        private void OnDisable()
        {
            ReleasePointer();
            _view.EnableMouse = _previousMouseInput;
        }

        private Vector2Int ProjectPointer(UnityEngine.Vector2 position)
        {
            var ray = _camera.ScreenPointToRay(position);
            if (!Physics.Raycast(ray, out var hit) || hit.collider != _collider)
            {
                return new Vector2Int(-1, -1);
            }

            var texture = _view.Texture;
            return new Vector2Int(
                Mathf.Clamp((int)(hit.textureCoord.x * texture.width), 0, texture.width - 1),
                Mathf.Clamp((int)((1 - hit.textureCoord.y) * texture.height), 0, texture.height - 1));
        }

        private void ReleasePointer()
        {
            _view.MouseMove(-1, -1);
            if (_pressed)
            {
                _pressed = false;
                _view.MouseButtonUp(-1, -1, MouseButton.Left);
            }
        }
    }
}
