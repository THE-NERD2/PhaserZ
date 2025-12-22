using Godot;

public abstract partial class Ship : RigidBody3D
{
	[Export] protected bool Manned = true;
    protected abstract float Thrust { get; }
	protected abstract float Turn { get; }

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		var input = Input.Singleton;
		if(Manned)
		{
			var force = Vector3.Zero;
			var torque = Vector3.Zero;

			if(input.IsActionPressed("thrust_forward"))
			{
				force.Z += 1.0f;
			}
			// TODO: other thrust things?
			if(input.IsActionPressed("pitch_up"))
			{
				torque.X -= 1.0f;
			}
			if(input.IsActionPressed("pitch_down"))
			{
				torque.X += 1.0f;
			}
			if(input.IsActionPressed("yaw_left"))
			{
				torque.Y += 1.0f;
			}
			if(input.IsActionPressed("yaw_right"))
			{
				torque.Y -= 1.0f;
			}
			// TODO: roll

			if(SettingsHandler.GetSetting("PlayerControlSettings.RotationDamping").AsBool() && torque == Vector3.Zero)
			{
				torque = -AngularVelocity.Normalized();
			}

			var rotation = GlobalBasis;
			var absoluteForce = rotation * force * Thrust;
			var absoluteTorque = rotation * torque * Turn;
			ApplyCentralForce(absoluteForce);
			ApplyTorque(absoluteTorque);
		}
	}
}
