using Godot;
using System;

public partial class PhysicsServer3dTest : Node3D
{

	Rid collisionObject;
	Rid shape;

	[Export]
	Node3D visual;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        

		collisionObject=PhysicsServer3D.BodyCreate();
		shape=PhysicsServer3D.CylinderShapeCreate();
		PhysicsServer3D.ShapeSetData(shape, new Godot.Collections.Dictionary<string, Variant> { { "radius", 1.813 }, { "height", 2.0f }, {"margin",0.04f} });
		PhysicsServer3D.BodyAddShape(collisionObject,shape);


		PhysicsServer3D.BodySetSpace(collisionObject,GetWorld3D().Space);

		//forma del objeto y posición, rotación ubicación etc..
		Transform3D transform=new Transform3D(Basis,this.GlobalPosition);//no se si está bien
		PhysicsServer3D.BodySetState(collisionObject,PhysicsServer3D.BodyState.Transform,transform);

		//hay que hacer que se mueva el objeto visual o mesh relacionado con la física o bindearlo..

		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//hay que hacer que se mueva el objeto visual o mesh relacionado con la física o bindearlo..
		Transform3D transform= (Transform3D)PhysicsServer3D.BodyGetState(collisionObject,PhysicsServer3D.BodyState.Transform);//no se si está bien
		this.GlobalTransform=transform;

		
	}

    public override void _ExitTree()
    {
        base._ExitTree();
		PhysicsServer3D.FreeRid(collisionObject);
		PhysicsServer3D.FreeRid(shape);
    }
}
