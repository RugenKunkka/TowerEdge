using Godot;
using Godot.Collections;
using System.Threading.Tasks;

public partial class WaveSpawner : Node3D
{
    [Export] public string spawnerId = "";
    
    // Lista de Markers en la escena (deben ser 5 para coincidir con el Resource)
    [Export] public Array<Marker3D> SpawnPointsList;

    // Referencia al Grid para inyectar a los enemigos (opcional, según tu arquitectura)
    // [Export] public SpatialGridManager GridManager;

    public override void _Ready()
    {
        // Verificación de seguridad
        if (SpawnPointsList.Count != 5)
        {
            GD.PrintErr($"WaveSpawner [{spawnerId}]: Se esperan exactamente 5 SpawnPoints, pero hay {SpawnPointsList.Count}.");
        }
    }

    /// <summary>
    /// Método principal llamado por el WaveManager para iniciar la ejecución de la oleada.
    /// </summary>
    public async void StartSpawning(Array<ResSpawnGroup> groups)
    {
        foreach (ResSpawnGroup group in groups)
        {
            // 1. Espera inicial antes de arrancar este grupo específico
            if (group.initialDelay > 0)
            {
                await ToSignal(GetTree().CreateTimer(group.initialDelay), "timeout");
            }

            // 2. Procesamos los 5 slots del array
            for (int i = 0; i < group.enemiesList.Count; i++)
            {
                PackedScene enemyScene = group.enemiesList[i];

                // Si el slot es null, saltamos al siguiente carril
                if (enemyScene == null) continue;

                // Verificamos que tengamos un Marker físico para este índice
                if (i < SpawnPointsList.Count)
                {
                    SpawnEnemy(enemyScene, SpawnPointsList[i]);
                }

                // 3. Intervalo entre enemigos del mismo grupo (para que no salgan todos en el mismo frame)
                if (group.interval > 0)
                {
                    await ToSignal(GetTree().CreateTimer(group.interval), "timeout");
                }
            }
        }

        // 4. Notificamos al Manager que este spawner terminó su tarea
        WaveManager.Instance.SpawnerFinishedSpawning();
    }

    private void SpawnEnemy(PackedScene scene, Marker3D point)
    {
        GD.Print("Spawmeando enemigo: {}",scene.ToString());
        if (scene == null) return;

        // Instanciar el enemigo
        Node3D enemy = scene.Instantiate<Node3D>();
        
        // Lo añadimos a la escena (preferiblemente a un nodo contenedor de unidades)
        GetTree().CurrentScene.AddChild(enemy);

        // Posicionar y rotar según el Marker
        enemy.GlobalPosition = point.GlobalPosition;
        enemy.GlobalRotation = point.GlobalRotation;

        // Inyectar dependencias si el enemigo tiene la lógica de Skeleton
        if (enemy is Skeleton skeleton)
        {
            // Aquí pasamos el Nexo (null por ahora) y la referencia al Grid si la tenés
            skeleton.Initialize(null, point.GlobalPosition, null); 
        }
    }
}