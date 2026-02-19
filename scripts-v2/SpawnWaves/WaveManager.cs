using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para Linq

//
public partial class WaveManager : Node
{
    // --- SINGLETON ---
    public static WaveManager Instance { get; private set; }

    // --- CONFIGURACIÓN (EDITOR) ---
    [Export] public Array<WaveSpawner> WaveSpawnersList; // Arrastrá tus spawners acá

    //en la posicion 0 por ejemplo esta la wave 1 de todos los spawnners.. 
    //vas a tener un ResWaveDefinition por cada oleada!!!
    [Export] public Array<ResWaveDefinition> WavesData;     // Arrastrá tus archivos .tres de oleadas
    [Export] public float TimeBetweenWaves = 2.0f;     // Tiempo de descanso

    // --- SEÑALES (PARA COMUNICARSE CON UI Y GAME MANAGER) ---
    [Signal] public delegate void WaveStartedEventHandler(int waveNumber);
    [Signal] public delegate void WaveCompletedEventHandler();
    [Signal] public delegate void LevelCompletedEventHandler();
    [Signal] public delegate void TimeToNextWaveUpdatedEventHandler(int secondsLeft); // Para el cartel
    [Signal] public delegate void EnemiesCountUpdatedEventHandler(int count);

    // --- ESTADO INTERNO ---
    private int _currentWaveIndex = -1;
    private int _activeEnemies = 0;
    private int _spawnersFinishedCount = 0; // Cuántos spawners terminaron de tirar sus bichos
    
    private bool _isWaveInProgress = false;
    private bool _isWaitingForNextWave = false;
    private double _countdownTimer = 0;

    // ========================================================================
    //                 CICLO DE VIDA GODOT
    // ========================================================================

    public override void _EnterTree()
    {
        // Configuración del Singleton
        GD.Print("aaaaaaaaaa");
        if (Instance != null) {
			QueueFree();
		}
        Instance = this;
    }

    public override void _Ready()
    {
        // Iniciamos el contador para la primera oleada
        StartCountdown(TimeBetweenWaves);
    }

    public override void _Process(double delta)
    {
        // Lógica del Contador entre oleadas
        if (_isWaitingForNextWave)
        {
            _countdownTimer -= delta;
            
            // Emitimos señal para que la UI actualice el texto (ej: "Siguiente ola en: 5...")
            EmitSignal(SignalName.TimeToNextWaveUpdated, Mathf.CeilToInt(_countdownTimer));

            if (_countdownTimer <= 0)
            {
                _isWaitingForNextWave = false;
                StartNextWave();
            }
        }
    }

    // ========================================================================
    //                 LÓGICA DE OLEADAS
    // ========================================================================

    private void StartCountdown(float time)
    {
        _isWaitingForNextWave = true;
        _countdownTimer = time;
        GD.Print($"Descanso iniciado. Próxima ola en {time} segundos.");
    }

    private void StartNextWave()
    {
        _currentWaveIndex++;

        // 1. Chequear si terminamos el nivel
        if (_currentWaveIndex >= WavesData.Count && _activeEnemies==0 && _spawnersFinishedCount==WaveSpawnersList.Count)
        {
            GD.Print("¡NIVEL COMPLETADO!");
            EmitSignal(SignalName.LevelCompleted); // Avisar al GameManager
            return;
        }

        // 2. Configurar estado de inicio
        _isWaveInProgress = true;
        _spawnersFinishedCount = 0;
        ResWaveDefinition currentWaveData = WavesData[_currentWaveIndex];

        GD.Print($"--- INICIANDO WAVE {_currentWaveIndex + 1} ---");
        EmitSignal(SignalName.WaveStarted, _currentWaveIndex + 1);

        // 3. Sincronizar Spawners (Darles el 'Ready')
        foreach (WaveSpawner spawner in WaveSpawnersList)
        {
            // Buscamos si hay instrucciones para este spawner en particular
            //Stefano!!
            
            var instruction = currentWaveData.spawnerInstructions.FirstOrDefault(i => i.TargetSpawnerId == spawner.spawnerId);

            if (instruction != null){
                // ¡GO! Le damos sus instrucciones
                spawner.StartSpawning(instruction.Groups);
            }
            else{
                // Si no tiene nada que hacer, avisamos que "terminó" inmediatamente
                SpawnerFinishedSpawning(); 
            }
            
        }
    }

    private void CheckWaveCompletion()
    {
        // La oleada termina SOLO SI:
        // 1. Todos los spawners terminaron de tirar sus enemigos.
        // 2. No quedan enemigos vivos en el mapa.
        bool allSpawnersDone = _spawnersFinishedCount >= WaveSpawnersList.Count;
        
        if (allSpawnersDone && _activeEnemies <= 0)
        {
            GD.Print($"Wave {_currentWaveIndex + 1} Completada.");
            _isWaveInProgress = false;
            
            EmitSignal(SignalName.WaveCompleted);
            
            // Arrancamos el contador para la siguiente
            StartCountdown(TimeBetweenWaves);
        }
    }

    // ========================================================================
    //                 API PÚBLICA (LLAMADA POR OTROS NODOS)
    // ========================================================================

    /// <summary>
    /// Llamado por Skeleton.cs en su Initialize/Spawn.
    /// </summary>
    public void RegisterEnemy()
    {
        _activeEnemies++;
        EmitSignal(SignalName.EnemiesCountUpdated, _activeEnemies);
    }

    /// <summary>
    /// Llamado por Skeleton.cs cuando muere (OnDestroy o similar).
    /// </summary>
    public void UnregisterEnemy()
    {
        _activeEnemies--;
        if (_activeEnemies < 0) _activeEnemies = 0; // Seguridad

        EmitSignal(SignalName.EnemiesCountUpdated, _activeEnemies);

        // Cada vez que muere uno, chequeamos si ganamos la oleada
        if (_isWaveInProgress)
        {
            CheckWaveCompletion();
        }
    }

    /// <summary>
    /// Llamado por WaveSpawner.cs cuando termina su lista de grupos.
    /// </summary>
    public void SpawnerFinishedSpawning()
    {
        _spawnersFinishedCount++;
        CheckWaveCompletion(); // Por si acaso era lo último que faltaba
    }
}