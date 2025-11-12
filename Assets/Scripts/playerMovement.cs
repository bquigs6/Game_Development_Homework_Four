using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class playerMovement : MonoBehaviour
{
    InputSystem_Actions controls;
    CharacterController player;
    Vector2 moveInput;
    float speed = 5f;
    float groundCheckRadius = 0.1f;
    bool isGrounded;
    public LayerMask groundMask;
    public Transform groundCheck;
    float gravity = -9.81f;
    Vector3 velocity;
    float jumpHeight = 2f;

    Vector2 lookInput;
    public float mouseSensitivity = 3f;
    float rotation = 0f;
    public Transform cameraTransform;

    public GameObject colorSpotPrefab;
    public Camera playerCamera;

    public GameObject portal1;
    public GameObject portal2;
    bool first = true;

    float teleportCooldown = 2f;
    float lastTeleport = 0f;

    int equipped = 0;
    int numWeapons = 3;
    public GameObject projectilePrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<CharacterController>();
        controls = new InputSystem_Actions();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Jump.performed += Jump;
        controls.Player.Attack.performed += Attack;

        controls.Player.Enable();

        lastTeleport = Time.time;

        portal1.gameObject.SetActive(false);
        portal2.gameObject.SetActive(false);

        controls.Player.Next.performed += ctx => equipped = (equipped + 1) % numWeapons;
        controls.Player.Previous.performed += ctx => equipped = (equipped - 1 + numWeapons) % numWeapons;
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (Time.time - lastTeleport > teleportCooldown)
        {
            lastTeleport = Time.time;
            if (collision.gameObject == portal1)
            {
                if (portal2.activeSelf)
                {
                    portal1.gameObject.SetActive(false);
                    portal2.gameObject.SetActive(false);
                    player.enabled = false;
                    transform.position = portal2.transform.position;
                    Quaternion relativeRotation = Quaternion.Inverse(portal1.transform.rotation) * transform.rotation;
                    float relativeYaw = relativeRotation.eulerAngles.y;
                    float targetYaw = portal2.transform.eulerAngles.y;
                    float finalYaw = targetYaw + relativeYaw;
                    cameraTransform.rotation = Quaternion.Euler(0, 180 + finalYaw, 0);
                    velocity = velocity.magnitude * portal2.transform.forward;
                    player.enabled = true;
                }
            }
            else if (collision.gameObject == portal2)
            {
                if (portal1.activeSelf)
                {
                    portal1.gameObject.SetActive(false);
                    portal2.gameObject.SetActive(false);
                    player.enabled = false;
                    transform.position = portal1.transform.position;
                    Quaternion relativeRotation = Quaternion.Inverse(portal2.transform.rotation) * transform.rotation;
                    float relativeYaw = relativeRotation.eulerAngles.y;
                    float targetYaw = portal1.transform.eulerAngles.y;
                    float finalYaw = targetYaw + relativeYaw;
                    cameraTransform.rotation = Quaternion.Euler(0, 180 + finalYaw, 0);
                    velocity = velocity.magnitude * portal1.transform.forward;
                    player.enabled = true;
                }
            }
        }
        if (collision.gameObject.CompareTag("NextLevel"))
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            if (currentScene < SceneManager.sceneCountInBuildSettings - 1)
            {
                SceneManager.LoadScene(currentScene + 1);
            }
        }
        if (collision.gameObject.CompareTag("Lava"))
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentScene);
        }
    }

    public void placePortal(Vector3 position, Vector3 normal)
    {
        if (first)
        {
            first = false;
            portal1.transform.position = position;
            portal1.transform.rotation = Quaternion.LookRotation(normal);                
            portal1.SetActive(true);
        }
        else
        {
            first = true;
            portal2.transform.position = position;
            portal2.transform.rotation = Quaternion.LookRotation(normal);
            portal2.SetActive(true);
        }
    }
    
    void Attack(InputAction.CallbackContext ctx)
    {
        if (equipped == 0)
        {
            return;
        }
        else if (equipped == 1)
        {
           GameObject projectile = Instantiate(projectilePrefab,
            cameraTransform.position + cameraTransform.forward * 1f,
            Quaternion.LookRotation(cameraTransform.forward));
            float projectileSpeed = 100f;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = cameraTransform.forward * projectileSpeed;
        }
        else if(equipped == 2)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                placePortal(hit.point, hit.normal);
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        player.Move(move * speed * Time.deltaTime);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        player.Move(velocity * Time.deltaTime);
        
        //Rotation
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        rotation -= mouseY;
        rotation = Mathf.Clamp(rotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(rotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
