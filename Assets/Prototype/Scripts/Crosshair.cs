using UnityEngine;
using UnityEngine.InputSystem;

namespace Prototype.Scripts
{
    public class Crosshair : MonoBehaviour
    {
        private InputAction _lookAction;
        
        private Camera _camera;
        
        void Start()
        {
            _lookAction = InputSystem.actions.FindAction("Look");
            _camera = Camera.main;
            transform.position = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }

        void Update()
        {
            var ds = _lookAction.ReadValue<Vector2>();
            transform.position = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
    }
}
