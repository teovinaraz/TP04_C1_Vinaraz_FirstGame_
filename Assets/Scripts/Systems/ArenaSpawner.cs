using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{
    private GameSettings settings;
    private ObjectPool obstaclePool;
    private ObjectPool powerUpPool;
    private readonly Dictionary<GameObject, Coroutine> obstacleLifetimes = new Dictionary<GameObject, Coroutine>();
    private readonly Dictionary<GameObject, Coroutine> powerUpLifetimes = new Dictionary<GameObject, Coroutine>();
    private Sprite obstacleSprite;
    private Sprite speedSprite;
    private Sprite sizeSprite;

    public void Initialize(GameSettings gameSettings)
    {
        settings = gameSettings;
        obstacleSprite = Resources.Load<Sprite>("ObstacleRock");
        speedSprite = Resources.Load<Sprite>("PowerUpSpeed");
        sizeSprite = Resources.Load<Sprite>("PowerUpSize");
        CreateCenterDivider();
        CreateBoundaryVisuals();

        if (settings.enableObstacles)
        {
            obstaclePool = new ObjectPool(settings.obstaclePoolSize, transform, CreateObstacle);
            StartCoroutine(ObstacleLoop());
        }

        if (settings.enablePowerUps)
        {
            powerUpPool = new ObjectPool(settings.powerUpPoolSize, transform, CreatePowerUp);
            StartCoroutine(PowerUpLoop());
        }
    }

    private void CreateBoundaryVisuals()
    {
        Sprite sprite = Resources.Load<Sprite>("BoundaryRock");
        if (sprite == null) return;

        for (float x = -8.4f; x <= 8.4f; x += 0.9f)
        {
            CreateBoundaryRock(sprite, new Vector3(x, 4.5f, -0.3f));
            CreateBoundaryRock(sprite, new Vector3(x, -4.5f, -0.3f));
        }

        for (float y = -3.6f; y <= 3.6f; y += 0.9f)
        {
            CreateBoundaryRock(sprite, new Vector3(-8.65f, y, -0.3f));
            CreateBoundaryRock(sprite, new Vector3(8.65f, y, -0.3f));
        }
    }

    private void CreateBoundaryRock(Sprite sprite, Vector3 position)
    {
        GameObject rock = new GameObject("Boundary Rock");
        rock.transform.SetParent(transform);
        rock.transform.position = position;
        rock.transform.localScale = Vector3.one * Random.Range(0.45f, 0.7f);
        SpriteRenderer renderer = rock.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 1;
    }

    private void CreateCenterDivider()
    {
        GameObject divider = new GameObject("Center Divider");
        divider.transform.SetParent(transform);
        divider.transform.position = new Vector3(0f, 0f, -0.4f);
        divider.transform.localScale = new Vector3(0.025f, 8.2f, 1f);
        SpriteRenderer renderer = divider.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateCircleSprite(Color.white, 4, 1f);
        renderer.color = new Color(1f, 1f, 1f, 0.35f);
        renderer.sortingOrder = 2;
    }

    private IEnumerator ObstacleLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(settings.obstacleSpawnMin, settings.obstacleSpawnMax));
            GameObject obstacle = obstaclePool.Get();
            obstacle.transform.position = RandomObstaclePosition();
            float scale = Random.Range(0.55f, 0.9f);
            obstacle.transform.localScale = new Vector3(scale, scale, 1f);
            obstacleLifetimes[obstacle] = StartCoroutine(ReleaseObstacleAfter(obstacle, Random.Range(settings.obstacleLifetimeMin, settings.obstacleLifetimeMax)));
        }
    }

    private IEnumerator PowerUpLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(settings.powerUpSpawnMin, settings.powerUpSpawnMax));
            GameObject powerUp = powerUpPool.Get();
            powerUp.transform.position = RandomPowerUpPosition();
            PowerUp component = powerUp.GetComponent<PowerUp>();
            PowerUpType type = (PowerUpType)Random.Range(0, 2);
            component.Initialize(this, type, settings, type == PowerUpType.BallSpeed ? speedSprite : sizeSprite);
            powerUpLifetimes[powerUp] = StartCoroutine(ReleasePowerUpAfter(powerUp, settings.powerUpLifetime));
        }
    }

    private Vector3 RandomObstaclePosition()
    {
        return new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-2.8f, 2.8f), -0.5f);
    }

    private Vector3 RandomPowerUpPosition()
    {
        float x = Random.value < 0.5f ? Random.Range(-6.5f, -1.5f) : Random.Range(1.5f, 6.5f);
        return new Vector3(x, Random.Range(-3.2f, 3.2f), -0.5f);
    }

    private GameObject CreateObstacle()
    {
        GameObject obstacle = new GameObject("Obstacle (Pooled)");
        SpriteRenderer renderer = obstacle.AddComponent<SpriteRenderer>();
        renderer.sprite = obstacleSprite;
        renderer.sortingOrder = 5;
        obstacle.AddComponent<CircleCollider2D>();
        return obstacle;
    }

    private GameObject CreatePowerUp()
    {
        GameObject powerUp = new GameObject("Power Up (Pooled)");
        SpriteRenderer renderer = powerUp.AddComponent<SpriteRenderer>();
        renderer.sprite = speedSprite;
        renderer.sortingOrder = 6;
        CircleCollider2D collider = powerUp.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        powerUp.AddComponent<PowerUp>();
        return powerUp;
    }

    private IEnumerator ReleaseObstacleAfter(GameObject obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseObstacle(obstacle);
    }

    private IEnumerator ReleasePowerUpAfter(GameObject powerUp, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleasePowerUp(powerUp);
    }

    private void ReleaseObstacle(GameObject obstacle)
    {
        if (obstacleLifetimes.ContainsKey(obstacle)) obstacleLifetimes.Remove(obstacle);
        obstaclePool?.Release(obstacle);
    }

    public void ReleasePowerUp(GameObject powerUp)
    {
        if (powerUpLifetimes.TryGetValue(powerUp, out Coroutine coroutine))
        {
            powerUpLifetimes.Remove(powerUp);
            if (coroutine != null) StopCoroutine(coroutine);
        }
        powerUpPool?.Release(powerUp);
    }




    private Sprite CreateCircleSprite(Color color, int size, float radius)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float max = (size - 1) * 0.5f * radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= max ? color : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

}
