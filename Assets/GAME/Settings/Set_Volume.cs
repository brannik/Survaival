using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Set_Volume : MonoBehaviour
{
    [Header("Master volume")]
    [SerializeField] private Slider masterVolSlider;
    [SerializeField] private TextMeshProUGUI masterValue;
    [SerializeField] private Toggle masterMute;
    [Header("Music volume")]
    [SerializeField] private Slider musicVolSlider;
    [SerializeField] private TextMeshProUGUI musicValue;
    [SerializeField] private Toggle musicMute;
    [Header("SFX volume")]
    [SerializeField] private Slider SFXVolSlider;
    [SerializeField] private TextMeshProUGUI SFXValue;
    [SerializeField] private Toggle SFXMute;

    [SerializeField] private AudioMixer myMixer;

    void Awake(){
        DontDestroyOnLoad(gameObject);
        LoadSoundSettings();
    }
    void Start(){
        LoadSoundSettings();
        masterMute.onValueChanged.AddListener(delegate{
            if(!masterMute.isOn) SetMasterVolume();
            else myMixer.SetFloat("masterVolume",-80.0f);
            SaveSoundSettings();
        });
        musicMute.onValueChanged.AddListener(delegate{
            if(!musicMute.isOn) SetMusicVolume();
            else myMixer.SetFloat("musicVolume",-80.0f);
            SaveSoundSettings();
        });
        SFXMute.onValueChanged.AddListener(delegate{
            if(!SFXMute.isOn) SetSFXVolume();
            else myMixer.SetFloat("SFXVolume",-80.0f);
            SaveSoundSettings();
        });
        
    }
    void OnEnable(){
        
    }
    public void SetMasterVolume(){
        float volume = masterVolSlider.value;
        myMixer.SetFloat("masterVolume",volume);
        masterValue.text = $"{ConvertSliderToPercentage(masterVolSlider.value,masterVolSlider.minValue,masterVolSlider.maxValue):0}%";
        SaveSoundSettings();
    }

    public void SetMusicVolume(){
        float volume = musicVolSlider.value;
        myMixer.SetFloat("musicVolume",volume);
        musicValue.text = $"{ConvertSliderToPercentage(musicVolSlider.value,musicVolSlider.minValue,musicVolSlider.maxValue):0}%";
        SaveSoundSettings();
    }

    public void SetSFXVolume(){
        float volume = SFXVolSlider.value;
        myMixer.SetFloat("SFXVolume",volume);
        SFXValue.text = $"{ConvertSliderToPercentage(SFXVolSlider.value,SFXVolSlider.minValue,SFXVolSlider.maxValue):0}%";
        SaveSoundSettings();
    }

    float ConvertSliderToPercentage(float currentValue, float minValue, float maxValue)
    {
        // Ensure minValue is less than maxValue
        if (minValue >= maxValue)
            throw new ArgumentException("minValue must be less than maxValue");

        // Perform the conversion
        float percentage = ((currentValue - minValue) / (maxValue - minValue)) * 100.0f;
        return percentage;
    }

    private void SaveSoundSettings(){
        PlayerPrefs.SetFloat("master_vol",masterVolSlider.value);
        PlayerPrefs.SetInt("master_mute",GetIntFromBool(masterMute.isOn));

        PlayerPrefs.SetFloat("music_vol",musicVolSlider.value);
        PlayerPrefs.SetInt("music_mute",GetIntFromBool(musicMute.isOn));

        PlayerPrefs.SetFloat("sfx_vol",SFXVolSlider.value);
        PlayerPrefs.SetInt("sfx_mute",GetIntFromBool(SFXMute.isOn));
        PlayerPrefs.Save();
    }
    private void LoadSoundSettings(){
        float mvol = PlayerPrefs.GetFloat("master_vol",0.0f);
        masterVolSlider.value = mvol;
        masterMute.isOn = GetBoolFromInt(PlayerPrefs.GetInt("master_mute",0));

        float mcvol = PlayerPrefs.GetFloat("music_vol",0.0f);
        musicVolSlider.value = mcvol;
        musicMute.isOn = GetBoolFromInt(PlayerPrefs.GetInt("music_mute",0));

        float sfxvol = PlayerPrefs.GetFloat("sfx_vol",0.0f);
        SFXVolSlider.value = sfxvol;
        SFXMute.isOn = GetBoolFromInt(PlayerPrefs.GetInt("sfx_mute",0));

        LoadSoundSettings();
        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }

    private int GetIntFromBool(bool val){
        return val ? 1 : 0;
    }
    private bool GetBoolFromInt(int val){
        return val != 0;
    }
}
