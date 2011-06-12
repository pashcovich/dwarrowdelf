﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Dwarrowdelf.Jobs.Assignments
{
	public abstract class Assignment : IAssignment
	{
		protected Assignment(IJob parent, ActionPriority priority)
		{
			this.Parent = parent;
			this.Priority = priority;
		}

		protected Assignment(GameSerializationContext ctx)
		{
		}

		public JobType JobType { get { return JobType.Assignment; } }
		[GameProperty]
		public IJob Parent { get; private set; }
		[GameProperty]
		public ActionPriority Priority { get; private set; }

		public bool IsAssigned
		{
			get
			{
				Debug.Assert(m_worker == null || this.JobStatus == Jobs.JobStatus.Ok);
				return m_worker != null;
			}
		}

		[GameProperty]
		public JobStatus JobStatus { get; private set; }

		public void Retry()
		{
			Debug.Assert(this.JobStatus != JobStatus.Ok);
			Debug.Assert(this.IsAssigned == false);

			SetState(JobStatus.Ok);
		}

		public void Abort()
		{
			SetState(JobStatus.Abort);
		}

		public void Fail()
		{
			SetState(JobStatus.Fail);
		}

		ILiving m_worker;
		[GameProperty]
		public ILiving Worker
		{
			get { return m_worker; }
			private set { if (m_worker == value) return; m_worker = value; Notify("Worker"); }
		}

		GameAction m_action;
		[GameProperty]
		public virtual GameAction CurrentAction
		{
			get { return m_action; }
			private set { if (m_action == value) return; m_action = value; Notify("CurrentAction"); }
		}

		public JobStatus Assign(ILiving worker)
		{
			Debug.Assert(this.IsAssigned == false);
			Debug.Assert(this.JobStatus == JobStatus.Ok);

			var state = AssignOverride(worker);
			SetState(state);
			if (state != JobStatus.Ok)
				return state;

			this.Worker = worker;

			return state;
		}

		protected virtual JobStatus AssignOverride(ILiving worker)
		{
			return JobStatus.Ok;
		}



		public JobStatus PrepareNextAction()
		{
			Debug.Assert(this.CurrentAction == null);

			JobStatus status;
			var action = PrepareNextActionOverride(out status);
			Debug.Assert((action == null && status != Jobs.JobStatus.Ok) || (action != null && status == Jobs.JobStatus.Ok));
			this.CurrentAction = action;
			SetState(status);
			return status;
		}

		protected abstract GameAction PrepareNextActionOverride(out JobStatus status);

		public JobStatus ActionProgress(ActionProgressChange e)
		{
			Debug.Assert(this.Worker != null);
			Debug.Assert(this.JobStatus == JobStatus.Ok);
			Debug.Assert(this.CurrentAction != null);

			var state = ActionProgressOverride(e);
			SetState(state);

			if (e.TicksLeft == 0)
				this.CurrentAction = null;

			return state;
		}

		protected virtual JobStatus ActionProgressOverride(ActionProgressChange e)
		{
			return JobStatus.Ok;
		}

		void SetState(JobStatus status)
		{
			if (this.JobStatus == status)
				return;

			switch (status)
			{
				case JobStatus.Ok:
					break;

				case JobStatus.Done:
					Debug.Assert(this.JobStatus == JobStatus.Ok);
					break;

				case JobStatus.Abort:
					Debug.Assert(this.JobStatus == JobStatus.Ok || this.JobStatus == JobStatus.Done);
					break;

				case JobStatus.Fail:
					Debug.Assert(this.JobStatus == JobStatus.Ok);
					break;
			}

			switch (status)
			{
				case JobStatus.Ok:
					break;

				case JobStatus.Done:
				case JobStatus.Abort:
				case JobStatus.Fail:
					this.Worker = null;
					this.CurrentAction = null;
					break;
			}

			this.JobStatus = status;
			OnStateChanged(status);
			if (this.StatusChanged != null)
				StatusChanged(this, status);
			Notify("JobStatus");
		}

		public event Action<IJob, JobStatus> StatusChanged;

		protected virtual void OnStateChanged(JobStatus status) { }

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		void Notify(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
