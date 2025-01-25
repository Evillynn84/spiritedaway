using Godot;
using System;

public partial class Obstacle : Node3D
{
	[Export]
	public int _dmg = 1;
	[Export]
	public bool _changeSpeed = false;
	[Export]
	public float _movementSpeed = 1;

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
			if (_changeSpeed)
				player.OverrideSpeed(_movementSpeed);
		}
	}

	private void OnBodyExited(Node3D body)
	{
		// Is player
		Player player = (Player)body;
		if (player != null)
		{
			// Clear speed
			if (_changeSpeed)
				player.ClearOverridenSpeed();
		}
	}
}
