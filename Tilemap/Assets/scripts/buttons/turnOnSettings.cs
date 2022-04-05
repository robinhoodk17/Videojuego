using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnOnSettings : MonoBehaviour
{
    public GameObject SettingsPanel;
    public AudioSettings AudiosettingsManager;
    public KeyBindings controlSettings;
    public static turnOnSettings instance;
    private void Start()
    {
        
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        TurnOffSettings();
    }
    public void ActivateSettings()
    {
        SettingsPanel.SetActive(true);
        AudiosettingsManager.turnOn();
    }
    public void ActivateAudioSettings()
    {
        AudiosettingsManager.turnOn();
        controlSettings.turnOff();
        
    }

    public void ActivateControlSettings()
    {
        AudiosettingsManager.turnOff();
        controlSettings.turnOn();
    }
    public void TurnOffSettings()
    {
        controlSettings.turnOff();
        AudiosettingsManager.turnOff();
        SettingsPanel.SetActive(false);
    }
    public void Save()
    {
        AudiosettingsManager.SaveSettings();
        controlSettings.SaveSettings();
        SettingsPanel.SetActive(false);
    }
}
