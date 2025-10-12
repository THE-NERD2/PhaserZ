using Godot;
using System;

public partial class Comet : RigidBody3D
{
	private bool manned = true;
	private float thrust = 500;
	private float turn = 500;

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		var input = Input.Singleton;
		if(manned)
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

			var rotation = GlobalBasis;
			var absoluteForce = rotation * force * thrust;
			var absoluteTorque = rotation * torque * turn;
			ApplyCentralForce(absoluteForce);
			ApplyTorque(absoluteTorque);
		}
	}
}
