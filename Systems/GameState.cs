using Godot;
using System;

public partial class GameState : Node
{
	private static GameState _instance;

	public static GameState GetInstance()
	{
		return _instance;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;
		Input.MouseMode = Input.MouseModeEnum.Captured;  
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RestartLevel()
	{
		GetTree().ReloadCurrentScene();
	}
}
