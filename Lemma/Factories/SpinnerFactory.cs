﻿using System; using ComponentBind;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemma.Components;
using Microsoft.Xna.Framework;
using BEPUphysics.Paths.PathFollowing;
using Lemma.Util;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.SolverGroups;

namespace Lemma.Factories
{
	public class SpinnerFactory : Factory<Main>
	{
		public override Entity Create(Main main)
		{
			Entity entity = new Entity(main, "Spinner");

			entity.Add("MapTransform", new Transform());
			entity.Add("Transform", new Transform());
			entity.Add("Voxel", new DynamicVoxel(0, 0, 0));

			return entity;
		}

		public override void Bind(Entity entity, Main main, bool creating = false)
		{
			Components.Joint joint = entity.GetOrCreate<Components.Joint>("Joint");
			Spinner spinner = entity.GetOrCreate<Spinner>("Spinner");

			spinner.Add(new Binding<Direction>(spinner.Direction, joint.Direction));

			JointFactory.Bind(entity, main, spinner.CreateJoint, true, creating);

			BindCommand(entity, spinner.On, "On");
			BindCommand(entity, spinner.Off, "Off");
			BindCommand(entity, spinner.Forward, "Forward");
			BindCommand(entity, spinner.Backward, "Backward");
			BindCommand(entity, spinner.HitMax, "HitMax");
			BindCommand(entity, spinner.HitMin, "HitMin");
		}
	}
}
