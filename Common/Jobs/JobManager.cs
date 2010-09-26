﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;

namespace Dwarrowdelf.Jobs
{
	public class JobManager
	{
		ObservableCollection<IJob> m_jobs;
		public ReadOnlyObservableCollection<IJob> Jobs { get; private set; }

		public JobManager(IWorld world)
		{
			m_jobs = new ObservableCollection<IJob>();
			this.Jobs = new ReadOnlyObservableCollection<IJob>(m_jobs);
		}

		public void Add(IJob job)
		{
			Debug.Assert(job.Parent == null);
			m_jobs.Add(job);
		}

		public void Remove(IJob job)
		{
			Debug.Assert(job.Parent == null);
			job.Abort();
			m_jobs.Remove(job);
		}

		public IAssignment FindJob(ILiving living)
		{
			return FindJob(m_jobs, living);
		}

		static IAssignment FindJob(IEnumerable<IJob> jobs, ILiving living)
		{
			return FindJob(jobs, JobGroupType.Parallel, living);
		}

		static IAssignment FindJob(IEnumerable<IJob> jobs, JobGroupType type, ILiving living)
		{
			if (type != JobGroupType.Parallel && type != JobGroupType.Serial)
				throw new Exception();

			foreach (var job in jobs)
			{
				if (job.Progress == Progress.Done || job.Progress == Progress.Fail)
					continue;

				if (job.Progress == Progress.None)
				{
					// job can be taken

					if (job is IAssignment)
					{
						var ajob = (IAssignment)job;
						return ajob;
					}
					else if (job is IJobGroup)
					{
						var gjob = (IJobGroup)job;

						var j = FindJob(gjob.SubJobs, gjob.JobGroupType, living);

						if (j != null)
							return j;
					}
					else
					{
						throw new Exception();
					}
				}

				// job cannot be taken

				if (type == JobGroupType.Serial)
					return null;
			}

			return null;

		}
	}
}
