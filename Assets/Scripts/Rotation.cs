using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Rotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || rb == null) return;

        float direction = 0f;
        if (keyboard.eKey.isPressed) direction += 1f;
        if (keyboard.qKey.isPressed) direction -= 1f;

        if (!Mathf.Approximately(direction, 0f))
        {
            rb.AddTorque(direction * rotationSpeed, ForceMode2D.Force);
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -rotationSpeed, rotationSpeed);
        }
    }
}
