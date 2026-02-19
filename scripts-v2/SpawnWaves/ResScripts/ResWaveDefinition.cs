using System;
using Godot;
using Godot.Collections;

//ResWaveDefinition==>ResSpawnerInstruction==>ResSpawnGroup
//vas a tener un ResWaveDefinition por cada oleada por lo tanto si tenes 3 spawns.. vas a tener 3 registros acá , habria que ver el caso donde
//vos no vas a querer spawmear en un spawmer especifico nada más, yo por ejemplo si tengo 3 spawmers y en el 2 no quiero nada
//tendria 2 entradas aca en vez de 3 y se deberia de dar cuenta el WaveManager de esto
[GlobalClass]
public partial class ResWaveDefinition : Resource
{
    // Una lista de "sobres".
    // Elemento 0: Instrucciones para el Norte
    // Elemento 1: Instrucciones para el Sur
	//Cada Spawner tiene su conjunto de instrucciones o waves por asi decirlo
    [Export] public Array<ResSpawnerInstruction> spawnerInstructions = new Array<ResSpawnerInstruction>();
}