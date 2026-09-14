using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [SerializeField] private float movSpeed = 15f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool isPlayerOne;
    [SerializeField] private bool autoDetectSide = true;

    private GameSettings settings;
    private Vector3 originalScale;
    private float currentMaxSpeed;
    private float baseScaleMultiplier = 1f;
    private float powerUpScaleMultiplier = 1f;
    private bool controlsEnabled = true;

    public float MovSpeed
    {
        get => movSpeed;
        set
        {
            movSpeed = Mathf.Max(0.1f, value);
            currentMaxSpeed = movSpeed;
        }
    }

    public bool IsPlayerOne => isPlayerOne;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        settings = GameSettings.Load();

        if (autoDetectSide) isPlayerOne = transform.position.x < 0f;

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.mass = 50f;

        movSpeed = isPlayerOne ? settings.playerOneSpeed : settings.playerTwoSpeed;
        currentMaxSpeed = movSpeed;
        SetPaddleScale(settings.playerPaddleScale);
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled) return;

        Vector2 input = ReadInput();
        rb.AddForce(input * settings.playerAcceleration * rb.mass, ForceMode2D.Force);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, currentMaxSpeed * powerUpScaleMultiplier);
        KeepInsidePlayerHalf();
    }

    private Vector2 ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;

        Vector2 input = Vector2.zero;

        if (isPlayerOne)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
        }
        else
        {
            if (keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.leftArrowKey.isPressed) input.x -= 1f;
        }

        return input.normalized;
    }

    private void KeepInsidePlayerHalf()
    {
        float half = settings.playerHalfWidth;
        float vertical = settings.verticalLimit;
        Collider2D collider = GetComponent<Collider2D>();
        float halfWidth = collider != null ? collider.bounds.extents.x : 0f;
        float halfHeight = collider != null ? collider.bounds.extents.y : 0f;

        float minX = isPlayerOne ? -half + halfWidth : halfWidth;
        float maxX = isPlayerOne ? -halfWidth : half - halfWidth;
        float minY = -vertical + halfHeight;
        float maxY = vertical - halfHeight;

        Vector2 position = rb.position;
        Vector2 target = new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY));

        if (target == position) return;

        rb.position = target;
        Vector2 velocity = rb.linearVelocity;
        if (!Mathf.Approximately(target.x, position.x)) velocity.x = 0f;
        if (!Mathf.Approximately(target.y, position.y)) velocity.y = 0f;
        rb.linearVelocity = velocity;
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        if (!enabled) rb.linearVelocity = Vector2.zero;
    }

    public void SetPaddleScale(float multiplier)
    {
        baseScaleMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
        transform.localScale = originalScale * baseScaleMultiplier * powerUpScaleMultiplier;
    }

    public void ApplyPaddleSizePowerUp(float multiplier, float duration)
    {
        CancelInvoke(nameof(ResetPaddleScalePowerUp));
        powerUpScaleMultiplier = multiplier;
        transform.localScale = originalScale * baseScaleMultiplier * powerUpScaleMultiplier;
        Invoke(nameof(ResetPaddleScalePowerUp), duration);
    }

    private void ResetPaddleScalePowerUp()
    {
        powerUpScaleMultiplier = 1f;
        transform.localScale = originalScale * baseScaleMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PowerUp powerUp = other.GetComponent<PowerUp>();
        if (powerUp != null) powerUp.Collect(this);
    }
}
