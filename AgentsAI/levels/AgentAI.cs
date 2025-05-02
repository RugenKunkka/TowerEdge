using Godot;
using System;

public partial class AgentAI : CharacterBody3D
{
	[Export] Node3D targetNode;
	[Export] Label debugLabel;

	[Export] float steeringFactor=2.0f;
	[Export] float maxSteering=1.5f;//es pequeña xq no queres que pase de 0 a 70 en un solo frame, es como la ACELERACION POR FRAME

	[Export(PropertyHint.Range, "0,4,0.1")] float seekFactor=0.7f;
	[Export(PropertyHint.Range, "0,2,0.02")] float maxSeekForce=1.02f;

	[Export(PropertyHint.Range, "0,4,0.01")] float separationFactor=2.07f;
	[Export(PropertyHint.Range, "0,3,0.02")] float maxSeparationForce=1.04f;

	[Export(PropertyHint.Range, "0,4,0.01")] float avoidanceFactor=2.07f;
	[Export(PropertyHint.Range, "0,3,0.02")] float maxAvoidanceForce=1.04f;

	[Export(PropertyHint.Range, "0,5,0.2")] float maxSpeed=2.0f;
	[Export] float avoidForce=8.0f;

	[Export] Area3D area;

    public override void _Ready()
    {
		this.area.AreaEntered+=OnAreaEntered;
    }

    private void OnAreaEntered(Area3D area)
    {
        
    }

    public override void _PhysicsProcess(double delta)
	{
		//primerIntento(delta);
		segundoIntento(delta);
	}

	//-------------------SEGUNDO INTENTO

	public void segundoIntento(double delta){
		float speed=4.0f;
		float separationWeight=1.5f;



		Vector3 targetDir = (targetNode.Position - GlobalTransform.Origin).Normalized();
		Vector3 separation = CalculateSeparationForce() * separationWeight;
		Vector3 avoidance = Vector3.Zero;//CalculateAvoidanceForce() * avoidanceWeight;
		
		if (this.Position!=targetNode.Position)
		{
			Vector3 finalDirection = (targetDir + separation + avoidance).Normalized();
			
			Velocity = finalDirection * speed;
			
			MoveAndSlide();
		}
		else
		{
			Velocity = Vector3.Zero;
		}
	}

	public Vector3 CalculateSeekForce(Vector3 targetPosition)
	{
		Vector3 desiredDirection = (targetPosition - GlobalTransform.Origin).Normalized();
		return desiredDirection;
	}

	public Vector3 CalculateSmartSeparationForce(Vector3 targetPosition)
	{
		float minSeparationZone = 2.0f;
		float distanceToTarget = GlobalTransform.Origin.DistanceTo(targetPosition);

		if (distanceToTarget < minSeparationZone)
			return Vector3.Zero; // se desactiva separación cerca del target

		return CalculateSeparationForce(); // la de antes
	}

	public Vector3 CalculateSeparationForce()
	{
		Vector3 force = Vector3.Zero;

		// Área 3D del agente (debe estar configurada como child del agente)
		Area3D separationArea = GetNode<Area3D>("SeparationArea");
		var bodies = separationArea.GetOverlappingBodies();

		foreach (var body in bodies)
		{
			if (body == this) continue; // ignorarse a sí mismo

			Vector3 direction = GlobalTransform.Origin - ((Node3D)body).GlobalTransform.Origin;
			float distance = direction.Length();

			if (distance > 0)
			{
				direction = direction.Normalized() / distance; // fuerza inversa a la distancia
				force += direction;
			}
		}

		return force.Normalized();
	}

	public float GetSeparationWeight(Vector3 targetPosition)
	{
		float distance = GlobalTransform.Origin.DistanceTo(targetPosition);
		return Mathf.Clamp(distance / 5.0f, 0.0f, 1.0f); // 0 cerca, 1 lejos
	}

/*
	public bool ShouldSmartStop(Vector3 targetPosition, float delta)
	{
		float distance = GlobalTransform.Origin.DistanceTo(targetPosition);
		if (distance > smartStopRadius)
		{
			timeInPlace = 0;
			return false;
		}

		// Verifica si los demás están quietos cerca
		Area3D separationArea = GetNode<Area3D>("SeparationArea");
		var bodies = separationArea.GetOverlappingBodies();

		int stillAgents = 0;
		foreach (var body in bodies)
		{
			if (body == this) continue;
			if (body is CharacterBody3D other)
			{
				if (other.Velocity.Length() < 0.1f)
					stillAgents++;
			}
		}

		if (stillAgents >= 3)
			return true;

		// Si está en lugar mucho tiempo, frena igual
		timeInPlace += delta;
		if (timeInPlace > stopDelay)
			return true;

		return false;
	}*/








