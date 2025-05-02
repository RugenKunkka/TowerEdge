using Godot;
using System;
using System.Collections.Generic;

//supuestos.. vamos a trbaajr con X+ y Z- damos las dimensiones del mapa por ejemplo 50, 50 pero el mapa tenemos que tener en cuenta 
//que para el cálculo vamos a suponer que el mapa, a nivel local arranca en x=0,z=0, y el offset lo vamos a dar como Vector3 pero
//vamosa  trabajar en 2 dimensiones SIEMPRE!!!  X y Z el eje Y no lo vamos atener en cuenta por ahora.

// por ahora vamosa d esarrollar esto como que está todo en un único cuadrante porque se me acaba de trabar el cerebro jajajaj
public partial class SpatialHashMapping : Node
{
	Dictionary<(int,int), List<Node3D>> cells;//int.int es x,z
	Dictionary<Node3D, (int, int)> nodesMap ;//key el nodo, value son las cellcords
	public static SpatialHashMapping INSTANCE;
	float cellSize=5.0f;

    public override void _Ready()
    {
        cells = new Dictionary<(int, int), List<Node3D>>();
		nodesMap = new Dictionary<Node3D, (int, int)>();
		INSTANCE=this;
    }

	//el nodo cuando hace un ready hace un insert
	//cuando se mueva va a hacer un update
	//y cuando se muera va a hacer un delete directamente
	

	//esta función determina la celda donde tiene que ir el nodo sí o sí
	private void insertOnCells(Node3D node){

		(int,int) cellCords=this.getCellCords(node.GlobalPosition);
		if(!cells.ContainsKey(cellCords)){
			cells[cellCords]=new List<Node3D>();
		}
		cells[cellCords].Add(node);

	}

	public void insertFull(Node3D node){
		insertOnCells(node);
		nodesMap.Add(node, this.getCellCords(node.GlobalPosition));
	}

	//la idea es agente corrobore su posicion actual con su posicion previay si cambiaron en algo.. actualice acá directamente
	//pero como estamos almacenando la infomación del Node3D capaz solo ahga falta actualizar de chunk.. nisiquiera vamos a tener que actualizar
	//el Node3D si no en que chunk o cell de la grilla está
	public void update(Node3D node){
		(int,int) newCellCords=this.getCellCords(node.GlobalPosition);
		//esto puede ser null pero supuestamente va a estar registrado el objeto antes de llamar el update asi que estimo que no hace falta
		bool existOldCellCords=nodesMap.TryGetValue(node, out (int,int) oldCellCords);
		//GD.Print("oldCellCords: "+oldCellCords);
		if(existOldCellCords && newCellCords!=oldCellCords){//si cambia de celda
			cells[oldCellCords].Remove(node);
			insertOnCells(node);
			nodesMap[node]=newCellCords;//actualizo la cellCord 
		}
	}

	private void deleteOnCells(Node3D node){

		(int,int) cellCords=this.getCellCords(node.GlobalPosition);
		cells[cellCords].Remove(node);
		
	}

	//esta funcion hay que llamarla cuando el nodo va a ahcer un kill osea.. un _exit para desocupar el HashMapping
	public void deleteFull(Node3D node){
		(int,int) cellCords=this.getCellCords(node.GlobalPosition);
		cells[cellCords].Remove(node);
		nodesMap.Remove(node);

	}

	public (int,int) getCellCords(Vector3 position){
		int x= (int)Math.Ceiling(position.X/cellSize);
		int z= (int)Math.Ceiling(position.Z/cellSize);
		
		return (x,z);

	}

	public List<Node3D> findNearbyAgents(Node3D node, float radius){

		List<Node3D> nearbyAgents=new List<Node3D>();

		int cellRadius= (int)(radius / cellSize);
		float squaredRadius=radius*radius;
		Vector3 nodePosition=node.GlobalPosition;
		(int,int) cellIndex=this.getCellCords(nodePosition);
		(int,int) minCellRadius=(cellIndex.Item1-cellRadius,cellIndex.Item2-cellRadius);
		(int,int) maxCellRadius=(cellIndex.Item1+cellRadius,cellIndex.Item2+cellRadius);
		//GD.Print("minCellRadius: "+minCellRadius);
		//GD.Print("maxCellRadius: "+maxCellRadius);
		for(int i=minCellRadius.Item1; i<maxCellRadius.Item1+1;i++){
			for(int j=minCellRadius.Item2; j<maxCellRadius.Item2+1;j++){
				List<Node3D> agentsList;
				if (!cells.TryGetValue((i,j), out agentsList))
				{
					agentsList = new List<Node3D>(); // o devolver null, según el caso
				}
				foreach (Node3D agentNode in agentsList){
					float squaredDistance=nodePosition.DistanceTo(agentNode.GlobalPosition);
					if(squaredDistance!=0 && squaredDistance<radius){//acá hjabría que corroborar que no sea el mismo nodo ya sea con el nombre e el Node3D
						nearbyAgents.Add(agentNode);
						//GD.Print("Entró en contacto con: "+agentNode.Name);
					}
				}
				

			}
		}
		//GD.Print("Cantidad de colisiones!!!! "+nearbyAgents.Count);
		return nearbyAgents;
	}

	
	
}
