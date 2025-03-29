using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InputSystem.IPlayerActions
{
    private InputSystem controls;
    public Vector2 MoveComposite;
    public Vector2 LookComposite;
    public event Action OnJumpPerformed;
    public event Action OnInteractPerformed;
    public bool cursorIsLocked = false;
    public bool isSprinting = false;
    public bool isBlocking = false;
    public Action OnAttackPerformed;
    public Action OnDashPerformed;
    public Action OnInteract2Performed;
    public bool isAiming = false;

    void OnEnable() {
        if (controls!=null) {
            return;
        }
        controls = new InputSystem();
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();

    }

    void OnDisable() {
        controls.Player.Disable();
    }
    void InputSystem.IPlayerActions.OnMove(InputAction.CallbackContext context)
    {
        MoveComposite = context.ReadValue<Vector2>();
    }
    void InputSystem.IPlayerActions.OnLook(InputAction.CallbackContext context)
    {
        LookComposite = context.ReadValue<Vector2>();
    }
    void InputSystem.IPlayerActions.OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Trigger only once per press
        OnJumpPerformed?.Invoke();
    }
    void InputSystem.IPlayerActions.OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Trigger only once per press
        OnAttackPerformed?.Invoke();
    }

    void InputSystem.IPlayerActions.OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Trigger only once per press
        OnDashPerformed?.Invoke();
    }


    void InputSystem.IPlayerActions.OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Trigger only once per press
        OnInteractPerformed?.Invoke();
    }
    void InputSystem.IPlayerActions.OnInteract2(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Trigger only once per press
        OnInteract2Performed?.Invoke();
    }
    void InputSystem.IPlayerActions.OnCrouch(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    void InputSystem.IPlayerActions.OnPrevious(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    void InputSystem.IPlayerActions.OnNext(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    void InputSystem.IPlayerActions.OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }
    void InputSystem.IPlayerActions.OnCursorToggle(InputAction.CallbackContext context)
    {
        cursorIsLocked = !cursorIsLocked;
    }

    public void ResetInput()
    {
        controls?.Player.Disable();
        controls = new InputSystem();
        controls.Player.SetCallbacks(this);
        controls.Player.Enable();
    }
    void InputSystem.IPlayerActions.OnBlock(InputAction.CallbackContext context)
    {
        isBlocking = context.ReadValueAsButton();
    }
    void InputSystem.IPlayerActions.OnAim(InputAction.CallbackContext context)
    {
        isAiming = context.ReadValueAsButton();
    }
    // Optional: Add these if you need more granular control
    public void DisableInput()
    {
        controls?.Player.Disable();
    }

    public void EnableInput()
    {
        controls?.Player.Enable();
    }
    
}
