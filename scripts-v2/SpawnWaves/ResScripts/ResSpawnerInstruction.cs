using Godot;
using Godot.Collections; // Necesario para usar Array<T> compatible con el Inspector

//ResSpawnerInstruction ==> ResSpawnGroup

[GlobalClass] // Importante para que puedas crear "New SpawnerInstruction" dentro de la Wave
public partial class ResSpawnerInstruction : Resource
{
    // A QUIÉN VA DIRIGIDO:
    // El ID del Spawner en la escena que tiene que ejecutar esto.
    // Ej: "Puerta_Norte", "Cementerio", "Spawner_Lateral"
    [Export] public string TargetSpawnerId = "Puerta_Norte";
    

    // QUÉ TIENE QUE HACER:
    // La lista secuencial de grupos de enemigos para esta oleada.
    [Export] public Array<ResSpawnGroup> Groups = new Array<ResSpawnGroup>();
}