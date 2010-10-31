﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Dwarrowdelf.Server
{
	// XXX move somewhere else, but inside Server side */
	public interface IArea
	{
		void InitializeWorld(World world);
	}

	enum WorldTickMethod
	{
		Simultaneous,
		Sequential,
	}

	public partial class World : IWorld
	{
		class WorldConfig
		{
			public WorldTickMethod TickMethod { get; set; }

			// Require an user to be in game for ticks to proceed
			public bool RequireUser { get; set; }

			// Require an controllables to be in game for ticks to proceed
			public bool RequireControllables { get; set; }

			// Maximum time for one living to make its move. After this time has passed, the living
			// will be skipped
			public TimeSpan MaxMoveTime { get; set; }

			// Minimum time between ticks. Ticks will never proceed faster than this.
			public TimeSpan MinTickTime { get; set; }
		}

		WorldConfig m_config = new WorldConfig
		{
			TickMethod = WorldTickMethod.Simultaneous,
			RequireUser = true,
			RequireControllables = false,
			MaxMoveTime = TimeSpan.Zero,
			MinTickTime = TimeSpan.FromMilliseconds(50),
		};

		public IArea Area { get; private set; }

		MyTraceSource trace = new MyTraceSource("Dwarrowdelf.Server.World", "World");

		// only for debugging
		public bool IsWritable { get; private set; }

		ReaderWriterLockSlim m_rwLock = new ReaderWriterLockSlim();

		Dictionary<ObjectID, WeakReference> m_objectMap = new Dictionary<ObjectID, WeakReference>();
		int m_objectIDcounter;

		public event Action WorkEnded;
		public event Action<Change> WorldChanged;

		AutoResetEvent m_worldSignal = new AutoResetEvent(false);

		Thread m_worldThread;
		volatile bool m_exit = false;

		WorldLogger m_worldLogger;

		InvokeList m_preTickInvokeList;
		InvokeList m_instantInvokeList;

		public World(IArea area)
		{
			this.Area = area;
			m_worldThread = new Thread(Main);
			m_worldThread.Name = "World";

			m_worldLogger = new WorldLogger(this);

			m_preTickInvokeList = new InvokeList(this);
			m_instantInvokeList = new InvokeList(this);

			InitializeWorldTick();
		}

		public void Start()
		{
			Debug.Assert(!m_worldThread.IsAlive);

			using (var initEvent = new ManualResetEvent(false))
			{
				m_worldThread.Start(initEvent);
				initEvent.WaitOne();
			}
		}

		public void Stop()
		{
			Debug.Assert(m_worldThread.IsAlive);

			m_exit = true;
			SignalWorld();
			m_worldThread.Join();
		}

		void Init()
		{
			EnterWriteLock();

			trace.TraceInformation("Initializing area");
			var sw = Stopwatch.StartNew();
			this.Area.InitializeWorld(this);
			sw.Stop();
			trace.TraceInformation("Initializing area took {0}", sw.Elapsed);

			var envs = this.Environments;
			foreach (var env in envs)
				env.MapChanged += this.MapChangedCallback;

			ExitWriteLock();
		}

		public void SetMinTickTime(TimeSpan minTickTime)
		{
			m_config.MinTickTime = minTickTime;
		}

		void Main(object arg)
		{
			VerifyAccess();

			trace.TraceInformation("WorldMain");

			Init();

			m_worldLogger.Start();
			m_worldLogger.LogFullState();

			EventWaitHandle initEvent = (EventWaitHandle)arg;
			initEvent.Set();

			while (m_exit == false)
			{
				m_worldSignal.WaitOne();

				do
				{
					Work();
				} while (WorkAvailable());
			}

			m_worldLogger.Stop();

			trace.TraceInformation("WorldMain end");
		}

		void VerifyAccess()
		{
			if (Thread.CurrentThread != m_worldThread)
				throw new Exception();
		}

		void EnterWriteLock()
		{
			m_rwLock.EnterWriteLock();
#if DEBUG
			this.IsWritable = true;
#endif
		}

		void ExitWriteLock()
		{
#if DEBUG
			this.IsWritable = false;
#endif
			m_rwLock.ExitWriteLock();
		}

		public void EnterReadLock()
		{
			m_rwLock.EnterReadLock();
		}

		public void ExitReadLock()
		{
			m_rwLock.ExitReadLock();
		}

		// thread safe
		public void SignalWorld()
		{
			trace.TraceVerbose("SignalWorld");
			m_worldSignal.Set();
		}


		public void AddChange(Change change)
		{
			VerifyAccess();
			if (WorldChanged != null)
				WorldChanged(change);
		}

		void MapChangedCallback(Environment map, IntPoint3D l, TileData tileData)
		{
			VerifyAccess();
			AddChange(new MapChange(map, l, tileData));
		}

		internal void AddGameObject(IBaseGameObject ob)
		{
			if (ob.ObjectID == ObjectID.NullObjectID)
				throw new ArgumentException("Null ObjectID");

			lock (m_objectMap)
				m_objectMap.Add(ob.ObjectID, new WeakReference(ob));
		}

		internal void RemoveGameObject(IBaseGameObject ob)
		{
			if (ob.ObjectID == ObjectID.NullObjectID)
				throw new ArgumentException("Null ObjectID");

			lock (m_objectMap)
				if (m_objectMap.Remove(ob.ObjectID) == false)
					throw new Exception();
		}

		public IBaseGameObject FindObject(ObjectID objectID)
		{
			if (objectID == ObjectID.NullObjectID)
				throw new ArgumentException("Null ObjectID");

			IBaseGameObject ob = null;

			lock (m_objectMap)
			{
				WeakReference weakref;

				if (m_objectMap.TryGetValue(objectID, out weakref))
				{
					ob = weakref.Target as IBaseGameObject;

					if (ob == null)
						m_objectMap.Remove(objectID);
				}
			}

			return ob;
		}

		public T FindObject<T>(ObjectID objectID) where T : class, IBaseGameObject
		{
			var ob = FindObject(objectID);

			if (ob == null)
				return null;

			return (T)ob;
		}

		internal ObjectID GetNewObjectID()
		{
			return new ObjectID(Interlocked.Increment(ref m_objectIDcounter));
		}

		// XXX slow & bad
		public IEnumerable<Environment> Environments
		{
			get
			{
				Environment[] envs;
				lock (m_objectMap)
					envs = m_objectMap.Values.Select(wr => wr.Target).OfType<Environment>().ToArray();
				return envs;
			}
		}


		/* helpers for ironpython */
		public ItemObject[] IPItems
		{
			get { return m_objectMap.Values.Select(wr => wr.Target).OfType<ItemObject>().ToArray(); }
		}

		public Living[] IPLivings
		{
			get { return m_livingList.ToArray(); }
		}

		public IBaseGameObject IPGet(object target)
		{
			IBaseGameObject ob = null;

			if (target is int)
			{
				ob = FindObject(new ObjectID((int)target));
			}

			if (ob == null)
				throw new Exception(String.Format("object {0} not found", target));

			return ob;
		}
	}
}
