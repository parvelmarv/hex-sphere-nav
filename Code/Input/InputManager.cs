using UnityEngine;
using UnityEngine.InputSystem;
using System;
public enum InputMode
{
    Planet,
    Ship,
    Building,
    UI
}
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    public PlayerInputsystem Actions { get; private set; } 
    
    [SerializeField] private InputMode defaultInputMode =  InputMode.Building;
    public event Action OnPlanetModeEnabled;
    public event Action OnShipModeEnabled;
    public event Action OnBuildingModeEnabled;
    
    public string CurrentActionMap { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
            return; 
        }
        Instance = this;
        Actions = new PlayerInputsystem(); 
        Debug.Log($"[InputManager] Initialized. Actions created: {Actions != null}");
    }

    private void Start()
    {
        SetInputMode(defaultInputMode);
    }

    public void SetInputMode(InputMode mode)
    {
        CurrentActionMap = mode.ToString();
        Debug.Log("Trying to set up");
        // Disable all maps first
        Actions.RocketController.Disable();
        Actions.PlanetController.Disable();
        Actions.PlanetBuilder.Disable();
        
        switch (mode)
        {
            case InputMode.Planet:
                Actions.PlanetController.Enable();
                OnPlanetModeEnabled?.Invoke();
                break;
            case InputMode.Ship:
                Actions.RocketController.Enable();
                OnShipModeEnabled?.Invoke();
                break;
            case InputMode.Building:
                Actions.PlanetBuilder.Enable();
                OnBuildingModeEnabled?.Invoke();
                break;
        }
    }

    private void OnDisable()
    {
        Actions?.Disable();
    }

    private void OnDestroy()
    {
        Actions?.Dispose();
    }
}