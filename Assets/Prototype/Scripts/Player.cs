using System;
using Prototype.Scripts.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Prototype.Scripts
{
    [DebugGUI]
    public class Player : MonoBehaviour
    {
        [SerializeField] private float speed = 16.0f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float minCameraZoom = 1;
        [SerializeField] private float maxCameraZoom = 10;
        [SerializeField] private float cameraZoomSmoothFactor = 0.1f;

        private Camera _camera;
        private Rigidbody2D _rigidbody;
        private GameObject _crosshair;
        
        private bool _isSprinting = false;
        private Vector2 _velocity = Vector2.zero;

        private void Start()
        {
            _camera = Camera.main;
            _rigidbody = GetComponent<Rigidbody2D>();
            _crosshair = GameObject.FindWithTag("Crosshair");
            
            var moveAction = InputSystem.actions.FindAction("Move");
            moveAction.performed += OnMoveStarted;
            moveAction.canceled += OnMoveStopped;
            
            var cameraZoomAction = InputSystem.actions.FindAction("CameraZoom");
            cameraZoomAction.performed += OnZoom;
            
            var sprintAction = InputSystem.actions.FindAction("Sprint");
            sprintAction.performed += OnSprintStarted;
            sprintAction.canceled += OnSprintStopped;
        }

        private void Update()
        {
            _camera.transform.position = transform.position;
        }

        private void FixedUpdate()
        {
            var velocity = _velocity * (_isSprinting ? sprintMultiplier : 1.0f);
            _rigidbody.linearVelocity = velocity * Time.fixedDeltaTime;
            var crosshairDirection = (_crosshair.transform.position - transform.position).normalized;
            _rigidbody.MoveRotation(Quaternion.LookRotation(crosshairDirection, transform.up));
        }

        private void OnMoveStarted(InputAction.CallbackContext act)
        {
            var movement = act.ReadValue<Vector2>();
            float mul = _isSprinting ? sprintMultiplier : 1.0f;
            _velocity = movement * speed;
        }
        
        private void OnMoveStopped(InputAction.CallbackContext act)
        {
            _velocity = Vector2.zero;
        }
        
        private void OnZoom(InputAction.CallbackContext act)
        {
            var zoom = act.ReadValue<Vector2>();
            if (zoom != Vector2.zero)
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - zoom.y * cameraZoomSmoothFactor, minCameraZoom, maxCameraZoom);
        }

        private void OnSprintStarted(InputAction.CallbackContext act)
        {
            _isSprinting = true;
        }

        private void OnSprintStopped(InputAction.CallbackContext act)
        {
            _isSprinting = false;
        }
    }
}