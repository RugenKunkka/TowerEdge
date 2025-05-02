using Godot;
using System;

public partial class EnemyDebugLabel : Sprite3D
{
	[Export]
	Label label;
	Camera3D camera;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.camera=GetViewport().GetCamera3D();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (camera != null)
		{
			Transform3D t = GlobalTransform;
			t.Basis = Basis.Identity; // Resetear rotación
			GlobalTransform = t;

			//LookAt(camera.GlobalTransform.Origin, Vector3.Up);
		}
	}

	public void setLabelText(String text){
		this.label.Text=text;
	}
	public String getLabelText(){
		return this.label.Text;
	}
}
