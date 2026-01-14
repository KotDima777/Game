using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingsMenu_TMP : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Slider masterVolumeSlider;

    [Header("Audio")]
    public AudioMixer audioMixer;

    private Resolution[] resolutions;

    void Start()
    {
        FindDropdownsIfNotAssigned();
        InitializeResolutionDropdown();
        InitializeQualityDropdown();
        LoadSettings();
    }

    void FindDropdownsIfNotAssigned()
    {
        if (resolutionDropdown == null)
        {
            GameObject resGO = GameObject.Find("ResolutionDropdown");
            if (resGO != null) resolutionDropdown = resGO.GetComponent<TMP_Dropdown>();
        }

        if (qualityDropdown == null)
        {
            GameObject qualGO = GameObject.Find("QualityDropdown");
            if (qualGO != null) qualityDropdown = qualGO.GetComponent<TMP_Dropdown>();
        }

        if (fullscreenToggle == null)
        {
            GameObject toggleGO = GameObject.Find("FullscreenToggle");
            if (toggleGO != null) fullscreenToggle = toggleGO.GetComponent<Toggle>();
        }

        if (masterVolumeSlider == null)
        {
            GameObject sliderGO = GameObject.Find("MasterVolumeSlider");
            if (sliderGO != null) masterVolumeSlider = sliderGO.GetComponent<Slider>();
        }
    }

    void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("TMP_Dropdown для разрешения не найден!");
            return;
        }

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int refreshRate = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            string option = $"{resolutions[i].width} × {resolutions[i].height} @ {refreshRate}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        Debug.Log($"Resolution Dropdown готов. Вариантов: {options.Count}");
    }

    void InitializeQualityDropdown()
    {
        if (qualityDropdown == null)
        {
            Debug.LogError("TMP_Dropdown для качества не найден!");
            return;
        }

        qualityDropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (string name in QualitySettings.names)
        {
            options.Add(name);
        }

        qualityDropdown.AddOptions(options);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();

        Debug.Log($"Quality Dropdown готов. Вариантов: {options.Count}");
    }

    // === ОСНОВНЫЕ МЕТОДЫ (исправленные) ===

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex < 0 || resolutionIndex >= resolutions.Length)
        {
            Debug.LogError("Неверный индекс разрешения: " + resolutionIndex);
            return;
        }

        Resolution res = resolutions[resolutionIndex];
        FullScreenMode mode = fullscreenToggle != null && fullscreenToggle.isOn ?
            FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Screen.SetResolution(res.width, res.height, mode, res.refreshRateRatio);

        Debug.Log($"Разрешение: {res.width}×{res.height}");
    }

    public void SetQuality(int qualityIndex)
    {
        if (qualityIndex >= 0 && qualityIndex < QualitySettings.names.Length)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            Debug.Log("Качество: " + QualitySettings.names[qualityIndex]);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Полноэкранный: " + (isFullscreen ? "Да" : "Нет"));
    }

    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }
    }

    // === СОХРАНЕНИЕ ===

    public void SaveSettings()
    {
        if (resolutionDropdown != null)
            PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);

        if (qualityDropdown != null)
            PlayerPrefs.SetInt("QualityPreference", qualityDropdown.value);

        PlayerPrefs.SetInt("FullscreenPreference", Screen.fullScreen ? 1 : 0);

        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("VolumePreference", masterVolumeSlider.value);

        PlayerPrefs.Save();
        Debug.Log("Настройки сохранены!");
    }

    void LoadSettings()
    {
        if (resolutionDropdown != null && PlayerPrefs.HasKey("ResolutionPreference"))
        {
            int index = PlayerPrefs.GetInt("ResolutionPreference");
            if (index < resolutions.Length)
            {
                resolutionDropdown.value = index;
                SetResolution(index);
            }
        }

        if (qualityDropdown != null && PlayerPrefs.HasKey("QualityPreference"))
        {
            int index = PlayerPrefs.GetInt("QualityPreference");
            if (index < QualitySettings.names.Length)
            {
                qualityDropdown.value = index;
                SetQuality(index);
            }
        }

        if (fullscreenToggle != null && PlayerPrefs.HasKey("FullscreenPreference"))
        {
            bool fs = PlayerPrefs.GetInt("FullscreenPreference") == 1;
            fullscreenToggle.isOn = fs;
            SetFullscreen(fs);
        }

        if (masterVolumeSlider != null && PlayerPrefs.HasKey("VolumePreference"))
        {
            float vol = PlayerPrefs.GetFloat("VolumePreference");
            masterVolumeSlider.value = vol;
            SetMasterVolume(vol);
        }

        Debug.Log("Настройки загружены");
    }

    public void ApplySettings()
    {
        SaveSettings();
        Debug.Log("Настройки применены!");
    }

    public void ExitSettings()
    {
        SaveSettings();
        SceneManager.LoadScene("Lobby"); // Изменено с "MainMenu" на "Lobby"
    }
}