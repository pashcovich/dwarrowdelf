﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Dwarrowdelf.Jobs.Assignments;
using System.Diagnostics;

namespace Dwarrowdelf.Jobs.AssignmentGroups
{
	[SaveGameObject]
	public sealed class MoveInstallItemAssignment : AssignmentGroup
	{
		[SaveGameProperty("Item")]
		readonly IItemObject m_item;
		[SaveGameProperty]
		InstallMode m_mode;
		[SaveGameProperty("State")]
		int m_state;

		public MoveInstallItemAssignment(IJobObserver parent, IItemObject item, InstallMode mode)
			: base(parent)
		{
			m_item = item;
			m_mode = mode;
			m_state = 0;
		}

		MoveInstallItemAssignment(SaveGameContext ctx)
			: base(ctx)
		{
		}

		protected override void OnStatusChanged(JobStatus status)
		{
			Debug.Assert(status != JobStatus.Ok);

			base.OnStatusChanged(status);
		}

		protected override void OnAssignmentDone()
		{
			if (m_state == 1)
				SetStatus(JobStatus.Done);
			else
				m_state = m_state + 1;
		}

		protected override IAssignment PrepareNextAssignment()
		{
			IAssignment assignment;

			switch (m_state)
			{
				case 0:
					assignment = new MoveAssignment(this, m_item.Environment, m_item.Location, DirectionSet.Exact);
					break;

				case 1:
					assignment = new InstallItemAssignment(this, m_item, m_mode);
					break;

				default:
					throw new Exception();
			}

			return assignment;
		}

		public override string ToString()
		{
			return "MoveInstallItemAssignment";
		}
	}
}
