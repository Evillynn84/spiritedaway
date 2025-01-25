using Godot;
using System;

public partial class EnemyScarer : Marker3D {
	
	[Export]
	public float scaryRadius = 10;

	public override void _Ready() {
		EnemyMover.enemyScarers.Add(this);
	}
	
}
