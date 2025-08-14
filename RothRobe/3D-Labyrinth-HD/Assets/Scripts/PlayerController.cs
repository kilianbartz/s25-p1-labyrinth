using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    private CharacterController _controller;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _rotationY;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        // Bewegung im lokalen Raum des Spielers
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _controller.Move(move * (moveSpeed * Time.deltaTime));
    }

    void HandleMouseLook()
    {
        _rotationY += _lookInput.x * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, _rotationY, 0f);
    }

    // Diese Methoden werden vom PlayerInput-Component automatisch aufgerufen
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            _moveInput = context.ReadValue<Vector2>();
        else if (context.canceled)
            _moveInput = Vector2.zero;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed)
            _lookInput = context.ReadValue<Vector2>();
        else if (context.canceled)
            _lookInput = Vector2.zero;
    }
}