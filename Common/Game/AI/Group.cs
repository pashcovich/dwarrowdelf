﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Dwarrowdelf.AI
{
	[SaveGameObject]
	public sealed class Group
	{
		[SaveGameProperty]
		List<IAI> m_members;

		public Group()
		{
			m_members = new List<IAI>();
		}

		Group(SaveGameContext ctx)
		{
		}

		public void AddMember(IAI ai)
		{
			Debug.Assert(!m_members.Contains(ai));

			m_members.Add(ai);
			ai.Worker.Destructed += OnDestructed;
		}

		public void RemoveMember(IAI ai)
		{
			Debug.Assert(m_members.Contains(ai));

			m_members.Remove(ai);
		}

		void OnDestructed(IBaseObject ob)
		{
			var a = m_members.Single(ai => ai.Worker == ob);

			RemoveMember(a);
		}

		public int GroupSize
		{
			get { return m_members.Count; }
		}

		public IntVector3 GetCenter()
		{
			var locations = m_members.Select(ai => ai.Worker.Location);

			return IntVector3.Center(locations);
		}
	}
}
