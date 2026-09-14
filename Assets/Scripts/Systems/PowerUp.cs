using UnityEngine;

public enum PowerUpType
{
    BallSpeed,
    PaddleSize
}

public class PowerUp : MonoBehaviour
{
    private PowerUpType type;
    private float lifetime;
    private float duration;
    private float ballSpeedMultiplier;
    private float paddleScaleMultiplier;
    private float elapsed;
    private ArenaSpawner owner;
    private SpriteRenderer spriteRenderer;

    public void Initialize(ArenaSpawner spawner, PowerUpType powerUpType, GameSettings settings, Sprite sprite)
    {
        owner = spawner;
        type = powerUpType;
        lifetime = settings.powerUpLifetime;
        duration = settings.powerUpDuration;
        ballSpeedMultiplier = settings.powerUpBallSpeedMultiplier;
        paddleScaleMultiplier = settings.powerUpPaddleScaleMultiplier;
        elapsed = 0f;
        spriteRenderer ??= GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
        }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= lifetime) owner.ReleasePowerUp(gameObject);
    }

    public void Collect(Movement player)
    {
        BallMovement ball = FindFirstObjectByType<BallMovement>();
        if (type == PowerUpType.BallSpeed) ball?.MultiplySpeed(ballSpeedMultiplier);
        else player.ApplyPaddleSizePowerUp(paddleScaleMultiplier, duration);
        owner.ReleasePowerUp(gameObject);
    }
}
