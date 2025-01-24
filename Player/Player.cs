using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[Export]
	public float _mouseSensitiviy = 0.5f;
	[Export]
	public float _rotationVLimit = 50f;
	[Export]
	public Node3D _cameraHolder;

	public float _cameraRotationV = 0;

	public override void _PhysicsProcess(double delta)
	{
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
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}


		// Rotate 

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
}
