﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComponentBind;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;

namespace Lemma.Components
{
	public class FallDamage : Component<Main>, IUpdateableComponent
	{
		public const float DamageVelocity = -20.0f; // Vertical velocity above which damage occurs
		public const float RollingDamageVelocity = -28.0f; // Damage velocity when rolling

		// Input commands
		public Command<float> Apply = new Command<float>();
		public Command<BEPUphysics.BroadPhaseEntries.Collidable, ContactCollection> Collided = new Command<BEPUphysics.BroadPhaseEntries.Collidable,ContactCollection>();

		// Output commands
		public Command<string, int, bool, float> StartClip = new Command<string, int, bool, float>();

		// Input properties
		public Property<Vector3> LastLinearVelocity = new Property<Vector3>();
		public Property<bool> IsSupported = new Property<bool>();
		public Property<bool> LastSupported = new Property<bool>();
		public ListProperty<SkinnedModel.Clip> AnimationClips = new ListProperty<SkinnedModel.Clip>();

		// Output properties
		public Property<float> Health = new Property<float>();

		// Input/output properties
		public Property<Vector3> LinearVelocity = new Property<Vector3>();

		public override void Awake()
		{
			base.Awake();
			this.Apply.Action = delegate(float verticalAcceleration)
			{
				bool rolling = AnimatedModel.IsPlaying(this.AnimationClips, "Roll");
				float v = rolling ? RollingDamageVelocity : DamageVelocity;
				if (verticalAcceleration < v)
				{
					this.Health.Value += (verticalAcceleration - v) * 0.2f;
					if (this.Health.Value == 0.0f)
					{
						((GameMain)main).RespawnDistance = GameMain.DefaultRespawnDistance;
						((GameMain)main).RespawnInterval = GameMain.DefaultRespawnInterval;
					}
					else
					{
						this.LinearVelocity.Value = new Vector3(0, this.LinearVelocity.Value.Y, 0);
						if (!rolling)
						{
							this.StartClip.Execute("Land", 1, false, 0.1f);
							AkSoundEngine.PostEvent("Play_land", this.Entity);
						}
					}
				}
			};

			// Damage the player if they hit something too hard
			this.Collided.Action = delegate(BEPUphysics.BroadPhaseEntries.Collidable other, ContactCollection contacts)
			{
				DynamicMap map = other.Tag as DynamicMap;
				if (map != null)
				{
					float force = contacts[contacts.Count - 1].NormalImpulse;
					float threshold = map.Entity.Type == "FallingTower" ? 14.0f : 24.0f;
					float playerLastSpeed = Vector3.Dot(this.LastLinearVelocity, Vector3.Normalize(-contacts[contacts.Count - 1].Contact.Normal)) * 2.5f;
					if (force > threshold + playerLastSpeed + 4.0f)
						this.Health.Value -= (force - threshold - playerLastSpeed) * 0.04f;
				}
			};
		}

		public void Update(float dt)
		{
			if (!this.LastSupported && this.IsSupported)
			{
				// Damage the player if they fall too hard and they're not smashing or rolling
				float accel = this.LastLinearVelocity.Value.Y - this.LinearVelocity.Value.Y;
				this.Apply.Execute(accel);
			}
		}
	}
}