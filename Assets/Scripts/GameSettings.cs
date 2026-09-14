using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Pong/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Players")]
    [Min(0.1f)] public float playerOneSpeed = 15f;
    [Min(0.1f)] public float playerTwoSpeed = 15f;
    [Min(0f)] public float playerAcceleration = 100f;
    [Range(0.5f, 2f)] public float playerPaddleScale = 1f;

    [Header("Ball")]
    [Min(0.1f)] public float initialBallSpeed = 6f;
    [Min(0f)] public float ballSpeedIncreasePerHit = 0.75f;
    [Min(0.1f)] public float maxBallSpeed = 14f;
    [Range(0.05f, 0.95f)] public float minimumHorizontalDirection = 0.35f;
    [Range(0f, 1f)] public float maximumVerticalDirection = 0.85f;
    [Min(0f)] public float ballRotationSpeed = 25f;

    [Header("Match")]
    [Range(1, 5)] public int pointsToWin = 3;
    [Min(1f)] public float goalTimeLimit = 20f;
    [Min(0.1f)] public float goalX = 8.9f;
    [Min(0.1f)] public float verticalLimit = 4.15f;
    [Min(0.1f)] public float playerHalfWidth = 8.25f;

    [Header("Round")]
    [Min(0f)] public float roundStartDelay = 0.75f;
    [Min(0f)] public float roundResetDelay = 0.8f;

    [Header("Advanced - Obstacles")]
    public bool enableObstacles = true;
    [Min(1f)] public float obstacleSpawnMin = 4f;
    [Min(1f)] public float obstacleSpawnMax = 7f;
    [Min(1f)] public float obstacleLifetimeMin = 3f;
    [Min(1f)] public float obstacleLifetimeMax = 7f;
    [Min(0)] public int obstaclePoolSize = 5;

    [Header("Advanced - Power Ups")]
    public bool enablePowerUps = true;
    [Min(2f)] public float powerUpSpawnMin = 6f;
    [Min(2f)] public float powerUpSpawnMax = 10f;
    [Min(0)] public int powerUpPoolSize = 4;
    [Min(1f)] public float powerUpLifetime = 8f;
    [Min(1f)] public float powerUpDuration = 5f;
    [Min(0.1f)] public float powerUpBallSpeedMultiplier = 1.35f;
    [Min(0.1f)] public float powerUpPaddleScaleMultiplier = 1.35f;

    public static GameSettings Load() => GameSettingsLoader.Load();
}
