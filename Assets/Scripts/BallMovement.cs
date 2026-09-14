using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameSettings settings;
    private float currentSpeed;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        settings = GameSettings.Load();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.freezeRotation = false;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        currentSpeed = settings.initialBallSpeed;
    }

    private void FixedUpdate()
    {
        float spin = settings != null ? settings.ballRotationSpeed : 25f;
        rb.angularVelocity = -rb.linearVelocity.x * spin;
    }

    public void LaunchRandom()
    {
        float horizontal = Random.value < 0.5f ? -1f : 1f;
        float vertical = Random.Range(-settings.maximumVerticalDirection, settings.maximumVerticalDirection);
        Launch(new Vector2(horizontal, vertical).normalized);
    }

    public void Launch(Vector2 direction)
    {
        currentSpeed = settings.initialBallSpeed;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * currentSpeed, ForceMode2D.Impulse);
    }

    public void ResetBall(Vector2 position)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = position;
        currentSpeed = settings.initialBallSpeed;
    }

    public void IncreaseSpeed()
    {
        currentSpeed = Mathf.Min(currentSpeed + settings.ballSpeedIncreasePerHit, settings.maxBallSpeed);
        SetVelocity(rb.linearVelocity.normalized * currentSpeed);
    }

    public void MultiplySpeed(float multiplier)
    {
        currentSpeed = Mathf.Min(currentSpeed * multiplier, settings.maxBallSpeed);
        SetVelocity(rb.linearVelocity.normalized * currentSpeed);
    }

    private void SetVelocity(Vector2 velocity)
    {
        if (velocity.sqrMagnitude > 0.001f) rb.linearVelocity = velocity;
    }

    private void BounceFromPaddle(Movement paddle)
    {
        IncreaseSpeed();

        Collider2D paddleCollider = paddle.GetComponent<Collider2D>();
        float height = paddleCollider != null ? paddleCollider.bounds.extents.y : 1f;
        float offset = height > 0f ? (transform.position.y - paddle.transform.position.y) / height : 0f;
        float horizontal = transform.position.x >= paddle.transform.position.x ? 1f : -1f;
        Vector2 direction = new Vector2(horizontal, Mathf.Clamp(offset, -0.85f, 0.85f)).normalized;
        rb.linearVelocity = direction * currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Movement paddle = other.GetComponent<Movement>();
        if (paddle != null) BounceFromPaddle(paddle);
    }
}
