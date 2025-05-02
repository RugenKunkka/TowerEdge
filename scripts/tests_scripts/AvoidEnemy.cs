using Godot;
using System;

public partial class AvoidEnemy : CharacterBody3D
{
	
	
	[Export] Node3D targetNode;

	[Export] float maxSteering=0.25f;//es pequeña xq no queres que pase de 0 a 70 en un solo frame, es como la ACELERACION POR FRAME
	[Export] float maxSpeed=5.0f;
	[Export] float avoidForce=8.0f;

	[Export] Godot.Collections.Array<RayCast3D> raycastList;
	 

    public override void _Ready()
    {
        
    }
	
	public override void _PhysicsProcess(double delta)
	{
		
		Vector3 gravity = Vector3.Zero;
		// Add the gravity.
		if (!IsOnFloor())
		{
			gravity += GetGravity() * (float)delta;
		}

		if(this.Position.DistanceTo(targetNode.Position)>2.0f){
			Vector3 steering=Vector3.Zero;
			steering+=seekSteering(this,targetNode.GlobalPosition);
			steering+=avoidObstaclesSteering();
			steering=clampedVector3(steering,this.maxSteering);

			this.Velocity+=steering;
			this.Velocity+=gravity;//NOTA.. la velocidad se aplica calculando globalmente y se aplica de manera global, no importa si el objeto rota
			this.Velocity=clampedVector3(this.Velocity,this.maxSpeed);
			LookAt(targetNode.GlobalPosition);
			MoveAndSlide();
		}
		
	}

	private Vector3 seekSteering(CharacterBody3D node, Vector3 pointTo){

		Vector3 desiredVelocity = (pointTo-node.GlobalPosition).Normalized()*maxSpeed;//calculo la dirección a la que quiero ir , normalizo y multiplico para obtener la velocidad
		Vector3 seekSteeringForce=desiredVelocity-node.Velocity;// es la velocidad de corrección, es cuanto me falta a nivel vectorial para ir hacia donde quiero

		return seekSteeringForce;
	}

	


	//no permite que el vector exceda una magnitud determinada
	public static Vector3 clampedVector3(Vector3 vector, float max)
    {
        if (vector.Length() > max){
			return vector.Normalized() * max;
		} 
		return vector;
	}


	private Vector3 avoidObstaclesSteering(){
		Vector3 obstaclesSteering =  Vector3.Zero;
		foreach (RayCast3D raycast in this.raycastList){
			raycast.TargetPosition = new Vector3(this.Velocity.Length(),0,0);
			if(raycast.IsColliding()){
				PhysicsBody3D obstacle= (PhysicsBody3D)raycast.GetCollider();
				obstaclesSteering=(this.Position+this.Velocity-obstacle.Position).Normalized()*avoidForce;
				obstaclesSteering.Y=0;
				return obstaclesSteering;
			}
		}
		

		return obstaclesSteering;
	}

}
