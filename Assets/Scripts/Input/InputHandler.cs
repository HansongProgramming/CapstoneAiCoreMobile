using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static event Action OnTap;
    private UserInput _userInput;

    private void Awake()
    {
        _userInput = new UserInput();
        _userInput.Enable();
    }
    private void Start()
    {
        _userInput.MobileTouch.Tap.performed += OnTapPerformed;
    }

    private void OnTapPerformed(InputAction.CallbackContext context)
    {
        OnTap?.Invoke();
    }

    private void OnDestroy()
    {
        _userInput.MobileTouch.Tap.performed -= OnTapPerformed;
    }

}
