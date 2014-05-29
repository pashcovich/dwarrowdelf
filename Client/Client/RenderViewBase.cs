﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Dwarrowdelf.Client.TileControl;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dwarrowdelf.Client
{
	abstract class RenderViewBase<T> where T : struct
	{
		protected readonly DataGrid2D<T> m_renderData;

		bool m_isVisibilityCheckEnabled;
		protected EnvironmentObject m_environment;
		protected IntPoint3 m_centerPos;

		protected RenderViewBase(DataGrid2D<T> renderData)
		{
			m_renderData = renderData;
		}

		public abstract void Resolve();

		public IntPoint3 CenterPos
		{
			get { return m_centerPos; }
			set
			{
				if (value == m_centerPos)
					return;

				var diff = value - m_centerPos;

				m_centerPos = value;

				OnCenterPosChanged(diff);
			}
		}

		protected abstract void OnCenterPosChanged(IntVector3 diff);

		public void SetMaxSize(IntSize2 size)
		{
			m_renderData.SetMaxSize(size);
		}

		public void SetSize(IntSize2 size)
		{
			if (size != m_renderData.Size)
			{
				m_renderData.SetSize(size);

				OnSizeChanged();
			}
		}

		protected abstract void OnSizeChanged();

		public bool IsVisibilityCheckEnabled
		{
			get { return m_isVisibilityCheckEnabled; }

			set
			{
				m_isVisibilityCheckEnabled = value;
				m_renderData.Invalid = true;
			}
		}

		public EnvironmentObject Environment
		{
			get { return m_environment; }

			set
			{
				if (m_environment == value)
					return;

				if (m_environment != null)
				{
					m_environment.MapTileTerrainChanged -= MapChangedCallback;
					m_environment.MapTileObjectChanged -= MapObjectChangedCallback;
				}

				m_environment = value;
				m_renderData.Invalid = true;

				if (m_environment != null)
				{
					m_environment.MapTileTerrainChanged += MapChangedCallback;
					m_environment.MapTileObjectChanged += MapObjectChangedCallback;
				}
			}
		}

		public abstract bool Invalidate(IntPoint3 ml);

		public void Invalidate()
		{
			m_renderData.Invalid = true;
		}

		void MapChangedCallback(IntPoint3 ml)
		{
			MapChangedOverride(ml);
		}

		void MapObjectChangedCallback(MovableObject ob, IntPoint3 ml, MapTileObjectChangeType changeType)
		{
			MapChangedOverride(ml);
		}

		protected abstract void MapChangedOverride(IntPoint3 ml);

		protected static bool TileVisible(IntPoint3 ml, EnvironmentObject env)
		{
			switch (env.VisibilityMode)
			{
				case VisibilityMode.AllVisible:
					return true;

				case VisibilityMode.GlobalFOV:
					return !env.GetHidden(ml);

				case VisibilityMode.LivingLOS:

					var controllables = env.World.Controllables;

					switch (env.World.LivingVisionMode)
					{
						case LivingVisionMode.LOS:
							foreach (var l in controllables)
							{
								if (l.Environment != env || l.Location.Z != ml.Z)
									continue;

								IntPoint2 vp = new IntPoint2(ml.X - l.Location.X, ml.Y - l.Location.Y);

								if (Math.Abs(vp.X) <= l.VisionRange && Math.Abs(vp.Y) <= l.VisionRange &&
									l.VisionMap[vp] == true)
									return true;
							}

							return false;

						case LivingVisionMode.SquareFOV:
							foreach (var l in controllables)
							{
								if (l.Environment != env || l.Location.Z != ml.Z)
									continue;

								IntPoint2 vp = new IntPoint2(ml.X - l.Location.X, ml.Y - l.Location.Y);

								if (Math.Abs(vp.X) <= l.VisionRange && Math.Abs(vp.Y) <= l.VisionRange)
									return true;
							}

							return false;

						default:
							throw new Exception();
					}

				default:
					throw new Exception();
			}
		}

		protected static SymbolID GetDesignationSymbolAt(Designation designation, IntPoint3 p)
		{
			var dt = designation.ContainsPoint(p);

			switch (dt)
			{
				case DesignationType.None:
					return SymbolID.Undefined;

				case DesignationType.Mine:
					return SymbolID.DesignationMine;

				case DesignationType.CreateStairs:
					return SymbolID.StairsUp;

				case DesignationType.Channel:
					return SymbolID.DesignationChannel;

				case DesignationType.FellTree:
					return SymbolID.Log;

				default:
					throw new Exception();
			}
		}

		protected static SymbolID GetConstructSymbolAt(ConstructManager mgr, IntPoint3 p)
		{
			var dt = mgr.ContainsPoint(p);

			switch (dt)
			{
				case ConstructMode.None:
					return SymbolID.Undefined;

				case ConstructMode.Pavement:
					return SymbolID.Floor;

				case ConstructMode.Floor:
					return SymbolID.Floor;

				case ConstructMode.Wall:
					return SymbolID.Wall;

				default:
					throw new Exception();
			}
		}

		protected static SymbolID GetInstallSymbolAt(InstallItemManager mgr, IntPoint3 p)
		{
			var item = mgr.ContainsPoint(p);

			if (item == null)
				return SymbolID.Undefined;

			return item.SymbolID;
		}
	}
}