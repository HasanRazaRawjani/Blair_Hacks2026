using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private float sensitivity = 200f;
    

    private float pitch;
    private float yaw;

    private void Start()
    {
        if (playerBody == null)
            playerBody = transform.root;

        yaw = playerBody.eulerAngles.y;

        CursorManager.Lock();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockCursor();

        if (!Cursor.lockState.Equals(CursorLockMode.Locked))
            return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    private void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}