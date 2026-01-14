using UnityEngine;

public class AnimatedDoorController : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator doorAnimator;
    public string openAnimationName = "DoorOpen";
    public string closeAnimationName = "DoorClose";

    [Header("Activation Settings")]
    public float activationRadius = 2f;
    public bool requireBothPlayers = true;

    private bool firePlayerInRange = false;
    private bool waterPlayerInRange = false;
    private bool isDoorOpen = false;

    void Start()
    {
        // Находим Animator если не назначен
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }

        // Создаем триггер коллайдер для 2D
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = activationRadius;
            collider.isTrigger = true;
        }
    }

    void Update()
    {
        // Проверяем состояние и обновляем анимацию
        bool shouldBeOpen = requireBothPlayers ?
            (firePlayerInRange && waterPlayerInRange) :
            (firePlayerInRange || waterPlayerInRange);

        if (shouldBeOpen != isDoorOpen)
        {
            isDoorOpen = shouldBeOpen;
            UpdateDoorAnimation();
        }
    }

    void UpdateDoorAnimation()
    {
        if (doorAnimator == null) return;

        if (isDoorOpen)
        {
            doorAnimator.Play(openAnimationName);
            Debug.Log("Door opened via animation");
        }
        else
        {
            doorAnimator.Play(closeAnimationName);
            Debug.Log("Door closed via animation");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FirePlayer"))
        {
            firePlayerInRange = true;
            Debug.Log("Fire player entered door area");
        }
        else if (other.CompareTag("WaterPlayer"))
        {
            waterPlayerInRange = true;
            Debug.Log("Water player entered door area");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("FirePlayer"))
        {
            firePlayerInRange = false;
            Debug.Log("Fire player exited door area");
        }
        else if (other.CompareTag("WaterPlayer"))
        {
            waterPlayerInRange = false;
            Debug.Log("Water player exited door area");
        }
    }

    [ContextMenu("Open Door")]
    public void OpenDoor()
    {
        if (!isDoorOpen)
        {
            isDoorOpen = true;
            UpdateDoorAnimation();
        }
    }

    [ContextMenu("Close Door")]
    public void CloseDoor()
    {
        if (isDoorOpen)
        {
            isDoorOpen = false;
            UpdateDoorAnimation();
        }
    }

    // Метод для принудительного открытия/закрытия
    public void SetDoorState(bool open)
    {
        isDoorOpen = open;
        UpdateDoorAnimation();
    }

    void OnDrawGizmosSelected()
    {
        // Визуализация радиуса активации
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}