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

    [Export] public float Gravity = 20.0f;

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
    float attackRange=1.7f;
    [Export]
    Timer attackCooldownTimer;

    List<House> buildingsList;

    EnumEnemyStates currentState;

    [Export]
    AnimationPlayer attackAnimation;

    [Export]
    float damage =2.0f;
    [Export]
    EnemyDebugLabel debugLabel;

    Rid persecutionArea;
    Rid persecutionShape;

    FlockingController flockingController;
    
    bool needReorderBuildingList=false;


    private Vector3 velocity = Vector3.Zero;
    private int rotateDirection=1;

    private bool canAttack=true;

    Vector3 previousTargetPosition= Vector3.Zero;
    Vector3 newTargetPosition= Vector3.Zero;

    PathfindingManager pathFindingManagerInstance;
    SpatialHashMapping spatialHashMapping;
    float persecutionRadius2=0.0f;

    Vector3 oldPosition=Vector3.Zero;
    [Export]
    float detectionFlockingRadius=5.0f;
    private Vector3 previousMove = Vector3.Zero; 

    float avoidanceWeight=1.0f;
    public override void _Ready()
    {
        spatialHashMapping=SpatialHashMapping.INSTANCE;
		spatialHashMapping.insertFull(this);
        flockingController=new FlockingController(this,detectionFlockingRadius);
        this.pathFindingManagerInstance=PathfindingManager.INSTANCE;

        buildingsList= new List<House>();
        currentState=EnumEnemyStates.IDLE;
        attackCooldownTimer.Timeout += OnAttackCooldownTimeout;

        //setupAreas();
    }

    public override void _PhysicsProcess(double delta)
    {
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
            //moveAgent(distanceToTarget, delta);
            lookAtTargetInstant(this.targetPosition);
            Vector3 flockingForce = this.flockingController.calculateTotalForce();
            //Vector3 avoidanceForce = avoidance(delta)*2000;
            

            Vector3 separationForce=this.flockingController.calculateSeparationForce(this.spatialHashMapping.findNearbyAgents(this,3.0f));
            Vector3 steeringForce=this.flockingController.calculateSteeringForce(this,this.targetPosition.GlobalPosition,speed,this.Velocity);
            Vector3 seekForce=this.flockingController.CalculateSIC(this,2.0f);
            Vector3 evationForce=this.flockingController.calculateEvasionForce(this,this.spatialHashMapping.findNearbyAgents(this,3.0f),10.0f,5.0f);

            //Vector3 separationForce=Vector3.Zero;
            //Vector3 steeringForce=Vector3.Zero;
            //Vector3 seekForce=Vector3.Zero;

            String debugInfo = 
                $"separation: {separationForce}\n" +
                $"steering: {steeringForce}\n" +
                $"seek: {seekForce}\n"+
                $"velocity: {this.Velocity}";
            this.debugLabel.setLabelText(debugInfo);
            
            applyMovement(evationForce,steeringForce,separationForce,seekForce,10.0f,5.0f,delta);
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


    // La función para calcular la fuerza de Separation
    
    private void applyMovement(Vector3 evationForce,Vector3 steeringForce, Vector3 separationForce, Vector3 sicForce, float MaxForce, float maxSpeed, double delta)
    {
        // 1. Sumar fuerzas
        Vector3 totalForce = steeringForce + separationForce*2.0f + sicForce+evationForce;
        this.debugLabel.setLabelText(this.debugLabel.getLabelText()+$"\ntotalForce:{totalForce}");
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
            newTargetPosition = targetPosition.GlobalPosition;
        }
    }
/*
    private void moveAgent(Vector3 distanceToTarget, double delta){
        
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
    }*/

//esta funcion es para cuando el enemy lo ponemos como que estiende de Node3D y no de character body..e soty probando rendimientos


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
        this.targetPosition=target;
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
