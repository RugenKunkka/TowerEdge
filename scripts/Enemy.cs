using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Enemy : CharacterBody3D, IDamageDealer
{
	[Export]
    public Node3D targetPosition;
    [Export]
    NavigationRegion3D navigationRegion3D;

    [Export]
    public float speed = 5.0f;

    [Export]
    public float acceleration = 10.0f;

    [Export]
    private NavigationAgent3D navAgent;

    [Export]
    float turnSpeed = 2.0f;
    [Export]
    float minTurnSpeed = 1.0f; // velocidad mínima en rad/s
    [Export]
    float angleTolerance=2.0f;
    [Export]
    Area3D attackRangeArea3d;
    [Export]
    Area3D detectionRangeArea;
    [Export]
    Area3D persecutionRadious;
    [Export]
    float attackRange=1.5f;
    [Export]
    Timer attackCooldownTimer;

    List<House> buildingsList;

    EnumEnemyStates currentState;

    [Export]
    AnimationPlayer attackAnimation;

    [Export]
    float damage =2.0f;
    
    
    bool needReorderBuildingList=false;


    private Vector3 velocity = Vector3.Zero;
    private int rotateDirection=1;

    private bool canAttack=true;

    Vector3 previousTargetPosition= Vector3.Zero;
    Vector3 newTargetPosition= Vector3.Zero;

    PathfindingManager pathFindingManagerInstance;

    public override void _Ready()
    {
        this.pathFindingManagerInstance=PathfindingManager.INSTANCE;
        detectionRangeArea.AreaEntered += OnDetectionRangeAreaEntered;
        detectionRangeArea.AreaExited += OnDetectionRangeAreaExited;

        persecutionRadious.AreaExited += OnPersecutionRadiousAreaExited;

        buildingsList= new List<House>();
        currentState=EnumEnemyStates.IDLE;
        attackCooldownTimer.Timeout += OnAttackCooldownTimeout;
    }

    private void OnPersecutionRadiousAreaExited(Area3D area)
    {
        House building=area.GetParent() as House;
        if(building!=null){
            buildingsList.Remove(building);
            currentState=EnumEnemyStates.RUNNING;
            //needReorderBuildingList=true;
            //canAttack=true;
        }
    }

    private void OnAttackCooldownTimeout()
    {
        canAttack=true;
    }

    private void OnDetectionRangeAreaEntered(Area3D area)
    {
        House building=area.GetParent() as House;
        if(building!=null){
            buildingsList.Add(building);
            needReorderBuildingList=true;
        }
        
    }

    private void OnDetectionRangeAreaExited(Area3D area)
    {
        
    }

    

    public override void _PhysicsProcess(double delta)
    {
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

        //cambiamos de target acá
        
        if(buildingsList!=null && buildingsList.Count!=0){
            newTargetPosition = buildingsList[0].GlobalPosition;
        } else {
            newTargetPosition = targetPosition.GlobalPosition;
        }
        
        
        if(navAgent.TargetPosition!=newTargetPosition){
            this.pathFindingManagerInstance.suscribe(this);
        }
        

        Vector3 distanceToTarget = navAgent.TargetPosition - GlobalPosition;
        distanceToTarget.Y = 0;

        //mover el personaje
        if (!navAgent.IsNavigationFinished() && (distanceToTarget.Length()>attackRange) )
        {
            
            Vector3 nextPathPoint = navAgent.GetNextPathPosition()  ;
            Vector3 direction = (nextPathPoint - GlobalPosition).Normalized();

            // Suavizado con aceleración
            Vector3 forwardDirection = -GlobalTransform.Basis.Z.Normalized();

            velocity = velocity.Lerp(forwardDirection * speed, (float)(acceleration * delta));


            //-----calculo el ángulo X Z entre el target y yo-----
            Vector3 forward = -GlobalTransform.Basis.Z;
            forward.Y = 0;
            forward = forward.Normalized();// normalizás los vectores porque es más conveniente para que no de error el Acos y porque si no estás metiendo la magnitud de los vectores en el medio y eso no es lo que querés.. todo esto es álgebra
            //GD.Print(targetPosition.GlobalPosition+"-this--"+this.GlobalPosition);
            
            
            Vector3 toTarget = direction;//targetPosition.GlobalPosition - this.GlobalPosition; // da un vector que apunta desde este objeto hasta el target.. 
            toTarget.Y = 0;
            toTarget = toTarget.Normalized();

            // Ángulo con signo
            float dot = forward.Dot(toTarget); //haceés el producto punto o escalar
            dot = Mathf.Clamp(dot, -1f, 1f); // previene errores con acos // no se difide por el modulo antes el dotResult porque cuando normalizás los vectores hacés que los módulos de los mismos de 1
            float angleRad = Mathf.Acos(dot);
            float angleDeg = Mathf.RadToDeg(angleRad);
            float crossY = forward.Cross(toTarget).Y;

            // Si el ángulo es suficientemente grande, giramos
            if (angleDeg > angleTolerance)
            {
                float directionSign = Mathf.Sign(crossY); // +1 derecha, -1 izquierda

                
                float smoothFactor = Mathf.Clamp(angleDeg / 90f, 0f, 1f);
                float smoothedTurnSpeed = Mathf.Lerp(minTurnSpeed, turnSpeed, smoothFactor);

                RotateY(directionSign * smoothedTurnSpeed * (float)delta);
                //RotateY(directionSign * turnSpeed * (float)delta);
            }
            
            this.Velocity = velocity;

            MoveAndSlide();
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

    public float getDamageAmount()
    {
        return this.damage;
    }

    public void setTargetPosition(Node3D target){
        this.targetPosition=target;
    }

    public void setNavigationRegion3D(NavigationRegion3D navigationRegion3D){
        this.navigationRegion3D=navigationRegion3D;
    }

    public void updatePath(){
        navAgent.TargetPosition=newTargetPosition;
        previousTargetPosition=newTargetPosition;
    }
}
