﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Dwarrowdelf.Jobs.Assignments
{
	[GameObject(UseRef = true)]
	public class DropItemAssignment : Assignment
	{
		[GameProperty]
		readonly IItemObject m_item;

		public DropItemAssignment(IJob parent, ActionPriority priority, IItemObject item)
			: base(parent, priority)
		{
			m_item = item;
		}

		protected DropItemAssignment(GameSerializationContext ctx)
			: base(ctx)
		{
		}

		protected override GameAction PrepareNextActionOverride(out JobStatus progress)
		{
			var action = new DropAction(new IItemObject[] { m_item }, this.Priority);
			progress = JobStatus.Ok;
			return action;
		}

		protected override JobStatus ActionProgressOverride(ActionProgressChange e)
		{
			switch (e.State)
			{
				case ActionState.Ok:
					return JobStatus.Ok;

				case ActionState.Done:
					return JobStatus.Done;

				case ActionState.Fail:
					return JobStatus.Fail;

				case ActionState.Abort:
					return JobStatus.Abort;

				default:
					throw new Exception();
			}
		}

		public override string ToString()
		{
			return "DropItemAssignment";
		}
	}
}
