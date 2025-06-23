using Godot;

[GlobalClass]
public partial class FreeLookCamera : Camera3D
{
    // Constants
    private const float SHIFT_MULTIPLIER = 10f;
    private const float ALT_MULTIPLIER = 1f / SHIFT_MULTIPLIER;
    private const float CHECKPOINT_DIST_DELTA_THRESHOLD = 50f;
    private const float DEFAULT_SPEED = 25f;
    private const float SIGNAL_COOLDOWN_TIME = 1f; // 1 second cooldown

    // Exported properties
    [Export(PropertyHint.Range, "0.0,1.0")]
    public float Sensitivity { get; set; } = 0.25f;

    // Mouse state
    private Vector2 _mousePosition = Vector2.Zero;
    private float _totalPitch = 0f;

    // Movement state
    private Vector3 _direction = Vector3.Zero;
    private Vector3 _velocity = Vector3.Zero;
    private float _acceleration = 30f;
    private float _deceleration = -10f;
    private float _velMultiplier = 25f;

    // Keyboard state
    private bool _w, _s, _a, _d, _q, _e, _shift, _alt;

    private float _speedMulti = DEFAULT_SPEED;
    private Vector3 _checkpointPosition = Vector3.Zero;

    // Signal cooldown
    private float _lastSignalTime = 0f;

    public override void _Ready()
    {
        _checkpointPosition = GlobalTransform.Origin;
        // Defer the PlayerSpawn signal emission to ensure all subscribers are ready
        Callable.From(() => EmitPlayerSpawnSignal()).CallDeferred();

        GD.Print($"[FreeLookCamera] Player spawned at position: {GlobalTransform.Origin}");
    }

    private void EmitPlayerSpawnSignal()
    {
        SignalBus.Instance.CallDeferred(
            "emit_signal",
            SignalBus.SignalName.PlayerSpawn,
            GlobalTransform.Origin
        );
        GD.Print($"[FreeLookCamera] PlayerSpawn signal emitted at position: {GlobalTransform.Origin}");
    }

    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion mouseMotion:
                _mousePosition = mouseMotion.Relative;
                break;

            case InputEventMouseButton mouseButton:
                HandleMouseButton(mouseButton);
                break;

            case InputEventKey keyEvent:
                HandleKeyEvent(keyEvent);
                break;
        }
    }

    private void HandleMouseButton(InputEventMouseButton mouseButton)
    {
        switch (mouseButton.ButtonIndex)
        {
            case MouseButton.Right:
                Input.MouseMode = mouseButton.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
                break;

            case MouseButton.WheelUp:
                _velMultiplier = Mathf.Clamp(_velMultiplier * 1.1f, 0.2f, 200f);
                break;

            case MouseButton.WheelDown:
                _velMultiplier = Mathf.Clamp(_velMultiplier / 1.1f, 0.2f, 200f);
                break;
        }
    }

    private void HandleKeyEvent(InputEventKey keyEvent)
    {
        bool pressed = keyEvent.Pressed;

        switch (keyEvent.Keycode)
        {
            case Key.W: _w = pressed; break;
            case Key.S: _s = pressed; break;
            case Key.A: _a = pressed; break;
            case Key.D: _d = pressed; break;
            case Key.Q: _q = pressed; break;
            case Key.E: _e = pressed; break;
            case Key.Shift: _shift = pressed; break;
            case Key.Alt: _alt = pressed; break;
        }
    }

    public override void _Process(double delta)
    {
        UpdateLazyEvaluationCheckpoint();
        UpdateMouselook();
        UpdateMovement((float)delta);
    }

    private float ComputeDistanceFromCheckpointInXZ()
    {
        var checkpointXZ = new Vector2(_checkpointPosition.X, _checkpointPosition.Z);
        var currentXZ = new Vector2(GlobalTransform.Origin.X, GlobalTransform.Origin.Z);
        return checkpointXZ.DistanceTo(currentXZ);
    }

    private bool _pendingSignalEmission = false;

    private void UpdateLazyEvaluationCheckpoint()
    {
        float distanceFromCheckpoint = ComputeDistanceFromCheckpointInXZ();

        if (distanceFromCheckpoint > CHECKPOINT_DIST_DELTA_THRESHOLD && !_pendingSignalEmission)
        {
            // Check cooldown
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            float timeSinceLastSignal = currentTime - _lastSignalTime;

            if (timeSinceLastSignal >= SIGNAL_COOLDOWN_TIME)
            {
                // Set flag to prevent multiple emissions
                _pendingSignalEmission = true;

                // Update checkpoint FIRST to prevent multiple frames from meeting the threshold
                Vector3 oldCheckpoint = _checkpointPosition;
                _checkpointPosition = GlobalTransform.Origin;
                _lastSignalTime = currentTime;

                // Emit signal through SignalBus
                var signalBus = GetNode<SignalBus>("/root/SignalBus");
                signalBus.EmitSignal(SignalBus.SignalName.ReconstructNodes,
                    oldCheckpoint,
                    GlobalTransform.Origin,
                    distanceFromCheckpoint);

                // Clear the flag after updating checkpoint
                _pendingSignalEmission = false;

                // GD.Print($"Emitted ReconstructNodes signal (distance: {distanceFromCheckpoint:F1}, cooldown: {timeSinceLastSignal:F1}s)");
            }
        }
    }

    private void UpdateMovement(float delta)
    {
        // Compute desired direction from key states
        _direction = new Vector3(
            (_d ? 1f : 0f) - (_a ? 1f : 0f),
            (_e ? 1f : 0f) - (_q ? 1f : 0f),
            (_s ? 1f : 0f) - (_w ? 1f : 0f)
        );

        // Compute the change in velocity due to desired direction and "drag"
        var offset = _direction.Normalized() * _acceleration * _velMultiplier * delta +
                     _velocity.Normalized() * _deceleration * _velMultiplier * delta;

        // Compute modifiers' speed multiplier
        float currentSpeedMulti = _speedMulti;
        if (_shift) currentSpeedMulti *= SHIFT_MULTIPLIER;
        if (_alt) currentSpeedMulti *= ALT_MULTIPLIER;

        // Check if we should bother translating the camera
        if (_direction == Vector3.Zero && offset.LengthSquared() > _velocity.LengthSquared())
        {
            // Set velocity to 0 to prevent jittering due to imperfect deceleration
            _velocity = Vector3.Zero;
        }
        else
        {
            // Clamp speed to stay within maximum value
            _velocity.X = Mathf.Clamp(_velocity.X + offset.X, -_velMultiplier, _velMultiplier);
            _velocity.Y = Mathf.Clamp(_velocity.Y + offset.Y, -_velMultiplier, _velMultiplier);
            _velocity.Z = Mathf.Clamp(_velocity.Z + offset.Z, -_velMultiplier, _velMultiplier);

            Translate(_velocity * delta * currentSpeedMulti);
        }
    }

    private void UpdateMouselook()
    {
        // Only rotate mouse if the mouse is captured
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _mousePosition *= Sensitivity;
            float yaw = _mousePosition.X;
            float pitch = _mousePosition.Y;
            _mousePosition = Vector2.Zero;

            // Prevent looking up/down too far
            pitch = Mathf.Clamp(pitch, -90 - _totalPitch, 90 - _totalPitch);
            _totalPitch += pitch;

            RotateY(Mathf.DegToRad(-yaw));
            RotateObjectLocal(Vector3.Right, Mathf.DegToRad(-pitch));
        }
    }

    // For UI integration
    public void OnSpeedSliderValueChanged(float newSpeed)
    {
        _speedMulti = newSpeed;
    }
}
