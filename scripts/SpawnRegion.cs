using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

//NOTA: a la hora de rotar el apwnRegion, hacerlo desde el nodo padre xq si no la grilla se genera mal.

public partial class SpawnRegion : Node3D
{
	
	[Export]
	MeshInstance3D spawnArea;//tratar de que sea una caja y listo es para que sea más visual nada más

	Enemy enemyToSpawn;
/*
	[Export]
	Godot.Collections.Array<Godot.Collections.Array<Enemy>> enemiesToSpawnInWaveListList;*/ //aaa
	[Export]  
	WaveRes[] enemyWavesList;

	List<List<PackedScene>> enemyWavesSortedList = new ();

	[Export] 
	Timer spawnTimerBetweenEnemies;
	[Export]
	Node3D targetPosition;

	[Export]
	NavigationRegion3D navigationRegion;

	int currentQuantityEnemiesSpawned=0;
	int currentWave=0;

	int counterCreepsSpawnedInCurrentWave=0;
	
	Marker3D[] localSpawnMarker3DPoints;
	int counterLocalSpawnMarker=0;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		spawnTimerBetweenEnemies.Timeout+=OnSpawnTimerBetweenEnemiesTimeOut;
		creteEnemyWavesSortedList();
		generateSpawnPoints();
		spawnTimerBetweenEnemies.Start();
	}

	public void creteEnemyWavesSortedList(){
		int i=0;
		foreach (WaveRes waveRes in enemyWavesList)
		{
			enemyWavesSortedList.Add(new());
			List<PackedScene> tempPakcedSceneReference=enemyWavesSortedList[i];
			EnemyWaveRes[] enemyWAveRes=waveRes.Enemies;
			foreach (EnemyWaveRes enemy in enemyWAveRes)
			{
				PackedScene enemyPackedScene=enemy.EnemyScene;
				int enemiesQuantity=enemy.Count;
				for(int j=0;j<enemiesQuantity;j++){
					tempPakcedSceneReference.Add(enemyPackedScene);
				}
			}
			i++;
		}
		
	}

    private void OnSpawnTimerBetweenEnemiesTimeOut()
    {
		int creepsQuantity=enemyWavesSortedList[currentWave].Count;
		if(counterCreepsSpawnedInCurrentWave<creepsQuantity){
			SpawnMonsters();
			this.spawnTimerBetweenEnemies.Start();
			//counterCreepsSpawnedInCurrentWave++;
		}
        else {
			spawnTimerBetweenEnemies.Stop();
			counterCreepsSpawnedInCurrentWave=0;
			counterLocalSpawnMarker=0;
		}
    }

	private void generateSpawnPoints(){
		Aabb aabb=spawnArea.GetAabb();
		float xSize=aabb.Size.X;
		float zSize=aabb.Size.Z;
		//tengo que trazar una grilla con cierta separación en base al aabb del creep más grande pero por ahora vamos a hardcodear esto
		float xGridSize=1.1f;
		float zGridSize=1.1f;
		
		int xPointQuantity=(int)(xSize/xGridSize);
		int zPointQuantity=(int)(zSize/zGridSize);

		//Vector3[,] spawnPoints = new Vector3[xPointQuantity, zPointQuantity];
		//localSpawnMarker3DPoints = new Marker3D[xPointQuantity, zPointQuantity];
		localSpawnMarker3DPoints = new Marker3D[xPointQuantity*zPointQuantity];

		/*
		for(int x=0; x<xPointQuantity;x++){
			for(int z=0; z<zPointQuantity;z++){
				float xPosition=+(x*xGridSize)+(xGridSize/2);
				float zPosition=-(z*zGridSize)-(zGridSize/2);
				Vector3 point = new Vector3(xPosition,0.5f,zPosition);
				//spawnPoints[x,z]=point;

				// Crear Marker3D en la posición
				Marker3D marker = new Marker3D();
				AddChild(marker);
				marker.Position = point;

				// Opcional: cambiar color o nombre para debug
				marker.Name = $"SpawnMarker_{x}_{z}";	
				//localSpawnMarker3DPoints[x,z]=marker;
				int linearIndex=(x*zPointQuantity)+z;
				localSpawnMarker3DPoints[linearIndex]=marker;
			}
		}
		*/
		int linearIndexCounter=0;
		for(int z=zPointQuantity-1; z>=0;z--){
			for(int x=0; x<xPointQuantity;x++){
				float xPosition=+(x*xGridSize)+(xGridSize/2);
				float zPosition=-(z*zGridSize)-(zGridSize/2);
				Vector3 point = new Vector3(xPosition,0.5f,zPosition);
				/*spawnPoints[x,z]=point;*/

				// Crear Marker3D en la posición
				Marker3D marker = new Marker3D();
				AddChild(marker);
				marker.Position = point;

				// Opcional: cambiar color o nombre para debug
				marker.Name = $"SpawnMarker_{x}_{z}";	
				//localSpawnMarker3DPoints[x,z]=marker;
				int linearIndex=(z*xPointQuantity)+x;
				localSpawnMarker3DPoints[linearIndexCounter]=marker;
				linearIndexCounter++;
			}
		}
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame..
    public override void _Process(double delta)
	{

	}

	public void SpawnMonsters(){
        List<PackedScene> enemiesToSpawnList= enemyWavesSortedList[currentWave];
		PackedScene enemyToSpawn=enemiesToSpawnList[counterCreepsSpawnedInCurrentWave];
		
		Node enemyInstance = enemyToSpawn.Instantiate();
		Enemy enemy = enemyInstance as Enemy;
		if (enemy != null)
		{
			this.AddChild(enemy); // o GetTree().Root.AddChild(enemy) si querés al root directamente
			enemy.setTargetPosition(this.targetPosition);
			enemy.setNavigationRegion3D(this.navigationRegion);
			// Setear posición global (por ejemplo en un punto que ya tenés)
			if(counterLocalSpawnMarker>this.localSpawnMarker3DPoints.Length-1){
				counterLocalSpawnMarker=0;
			}
			enemy.GlobalPosition = this.localSpawnMarker3DPoints[counterLocalSpawnMarker].GlobalPosition; // Vector3 que vos definas
			counterLocalSpawnMarker++;
			GD.Print("counterCreepsSpawnedInCurrentWave: "+counterCreepsSpawnedInCurrentWave);
			// Agregarlo al árbol de nodos (usualmente al escenario o a algún contenedor)
			
		}
		else
		{
			GD.PrintErr("La escena instanciada no es un Node3D");
		}
		

		counterCreepsSpawnedInCurrentWave++;
	}

	public int calculateQuantityEnemiesToSpawn(){
		int totalQuantityEnemiesToSpawn=0;

		return totalQuantityEnemiesToSpawn;
	}
}
