using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float lookSpeed = 3f;
    public float moveSpeed = 10f;
    public float scrollSpeed = 100f;
    public float shiftMultiplier = 2f;

    private Vector3 lastMousePos;
    private Vector3 defaultPos;
    private Quaternion defaultRot;

    void Start()
    {
        defaultPos = transform.position;
        defaultRot = transform.rotation;
    }

    void Update()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f);

        // === LOOK AROUND WITH RIGHT MOUSE ===
        if (Input.GetMouseButtonDown(1))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            float yaw = delta.x * lookSpeed * Time.deltaTime;
            float pitch = -delta.y * lookSpeed * Time.deltaTime;

            transform.eulerAngles += new Vector3(pitch, yaw, 0);
            lastMousePos = Input.mousePosition;
        }

        // === MOVE WITH WASD / ARROWS ===
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move -= transform.right;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move += transform.right;
        if (Input.GetKey(KeyCode.E)) move += transform.up;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;

        transform.position += move.normalized * speed * Time.deltaTime;

        // === PAN WITH MIDDLE MOUSE ===
        if (Input.GetMouseButtonDown(2))
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            Vector3 pan = -transform.right * delta.x - transform.up * delta.y;
            transform.position += pan * Time.deltaTime * lookSpeed;
            lastMousePos = Input.mousePosition;
        }

        // === ZOOM WITH SCROLL WHEEL ===
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * scrollSpeed * Time.deltaTime;

        // Reset camera on "R" key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = defaultPos;
            transform.rotation = defaultRot;
        }
    }
    
}
