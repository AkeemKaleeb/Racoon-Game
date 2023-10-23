using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { StartMenu, Playing, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController2D _player;
    [SerializeField] GameObject _pauseMenu; 
    [SerializeField] GameObject _startMenu; 

    private static GameState gameState;
    PlayerInputActions UI_Input;
    InputAction escape;
    GameObject currMenu;

    // Start is called before the first frame update
    void Awake()
    {        
        UI_Input = new PlayerInputActions();
        escape = UI_Input.UI.Escape;
        escape.performed += PauseMenu;

        gameState = GameState.StartMenu;
        _startMenu.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.StartMenu)
        {
            _player.enabled = false;
            escape.Disable();
        }
        else if (gameState == GameState.Playing)
        {
            _player.enabled = true;
            escape.Enable();
        }
        else if (gameState == GameState.Paused)
        {
            _player.enabled = false;
        }

        currMenu = gameState == GameState.StartMenu ? _startMenu : _pauseMenu;
    }

    void PauseMenu(InputAction.CallbackContext context)
    {
        gameState = GameState.Paused;
        _pauseMenu.SetActive(true);
    }

    public void Play()
    {
        gameState = GameState.Playing;
        currMenu.SetActive(false);
    }

    public void Settings()
    {
        //TODO -- Settings menu
    }

    public void BackToStart()
    {
        gameState = GameState.StartMenu;
        currMenu.SetActive(false);
        _startMenu.SetActive(true);
    }

    public void ExitApp()
    {
        #if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
