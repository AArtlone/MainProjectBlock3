﻿using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class SettingsManager : MonoBehaviour
{
    [NonSerialized]
    public static SettingsManager Instance;

    public string Language;
    public string Font;
    public bool DevelopmentMode;
    public int ResolutionWidth;
    public int ResolutionHeight;
    public int MasterVolume;
    public int SoundEffectsVolume;
    public int BackgroundMusicVolume;

    public List<AudioSource> BackgroundSources;
    public List<AudioSource> SoundSources;

    private string _pathToSettingsJson;
    public LanguageToggle[] LanguageIconObjects;
    public List<Image> LanguageIcons;
    public List<LanguageController> LanguageControllers = new List<LanguageController>();
    private int NumberOfTaps = 0;
    private bool _isDevModeEnabled = false;

    public object Scenemanager { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log(SceneManager.GetActiveScene().name);
            Destroy(Instance);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        _pathToSettingsJson = Application.persistentDataPath + "/Settings.json";

        if (File.Exists(_pathToSettingsJson))
        {
            LoadSettingsFromFile(_pathToSettingsJson);
        }
        else
        {
            ResetSettings();

            File.WriteAllText(_pathToSettingsJson, ExtractCurrentSettings());
        }
        UpdateHiglightedLanguageIcons();
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Logo Introduction" && _isDevModeEnabled == false)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    NumberOfTaps++;
                }
            }
            if (NumberOfTaps == 10)
            {
                if (DevelopmentMode == false)
                {
                    DevelopmentMode = true;
                    RefreshSettings();
                }
                SceneManager.LoadScene("Test Area");
                _isDevModeEnabled = true;
            }
            //if (NumberOfTaps == 10)
            //{
            //    if (DevelopmentMode == false)
            //    {
            //        DevelopmentMode = true;
            //        RefreshSettings();
            //    }
            //    SceneManager.LoadScene("Tutorial Complete");
            //    _isDevModeEnabled = true;
            //}
        }
    }

    public void RemoveSingletonInstance()
    {
        Destroy(Instance);
        Destroy(gameObject);
    }

    /// <summary>
    /// Loads the file from the data storage of the device instead of the game
    /// resources.
    /// </summary>
    /// <param name="filePath"></param>
    private void LoadSettingsFromFile(string filePath)
    {
        string settingsData = File.ReadAllText(_pathToSettingsJson);
        JsonData settingsJson = JsonMapper.ToObject(settingsData);

        Language = settingsJson["Settings"]["Language"].ToString();
        Font = settingsJson["Settings"]["Font"].ToString();
        if (settingsJson["Settings"]["DevelopmentMode"].ToString() == "True")
        {
            DevelopmentMode = true;
        } else if (settingsJson["Settings"]["DevelopmentMode"].ToString() == "False")
        {
            DevelopmentMode = false;
        }
        ResolutionWidth = int.Parse(settingsJson["Settings"]["ResolutionWidth"].ToString());
        ResolutionHeight = int.Parse(settingsJson["Settings"]["ResolutionHeight"].ToString());
        MasterVolume = int.Parse(settingsJson["Settings"]["MasterVolume"].ToString());
        SoundEffectsVolume = int.Parse(settingsJson["Settings"]["SoundEffectsVolume"].ToString());
        BackgroundMusicVolume = int.Parse(settingsJson["Settings"]["BackgroundMusicVolume"].ToString());
    }

    /// <summary>
    /// Gets the existing data from the settings singleton(this) and returns it
    /// as a json string.
    /// </summary>
    /// <returns></returns>
    private string ExtractCurrentSettings()
    {
        string settingsText = "{";
        settingsText += InsertNewLineTabs(1);
        settingsText += "\"Settings\": {";
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"DevelopmentMode\": " + (DevelopmentMode ? "true" : "false") + ",";
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"Language\": " + "\"" + Language + "\",";
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"Font\": " + "\"" + Font + "\",";
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"ResolutionWidth\": " + ResolutionWidth + ",";
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"ResolutionHeight\": " + ResolutionHeight + ","; ;
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"MasterVolume\": " + MasterVolume + ","; ;
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"SoundEffectsVolume\": " + SoundEffectsVolume + ",";
        settingsText += InsertNewLineTabs(2);
        settingsText += "\"BackgroundMusicVolume\": " + BackgroundMusicVolume;
        settingsText += InsertNewLineTabs(1);
        settingsText += "}" + Environment.NewLine + "}";

        return settingsText;
    }

    /// <summary>
    /// Resets the settings of the game using the default configuration in resources.
    /// </summary>
    public void ResetSettings()
    {
        TextAsset settingsText = Resources.Load<TextAsset>("Default World Data/Settings");
        JsonData settingsJson = JsonMapper.ToObject(settingsText.text);

        // Reading and assigning data from the settings file to this manager class.
        Language = settingsJson["Settings"]["Language"].ToString();
        Font = settingsJson["Settings"]["Font"].ToString();
        if (settingsJson["Settings"]["DevelopmentMode"].ToString() == "True")
        {
            DevelopmentMode = true;
        }
        else if (settingsJson["Settings"]["DevelopmentMode"].ToString() == "False")
        {
            DevelopmentMode = false;
        }
        ResolutionWidth = int.Parse(settingsJson["Settings"]["ResolutionWidth"].ToString());
        ResolutionHeight = int.Parse(settingsJson["Settings"]["ResolutionHeight"].ToString());
        MasterVolume = int.Parse(settingsJson["Settings"]["MasterVolume"].ToString());
        SoundEffectsVolume = int.Parse(settingsJson["Settings"]["SoundEffectsVolume"].ToString());
        BackgroundMusicVolume = int.Parse(settingsJson["Settings"]["BackgroundMusicVolume"].ToString());
    }

    /// <summary>
    /// Updates the existing settings configuration in the json file based on this
    /// class's parameters whenever it is configured by the player.
    /// </summary>
    public void RefreshSettings()
    {
        string newSettingsData = JsonUtility.ToJson(this);
        File.WriteAllText(_pathToSettingsJson, ExtractCurrentSettings());
    }

    /// <summary>
    /// Highlights and De-Highlights language flags in the settings window depending
    /// on the player's currently selected language.
    /// </summary>
    public void UpdateHiglightedLanguageIcons()
    {
        switch (Language)
        {
            case "English":
                foreach (Image languageIcon in LanguageIcons)
                {
                    if (languageIcon != null)
                    {
                        if (languageIcon.name == "English")
                        {
                            languageIcon.color = new Color(255, 255, 255, 1f);
                        }
                        else
                        {
                            languageIcon.color = new Color(255, 255, 255, 0.5f);
                        }
                    }
                }
                break;
            case "Dutch":
                foreach (Image languageIcon in LanguageIcons)
                {
                    if (languageIcon != null)
                    {
                        if (languageIcon.name == "Dutch")
                        {
                            languageIcon.color = new Color(255, 255, 255, 1f);
                        }
                        else
                        {
                            languageIcon.color = new Color(255, 255, 255, 0.5f);
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// This function runs whenever the audio settings' volume parameter of
    /// audio sources in the scenes is updated.
    /// </summary>
    public void UpdateAudioVolume()
    {
        foreach (AudioSource audioSource in SoundSources)
        {
            if (audioSource != null)
            {
                audioSource.volume = SoundEffectsVolume / 100f;
            }
        }

        foreach (AudioSource audioSource in BackgroundSources)
        {
            if (audioSource != null)
            {
                audioSource.volume = BackgroundMusicVolume / 100f;
            }
        }
    }

    private string InsertNewLineTabs(int numberOfTabs)
    {
        string whiteSpace = Environment.NewLine;
        for (int i = 0; i < numberOfTabs; i++)
        {
            whiteSpace += "\t";
        }

        return whiteSpace;
    }
}
