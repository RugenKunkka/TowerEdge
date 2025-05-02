using Godot;
using System;
using System.Collections.Generic;

public class FlockingController
{
	SpatialHashMapping spatialHashMapping;
	Node3D nodeFrom;
	float detectionRadius;

	float coeficientSeparationForce=0.3f;
	float coeficientAligmentForce=1.0f;
	float coeficientCohesionForce=1.0f;

	public FlockingController(Node3D nodeFrom, float detectionRadius){
		this.spatialHashMapping=SpatialHashMapping.INSTANCE;
		this.nodeFrom=nodeFrom;
		this.detectionRadius=detectionRadius;
	}

	private List<Node3D> getNeightbours(){
		return this.spatialHashMapping.findNearbyAgents(this.nodeFrom,this.detectionRadius);
	}

	public Vector3 calculateCohesionForce(List<Node3D> neightbours){
		Vector3 cohesionForce=Vector3.Zero;
		foreach(Node3D neightbour in neightbours){
			cohesionForce+=neightbour.GlobalPosition;
		}
		if(cohesionForce!=Vector3.Zero && neightbours.Count!=0){
			cohesionForce/=neightbours.Count;
			cohesionForce=cohesionForce.Normalized();
		}
		
		return cohesionForce;
	}

//todos los agentes que están cerca, ejercen una fuera contraria al nodo nodeFrom
	public Vector3 calculateSeparationForce(List<Node3D> neightbours){
		Vector3 separationForce=Vector3.Zero;
		foreach (Node3D neightbour in neightbours){
            //Vector3 direction = neightbour.GlobalPosition - nodeFrom.GlobalPosition;
			Vector3 direction = nodeFrom.GlobalPosition - neightbour.GlobalPosition;
			float distance=direction.Length();
			Vector3 normalizedDirection=direction.Normalized();

			if(distance>0){
				separationForce+=normalizedDirection/distance;//Hace que a medida que la distancia se achique sea mas fuerte al fuerza y mientras mas lejos este mas debil sea
			}
			
		}
		return separationForce;
	}

	// La función para calcular la fuerza de Steering
    public Vector3 calculateSteeringForce(Node3D agent, Vector3 targetPosition, float maxSpeed, Vector3 currentVelocity)
	{
		// 1. Obtener la dirección deseada hacia el objetivo
		Vector3 desiredVelocity = (targetPosition - agent.GlobalPosition).Normalized() * maxSpeed;

		// 2. Invertir la dirección en el eje -Z si no apunta hacia adelante
		if (desiredVelocity.Z > 0) 
		{
			desiredVelocity.Z = -desiredVelocity.Z; // Invertimos el Z para asegurar que avance en -Z
		}

		// 3. Calcular la diferencia entre la velocidad deseada y la actual
		Vector3 steeringForce = desiredVelocity - currentVelocity;

		// 4. Retornar la fuerza de steering
		return steeringForce;
	}

	public Vector3 CalculateSIC(Node3D currentPosition, float detectionRadius)
	{
		Vector3 avoidanceForce = Vector3.Zero;

		var obstacles = this.spatialHashMapping.findNearbyAgents(currentPosition, detectionRadius);

		foreach (var obstacle in obstacles)
		{
			if (obstacle == currentPosition)
				continue;

			Vector3 toObstacle = obstacle.GlobalPosition - currentPosition.GlobalPosition;
			float distance = toObstacle.Length();

			if (distance > 0 && distance < detectionRadius)
			{
				// Evitar desviándonos perpendicularmente
				Vector3 perpendicular = toObstacle.Cross(Vector3.Up).Normalized();
				avoidanceForce += perpendicular / distance;
			}
		}

		return avoidanceForce.Normalized();
	}

	public Vector3 calculateEvasionForce(Node3D agent, List<Node3D> neighbors, float evadeStrength, float evadeRadius)
	{
		Vector3 totalEvadeForce = Vector3.Zero;
		int evadeCount = 0;

		foreach (var neighbor in neighbors)
		{
			Vector3 toNeighbor = neighbor.GlobalPosition - agent.GlobalPosition;
			float distance = toNeighbor.Length();

			if (distance > 0f && distance < evadeRadius)
			{
				// Cálculo del vector lateral perpendicular
				Vector3 lateral = toNeighbor.Cross(Vector3.Up).Normalized();

				// Sumamos la fuerza lateral
				// Opcional: Podrías hacer que la fuerza dependa de la cercanía
				float strengthFactor = (evadeRadius - distance) / evadeRadius; // Más fuerte si más cerca
				Vector3 evadeForce = lateral * evadeStrength * strengthFactor;

				totalEvadeForce += evadeForce;
				evadeCount++;
			}
		}

		// Si hay varios vecinos, hacemos un promedio para no sobrepotenciar
		if (evadeCount > 0)
		{
			totalEvadeForce /= evadeCount;
		}

		return totalEvadeForce;
	}

	//tiene que ver con el punto al cual se dirigen...y sabiendo el forward sacás eso 
	public Vector3 calculateAlligmentForce(List<Node3D> neightbours){
		Vector3 aligmentForce=Vector3.Zero;
		foreach(Node3D neightbour in neightbours){
			aligmentForce+=-neightbour.GlobalTransform.Basis.Z;
		}
		if(aligmentForce!=Vector3.Zero){
			aligmentForce=aligmentForce.Normalized();
		}
		return aligmentForce;
	}


	public Vector3 calculateAntiOverlapForce(Vector3 selfPosition, List<Node3D> neighbors, float antiOverlapDistance = 0.6f, float antiOverlapStrength = 8.0f)
	{
		Vector3 force = Vector3.Zero;

		foreach (var neighbor in neighbors)
		{
			if (neighbor == null || neighbor == this.nodeFrom) continue;

			Vector3 neighborPos = neighbor.GlobalTransform.Origin;
			Vector3 offset = selfPosition - neighborPos;
			float distance = offset.Length();

			if (distance < antiOverlapDistance && distance > 0.01f)
			{
				float strength = antiOverlapStrength / (distance * distance);
				Vector3 push = offset.Normalized() * strength;
				force += push;
			}
		}

		return force;
	}

	public Vector3 calculateFlockingForce(){
		Vector3 totalForce =Vector3.Zero;

		List<Node3D> neightbourNodes= getNeightbours();
		Vector3 separationForce = this.calculateSeparationForce(neightbourNodes);
		Vector3 alligmentForce =  this.calculateAlligmentForce(neightbourNodes);
		Vector3 cohesionForce = this.calculateCohesionForce(neightbourNodes);

		Vector3 overlappingForce= calculateAntiOverlapForce(nodeFrom.GlobalPosition,neightbourNodes,1.0f,100.0f);

		totalForce= separationForce*coeficientSeparationForce+alligmentForce*coeficientAligmentForce+cohesionForce*coeficientCohesionForce
		;//+overlappingForce;

		return totalForce;
	}

	public Vector3 calculateAvoidanceForce(){
		Vector3 avoidanceForce=Vector3.Zero;

		List<Node3D> neighbors=this.spatialHashMapping.findNearbyAgents(this.nodeFrom,2.5f);
		//tengo los nodos que están cerca o mejor dicho.. los que se interponen en mi paso

		// tengo que analizar en base a la ubicación de estos nodos, para donde tengo que ir, si izquierda

		return avoidanceForce;
	}

	public Vector3 calculateTotalForce(){
		Vector3 avoidanceForce=Vector3.Zero;
		
		avoidanceForce=calculateAvoidanceForce();//calculateFlockingForce();

		return avoidanceForce;
	}


}
