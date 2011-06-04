﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Dwarrowdelf.Server
{
	public partial class World
	{
		public event Action TickStartEvent;

		public event Action<Living> TurnStartEvent;

		[GameProperty]
		public int TickNumber { get; private set; }

		public event Action HandleMessagesEvent;

		enum WorldState
		{
			Idle,
			TickOngoing,
			TickDone,
			TickEnded,
		}

		WorldState m_state = WorldState.Idle;

		int m_currentLivingIndex;
		Living CurrentLiving { get { return m_livings.List[m_currentLivingIndex]; } }
		void ResetLivingIndex() { m_currentLivingIndex = 0; }
		bool MoveToNextLiving()
		{
			Debug.Assert(m_currentLivingIndex < m_livings.List.Count);
			++m_currentLivingIndex;
			return m_currentLivingIndex < m_livings.List.Count;
		}

		bool m_okToStartTick = true;
		public void SetOkToStartTick()
		{
			m_okToStartTick = true;
			Thread.MemoryBarrier();
		}

		bool IsTimeToStartTick()
		{
			VerifyAccess();

			Debug.Assert(m_state == WorldState.Idle);

			if (!m_okToStartTick)
				return false;

			if (m_config.RequireUser && m_users.List.Count == 0)
				return false;

			if (m_config.RequireControllables && !m_users.List.Any(u => u.Controllables.Count > 0))
				return false;

			return true;
		}

		bool m_forceMove = false;
		public void SetForceMove()
		{
			m_forceMove = true;
			Thread.MemoryBarrier();
		}

		bool IsMoveForced()
		{
			// Race condition. The living may have done its move when this is called.
			return m_forceMove;
		}

		public bool Work()
		{
			// Hack
			if (m_worldThread == null)
				m_worldThread = Thread.CurrentThread;

			VerifyAccess();

			EnterWriteLock();

			m_instantInvokeList.ProcessInvokeList();

			if (HandleMessagesEvent != null)
				HandleMessagesEvent();

			m_users.Process();

			bool again = true;

			if (m_state == WorldState.Idle)
			{
				PreTickWork();

				if (IsTimeToStartTick())
					StartTick();
				else
					again = false;
			}

			if (m_state == WorldState.TickOngoing)
			{
				if (m_tickMethod == WorldTickMethod.Simultaneous)
					again = SimultaneousWork();
				else if (m_tickMethod == WorldTickMethod.Sequential)
					again = SequentialWork();
				else
					throw new NotImplementedException();
			}

			if (m_state == WorldState.TickDone)
				EndTick();

			ExitWriteLock();

			// no point in entering read lock here, as this thread is the only one that can get a write lock
			if (WorkEnded != null)
				WorkEnded();

			if (m_state == WorldState.TickEnded)
				m_state = WorldState.Idle;

			return again;
		}

		void PreTickWork()
		{
			m_preTickInvokeList.ProcessInvokeList();
			m_livings.Process();
		}

		bool SimultaneousWork()
		{
			VerifyAccess();
			Debug.Assert(m_state == WorldState.TickOngoing);
			Debug.Assert(m_users.List.All(u => u.StartTurnSent));

			trace.TraceVerbose("SimultaneousWork");

			bool forceMove = IsMoveForced();

			if (!forceMove && !m_users.List.All(u => u.ProceedTurnReceived))
				return false;

			m_forceMove = false;

			foreach (var living in m_livings.List)
				living.TurnPreRun();

			foreach (var living in m_livings.List.Where(l => l.HasAction))
				living.PerformAction();

			EndTurn();

			m_state = WorldState.TickDone;

			trace.TraceVerbose("SimultaneousWork Done");

			return true;
		}


		bool SequentialWork()
		{
			VerifyAccess();
			Debug.Assert(m_state == WorldState.TickOngoing);

			bool forceMove = IsMoveForced();

			trace.TraceVerbose("SequentialWork");

			bool again = true;

			while (true)
			{
				if (m_livings.List.Count == 0)
				{
					trace.TraceVerbose("no livings to handled");
					m_state = WorldState.TickDone;
					break;
				}

				var living = this.CurrentLiving;

				if (m_livings.RemoveList.Contains(living))
					forceMove = true;

				if (!forceMove && !living.HasAction)
				{
					again = false;
					break;
				}

				m_forceMove = false;

				living.TurnPreRun();

				living.PerformAction();

				EndTurn(living);

				bool ok = MoveToNextLiving();
				if (ok)
				{
					StartTurnSequential(this.CurrentLiving);
				}
				else
				{
					trace.TraceVerbose("last living handled");
					m_state = WorldState.TickDone;
					break;
				}
			}

			trace.TraceVerbose("SequentialWork Done");

			return again;
		}

		void StartTick()
		{
			VerifyAccess();

			this.TickNumber++;
			AddChange(new TickStartChange(this.TickNumber));

			trace.TraceInformation("-- Tick {0} started --", this.TickNumber);

			m_state = WorldState.TickOngoing;

			if (TickStartEvent != null)
				TickStartEvent();

			if (m_tickMethod == WorldTickMethod.Simultaneous)
			{
				StartTurnSimultaneous();
			}
			else if (m_tickMethod == WorldTickMethod.Sequential)
			{
				ResetLivingIndex();

				bool ok = MoveToNextLiving();
				if (ok)
					StartTurnSequential(this.CurrentLiving);
			}
		}

		void StartTurnSimultaneous()
		{
			foreach (var living in m_livings.List)
				living.TurnStarted();

			AddChange(new TurnStartChange());

			TurnStartEvent(null);
		}

		void StartTurnSequential(Living living)
		{
			living.TurnStarted();

			AddChange(new TurnStartChange(living));

			TurnStartEvent(living);
		}

		void EndTurn(Living living = null)
		{
			AddChange(new TurnEndChange(living));
		}

		void EndTick()
		{
			VerifyAccess();

			trace.TraceInformation("-- Tick {0} ended --", this.TickNumber);
			m_state = WorldState.TickEnded;

			m_okToStartTick = false;

			if (TickEnded != null)
				TickEnded();
		}
	}
}
