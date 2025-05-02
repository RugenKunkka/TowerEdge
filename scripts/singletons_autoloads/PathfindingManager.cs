using Godot;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Reflection.Metadata;

public partial class PathfindingManager : Node
{
	public static PathfindingManager INSTANCE;
	const int quantityEnemyToProcess=30;
	private List<Enemy> agentsList = new List<Enemy>();
	// Called when the node enters the scene tree for the first time.


	bool haveDataToProcessing=false;


	public override void _Ready()
	{
		PathfindingManager.INSTANCE=this;
		agentsList = new List<Enemy>();
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(haveDataToProcessing){
			updateSuscribers();
		}
	}

	public void clearNodeData(){
		this.agentsList.Clear();
	}
	
	public void resetNode(){
		this.clearNodeData();
	}

	public void suscribe(Enemy enemy){
		if (!agentsList.Contains(enemy))
		{
			agentsList.Add(enemy);
			haveDataToProcessing=true;
		}		
	}

	public void unsuscribe(Enemy enemy){
		agentsList.Remove(enemy);
	}

	private void updateSuscribers(){
		string timestamp = Time.GetTimeStringFromSystem(); // Devuelve algo como "14:53:21"
		
		int elementsToProcess=elementsToProcess=Math.Min(agentsList.Count,quantityEnemyToProcess);
		
		for(int i=0;i<elementsToProcess;i++){
			//GD.Print($"[{timestamp}] Se movió el enemigo: []");
			agentsList[i].updatePath();
		}

		for(int i=0;i<elementsToProcess;i++){
			agentsList.RemoveAt(0);
		}

		if(agentsList.Count==0){
			haveDataToProcessing=false;
		}
	}

}
