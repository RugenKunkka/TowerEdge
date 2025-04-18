using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnManager : Node3D
{
	[Export]
	Godot.Collections.Array<SpawnRegion> spawnRegionsList;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{/*
		foreach (SpawnRegion spawnRegion in spawnRegionsList)
		{
			spawnRegion.SpawnMonsters();
		}*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
