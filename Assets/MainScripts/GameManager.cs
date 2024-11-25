using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance;

    // Game States
    public enum GameState { MainMenu, SettingsMenu, LevelSelection, Playing, Paused }
    public GameState CurrentState { get; private set; }

    // Player Data
    private PlayerData playerData;

    // Settings
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // Level Management
    public int totalLevels = 7;
    public int currentLevel = 1;

    // References
    private UIManager uiManager;
    private AudioManager audioManager;

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load player data
            LoadPlayerData();
        } else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Get references to UIManager and AudioManager
        uiManager = UIManager.Instance;
        audioManager = AudioManager.Instance;

        // Initialize game state when game begins
        CurrentState = GameState.MainMenu;
        uiManager.ShowMainMenu();

        // Apply settings
        ApplySettings();
    }

    /*
     *  GAME STATE MANAGEMENT (lines 66 - 107)
     */
    public void StartGame(int levelNumber)
    {
        currentLevel = levelNumber;
        CurrentState = GameState.Playing;

        // Load the selected level
        SceneManager.LoadScene("MainMenu");
        uiManager.ShowMainMenu();
    }

    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
        uiManager.ShowMainMenu();
    }

    public void OpenSettings()
    {
        CurrentState = GameState.SettingsMenu;
        uiManager.ShowSettingsMenu();
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            uiManager.ShowPauseMenu();
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            uiManager.HidePauseMenu();
        }
    }

    /*
     *  LEVEL MANAGEMENT (lines 113 - 138)
     */

    public bool IsLevelUnlocked(int levelNumber)
    {
        return playerData.unlockedLevels.Contains(levelNumber);
    }

    public void UnlockNextLevel()
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel <= totalLevels && !playerData.unlockedLevels.Contains(nextLevel))
        {
            playerData.unlockedLevels.Add(nextLevel);
            SavePlayerData();
        }
    }

    public void LevelCompleted()
    {
        // Unlock next level
        UnlockNextLevel();

        // Save progress
        SavePlayerData();

        // Return to level selection or show level completed UI
        OpenLevelSelection();
    }

    /*
     *  SETTINGS MANAGEMENT (lines 144 - 164)
     */

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioManager.SetMusicVolume(volume);
        playerData.sfxVolume = volume;
        SavePlayerData();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        audioManager.SetEffectsVolume(volume);
        playerData.sfxVolume = volume;
        SavePlayerData();
    }

    private void ApplySettings()
    {
        audioManager.SetMusicVolume(musicVolume);
        audioManager.SetEffectsVolume(sfxVolume);
    }

    /*
     *  PLAYER DATA MANAGEMENT (lines 170 - 211)
     */

    [System.Serializable]
    public class PlayerData
    {
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public System.Collections.Generic.List<int> unlockedLevels = new System.Collections.Generic.List<int>();
    }

    private void LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/playerdata.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        } 
        else
        {
            // Create new player data with default values
            playerData = new PlayerData();
            playerData.unlockedLevels.Add(1);
            SavePlayerData();
        }

        // Apply loaded settings
        musicVolume = playerData.musicVolume;
        sfxVolume = playerData.sfxVolume;
    }

    private void SavePlayerData()
    {
        string path = Application.persistentDataPath + "/playerdata.json";
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(path, json);
    }

    private void OnApplicationQuit()
    {
        // Save player data when the application is closed or quits
        SavePlayerData();
    }
}
