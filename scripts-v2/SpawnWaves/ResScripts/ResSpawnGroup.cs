using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class ResSpawnGroup : Resource
{
    //Las posiciones del array son las posiciones del spawnpoint del WaveSapwner. por lo tanto si queres una formacion tipo, 2 orcos melee a los laterales normales
	// y no se un arquero al centro va... {null, orc, archer, orc, null}
	//la posicion 0 y 4 son unas posicione particulares que la podemos llega a usar para spawmear scurridizos pero lo vamos a ver.. esta es la intención inicial
    [Export] public String groupId="1o-1a-1o_0.0_0.5";
    [Export] public Array<PackedScene> enemiesList = new Array<PackedScene>{null, null, null, null, null};
    
    [Export] public float interval = 0.0f;   // Tiempo entre cada bicho del grupo
    [Export] public float initialDelay = 0.5f; // Pausa antes de empezar el grupo completo
}