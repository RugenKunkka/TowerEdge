using Godot;
using System;
using System.ComponentModel;

//condicion hitbox y hurtbox.. los padres deben ser quienes tiene lso stasts y reciben lso daños.. si no.. nunk va a andar
public partial class HitBox : Area3D
{

	bool firendlyFire=false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.AreaEntered+=onAreaEntered;
	}

	//si entra alguien evaluamos las condiciones para hacer damage, recordá que es un componente general a la hora de desarrollar esto
    private void onAreaEntered(Area3D area)
    {
		
		IDamageable damageableObject = area.GetParent() as IDamageable;
		IDamageDealer damageDealerObject = this.GetParent() as IDamageDealer;
		if(damageableObject!=null){
			damageableObject.ApplyDamage(damageDealerObject.getDamageAmount());
		}
		
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

}