	//--------------------PRIMER INTENTO

	public void primerIntento(double delta){
		Vector3 gravity = Vector3.Zero;
		// Add the gravity.
		if (!IsOnFloor())
		{
			gravity += GetGravity() * (float)delta;
		}

		if(this.Position.DistanceTo(targetNode.Position)>2.0f){
			/*
			Vector3 steering=Vector3.Zero;
			steering+=seekSteering(this,targetNode.GlobalPosition);
			//steering+=avoidObstaclesSteering();
			steering=clampedVector3(steering,this.maxSteering);
			separationForcec();*/

			Vector3 seekForce = seekSteering(this,targetNode.GlobalPosition);
			seekForce=seekForce*seekFactor;
			seekForce=clampedVector3(seekForce,maxSeekForce);

			Vector3 separationForce=this.separationForce();
			
			separationForce=separationForce*separationFactor;
			separationForce=clampedVector3(separationForce,maxSeekForce);
			
			float umbral=0.35f;
			//separationForce=Vector3.Zero;
			if(separationForce!=Vector3.Zero && (this.Velocity.Length()>umbral || this.Velocity.Length()<-umbral)) {
				separationForce= new Vector3(-seekForce.X*1.40f,0,0);
			} else if (separationForce!=Vector3.Zero && this.Velocity.Length()>-umbral && this.Velocity.Length()<umbral){
				separationForce=new Vector3(-seekForce.X,0,0);
			}

			
			//separationForce=clampedVector3(separationForce,maxSeparationForce);

			Vector3 avoidanceForce=this.avoidanceForce();
			avoidanceForce=avoidanceForce*avoidanceFactor;
			avoidanceForce=clampedVector3(avoidanceForce,maxAvoidanceForce);

			avoidanceForce=Vector3.Zero;


			Vector3 force = seekForce+separationForce+avoidanceForce;
			if(this.Name.Equals("AgentAI")){
				GD.Print("SeekForce: "+seekForce);
			}

			this.Velocity+=force;
			this.Velocity+=gravity;//NOTA.. la velocidad se aplica calculando globalmente y se aplica de manera global, no importa si el objeto rota
			this.Velocity=clampedVector3(this.Velocity,this.maxSpeed);
			
			String debugInfo = 
				$"seek: {seekForce.Length()}\n"+
                $"separation: {separationForce.Length()}\n" +
               // $"steering: {steeringForce}\n" +
                $"velocity: {this.Velocity.Length()}\n"+
				$"velocityVect: {this.Velocity}\n" ;
            this.debugLabel.Text=debugInfo;
			
			LookAt(targetNode.GlobalPosition);

			MoveAndSlide();
		}

	}

	public Vector3 avoidanceForce(){
		Vector3 avoidanceForce=Vector3.Zero;

		var areas = this.area.GetOverlappingAreas();

		int counter=0;
        foreach (Area3D area in areas)
        {
			float distance=this.GlobalPosition.DistanceTo(area.GlobalPosition);

			Vector3 fromToVector = area.GlobalPosition-this.GlobalPosition;
			Vector3 perpendicular = new Vector3(fromToVector.Z,0,-fromToVector.X);
			if(distance>0){
				avoidanceForce+=((perpendicular).Normalized())/(distance);
			}
			counter++;
        }

		avoidanceForce.Y=0;

		return avoidanceForce;
	}
	public Vector3 separationForceFseek(Vector3 fseek){
		Vector3 separationForce= Vector3.Zero;

		var areas = this.area.GetOverlappingAreas();

		int counter=0;
        foreach (Area3D area in areas)
        {
			float distance=this.GlobalPosition.DistanceTo(area.GlobalPosition);
			if(distance>0){
				separationForce+=(-fseek/0.6f)+fseek;
			}
			counter++;
        }
		if(counter>0){
			separationForce/=counter;
		}
		
		separationForce.Y=0;

		return separationForce;
	}
	public Vector3 separationForce(){
		Vector3 separationForce= Vector3.Zero;

		var areas = this.area.GetOverlappingAreas();

		int counter=0;
        foreach (Area3D area in areas)
        {
			float distance=this.GlobalPosition.DistanceTo(area.GlobalPosition);
			if(distance>0){
				separationForce+=((this.GlobalPosition-area.GlobalPosition).Normalized())/(distance);
			}
			counter++;
        }
		if(counter>0){
			separationForce/=counter;
		}
		
		separationForce.Y=0;

		return separationForce;
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
			Vector3 toReturn=vector.Normalized() * max;;
			return toReturn;
		} 
		return vector;
	}
}
