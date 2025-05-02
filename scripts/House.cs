using Godot;
using Godot.Collections;
using System;

public partial class House : Node3D , IDamageable
{
	[Export] 
	float hp=3;
	// Called when the node enters the scene tree for the first time.
	SpatialHashMapping spatialHashMapping;

	public override void _Ready()
	{
		spatialHashMapping=SpatialHashMapping.INSTANCE;
		spatialHashMapping.insertFull(this);

		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//
		var hit = CircleCastWithMotionAndDebug(
			this,
			origin: this.GlobalPosition,//this.GlobalPosition.. si lo queres usar el padre tieen que estar en 0,0,0 si no.. te aplica una trasnformación y fuiste..
			direction: Vector3.Forward,
			distance: 5f,
			radius: 0.5f,
			height: 1.0f
		);

		if (hit != null)
		{
			//GD.Print("Hit: ", hit["collider"], " at ", hit["position"]);
		}
	}

    public void ApplyDamage(float damageAmmount)
    {
        this.hp-=damageAmmount;
		if(this.hp<=0){
			this.QueueFree();
		}
    }

    public override void _ExitTree()
    {
        spatialHashMapping.deleteFull(this);
    }

	//prueba de colisiones al bardoli!!! XD jajajaj
	public void DrawCircle(Vector3 center, float radius, int segments = 32)
	{
		var im = new ImmediateMesh();
		var arrayMesh = new MeshInstance3D();
		arrayMesh.Mesh = im;

		AddChild(arrayMesh);

		im.ClearSurfaces();
		im.SurfaceBegin(Mesh.PrimitiveType.LineStrip);

		for (int i = 0; i <= segments; i++)
		{
			float angle = i * Mathf.Tau / segments;
			float x = Mathf.Cos(angle) * radius;
			float z = Mathf.Sin(angle) * radius;
			im.SurfaceAddVertex(center + new Vector3(x, 0, z));
		}

		im.SurfaceEnd();
	}

	public Dictionary CircleCastWithMotionAndDebug(Node3D caller,Vector3 origin,Vector3 direction,float distance,
    											   float radius,float height,Color? debugColor = null,uint collisionMask = 0b11111)
	{
		var spaceState = PhysicsServer3D.SpaceGetDirectState(caller.GetWorld3D().Space);

		var shape = new CylinderShape3D
		{
			Radius = radius,
			Height = height
		};

		var motion = direction.Normalized() * distance;

		var query = new PhysicsShapeQueryParameters3D
		{
			Shape = shape,
			Transform = new Transform3D(Basis.Identity, origin),
			Motion = motion,
			CollisionMask = collisionMask,
			CollideWithBodies = true,
			CollideWithAreas = false
		};

		var results = spaceState.IntersectShape(query, 1);

		// DEBUG: Dibuja circunferencia de origen
		DrawCircle(origin, radius, 32);//, debugColor ?? Colors.Red);

		// DEBUG: Dibuja línea del barrido
		var line = new ImmediateMesh();
		var lineInstance = new MeshInstance3D { Mesh = line };
		var mat = new StandardMaterial3D();
		mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
		mat.AlbedoColor = debugColor ?? Colors.Yellow;
		lineInstance.MaterialOverride = mat;
		caller.AddChild(lineInstance);

		line.SurfaceBegin(Mesh.PrimitiveType.Lines, mat);
		line.SurfaceAddVertex(origin);
		line.SurfaceAddVertex(origin + motion);
		line.SurfaceEnd();

		// DEBUG: Si colisionó, dibujar circunferencia en punto de impacto
		if (results.Count > 0)
		{
			var hitCollider = (Node3D)results[0]["collider"];
			Vector3 hitPos = hitCollider.GlobalTransform.Origin; // Obtener la posición del collider

			// Dibuja circunferencia en el punto de colisión
			DrawCircle(hitPos, radius, 32);//, Colors.Green);

			return results[0]; // Devolver la información de la colisión
		}

		return null;
	}
}
