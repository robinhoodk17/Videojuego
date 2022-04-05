using UnityEngine;
using System;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class audioManager : MonoBehaviour
{
    public AudioMixer mixer;
    public static audioManager instance;
    const string mixerMusicVolume = "MusicVolume";
    const string mixerUnitsVolume = "UnitsVolume";
    const string mixerVoicesVolume = "VoicesVolume";
    const string mixerSFXVolume = "SFXVolume";
    bool started = false;
    // Start is called before the first frame update
    void Start()
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
        
    }

    void Update()
    {
        if(!started)
        {
            started = true;
            if(PlayerPrefs.HasKey(mixerMusicVolume))
            {
                UpdateAudioVolume();
            }
        }
    }

    // Update is called once per frame
    public void UpdateAudioVolume()
    {
        mixer.SetFloat(mixerMusicVolume, (float)(Math.Log10(PlayerPrefs.GetFloat(mixerMusicVolume))* 40));
        mixer.SetFloat(mixerUnitsVolume, (float)(Math.Log10(PlayerPrefs.GetFloat(mixerUnitsVolume))* 40));
        mixer.SetFloat(mixerVoicesVolume, (float)(Math.Log10(PlayerPrefs.GetFloat(mixerVoicesVolume))* 40));
        mixer.SetFloat(mixerSFXVolume,(float)(Math.Log10( PlayerPrefs.GetFloat(mixerSFXVolume))* 40));
    }
}
