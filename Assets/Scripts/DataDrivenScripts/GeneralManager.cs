using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralManager : MonoBehaviour
{
    [SerializeField] private DefaultPlayerData defaultPlayerData;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button directResetButton;
    [SerializeField] private Button directQuitButton;
    
    private void Awake()
    {
        PrepareApplicationSettings();
        PreparePlayerData();
        PrepareObjectListeners();
        InitializeUi();
    }

    private void PrepareApplicationSettings()
    {
        // Set the application as running in background, and the time scale should be in normal state
        Application.runInBackground = true;
        Time.timeScale = 1.0f;
    }

    private void PreparePlayerData()
    {
        // Initialize PlayerDataManager with the default data
        PlayerDataManager.Initialize(defaultPlayerData);
    }

    private void PrepareObjectListeners()
    {
        try { directResetButton.onClick.AddListener(PrepareResetData); } catch (Exception) { }
        try { directQuitButton.onClick.AddListener(ExecuteQuit); } catch (Exception) { }
    }

    private void InitializeUi()
    {
        try { menuPanel.SetActive(true); } catch (Exception) { }
        try { dialoguePanel.SetActive(true); } catch (Exception) { }
    }

    public void PrepareResetData()
    {
        PlayerDataManager.DeleteData();
        PlayerDataManager.Initialize(defaultPlayerData);
        PlayerDataManager.LoadData();
    }

    public void ExecuteQuit()
    {
        // Quit the application if the quit button is pressed
        Application.Quit();
        Debug.Log("Successfully executed quit.");
    }

    private void PauseOnDeviceOnly()
    {
        // Pause the device if the application loses focus
        if (Application.isEditor) return;
        Time.timeScale = 0.0f;
        Debug.Log("Device currently on pause. Unfocusing the device.");

        PlayerData playerData = PlayerDataManager.LoadData();
        if (playerData == null)
        {
            Debug.LogError("Failed to retrieve data.");
            return;
        }

        PlayerDataManager.SaveData(playerData);
    }
 
    // private void OnApplicationFocus(bool hasFocus)
    // {
    //     PauseOnDeviceOnly();
    // }

    // private void OnApplicationPause(bool hasPause)
    // {
    //     PauseOnDeviceOnly();
    // }

    // private void OnApplicationQuit()
    // {
    //     PauseOnDeviceOnly();
    // }
}
