using Godot;
using System;

[GlobalClass]
public partial class EnemyWaveRes: Resource
{
	[Export] public PackedScene EnemyScene;
    [Export] public int Count;
}
