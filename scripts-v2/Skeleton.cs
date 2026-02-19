using Godot;
using System.Collections.Generic;

public partial class Skeleton : CharacterBody3D
{
    // --- ENUMS & STATE ---
    public enum UnitState { IDLE, CHASING, ATTACKING, DEAD, WALKING }
	[Export] AnimationPlayer _animPlayer;
    [Export] SpatialGridLayerTypeEnum spatialGridLayerTypeEnum;
    
    // --- EXPORT VARIABLES (Configuración en Editor) ---
    [Export] public float MoveSpeed = 5.0f;
    [Export] public float RotationSpeed = 10.0f;
    [Export] public float AttackRange = 1.5f;
    [Export] public float Damage = 10.0f;
    public SpatialGridManager _spatialGridManager;

    
    
    // --- INTERNAL VARIABLES ---
    private UnitState _currentState = UnitState.IDLE;
	
    // Objetivos
    private Node3D _currentTarget;       // El objetivo actual (Dinámico)
    private Node3D _mainNexusObjective;  // El objetivo final (Backup)
    
    // Cache para movimiento suave entre frames de IA
    private Vector3 _moveDirection = Vector3.Zero; 

    // --- GODOT LIFECYCLE ---

    public override void _Ready()
    {

        // IMPORTANTE: Apagamos el procesamiento interno de Godot.
        // El Manager es quien llamará a nuestros métodos Update.
        SetProcess(false);
        SetPhysicsProcess(false);
    }

    /// <summary>
    /// Método llamado por el Manager al instanciar la unidad.
    /// </summary>
    public void Initialize(Node3D nexus, Vector3 startPosition, SpatialGridManager spatialGridManager)
    {
        _mainNexusObjective = nexus;
        _currentTarget = nexus; // Por defecto, vamos al nexo
        GlobalPosition = startPosition;
        _currentState = UnitState.CHASING;//Stefano--- no se que es esto
        _spatialGridManager=spatialGridManager;
    }

    // ========================================================================
    //              FRAME-BY-FRAME LOGIC (High Frequency)
    // ========================================================================

    /// <summary>
    /// Llamado CADA FRAME por el Manager. Se encarga de la física y suavizado visual.
    /// </summary>
    /// <param name="delta">Tiempo del frame</param>
    /// <param name="avoidanceVelocity">Vector calculado por tu sistema de Avoidance propio</param>
    public void UpdateMovement(double delta, Vector3 avoidanceVelocity)
    {
        if (_currentState == UnitState.DEAD) return;

        // 1. Aplicar Gravedad (Si no usamos navegación vertical)
        if (!IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
        }

        // 2. Si estamos atacando, nos quedamos quietos (o rotamos)
        if (_currentState == UnitState.ATTACKING)
        {
            Velocity = Vector3.Zero; // Frenar en seco
            RotateTowardsTarget((float)delta); // Solo girar para mirar al objetivo
        }
        else 
        {
            // 3. Si estamos persiguiendo, combinamos dirección al objetivo + evasión
            Vector3 finalVelocity = (_moveDirection * MoveSpeed) + avoidanceVelocity;
            
            // Mantenemos la velocidad Y (gravedad) pero aplicamos X/Z
            Velocity = new Vector3(finalVelocity.X, Velocity.Y, finalVelocity.Z);

            // 4. Rotación suave hacia donde nos movemos
            if (finalVelocity.LengthSquared() > 0.1f)
            {
                // Calculamos hacia donde mirar basado en la velocidad actual
                Vector3 lookDir = new Vector3(finalVelocity.X, 0, finalVelocity.Z).Normalized();
                SmoothLookAt(lookDir, (float)delta);
            }
        }

        // 5. Mover usando el motor de física (Collision Layers se encargan de paredes)
        MoveAndSlide();
        
        // 6. Actualizar Animaciones (Visual)
        UpdateAnimations();
    }

    // ========================================================================
    //              AI LOGIC (Low Frequency - Time Sliced)
    // ========================================================================

    /// <summary>
    /// Llamado CADA X FRAMES (ej: 10 frames) por el Manager.
    /// Aquí tomamos decisiones costosas.
    /// </summary>
    /// <param name="nearbyEnemies">Lista de enemigos (Héroes/Soldados) cercanos provista por el Grid</param>
    public void UpdateAI_LowFrequency(List<Node3D> nearbyEnemies)
    {
        if (_currentState == UnitState.DEAD) return;

        // 1. Validar si el objetivo actual murió o desapareció
        ValidateCurrentTarget();

        // 2. Buscar un objetivo mejor (Prioridad vs Distancia)
        FindBestTarget(nearbyEnemies);

        // 3. Calcular distancia al objetivo elegido
        if (_currentTarget == null) return; // No debería pasar si tenemos Nexo, pero por seguridad

        float distSq = GlobalPosition.DistanceSquaredTo(_currentTarget.GlobalPosition);
        float attackRangeSq = AttackRange * AttackRange;

        // 4. Máquina de Estados de Combate
        if (distSq <= attackRangeSq)
        {
            // Estamos en rango. ¿Estamos mirando al frente?
            if (IsFacingTarget())
            {
                _currentState = UnitState.ATTACKING;
                // Aquí podrías disparar el daño si la animación lo requiere
            }
            else
            {
                // Estamos cerca pero de espaldas. Necesitamos rotar antes de pegar.
                // Mantenemos estado Chasing o Idle para permitir rotación en UpdateMovement
                _currentState = UnitState.CHASING; 
                _moveDirection = Vector3.Zero; // No caminar, solo rotar
            }
        }
        else
        {
            // Estamos lejos, perseguir
            _currentState = UnitState.CHASING;
            
            // Actualizamos la dirección para el UpdateMovement
            _moveDirection = (_currentTarget.GlobalPosition - GlobalPosition).Normalized();
        }
    }

