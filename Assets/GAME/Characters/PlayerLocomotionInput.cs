using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
[DefaultExecutionOrder(-2)]
public class PlayerLocomotionInput : NetworkBehaviour,PlayerInput.IPlayerActions
{
    [SerializeField] private bool holdToSprint = true;
    public bool SprintToggledOn {get;private set;}
    public PlayerInput PlayerInput {get;private set;}
    public Vector2 MovementInput {get;private set;}
    public Vector2 LoockInput {get;private set;}
    public bool JumpPressed {get;private set;}
    public bool LockTheCamera {get;private set;}
    public float CameraZoon {get;private set;}
    public bool InventoryToggle {get;private set;} = false;
    public bool PauseToggle {get;private set;} = false;

    public bool IsPickingItem {get;private set;} = false;
    public bool CraftingToggle {get;private set;}
    public bool WalkToggleOn { get; private set;}

    void OnEnable(){
        //if(!IsOwner) return;
        PlayerInput = new PlayerInput();
        PlayerInput.Enable();

        PlayerInput.Player.Enable();
        PlayerInput.Player.AddCallbacks(this);
    }
    void OnDisable(){
        //if(!IsOwner) return;
        PlayerInput.Disable();
        PlayerInput.Player.RemoveCallbacks(this);
    }
    private void LateUpdate(){
        //if(!IsOwner) return;
        JumpPressed = false;
        InventoryToggle = false;
        PauseToggle = false;
        CraftingToggle = false;
    }
    public void OnInventory(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
        InventoryToggle = true;
        
    }

    public void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;

        JumpPressed = true;
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        MovementInput = context.ReadValue<Vector2>();
        //Debug.Log(MovementInput);
    }

    public void OnPause(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
        PauseToggle = true;
    }

    public void OnRun(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(context.performed){
            SprintToggledOn = holdToSprint || !SprintToggledOn;
        }else if(context.canceled){
            SprintToggledOn = !holdToSprint && SprintToggledOn;
        }
    }

    public void OnLock(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        LoockInput = context.ReadValue<Vector2>();
    }

    public void OnCameraLock(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(context.performed)
            LockTheCamera = true;
        else
            LockTheCamera = false;
    }


    public void OnZoom(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(context.performed){
            CameraZoon = context.ReadValue<float>();
            //print(CameraZoon);
        }
            
        else
            CameraZoon = 0f;
    }

    public void OnColect(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        context.action.started += ctx => IsPickingItem = true;
        context.action.canceled += ctx => IsPickingItem = false;
    }

    public void OnCraft(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(!context.performed)
            return;
        CraftingToggle = true;
    }

    public void OnWalk(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        WalkToggleOn = !WalkToggleOn;
    }
}
