using UnityEngine;
using System;

public abstract class BasePlayerController : MonoBehaviour, IPlayerController
{
    // Interface Implementation
    public Vector2 Move { get; protected set; }

    // Helper to protect event invocation from subclasses
    protected virtual void Start()
    {
        if (InputManager.Instance != null)
        {
            SubscribeInput();
        }
    }

    protected virtual void OnDisable()
    {  
        if (InputManager.Instance != null)
        {
            UnsubscribeInput();
        }
        ResetInput();
    }

    protected virtual void ResetInput()
    {
        Move = Vector2.zero;
    }

    // Abstract methods forces subclasses to implement their specific input map logic
    protected abstract void SubscribeInput();
    protected abstract void UnsubscribeInput();
}
