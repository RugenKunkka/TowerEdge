using Godot;
using System;

public partial class TestAgent : Node3D
{
	float detectionRadius=10.0f;
	// Called when the node enters the scene tree for the first time.
	SpatialHashMapping spatialHashMapping;

	Vector3 oldPosition;
	public override void _Ready()
	{
		spatialHashMapping=SpatialHashMapping.INSTANCE;
		//spatialHashMapping.insertFull(this);
		oldPosition=this.Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(oldPosition!=this.Position){
			//spatialHashMapping.update(this);
			oldPosition=this.Position;
			//spatialHashMapping.findNearbyAgents(this,detectionRadius);
		}
		movement(delta);
	}

	public void movement(double delta){
		Vector3 direction = Vector3.Zero;

        if (Input.IsActionPressed("move_forward")) direction.Z -= 1;
        if (Input.IsActionPressed("move_back"))    direction.Z += 1;
        if (Input.IsActionPressed("move_left"))    direction.X -= 1;
        if (Input.IsActionPressed("move_right"))   direction.X += 1;

        if (direction != Vector3.Zero)
        {
            direction = direction.Normalized();
            Position += direction * 5f * (float)delta;
        }
	}

    public override void _ExitTree()
    {
        //spatialHashMapping.deleteFull(this);
    }
}
