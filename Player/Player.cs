using Godot;
using System;

public partial class Player : CharacterBody3D
{
	// Movement
	[Export]
	public float _bubbleSpeed = 5.0f;
	[Export]
	public float _outSpeed = 3.0f;
	public const float JumpVelocity = 4.5f;

	// Rotation
	[Export]
	public float _mouseSensitiviy = 0.5f;
	[Export]
	public float _rotationVLimit = 50f;
	[Export]
	public Node3D _cameraHolder;

	public float _cameraRotationV = 0;

	// Bubble
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
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
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
	}

	public override void _Input(InputEvent @event)
    {
		// Handle mouse rotation
		InputEventMouseMotion mouseEvent = (InputEventMouseMotion)@event;

		if (mouseEvent != null)
		{
			// Horizontal player rotation
			RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * _mouseSensitiviy));

			/*
			// Vertical camera rotation
			float changeV = -mouseEvent.Relative.Y * _mouseSensitiviy;
			float rotationVUpdated = _cameraRotationV + changeV;

			// Check vertical limit
			if (rotationVUpdated > -_rotationVLimit && rotationVUpdated < _rotationVLimit)
			{
				_cameraRotationV = rotationVUpdated; 
				_cameraHolder.RotateX(Mathf.DegToRad(_cameraRotationV));
			}
			*/
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
}
