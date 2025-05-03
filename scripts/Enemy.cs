using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Enemy : CharacterBody3D, IDamageDealer
{
    [Export] NavigationRegion3D navigationRegion3D;
    [Export] Node3D targetNode;
	[Export] Label debugLabel;

	[Export] float steeringFactor=2.0f;
	[Export] float maxSteering=1.5f;//es pequeña xq no queres que pase de 0 a 70 en un solo frame, es como la ACELERACION POR FRAME

	[Export(PropertyHint.Range, "0,4,0.1")] float seekFactor=0.7f;
	[Export(PropertyHint.Range, "0,2,0.02")] float maxSeekForce=1.02f;

	[Export(PropertyHint.Range, "0,4,0.01")] float separationFactor=2.07f;
	[Export(PropertyHint.Range, "0,3,0.02")] float maxSeparationForce=0.98f;

	[Export(PropertyHint.Range, "0,4,0.01")] float avoidanceFactor=2.07f;
	[Export(PropertyHint.Range, "0,3,0.02")] float maxAvoidanceForce=1.04f;

	[Export(PropertyHint.Range, "0,5,0.2")] float maxSpeed=1.4f;
	[Export] float avoidForce=8.0f;
    [Export] Timer attackCooldownTimer;
    [Export] NavigationAgent3D navAgent;

	[Export] Area3D area;
    [Export] float damage = 1.0f;

	SpatialHashMapping spatialHashMapping;
    PathfindingManager pathFindingManagerInstance;

    List<House> buildingsList;
    EnumEnemyStates currentState;
    
    Vector3 oldPosition= Vector3.Zero;
    Vector3 newTargetPosition= Vector3.Zero;
    Vector3 previousTargetPosition=Vector3.Zero;
    bool canAttack=true;
    [Export] float attackRange=1.5f;
    [Export] AnimationPlayer attackAnimation;
    bool needReorderBuildingList=false;



    public override void _Ready()
    {
        spatialHashMapping=SpatialHashMapping.INSTANCE;
		spatialHashMapping.insertFull(this);
        this.pathFindingManagerInstance=PathfindingManager.INSTANCE;

        buildingsList= new List<House>();
        currentState=EnumEnemyStates.IDLE;
        attackCooldownTimer.Timeout += OnAttackCooldownTimeout;

        //setupAreas();
    }

    public override void _PhysicsProcess(double delta)
    {

        Vector3 gravity = Vector3.Zero;
		// Add the gravity.
		if (!IsOnFloor())
		{
			gravity += GetGravity() * (float)delta;
		}


        if(this.Position!=oldPosition){
            this.spatialHashMapping.update(this);
            oldPosition=this.Position;
        }
        reorderBuildingsList();

        //cambiamos de target acá
        changeTarget();
        
        //si el target es distinto.. hacemos suscribe para agregarlo a la cola de espera de recálculo de path por cambio de target
        if(navAgent.TargetPosition!=newTargetPosition){
            this.pathFindingManagerInstance.suscribe(this);
        }
        
        Vector3 distanceToTarget = navAgent.TargetPosition - GlobalPosition;
        distanceToTarget.Y = 0;

        //mover el personaje
        if (!navAgent.IsNavigationFinished() && (distanceToTarget.Length()>attackRange) )
        {
            Vector3 seekForce = seekSteering(this,targetNode.GlobalPosition);
			seekForce=seekForce*seekFactor;
			seekForce=clampedVector3(seekForce,maxSeekForce);

			Vector3 separationForce=this.separationForce();
			separationForce=separationForce*separationFactor;
			separationForce=clampedVector3(separationForce,maxSeparationForce);
			

			Vector3 avoidanceForce=this.avoidanceForce();
			avoidanceForce=avoidanceForce*avoidanceFactor;
			avoidanceForce=clampedVector3(avoidanceForce,maxAvoidanceForce);

			avoidanceForce=Vector3.Zero;


			Vector3 force = seekForce+separationForce+avoidanceForce;

			this.Velocity+=force;
			this.Velocity+=gravity;//NOTA.. la velocidad se aplica calculando globalmente y se aplica de manera global, no importa si el objeto rota
			this.Velocity=clampedVector3(this.Velocity,this.maxSpeed);
			
			
			
			LookAt(targetNode.GlobalPosition);

			MoveAndSlide();
            //simpleMovement(avoidanceForce,this.targetPosition.GlobalPosition,3.0f);


            //GD.Print("La fuerza de flocking es: "+flockingForce);
            currentState=EnumEnemyStates.RUNNING;
            //si la navegación terminó y está en rango de ataque...
        } else if (currentState==EnumEnemyStates.RUNNING && distanceToTarget.Length()<=attackRange){
            currentState=EnumEnemyStates.ATACKING;
        } else if(currentState==EnumEnemyStates.ATACKING  && canAttack){
            attackAnimation.Play("MeleeAttack");
            canAttack=false;
            attackCooldownTimer.Start();
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
				separationForce+=((this.GlobalPosition-area.GlobalPosition))/(distance*distance*distance);
			}
			counter++;
        }
		if(counter>0){
			separationForce/=counter;
		}
		
		separationForce.Y=0;
		separationForce=separationForce.Normalized();
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
	



    // La función para calcular la fuerza de Separation
    
    private void applyMovement(Vector3 evationForce,Vector3 steeringForce, Vector3 separationForce, Vector3 sicForce, float MaxForce, float maxSpeed, double delta)
    {
        // 1. Sumar fuerzas
        Vector3 totalForce = steeringForce + separationForce*2.0f + sicForce+evationForce;
        this.debugLabel.Text=(this.debugLabel.Text+$"\ntotalForce:{totalForce}");
        totalForce=new Vector3(totalForce.X,0,totalForce.Z);

        



        // 2. Limitar la fuerza total
        if (totalForce.Length() > MaxForce)
            totalForce = totalForce.Normalized() * MaxForce;

        if (separationForce.Length()*10 > 2.0) // si la fuerza de separación es fuerte
        {
            float rotationAngle = Mathf.DegToRad(90); // 20 grados para esquivar
            // Podés hacer que se alterne izquierda/derecha o calcularlo según cross product
            //totalForce = totalForce.Rotated(Vector3.Up, rotationAngle);
        }



        // 3. Aplicar la fuerza como aceleración
        Velocity += totalForce * (float)delta;

        // 4. Limitar la velocidad horizontal (X,Z) pero conservar Y
        Vector3 horizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);

        if (horizontalVelocity.Length() > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.Normalized() * maxSpeed;
            Velocity = new Vector3(horizontalVelocity.X, 0, horizontalVelocity.Z);
        }

        // 5. Aplicar gravedad manualmente si es necesario
        //Velocity.Y += (float)(ProjectSettings.GetSetting("physics/3d/default_gravity")) * (float)delta;

        // 6. Mover el cuerpo
        MoveAndSlide();
    }


    




    private void OnAttackCooldownTimeout()
    {
        canAttack=true;
    }


    
    private void reorderBuildingsList(){
        //buscamos reordenar los objetivos acá para darle priorirdad a los más cercanos y no irse al más lejano en caso de haber sido el más lejano el que se encontró primero
        if (needReorderBuildingList && buildingsList.Count!=0){
            // 1. Obtener el mapa de navegación actual
            Rid mapRID = navigationRegion3D.GetNavigationMap();

            // 2. Crear un diccionario para almacenar distancias reales
            Dictionary<House, float> buildingDistances = new Dictionary<House, float>();

            foreach (House building in buildingsList)
            {
                // 3. Calcular el camino desde el enemigo a esta building
                Vector3[] path = NavigationServer3D.MapGetPath(
                    mapRID,
                    GlobalPosition,
                    building.GlobalPosition,
                    false // no optimizar el camino, queremos el completo
                );

                // 4. Sumar la distancia entre cada par de puntos del path
                float totalLength = 0f;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    totalLength += path[i].DistanceTo(path[i + 1]);
                }

                buildingDistances[building] = totalLength;
            }

            // 5. Ordenar la lista original según la distancia del path
            buildingsList = buildingDistances
                .OrderBy(pair => pair.Value)
                .Select(pair => pair.Key)
                .ToList();

            needReorderBuildingList = false;
        }
    }



    public void changeTarget(){
        if(buildingsList!=null && buildingsList.Count!=0){
            newTargetPosition = buildingsList[0].GlobalPosition;
        } else {
            newTargetPosition = targetNode.GlobalPosition;
        }
    }


    void lookAtTargetInstant(Node3D target)
    {
        Vector3 toTarget = (target.GlobalTransform.Origin - GlobalTransform.Origin).Normalized();
        toTarget.Y = 0; // ignoramos la altura para rotar en plano XZ

        if (toTarget.Length() == 0)
            return;

        // Rotamos el personaje para que su -Z apunte hacia el target
        LookAt(GlobalTransform.Origin + toTarget, Vector3.Up);
    }

    
    
    public float getDamageAmount()
    {
        return this.damage;
    }

    public void setTargetPosition(Node3D target){
        this.targetNode=target;
    }

    public void setNavigationRegion3D(NavigationRegion3D navigationRegion3D){
        this.navigationRegion3D=navigationRegion3D;
    }

    public void updatePath(){
        navAgent.TargetPosition=newTargetPosition;
        previousTargetPosition=newTargetPosition;
    }

    public override void _ExitTree()
    {
        spatialHashMapping.deleteFull(this);
    }
}
