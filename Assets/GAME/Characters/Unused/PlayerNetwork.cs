using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Baracuda.Monitoring;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
// source https://www.youtube.com/watch?v=3yuBOB3VrCk&t=2s
public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] public Transform spawnNetworkObject;
    [SerializeField] private CinemachineFreeLook vc;
    public KeyCode rotateKey = KeyCode.Mouse1; // Key to enable rotation

    public float xAxisSensitivity = 300f; // Sensitivity for horizontal rotation
    public float yAxisSensitivity = 2f;   // Sensitivity for vertical rotation

    [SerializeField] private AudioListener listener;

    public Inventory inventory;
    public CharacterController controller;
    public Transform cam;
    public float speed = 6f;
    public float runSpeed = 10f;
    public float runAcceleration = 0.25f;
    private float currentSpeed;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // jump
    public float jumpHeight = 2f;      // Height of the jump
    public float gravity = -9.81f;     // Gravity force
    public Transform groundCheck;      // Transform for ground detection
    public float groundDistance = 0.4f; // Radius for ground check
    public LayerMask groundMask;  
    private Vector3 velocity;          // Tracks vertical velocity
    private bool isGrounded; 

    // zoom
    public float zoomSpeed = 10f;             // Speed of zooming
    public float minZoom = 2f;                // Minimum zoom distance
    public float maxZoom = 15f;               // Maximum zoom distance

    private float currentZoomLevel;

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    /*
    public override void OnNetworkSpawn(){
        randomNumber.OnValueChanged += (int prevValue, int newValue) =>{
            Debug.Log($"{OwnerClientId} randomNumber: {randomNumber.Value}");
        };

        if(IsOwner){
            listener.enabled = true;
            vc.Priority = 1;
            inventory = FindAnyObjectByType<Inventory>();
        }else{
            vc.Priority = 0;
        }
    }
    */
    void Awake(){
        if(!IsOwner) return;
        cam = FindAnyObjectByType<Camera>().transform;
        
    }
    void Start(){
        if(!IsOwner) return;
        if (vc != null)
        {
            // Disable the built-in input by clearing input axis names
            vc.m_XAxis.m_InputAxisName = ""; // Clear X-axis input
            vc.m_YAxis.m_InputAxisName = ""; // Clear Y-axis input
            currentZoomLevel = vc.m_Orbits[1].m_Radius;
        }
    }
    
    void Update()
    {
        
        if(!IsOwner) return;
        //PlayerMovement();
        //PlayerJump();
        //CameraMode();
        //ZoomInOut();
    }
    
    private void TestKeys(){
        if(Input.GetKeyDown(KeyCode.R)){
            randomNumber.Value = Random.Range(0,100);
        }

        if(Input.GetKeyDown(KeyCode.F)){
            // to spawn bullets - ServerRpc to spawn bulet and send it
            Transform spawnedObj = Instantiate(spawnNetworkObject);
            spawnedObj.GetComponent<NetworkObject>().Spawn(true);
        }
    }
    

    private void PlayerMovement(){
        if(controller.enabled == false) return;
        float horizontal = Input.GetAxisRaw("Horizontal");
        float verical = Input.GetAxisRaw("Vertical");

        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : speed;

        Vector3 direction = new Vector3(horizontal,0f,verical).normalized;

        if(direction.magnitude >= 0.1f){
            float targetAngle = Mathf.Atan2(direction.x,direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y,targetAngle,ref turnSmoothVelocity,turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f,angle,0f);
            Vector3 moveDirection = Quaternion.Euler(0f,targetAngle,0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }

    } 
    private void PlayerJump(){
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Reset vertical velocity when grounded
        }
        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    private void CameraMode(){
        if (vc != null && Input.GetKey(rotateKey))
        {
            Cursor.lockState = CursorLockMode.Locked; // Locks the cursor to the center of the screen
            Cursor.visible = false;
            // Provide custom input for the X and Y axes
            float mouseX = Input.GetAxis("Mouse X") * xAxisSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * yAxisSensitivity * Time.deltaTime;

            vc.m_XAxis.Value += mouseX;
            vc.m_YAxis.Value -= mouseY; // Inverted for proper camera movement
        }else{
            Cursor.lockState = CursorLockMode.None; // Unlocks the cursor
            Cursor.visible = true;
        }
    }
    private void ZoomInOut(){
        if (vc != null)
        {
            // Get scroll input
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Adjust zoom level
            currentZoomLevel -= scrollInput * zoomSpeed;
            currentZoomLevel = Mathf.Clamp(currentZoomLevel, minZoom, maxZoom);

            // Update orbit radii based on the current zoom level
            vc.m_Orbits[0].m_Radius = currentZoomLevel * 0.5f; // Top Rig
            vc.m_Orbits[1].m_Radius = currentZoomLevel;       // Middle Rig
            vc.m_Orbits[2].m_Radius = currentZoomLevel * 1.5f; // Bottom Rig
        }
    }
    public void EnablePlayer(){
        if(IsOwner){
            controller.enabled = true;
            vc.enabled = true;
        } 
    }
    public void DisablePlayer(){
        if(IsOwner){
            controller.enabled = false;
            vc.enabled = false;
        } 
    }
    [ServerRpc]
    private void TestServerRpc(){
        // send message to server to run this function
    }

    [ClientRpc]
    private void TestClientRpc(){
        // call from the server to run over the all clients
    }

}
