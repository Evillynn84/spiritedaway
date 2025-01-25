using Godot;
using System;

public partial class Player : CharacterBody3D
{
	// Movement
	[ExportCategory("Movement")]
	[Export]
	public float _bubbleSpeed = 5.0f;
	[Export]
	public float _outSpeed = 3.0f;
	[Export]
	public float _bubbleJumpVelocity = 10f;
	[Export]
	public float _outJumpVelocity = 4.5f;
	[Export]
	public float _jumpGravityModified = 1.5f;
	[Export]
	public float _fallGravityModified = 1.5f;

	// Rotation
	[ExportCategory("Rotation and Cam")]
	[Export]
	public float _mouseSensitivity = 0.5f;
	[Export]
	public float _rotationVLimit = 50f;
	[Export]
	public Node3D _cameraHolder;
	[Export]
	public Camera3D _camera;
	[Export]
	public RayCast3D _cameraRayCast;
	[Export]
	public float _cameraLerpSpeed = 0.2f;

	public float _cameraRotationV = 0;

	// Bubble
	[ExportCategory("Bubble")]
	[Export]
	public PlayerBubble _controlledBubble;

	public PlayerBubble _collidingBubble;

    //--------------------------------------------------
    // Overrides
    //--------------------------------------------------

    public override void _Process(double delta)
	{
		base._Process(delta);

		// Enter bubble
		if (Input.IsActionJustPressed("pl_enterBubble"))
		{
			if (_controlledBubble == null)
				ControlBubble(_collidingBubble);
			else
				LeaveBubble();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		ControlBubble(_controlledBubble);

		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			if (velocity.Y > 0)
				velocity += GetGravity() * (float)delta * _jumpGravityModified;
			else
				velocity += GetGravity() * (float)delta * _fallGravityModified;
		}

		// Handle Jump.
		float jumpVelocity = _bubbleJumpVelocity;
		if (_controlledBubble == null)
			jumpVelocity = _outJumpVelocity;

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = jumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("pl_left", "pl_right", "pl_front", "pl_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float curSpeed = _bubbleSpeed;
		if (_controlledBubble == null)
			curSpeed = _outSpeed;

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * curSpeed;
			velocity.Z = direction.Z * curSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, curSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, curSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();

		// Camera
		HandleCamera();
	}

	public override void _Input(InputEvent @event)
	{
		// Handle mouse rotation
		InputEventMouseMotion mouseEvent = (InputEventMouseMotion)@event;

		if (mouseEvent != null)
		{
			// Horizontal player rotation
			RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * _mouseSensitivity));

			float xRot = Mathf.Clamp(Mathf.DegToRad(-mouseEvent.Relative.Y * _mouseSensitivity), -_mouseSensitivity, _mouseSensitivity);
			//_cameraHolder.RotateX(xRot);

			// Vertical camera rotation
			float changeV = -mouseEvent.Relative.Y * _mouseSensitivity;
			float rotationVUpdated = Mathf.RadToDeg(_cameraHolder.Rotation.X) + changeV;

			// Check vertical limit
			if (rotationVUpdated > -_rotationVLimit && rotationVUpdated < _rotationVLimit)
			{
				_cameraRotationV = _cameraRotationV + changeV; 
				_cameraHolder.RotateX(Mathf.DegToRad(changeV));
			}
		}
	}

	//--------------------------------------------------
	// Methods
	//--------------------------------------------------

	public void SetCollidingBubble(PlayerBubble bubble)
	{
		_collidingBubble = bubble;
	}

	public void ControlBubble(PlayerBubble bubble)
	{
		if (bubble == null)
			return;

		bubble.Reparent(this);
		bubble.Position = Vector3.Zero;

		_controlledBubble = bubble;
	}

	public void LeaveBubble()
	{

		_controlledBubble.Reparent(GetTree().Root);
		//_controlledBubble.Position = Position;
		_controlledBubble = null;
	}

	private void HandleCamera()
	{
		// Check obstacles
		if (!_cameraRayCast.IsColliding())
		{
			//_camera.Position = _camera.GlobalPosition.Lerp(_cameraRayCast.TargetPosition, _cameraLerpSpeed);
			//_camera.Position = _cameraRayCast.TargetPosition;
			Vector3 target = _cameraHolder.GlobalTransform * _cameraRayCast.TargetPosition;
			//target = target.Rotated(_cameraRayCast.Rotation.);
			_camera.GlobalPosition = _camera.GlobalPosition.Lerp(target, _cameraLerpSpeed);
			return;
		}

		// Move camera
		_camera.GlobalPosition = _camera.GlobalPosition.Lerp(_cameraRayCast.GetCollisionPoint(), _cameraLerpSpeed);
	}
}
