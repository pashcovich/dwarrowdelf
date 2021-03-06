﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Dwarrowdelf.Jobs.JobGroups
{
	[SaveGameObject]
	public sealed class FetchItems : JobGroup
	{
		public FetchItems(IJobObserver parent, IEnvironmentObject env, IntVector3 location, IEnumerable<IItemObject> items)
			: this(parent, env, location, items, DirectionSet.Exact)
		{
		}

		public FetchItems(IJobObserver parent, IEnvironmentObject env, IntVector3 location, IEnumerable<IItemObject> items, DirectionSet positioning)
			: base(parent)
		{
			foreach (var item in items)
			{
				var job = new AssignmentGroups.FetchItemAssignment(this, env, location, item, positioning);
				AddSubJob(job);
			}

			Debug.Assert(this.SubJobs.Count > 0);
		}

		FetchItems(SaveGameContext ctx)
			: base(ctx)
		{
		}

		protected override void OnSubJobDone(IJob job)
		{
			this.RemoveSubJob(job);

			if (this.SubJobs.Count == 0)
				SetStatus(JobStatus.Done);
		}

		public override string ToString()
		{
			return "FetchItems";
		}
	}
}
