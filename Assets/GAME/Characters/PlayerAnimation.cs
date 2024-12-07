using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 0.02f;
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;
    // movement
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismach");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    // actions
    private static int isGatheringHash = Animator.StringToHash("isGathering");
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    private int[] actionHashes;

    private Vector3 _currentBlendInput = Vector3.zero;
    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue = 1.0f;
    private float _walkMaxBlendValue = 0.5f;

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>(); 
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();

        actionHashes = new int[] { isGatheringHash };

    }
    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (!IsOwner) return;
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        bool isRunning = _playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
        bool isGrounded = _playerState.IsGroundedState();

        bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));

        bool isRunBlendValue = isRunning || isJumping || isFalling;
        Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue :
                             isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue : _playerLocomotionInput.MovementInput * _walkMaxBlendValue;

        _currentBlendInput =Vector3.Lerp(_currentBlendInput, inputTarget,locomotionBlendSpeed * Time.deltaTime);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);

        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isIdlingHash, isIdling);
        _animator.SetBool(isJumpingHash, isJumping);

        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);

        _animator.SetBool(isGatheringHash, _playerLocomotionInput.IsPickingItem);
        
        _animator.SetBool(isPlayingActionHash, isPlayingAction);

        if(!_playerLocomotionInput.IsPickingItem || _playerController.ItemId == 0)
        {
            if(Mathf.FloorToInt(_animator.GetCurrentAnimatorStateInfo(0).normalizedTime) > 0)
            {
                _animator.SetBool(isGatheringHash, false);
            }
            
        }
    }
}
