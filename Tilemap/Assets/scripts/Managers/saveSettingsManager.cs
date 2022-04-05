using System;
using UnityEngine;
using UnityEngine.UI;

public class saveSettingsManager : MonoBehaviour
{
    [SerializeField] private Slider MusicVolume;
    [SerializeField] private Slider UnitVolume;
    [SerializeField] private Slider VoiceVolume;
    [SerializeField] private Slider SFXVolume;

    //public static saveSettingsManager instance;
    public eventsScript OnAudioSettingsChanged;
    public audioManager audiomanager;

    const string mixerMusicVolume = "MusicVolume";
    const string mixerUnitsVolume = "UnitsVolume";
    const string mixerVoicesVolume = "VoicesVolume";
    const string mixerSFXVolume = "SFXVolume";
    private void Awake()
    {
        turnOn();
        turnOff();
    }
    void Update()
    {
        MusicVolume.onValueChanged.AddListener(MusicChanged);
        UnitVolume.onValueChanged.AddListener(UnitChanged);
        VoiceVolume.onValueChanged.AddListener(VoiceChanged);
        SFXVolume.onValueChanged.AddListener(SFxChanged);
    }
    void MusicChanged(float value)
    {
        audiomanager.mixer.SetFloat(mixerMusicVolume, (float)(Math.Log10(value)* 40));
    }
    void UnitChanged(float value)
    {
        audiomanager.mixer.SetFloat(mixerUnitsVolume, (float)(Math.Log10(value)* 40));
    }

    void VoiceChanged(float value)
    {
        audiomanager.mixer.SetFloat(mixerVoicesVolume, (float)(Math.Log10(value)* 40));
    }
    void SFxChanged(float value)
    {
        audiomanager.mixer.SetFloat(mixerSFXVolume, (float)(Math.Log10(value)* 40));
    }

//This is turned on by an event called by the settings button
    public void turnOn()
    {
        gameObject.SetActive(true);
        if(PlayerPrefs.HasKey(mixerMusicVolume))
        {
            MusicVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(mixerMusicVolume));
        }
        if(PlayerPrefs.HasKey(mixerUnitsVolume))
        {
            UnitVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(mixerUnitsVolume));
        }
        if(PlayerPrefs.HasKey(mixerVoicesVolume))
        {
            VoiceVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(mixerVoicesVolume));
        }
        if(PlayerPrefs.HasKey(mixerSFXVolume))
        {
            SFXVolume.SetValueWithoutNotify(PlayerPrefs.GetFloat(mixerSFXVolume));
        }
    }
    public void turnOff()
    {
        gameObject.SetActive(false);
    }
    
    public void Cancel()
    {
        OnAudioSettingsChanged.Raise();
        gameObject.SetActive(false);
    }
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(mixerMusicVolume, MusicVolume.value);
        PlayerPrefs.SetFloat(mixerUnitsVolume, UnitVolume.value);
        PlayerPrefs.SetFloat(mixerVoicesVolume, VoiceVolume.value);
        PlayerPrefs.SetFloat(mixerSFXVolume, SFXVolume.value);
        OnAudioSettingsChanged.Raise();
        gameObject.SetActive(false);
    }
}
