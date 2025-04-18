using Godot;
using System;

[GlobalClass]
public partial class WaveRes : Resource
{
    [Export] public EnemyWaveRes[] Enemies;
}
