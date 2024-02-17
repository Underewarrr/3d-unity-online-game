using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Enumera��o para os diferentes estilos de c�mera
    public enum CameraStyle
    {
        Standard,
        Isometric,
        TopDown,
    }

    // Configura��es de movimento
    [Header("Movement Settings")]
    [SerializeField] private float walkingSpeed = 7.5f;
    [SerializeField] private float runningSpeed = 11.5f;
    [SerializeField] private float jumpSpeed = 8.0f;
    [SerializeField] private float gravity = 20.0f;

    // Configura��es de rota��o
    [Header("Look Settings")]
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 45.0f;

    // Configura��es da c�mera
    [Header("Camera Settings")]
    [SerializeField] private float cameraYOffset = 0.4f;
    [SerializeField] private CameraStyle cameraStyle = CameraStyle.Standard;

    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool canMove = true;

    private void Start()
    {
        InitializeComponents();
        LockCursor();
        SetCameraStyle(cameraStyle); // Configura o estilo da c�mera ao iniciar
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    // Inicializa os componentes necess�rios
    private void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;

        // Configura a posi��o inicial da c�mera
        if (playerCamera != null)
        {
            playerCamera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraYOffset, transform.position.z);
            playerCamera.transform.SetParent(transform);
        }
    }

    // Trava o cursor do mouse
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Lida com o movimento do jogador
    private void HandleMovement()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runningSpeed : walkingSpeed;

        float moveX = canMove ? Input.GetAxis("Vertical") * speed : 0;
        float moveY = canMove ? Input.GetAxis("Horizontal") * speed : 0;

        moveDirection = transform.TransformDirection(Vector3.forward) * moveX + transform.TransformDirection(Vector3.right) * moveY;

        // Aplica a gravidade ao movimento
        if (canMove && characterController.isGrounded && Input.GetButton("Jump"))
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    // Lida com a rota��o do jogador e da c�mera
    private void HandleRotation()
    {
        if (!canMove || playerCamera == null) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    // Define o estilo da c�mera
    private void SetCameraStyle(CameraStyle style)
    {
        switch (style)
        {
            case CameraStyle.Standard:
                // Implemente a l�gica para o estilo de c�mera padr�o, se necess�rio
                break;
            case CameraStyle.TopDown:
                // Implemente a l�gica para o estilo de c�mera de cima para baixo, se necess�rio
                break;
            case CameraStyle.Isometric:
                // Posiciona a c�mera atr�s do personagem e olhando para o mesmo
                if (playerCamera != null)
                {
                    Vector3 offset = -transform.forward * 2 + Vector3.up * cameraYOffset;
                    playerCamera.transform.position = transform.position + offset;
                    playerCamera.transform.LookAt(transform.position + Vector3.up * 0.5f);
                }
                break;
            default:
                Debug.LogError("Estilo de c�mera n�o reconhecido: " + style);
                break;
        }
    }
}
