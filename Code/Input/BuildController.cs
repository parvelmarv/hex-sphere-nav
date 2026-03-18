using UnityEngine;
using UnityEngine.InputSystem;
using System;


public class BuildController : BasePlayerController
{
    public Vector2 Look { get; private set; } 
    public float ZoomInput { get; private set; }
    public event Action OnBuild;
    public event Action OnExit;

    protected override void SubscribeInput()
    {
        var map = InputManager.Instance.Actions.PlanetBuilder;
        map.Navigate.performed += OnNavigate;
        map.Navigate.canceled += OnNavigate;
        map.RotateCamera.performed += OnRotateCamera;
        map.RotateCamera.canceled += OnRotateCamera;
        map.Build.performed += OnBuildPerformed;
        map.Exit.performed += OnExitPerformed;
        
        //Zoom
        map.Zoom.performed += OnZoom;
        map.Zoom.canceled += OnZoom;
    }

    protected override void UnsubscribeInput()
    {
        if (InputManager.Instance == null) return;
        var map = InputManager.Instance.Actions.PlanetBuilder;

        map.Navigate.performed -= OnNavigate;
        map.Navigate.canceled -= OnNavigate;

        map.RotateCamera.performed -= OnRotateCamera;
        map.RotateCamera.canceled -= OnRotateCamera;

        map.Build.performed -= OnBuildPerformed;
        map.Exit.performed -= OnExitPerformed;
        
        map.Zoom.performed -= OnZoom;
        map.Zoom.canceled -= OnZoom;
    }

    protected override void ResetInput()
    {
        base.ResetInput(); // Resets 'Move' to Vector2.zero
        Look = Vector2.zero;
    }

    // --- INPUT CALLBACKS ---

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        // Set the property defined in BasePlayerController
        Move = ctx.ReadValue<Vector2>();
    }

    private void OnRotateCamera(InputAction.CallbackContext ctx)
    {
        Look = ctx.ReadValue<Vector2>();
    }

    private void OnBuildPerformed(InputAction.CallbackContext ctx)
    {
        OnBuild?.Invoke();
    }

    private void OnExitPerformed(InputAction.CallbackContext ctx)
    {
        OnExit?.Invoke();
    }
    
    private void OnZoom(InputAction.CallbackContext ctx)
    {
        ZoomInput = ctx.ReadValue<float>();
    }
}
