using Godot;
using System;

public partial class Obstacle : Node3D
{
	[Export]
	public int _dmg = 1;
	[Export]
	public bool _changeMovement = false;
	[Export]
	public float _movementSpeed = 1;
	[Export]
	public float _jumpVelocity = 1;

	private void OnBodyEntered(Node3D body)
	{
		// Is player
		Player player = (Player)body;
		if (player != null)
		{
			// Apply dmg
			if (_dmg > 0)
				player.SubtractHP(_dmg);

			// Apply speed change
			if (_changeMovement)
			{
				player.OverrideSpeed(_movementSpeed);
				player.OverrideJumpVelocity(_jumpVelocity);
			}
		}
	}

	private void OnBodyExited(Node3D body)
	{
		// Is player
		Player player = (Player)body;
		if (player != null)
		{
			// Clear speed
			if (_changeMovement)
			{
				player.ClearOverridenSpeed();
				player.ClearOverridenJump();
			}
		}
	}
}
