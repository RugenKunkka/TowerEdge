using Godot;
using System;

//condicion hitbox y hurtbox.. los padres deben ser quienes tiene lso stasts y reciben lso daños.. si no.. nunk va a andar
public partial class HurtBox : StaticBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//this.AreaEntered+=OnAreaEntered;
	}

	/*
    private void OnAreaEntered(Area3D area)
    {
        HurtBox hurtBox = area as HurtBox;
		if(hurtBox!=null){
			
		}
    }
	*/

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
