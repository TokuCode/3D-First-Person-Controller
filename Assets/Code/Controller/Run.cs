using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Run : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Crouch crouch;
    [SerializeField] private Movement move;
    [SerializeField] private PhysicsManager physics;

    [Header("Runtime")]
    [SerializeField] private float currentMaxSpeed;
    [SerializeField] private float desiredMaxSpeed;
    [SerializeField] private float lastDesiredMaxSpeed;
    private Coroutine speedControl;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool runAction;

    [Header("Walking")]
    [SerializeField] private float walkMaxSpeed;
    [SerializeField] private float walkAcceleration;

    [Header("Running")]
    [SerializeField] private float runMaxSpeed;
    [SerializeField] private float runAcceleration;

    [Header("Crouching")]
    [SerializeField] private float crouchMaxSpeed;
    [SerializeField] private float crouchAcceleration;

    [Header("Additional Configurations")]
    [SerializeField] private float timeSmoothing;
    [SerializeField] private float smoothingThreshold;
    [SerializeField] private float coyoteRunTime;

    public void GetInput(InputAction.CallbackContext context)
    {
        if(context.performed) runAction = true;

        if(context.canceled) runAction = false;
    }

    private void ProcessInput()
    {
        if(runAction && !crouch.IsCrouching && Time.time - physics.LastGroundTime <= coyoteRunTime) isRunning = true;

        if(!runAction) isRunning = false;
    }

    private void Update()
    {
        if(!CheckComponents()) return;

        ProcessInput();

        SetDesiredMaxSpeedAndAcceleration();
        SetCurrentMaxSpeed();

        move.SetMaxSpeed(currentMaxSpeed);
    }

    private void SetDesiredMaxSpeedAndAcceleration()
    {
        float acceleration;

        if(crouch.IsCrouching)
        {
            desiredMaxSpeed = crouchMaxSpeed;
            acceleration = crouchMaxSpeed;
        }

        else if(isRunning)
        {
            desiredMaxSpeed = runMaxSpeed;
            acceleration = runAcceleration;
        }

        else
        {
            desiredMaxSpeed = walkMaxSpeed;
            acceleration = walkAcceleration;
        }

        move.SetAcceleration(acceleration);
    }

    private IEnumerator SmoothlyLerpMaxSpeed()
    {
        float time = 0f;
        float difference = Mathf.Abs(desiredMaxSpeed - currentMaxSpeed) * timeSmoothing;
        float startValue = currentMaxSpeed;

        while(time < difference)
        {
            currentMaxSpeed = Mathf.Lerp(startValue, desiredMaxSpeed, time/difference);
            time += Time.deltaTime;

            yield return null;
        }

        currentMaxSpeed = desiredMaxSpeed;
    }

    private void SetCurrentMaxSpeed()
    {
        if(lastDesiredMaxSpeed == 0) lastDesiredMaxSpeed = desiredMaxSpeed;

        if(Mathf.Abs(desiredMaxSpeed - lastDesiredMaxSpeed) > smoothingThreshold)
        {
            if(speedControl != null) StopCoroutine(speedControl);
            speedControl = StartCoroutine(SmoothlyLerpMaxSpeed());
        } else currentMaxSpeed = desiredMaxSpeed;

        lastDesiredMaxSpeed = desiredMaxSpeed;
    }

    private bool CheckComponents()
    {
        bool integrity = true;

        if(crouch == null)
        {
            Debug.LogError("Crouch Component isn't assigned");
            integrity = false;
        }

        if(rb == null)
        {
            Debug.LogError("Rigidbody isn't assigned");
            integrity = false;
        }

        if(move == null)
        {
            Debug.LogError("Movement Component isn't assigned");
            integrity = false;
        }

        if(physics == null)
        {
            Debug.LogError("Physics Manager isn't assigned");
            integrity = false;
        }

        return integrity;
    }
}

