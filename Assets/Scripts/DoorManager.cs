using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorManager : MonoBehaviour
{
    [Header("UI элементы (необязательно)")]
    public Text hintText;
    public GameObject completePanel;

    private Door fireDoor;
    private Door waterDoor;
    private bool levelComplete = false;

    void Start()
    {
        // Находим двери автоматически (используем новый метод)
        FindDoors();

        if (completePanel != null)
            completePanel.SetActive(false);
    }

    void FindDoors()
    {
        // Используем FindObjectsByType вместо устаревшего FindObjectsOfType
        Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);

        foreach (Door door in allDoors)
        {
            if (door.playerTag == "FirePlayer")
            {
                fireDoor = door;
                Debug.Log($"Найдена огненная дверь: {door.name}");
            }
            else if (door.playerTag == "WaterPlayer")
            {
                waterDoor = door;
                Debug.Log($"Найдена водная дверь: {door.name}");
            }
        }

        // Связываем двери между собой
        if (fireDoor != null && waterDoor != null)
        {
            SetupDoorConnection(fireDoor, waterDoor);
        }
    }

    void SetupDoorConnection(Door door1, Door door2)
    {
        // Просто связываем двери друг с другом
        door1.otherDoor = door2;
        door2.otherDoor = door1;
        Debug.Log("Двери связаны между собой");
    }

    void Update()
    {
        if (levelComplete) return;

        // Обновляем UI подсказки
        UpdateHintText();

        // Проверяем завершение уровня
        if (fireDoor != null && waterDoor != null)
        {
            // Проверяем, завершены ли обе двери
            if (fireDoor.IsSequenceComplete && waterDoor.IsSequenceComplete)
            {
                LevelComplete();
            }
        }
    }

    void UpdateHintText()
    {
        if (hintText == null) return;

        if (fireDoor == null || waterDoor == null)
        {
            hintText.text = "Ищем двери...";
            return;
        }

        // Проверяем состояния
        bool fireReady = fireDoor.IsPlayerInside;
        bool waterReady = waterDoor.IsPlayerInside;

        if (fireReady && waterReady)
        {
            hintText.text = "ОБА ИГРОКА ВНУТРИ!";
        }
        else if (fireReady)
        {
            hintText.text = "Огненный игрок внутри. Ждем водного...";
        }
        else if (waterReady)
        {
            hintText.text = "Водный игрок внутри. Ждем огненного...";
        }
        else
        {
            hintText.text = "Подведите игроков к дверям";
        }
    }

    void LevelComplete()
    {
        levelComplete = true;

        if (completePanel != null)
        {
            completePanel.SetActive(true);
        }

        // Автосохранение
        PlayerPrefs.SetInt("LastCompletedLevel", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();

        // Загрузка следующего уровня через 3 секунды
        Invoke("LoadNextLevel", 3f);
    }

    void LoadNextLevel()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            SceneManager.LoadScene(0); // Главное меню
        }
    }

    [ContextMenu("Тест: Сымитировать вход обоих игроков")]
    void TestBothPlayersEnter()
    {
        if (fireDoor != null)
        {
            GameObject testFire = new GameObject("TestFirePlayer");
            testFire.tag = "FirePlayer";
            testFire.transform.position = fireDoor.transform.position + Vector3.forward;

            // Вызываем метод входа через рефлексию (так как он private)
            fireDoor.SendMessage("PlayerEnter", testFire, SendMessageOptions.DontRequireReceiver);
        }

        if (waterDoor != null)
        {
            GameObject testWater = new GameObject("TestWaterPlayer");
            testWater.tag = "WaterPlayer";
            testWater.transform.position = waterDoor.transform.position + Vector3.forward;

            waterDoor.SendMessage("PlayerEnter", testWater, SendMessageOptions.DontRequireReceiver);
        }
    }
}