using Godot;
using System;

public partial class EnemyMover : NavigationAgent3D {
	
	public static System.Collections.Generic.List<EnemyScarer> enemyScarers = new System.Collections.Generic.List<EnemyScarer>();
	
	[Export]
	public float distanceToFlee = 40f;
	[Export]
	public float fleeSpeed = 10;
	
	private Node3D enemyNodeToMove = null;
	private NavigationAgent3D navAgent = null;
	private double newTargetCooldown = 0;

	public override void _Ready() {
		if (enemyNodeToMove==null) {
			enemyNodeToMove = GetParent<Node3D>();
		}
		if (navAgent==null) {
			navAgent = this;
			//if (NavigationAgent3D==typeof(this)) {
			//	navAgent = this;
			//} else {
			//	navAgent = GetNode<NavigationAgent3D>("theNavigationAgent3D");
			//}
		}
	}

	public override void _Process(double delta) {
		newTargetCooldown -= delta;
		if (newTargetCooldown<0) {
			
			var myCachedTransform = enemyNodeToMove.GlobalTransform;
			Vector3 myPos = myCachedTransform.Origin;
			
			Vector3 summedScaryPos = Vector3.Zero;
			int summedScaryPosCount = 0;
			
			for (var i=0; i<enemyScarers.Count; i++) {
				if (myPos.DistanceTo(enemyScarers[i].GlobalTransform.Origin)<enemyScarers[i].scaryRadius) {
					summedScaryPos = enemyScarers[i].GlobalTransform.Origin;
					summedScaryPosCount++;
				}
			}
			
			if (summedScaryPosCount>0) {
				
				Vector3 fleeFromPosFlatY = summedScaryPos/summedScaryPosCount;
				fleeFromPosFlatY.Y = 0;
				
				Vector3 myPosFlatY = myCachedTransform.Origin;
				myPosFlatY.Y = 0;
				Vector3 directionAway = (myPosFlatY-fleeFromPosFlatY).Normalized();
				if (directionAway==Vector3.Zero) {
					directionAway = Vector3.Forward;
				}
				Vector3 projectedPos = fleeFromPosFlatY + directionAway*distanceToFlee;
						
				Vector3 closestPointOnNavmesh =
					NavigationServer3D.MapGetClosestPointToSegment(enemyNodeToMove.GetWorld3D().NavigationMap, projectedPos, projectedPos);
			
				navAgent.SetTargetPosition(closestPointOnNavmesh);
				newTargetCooldown = 0.75;
				
			}
		}
	}
	
	public override void _PhysicsProcess(double delta) {
		if (!navAgent.IsNavigationFinished()) {
			Vector3 nextPos = navAgent.GetNextPathPosition();
			Vector3 dirTowards = (nextPos - enemyNodeToMove.GlobalPosition).Normalized();
			dirTowards.Y = 0;
			enemyNodeToMove.LookAt(enemyNodeToMove.GlobalPosition + dirTowards, Vector3.Up);
			var myCachedTransform = enemyNodeToMove.GlobalTransform;
			myCachedTransform.Origin = myCachedTransform.Origin.MoveToward(nextPos, (float)delta*fleeSpeed);
			enemyNodeToMove.GlobalTransform = myCachedTransform;
		}
	}
	
}
