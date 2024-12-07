using Cinemachine;
using Unity.Netcode;
using UnityEngine;
// https://www.youtube.com/watch?v=-PTtr3VCLOI&list=PLYvjPIZvaz-o-DIBhiHzSrrau9HKSmeEz&index=8

[DefaultExecutionOrder(-1)]
public class PlayerController : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] public Inventory inventory;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] public GameObject BuildingUI;
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    [SerializeField] private InteractionButton _interactionButton;
    private bool _isGamePaused = false;

    [Header("Components")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    [Header("Movement")]
    public float walkAcceleration = 0.15f;
    public float walkSpeeed = 3f;
    public float runAcceleration = 0.25f;
    public float runSpeed = 6f;
    public float drag = 0.1f;
    public float sprintAcceleration = 0.5f;
    public float sprintSpeed = 9f;
    public float inAirAcceleration = 0.15f;
    public float gravity = 0.1f;
    public float terminalVelocity = 50f;
    public float jumpSpeed = 1.0f;
    public float movingThreshold = 0.01f;

    [Header("Animation")]
    public float playerModelRotationSpeed = 10f;
    public float rotateToTargetTime = 0.67f;

    [Header("Camera settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    [Header("Enviromental details")]
    [SerializeField]
    public LayerMask _groundLayers;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    private bool _jumpedLastFrame = false;
    private bool _isRotatingClockwise = false;
    private float _rotationToTargetTimer = 0f;
    private float _verticalVelocity = 0f;
    private float _antiBump;
    private float _stepOffset;
    private float _currentTime;

    [Header("Camera zoom")]
    public float zoomSpeed = 10f; // Speed of zooming
    public float minFOV = 15f;    // Minimum FOV (zoomed in)
    public float maxFOV = 60f; 
    public float smoothFactor = 1.5f;

    // pickup
    public bool IsPickingItem{set;get;}
    public int ItemId { set; get; } = 0;
    public float ItemTimer { set; get; }
    public int ItemAmount { set; get; }
    public ulong objId { set; get; }

    // building
    public bool BuildingBussy { set; get; }
    public ulong BuildingNetId { set; get; }
    public int CurrentBuildingLevel { set; get; }

    private PlayerMovementState _lastMovementState = PlayerMovementState.Falling;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
    }
    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();

        _antiBump = sprintSpeed;
        _stepOffset = _controller.stepOffset;
    }
    void Start(){
        
    }
    void Update(){
        if(!IsOwner) return;
        _isGamePaused = pauseMenu.menuUI.activeSelf;
        
        UpdateMovementState();
        HandleVerticalMovement();
        Movement();
        CameraZoom();
        ListenForInventory();
        ListenForPause();
        ListenForPickupAction();
        ListenForCrafting();
    }
    void LateUpdate(){
        if(!IsOwner) return;
        if(_playerLocomotionInput.LockTheCamera && !_isGamePaused && IsOwner && PointerCanBeLocked()){
            UpdateCameraRotation();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }else if(!_playerLocomotionInput.LockTheCamera && !_isGamePaused && IsOwner){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
            
    }

    #region MOVEMENT
    private void UpdateMovementState()
    {
        _lastMovementState = _playerState.CurrentPlayerMovementState;

        bool canRun = CanRun();
        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;
        bool isWalking = (isMovingLaterally && !canRun) || _playerLocomotionInput.WalkToggleOn;
        bool isGrounded = IsGrounded();

        PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                           isSprinting ? PlayerMovementState.Sprinting : 
                                           isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;
        _playerState.SetPlayerMovementState(lateralState);

        // controll airborn
        if((!isGrounded || _jumpedLastFrame) && _controller.velocity.y >= 0f)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            _jumpedLastFrame = false;
            _controller.stepOffset = 0f;
        }else if((!isGrounded || _jumpedLastFrame) && _controller.velocity.y < 0f)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            _jumpedLastFrame = false;
            _controller.stepOffset = 0f;
        }
        else
        {
            _controller.stepOffset = _stepOffset;
        }

    }
    private bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_controller.velocity.x,0f,_controller.velocity.y);
        return lateralVelocity.magnitude > movingThreshold;
    }
    private void HandleVerticalMovement(){
        bool isGrounded = _playerState.IsGroundedState();

        _verticalVelocity -= gravity * Time.deltaTime;

        if (isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -_antiBump;

        if(_playerLocomotionInput.JumpPressed && isGrounded){
            _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
            _jumpedLastFrame = true;
        }

        if (_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded)
        {
            _verticalVelocity += _antiBump;
        }

        if(Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
        {
            _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
        }
    }
    private void Movement(){
        bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
        bool isGrounded = _playerState.IsGroundedState();
        bool isWalking = _playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                    isWalking ? walkAcceleration :
                                    isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                      isWalking ? walkSpeeed :
                                      isSprinting ? sprintSpeed : runSpeed;

        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x,0f,_playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x,0f,_playerCamera.transform.right.z).normalized;
        Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        Vector3 movementDelta = movementDirection * lateralAcceleration * Time.deltaTime;
        Vector3 newVelocity = _controller.velocity + movementDelta;

        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x,0f,newVelocity.z),clampLateralMagnitude);

        newVelocity.y += _verticalVelocity;

        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;

        _controller.Move(newVelocity * Time.deltaTime);
    }
    
    
    
    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_controller, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _controller.slopeLimit;
        if (!validAngle && _verticalVelocity < 0f)
            velocity = Vector3.ProjectOnPlane(velocity, normal);
        return velocity;
    }
    private bool IsGrounded(){
        bool grounded = _playerState.IsGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();
        return _controller.isGrounded;
    }
    private bool IsGroundedWhileAirborne()
    {

        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_controller, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _controller.slopeLimit;

        return _controller.isGrounded && validAngle;

        
    }
    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _controller.radius, transform.position.z);
        bool grounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayers, QueryTriggerInteraction.Ignore);
        return grounded;
    }
    private bool CanRun()
    {
        return _playerLocomotionInput.MovementInput.y >= Mathf.Abs(_playerLocomotionInput.MovementInput.x);
    }
    #endregion

    #region INVENTORY

    private void ListenForInventory()
    {
        if (_playerLocomotionInput.InventoryToggle)
        {

            inventory.InventoryUI.SetActive(!inventory.InventoryUI.activeSelf);
            inventory.UpdateUI();
        }

    }

    #endregion

    #region PAUSE_MENU
    private void ListenForPause()
    {
        if (_playerLocomotionInput.PauseToggle)
        {
            if (_isGamePaused)
            {
                pauseMenu.menuUI.SetActive(false);
                PlayerUI.SetActive(true);
            }
            else
            {
                pauseMenu.menuUI.SetActive(true);
                PlayerUI.SetActive(false);
            }
        }
    }
    #endregion

    #region CAMERA
    private void CameraZoom()
    {
        //print($"ZOOM: {_playerLocomotionInput.CameraZoon}");
        float scrollInput = _playerLocomotionInput.CameraZoon;
        // Get the current camera lens
        var lens = _virtualCamera.m_Lens;

        if (!lens.Orthographic)
        {
            // Adjust the Field of View for perspective cameras
            lens.FieldOfView -= scrollInput * zoomSpeed;
            lens.FieldOfView = Mathf.Clamp(lens.FieldOfView, minFOV, maxFOV);
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, lens.FieldOfView - scrollInput * zoomSpeed, Time.deltaTime * smoothFactor);
        }
        else
        {
            // Adjust Orthographic Size for orthographic cameras
            lens.OrthographicSize -= scrollInput * zoomSpeed;
            lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize, minFOV, maxFOV);
        }

        // Apply the lens changes back to the camera
        _virtualCamera.m_Lens = lens;
    }
    private void UpdateCameraRotation()
    {
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LoockInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LoockInput.y, -lookLimitV, lookLimitV);

        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LoockInput.x;

        float rotationTolerance = 90f;
        bool isIdling = _playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
        IsRotatingToTarget = _rotationToTargetTimer > 0;

        if (!isIdling)
        {
            RotatePlayerToTarget();

        }
        else if(Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
        {
            UpdateIdleRotation(rotationTolerance);
        }

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

        Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);

    }
    private void UpdateIdleRotation(float rotationTolerance)
    {
        
        if (Mathf.Abs(RotationMismatch) > rotationTolerance)
        {
            _rotationToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }
        _rotationToTargetTimer -= Time.deltaTime;
        if (_isRotatingClockwise && RotationMismatch > 0f || !_isRotatingClockwise && RotationMismatch < 0f)
        {
            RotatePlayerToTarget();
        }
        
    }

    private void RotatePlayerToTarget()
    {
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }
    #endregion
    
    #region BUILDING

    private void ListenForCrafting(){
        if(_playerLocomotionInput.CraftingToggle){
            if(BuildingUI != null)
            {
                BuildingUI.SetActive(!BuildingUI.activeSelf);
                if(BuildingNetId != 0)
                {
                    BuildingUI.gameObject.GetComponentInParent<BuildingUI>().InitSystem(BuildingNetId, BuildingBussy, CurrentBuildingLevel);
                }
                
            }
        }
    }

    // move to building UI
    
    #endregion

    #region PICKUP_COLECTABLES

    public void PickupFromGathering(ItemSO item,int amount){
        inventory.AddItem(item,amount);
    }
    private void ListenForPickupAction(){
        
        if (ItemId != 0)
        {
            _interactionButton.ShowUI("Hold to gather", "E");
            if (_playerLocomotionInput.IsPickingItem)
            {
                _interactionButton.LoadinUI();
                IsPickingItem = true;
                Pickup();
            }
            else if(ItemId != 0)
            {
                _interactionButton.ShowUI("Hold to gather", "E");
                _currentTime = 0f;
                
            }
            else
            {
                IsPickingItem = false;
                _interactionButton.HideUI();
            }
        }
        else
        {
            _interactionButton.HideUI();
        }

    }

    private void Pickup()
    {
        _currentTime += Time.deltaTime;
        _currentTime = Mathf.Clamp(_currentTime, 0f, ItemTimer);

        _interactionButton.UpdateData(_currentTime, ItemTimer);


        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objId, out var networkObject))
        {
            GameObject targetObject = networkObject.gameObject;
            targetObject.GetComponent<Gatherable>().FlagPickingUpServerRpc(IsPickingItem);

        }

        if (_currentTime >= ItemTimer)
        {

            PickupFromGathering(GetItemFromDB(ItemId), ItemAmount);

            _currentTime = 0f;

            GameObject targetObject = networkObject.gameObject;
            targetObject.GetComponent<Gatherable>().Despawn();
            ItemId = 0;
            IsPickingItem = false;
        }
    }

    private ItemSO GetItemFromDB(int itemId)
    {
        return ItemDatabase.Instance.ItemsDB[itemId];
    }

    #endregion

    #region DETECT_POINTER_OVER_INVENTORY
    private bool PointerCanBeLocked()
    {
        if(AllowPointerLock.Instance != null){
            return AllowPointerLock.Instance.AllowToLockThePointer;
        }
        return false;
    }
    #endregion

}
