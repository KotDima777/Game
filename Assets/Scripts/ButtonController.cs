using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header("Button Settings")]
    public Color buttonColor = Color.red;
    public Sprite pressedSprite;
    public Sprite releasedSprite;
    public PlatformController[] connectedPlatforms;

    [Header("Button Press Animation")]
    public float pressDepth = 0.15f;
    public float pressSpeed = 8f;

    [Header("Button State")]
    public bool isPressed = false;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private int pressCount = 0;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        originalPosition = transform.position;
        pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);

        spriteRenderer.sprite = releasedSprite;
        spriteRenderer.color = buttonColor;

        SetupCollider();
    }

    void SetupCollider()
    {
        // Основной коллайдер
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = false;
        collider.size = new Vector2(1f, 0.8f);

        // Триггер для детекции
        CreateTrigger();
    }

    void CreateTrigger()
    {
        GameObject trigger = new GameObject("ButtonTrigger");
        trigger.transform.SetParent(transform);
        trigger.transform.localPosition = new Vector3(0, 0.1f, 0);

        BoxCollider2D triggerCollider = trigger.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(0.9f, 0.3f);

        ButtonTrigger buttonTrigger = trigger.AddComponent<ButtonTrigger>();
        buttonTrigger.button = this;
    }

    void Update()
    {
        // Плавное движение кнопки
        Vector3 target = (pressCount > 0) ? pressedPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * pressSpeed);
    }

    public void AddPress()
    {
        if (pressCount == 0)
        {
            Press();
        }
        pressCount++;
    }

    public void RemovePress()
    {
        pressCount = Mathf.Max(0, pressCount - 1);
        if (pressCount == 0)
        {
            Release();
        }
    }

    void Press()
    {
        isPressed = true;
        spriteRenderer.sprite = pressedSprite;

        Color lighterColor = buttonColor * 1.3f;
        lighterColor.a = 1f;
        spriteRenderer.color = lighterColor;

        foreach (PlatformController platform in connectedPlatforms)
        {
            if (platform != null)
            {
                platform.Activate();
            }
        }
    }

    void Release()
    {
        isPressed = false;
        spriteRenderer.sprite = releasedSprite;
        spriteRenderer.color = buttonColor;

        foreach (PlatformController platform in connectedPlatforms)
        {
            if (platform != null)
            {
                platform.Deactivate();
            }
        }
    }
}

// Отдельный скрипт для триггера
public class ButtonTrigger : MonoBehaviour
{
    public ButtonController button;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FirePlayer") ||
            other.CompareTag("WaterPlayer") ||
            other.CompareTag("Box"))
        {
            button.AddPress();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("FirePlayer") ||
            other.CompareTag("WaterPlayer") ||
            other.CompareTag("Box"))
        {
            button.RemovePress();
        }
    }
}