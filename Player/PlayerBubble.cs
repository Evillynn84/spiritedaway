using Godot;
using System;

public partial class PlayerBubble : Node3D
{
	private void OnBodyEntered(Node3D body)
	{
		// Is player - setup colliding bubble
		Player player = (Player)body;
		if (player != null)
		{
			player.SetCollidingBubble(this);
		}
	}

	private void OnBodyExited(Node3D body)
	{
		// Is player - remove colliding bubble
		Player player = (Player)body;
		if (player != null)
		{
			player.SetCollidingBubble(null);
		}
	}
}
