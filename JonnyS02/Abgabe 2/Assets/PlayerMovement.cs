using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public Light flashLight;
    public bool hasFlashlight = false;

    // Beim Laufen und Gehen wackelt die Kamera auf und ab
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;
    public float runBobSpeedMultiplier = 1.6f;   
    public float runBobAmountMultiplier = 1.6f;  
    private float bobTimer = 0f;
    private float defaultCameraY;

    public AudioClip footstepSound;
    public float footstepInterval = 0.5f; 
    public float walkFootstepPitch = 1f;
    public float runFootstepPitch = 1.35f;
    private AudioSource audioSource;

    private bool isLightOn = false;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private float currentWalkSpeed;
    private float currentRunSpeed;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentWalkSpeed = walkSpeed;
        currentRunSpeed = runSpeed;

        defaultCameraY = playerCamera.transform.localPosition.y;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = footstepSound;
        audioSource.loop = true;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right   = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = canMove ? (isRunning ? currentRunSpeed : currentWalkSpeed) * Input.GetAxis("Vertical")   : 0;
        float curSpeedY = canMove ? (isRunning ? currentRunSpeed : currentWalkSpeed) * Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;         
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Springen (Leertaste)
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        // Kriechen (C)
        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            currentWalkSpeed = crouchSpeed;
            currentRunSpeed  = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            currentWalkSpeed = walkSpeed;
            currentRunSpeed  = runSpeed;
        }

        // Schwerkraft anwenden
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Taschenlampe (F)
        if (Input.GetKeyDown(KeyCode.F) && hasFlashlight)
        {
            isLightOn = !isLightOn;
            flashLight.enabled = isLightOn;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Kamera Rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX  = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Kamera Wackeln (Bob)
        float bobSpeedCurrent  = isRunning ? bobSpeed  * runBobSpeedMultiplier  : bobSpeed;
        float bobAmountCurrent = isRunning ? bobAmount * runBobAmountMultiplier : bobAmount;

        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeedCurrent;
            float newY  = defaultCameraY + Mathf.Sin(bobTimer) * bobAmountCurrent;

            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = newY;
            playerCamera.transform.localPosition = camPos;
        }
        else
        {
            // Zurücksetzen, wenn nicht am Boden
            Vector3 camPos = playerCamera.transform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, defaultCameraY, Time.deltaTime * bobSpeed);
            playerCamera.transform.localPosition = camPos;
            bobTimer = 0f;
        }

        // Fußstapfen Sound
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            audioSource.pitch = isRunning ? runFootstepPitch : walkFootstepPitch;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PickupFlashlight()
    {
        hasFlashlight = true;
        isLightOn = !isLightOn;
        flashLight.enabled = isLightOn;
    }
}