    // ========================================================================
    //              HELPER METHODS (Private Logic)
    // ========================================================================

    private void ValidateCurrentTarget()
    {
        // Si la referencia es nula o el nodo fue liberado
        if (!IsInstanceValid(_currentTarget))
        {
            _currentTarget = null;
        }
        // Opcional: Si el target tiene script con propiedad 'IsDead'
        // else if (_currentTarget is Hero hero && hero.IsDead) { _currentTarget = null; }
    }

    private void FindBestTarget(List<Node3D> candidates)
    {
        // Si no hay candidatos cerca, volvemos al objetivo por defecto (Nexo)
        if (candidates == null || candidates.Count == 0)
        {
            if (_currentTarget == null) _currentTarget = _mainNexusObjective;
            return;
        }

        Node3D bestCandidate = _currentTarget ?? _mainNexusObjective;
        float bestScore = -1f;

        foreach (var candidate in candidates)
        {
            if (!IsInstanceValid(candidate)) continue;

            // Evitamos calcular raiz cuadrada (DistanceTo) por rendimiento
            float distSq = GlobalPosition.DistanceSquaredTo(candidate.GlobalPosition);
            
            // --- CÁLCULO DE PRIORIDAD ---
            float priority = 1.0f; // Prioridad base (Nexo o Estructura)
            
            // Usamos Grupos de Godot para identificar roles sin castear clases
            if (candidate.IsInGroup("Hero")) priority = 50.0f;
            else if (candidate.IsInGroup("Soldier")) priority = 10.0f;

            // --- FÓRMULA DE PUNTAJE ---
            // Score = (Prioridad^2) / Distancia
            // Queremos alta prioridad y baja distancia.
            float score = (priority * priority) / (distSq + 0.001f);

            // Si encontramos uno mejor que el actual, lo tomamos
            if (score > bestScore)
            {
                bestScore = score;
                bestCandidate = candidate;
            }
        }

        _currentTarget = bestCandidate;
    }

    private bool IsFacingTarget()
    {
        if (_currentTarget == null) return false;

        Vector3 directionToTarget = (_currentTarget.GlobalPosition - GlobalPosition).Normalized();
        Vector3 forward = -GlobalTransform.Basis.Z; // En Godot, Forward es -Z

        // Dot Product: 1.0 (Frente), 0.0 (Costado), -1.0 (Espalda)
        // 0.9f equivale aprox a un cono de 25 grados.
        return forward.Dot(directionToTarget) > 0.9f;
    }

    private void RotateTowardsTarget(float delta)
    {
        if (_currentTarget == null) return;
        Vector3 dir = (_currentTarget.GlobalPosition - GlobalPosition).Normalized();
        SmoothLookAt(dir, delta);
    }

    private void SmoothLookAt(Vector3 targetDirection, float delta)
    {
        // Evitamos errores matemáticos con vectores cero
        if (targetDirection.LengthSquared() < 0.001f) return;

        // Calculamos la rotación deseada
        // Usamos Vector3.Up como eje Y fijo para que no se incline
        float targetAngle = Mathf.Atan2(targetDirection.X, targetDirection.Z);
        Vector3 currentRotation = Rotation;
        
        // Interpolación suave (LerpAngle maneja el salto de 360 a 0 grados)
        currentRotation.Y = Mathf.LerpAngle(currentRotation.Y, targetAngle, RotationSpeed * delta);
        
        Rotation = currentRotation;
    }
    
    private void UpdateAnimations()
    {
        string animName = "Idle";

        switch (_currentState)
        {
            case UnitState.CHASING:
                // Si la velocidad es muy baja, mostramos Idle aunque estemos en estado Chasing
                // (por ejemplo si estamos empujando una pared)
                animName = Velocity.LengthSquared() > 0.1f ? "Run" : "Idle";
                break;
            case UnitState.ATTACKING:
                animName = "Attack";
                break;
            case UnitState.DEAD:
                animName = "Death";
                break;
        }

        // Solo cambiamos si es distinta para no reiniciar el loop
        if (_animPlayer.CurrentAnimation != animName)
        {
            _animPlayer.Play(animName);
        }
    }
    
    // Método para recibir daño (Llamado por el Héroe)
    public void TakeDamage(float amount)
    {
        // Lógica de vida... si muere:
        // _currentState = UnitState.Dead;
        // Notificar al Manager para que me saque de la lista, instanciar Ragdoll, etc.
    }
}