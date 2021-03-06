﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;

using Dwarrowdelf;
using Dwarrowdelf.Client;
using AStarTest;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Dwarrowdelf.Client.TileControl;

namespace AStarTest
{
	class MapControl : Dwarrowdelf.Client.TileControl.TileControlCore3D, INotifyPropertyChanged
	{
		public class TileInfo
		{
			public IntVector3 Location { get; set; }
		}

		Map m_map;

		const int MapWidth = 400;
		const int MapHeight = 400;
		const int MapDepth = 10;

		int m_state;
		IntVector3 m_from, m_to;

		bool m_removing;

		public TileInfo CurrentTileInfo { get; private set; } // used to inform the UI

		public DirectionSet SrcPos { get; set; }
		public DirectionSet DstPos { get; set; }

		Renderer m_renderer;
		RenderData m_renderData;

		HashSet<IntVector3> m_path;
		IDictionary<IntVector3, AStarNode> m_nodes;

		public MapControl()
		{
			this.ClipToBounds = true;

			this.SrcPos = this.DstPos = DirectionSet.Exact;

			//this.UseLayoutRounding = false;
			//this.UseLayoutRounding = true;

			this.CurrentTileInfo = new TileInfo();

			this.Focusable = true;

			this.TileSize = 24;

			m_map = new Map(MapWidth, MapHeight, MapDepth);

			ClearMap();

			this.DragStarted += OnDragStarted;
			this.DragEnded += OnDragEnded;
			this.Dragging += OnDragging;
			this.DragAborted += OnDragAborted;
			this.MouseClicked += OnMouseClicked;
		}

		protected override void OnInitialized(EventArgs e)
		{
			m_renderData = new RenderData();

			m_renderer = new Renderer(m_renderData);

			this.GridSizeChanged += MapControl_GridSizeChanged;
			this.ScreenCenterPosChanged += MapControl_ScreenCenterPosChanged;

			base.OnInitialized(e);

			base.ScreenCenterPos = new DoubleVector3(19, 12, 0);
		}

		void MapControl_GridSizeChanged(object ob, IntSize2 gridSize)
		{
			m_renderData.SetGridSize(gridSize);
		}

		void MapControl_ScreenCenterPosChanged(object arg1, DoubleVector3 arg2, IntVector3 arg3)
		{
			UpdateCurrentTileInfo(Mouse.GetPosition(this));
		}

		void ClearMap()
		{
			m_path = null;
			m_nodes = null;
			InvalidateTileData();
		}

		protected override void OnRenderTiles(DrawingContext drawingContext, Size renderSize, TileRenderContext ctx)
		{
			if (ctx.TileDataInvalid)
			{
				var width = m_renderData.Width;
				var height = m_renderData.Height;
				var grid = m_renderData.Grid;

				for (int y = 0; y < height; ++y)
				{
					for (int x = 0; x < width; ++x)
					{
						var ml = ScreenTileToMapLocation(new IntVector2(x, y));

						UpdateTile(ref grid[y, x], ml);
					}
				}
			}

			m_renderer.Render(drawingContext, renderSize, ctx);
		}

		void UpdateTile(ref RenderTileData tile, IntVector3 ml)
		{
			tile = new RenderTileData(Brushes.Black);

			if (!m_map.Bounds.Contains(ml))
			{
				tile.Brush = Brushes.DarkBlue;
			}
			else
			{
				tile.Weight = m_map.GetWeight(ml);
				tile.Stairs = m_map.GetStairs(ml);

				if (m_nodes != null && m_nodes.ContainsKey(ml))
				{
					var node = m_nodes[ml];
					tile.G = node.G;
					tile.H = node.H;

					if (node.Parent == null)
						tile.From = Direction.None;
					else
						tile.From = (node.Parent.Loc - node.Loc).ToDirection();

					if (m_path != null && m_path.Contains(ml))
						tile.Brush = Brushes.DarkGray;
					else if (node.Closed)
						tile.Brush = Brushes.MidnightBlue;
				}

				if (m_map.GetBlocked(ml))
				{
					tile.Brush = Brushes.Blue;
				}
				else if (m_state > 0 && ml == m_from)
				{
					tile.Brush = Brushes.Green;
				}
				else if (m_state > 1 && ml == m_to)
				{
					tile.Brush = Brushes.Red;
				}
			}
		}

		void OnMouseClicked(object sender, MouseButtonEventArgs e)
		{
			var pos = e.GetPosition(this);
			var ml = ScreenPointToMapLocation(pos);

			if (!m_map.Bounds.Contains(ml))
			{
				Console.Beep();
				return;
			}

			if (e.ChangedButton == MouseButton.Left)
			{
				if (m_state == 0 || m_state == 3)
				{
					m_from = ml;
					m_state = 1;
					ClearMap();
				}
				else
				{
					m_to = ml;
					DoAStar(m_from, ml);
					m_state = 3;
				}
			}
			else if (e.ChangedButton == MouseButton.Right)
			{
				// XXX doesn't work, tilecontrol only sends left button
				m_removing = m_map.GetBlocked(ml);
				m_map.SetBlocked(ml, !m_removing);
				InvalidateTileData();
			}
		}

		public IntVector3 ScreenPointToMapLocation(Point p)
		{
			return RenderPointToScreen3(p).ToIntVector3();
		}

		public IntVector3 ScreenTileToMapLocation(IntVector2 st)
		{
			var ml = RenderTileToScreen(new Point(st.X, st.Y));
			return new IntVector3((int)ml.X, (int)ml.Y, (int)this.ScreenZ);
		}

		void UpdateCurrentTileInfo(Point pos)
		{
			var ml = ScreenPointToMapLocation(pos);

			if (this.CurrentTileInfo.Location != ml)
			{
				this.CurrentTileInfo.Location = ml;
				Notify("CurrentTileInfo");
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			var pos = e.GetPosition(this);

			UpdateCurrentTileInfo(pos);

			if (e.RightButton == MouseButtonState.Pressed)
			{
				var ml = ScreenPointToMapLocation(pos);

				if (!m_map.Bounds.Contains(ml))
				{
					Console.Beep();
					return;
				}

				m_map.SetBlocked(ml, !m_removing);

				InvalidateTileData();
			}
		}

		public void Signal()
		{
			m_contEvent.Set();
		}

		long m_memUsed;
		public long MemUsed
		{
			get { return m_memUsed; }
			set { m_memUsed = value; Notify("MemUsed"); }
		}

		long m_ticksUsed;
		public long TicksUsed
		{
			get { return m_ticksUsed; }
			set { m_ticksUsed = value; Notify("TicksUsed"); }
		}

		string m_gcCollections;
		public string GCCollections
		{
			get { return m_gcCollections; }
			set { m_gcCollections = value; Notify("GCCollections"); }
		}

		AStarStatus m_astarStatus;
		public AStarStatus Status
		{
			get { return m_astarStatus; }
			set { m_astarStatus = value; Notify("Status"); }
		}

		int m_pathLength;
		public int PathLength
		{
			get { return m_pathLength; }
			set { m_pathLength = value; Notify("PathLength"); }
		}

		int m_nodeCont;
		public int NodeCount
		{
			get { return m_nodeCont; }
			set { m_nodeCont = value; Notify("NodeCount"); }
		}

		public IEnumerable<Direction> GetPathReverse(AStarNode lastNode)
		{
			if (lastNode == null)
				yield break;

			AStarNode n = lastNode;
			while (n.Parent != null)
			{
				yield return (n.Parent.Loc - n.Loc).ToDirection();
				n = n.Parent;
			}
		}

		void DoAStar(IntVector3 src, IntVector3 dst)
		{
			long startBytes, stopBytes;
			Stopwatch sw = new Stopwatch();
			GC.Collect();
			startBytes = GC.GetTotalMemory(true);
			int gc0 = GC.CollectionCount(0);
			int gc1 = GC.CollectionCount(1);
			int gc2 = GC.CollectionCount(2);
			sw.Start();

			var initLocs = this.SrcPos.ToDirections().Select(d => src + d)
				.Where(p => m_map.Bounds.Contains(p) && !m_map.GetBlocked(p));

			var astar = new AStar(initLocs, new MyTarget(m_map, dst, this.DstPos));

			if (!this.Step)
			{
				var status = astar.Find();

				sw.Stop();
				gc0 = GC.CollectionCount(0) - gc0;
				gc1 = GC.CollectionCount(1) - gc1;
				gc2 = GC.CollectionCount(2) - gc2;
				stopBytes = GC.GetTotalMemory(true);

				this.MemUsed = stopBytes - startBytes;
				this.TicksUsed = sw.ElapsedTicks;
				this.GCCollections = String.Format("{0}/{1}/{2}", gc0, gc1, gc2);

				this.Status = status;
				m_nodes = astar.NodeMap;

				if (status != AStarStatus.Found)
				{
					m_path = null;
					this.PathLength = 0;
					this.NodeCount = 0;
					return;
				}

				m_path = new HashSet<IntVector3>(astar.GetPathLocationsReverse());
				var dirs = astar.GetPathReverse().ToArray();

				this.PathLength = dirs.Length;
				this.NodeCount = astar.NodeMap.Count;

				InvalidateTileData();

				Trace.TraceInformation("Ticks {0}, Mem {1}, Len {2}, NodeCount {3}, GC {4}", this.TicksUsed, this.MemUsed,
					this.PathLength, this.NodeCount, this.GCCollections);
			}
			else
			{
				astar.DebugCallback = AStarDebugCallback;

				m_contEvent.Reset();

				AStarStatus status = AStarStatus.NotFound;

				Task.Factory.StartNew(() =>
					{
						status = astar.Find();
					})
					.ContinueWith((task) =>
						{
							sw.Stop();
							stopBytes = GC.GetTotalMemory(true);

							this.MemUsed = stopBytes - startBytes;
							this.TicksUsed = sw.ElapsedTicks;

							this.Status = status;
							m_nodes = astar.NodeMap;

							if (status != AStarStatus.Found)
							{
								m_path = null;
								this.PathLength = 0;
								this.NodeCount = 0;
								return;
							}

							m_path = new HashSet<IntVector3>(astar.GetPathLocationsReverse());
							var dirs = astar.GetPathReverse().ToArray();

							this.PathLength = dirs.Length;
							this.NodeCount = astar.NodeMap.Count;

							InvalidateTileData();
						}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		void AStarDebugCallback(IDictionary<IntVector3, AStarNode> nodes)
		{
			if (!this.Step)
				return;

			Dispatcher.Invoke(new Action(delegate
			{
				m_nodes = nodes;
				InvalidateTileData();
				UpdateLayout();
			}));

			m_contEvent.WaitOne();
		}

		public void RunTest(int test)
		{
			switch (test)
			{
				case 1:
					m_from = new IntVector3(7, 6, 0);
					m_to = new IntVector3(12, 9, 0);
					break;
				case 2:
					m_from = new IntVector3(6, 0, 0);
					m_to = new IntVector3(0, 13, 1);
					break;
				case 3:
					m_from = new IntVector3(6, 0, 0);
					m_to = new IntVector3(0, 0, 0);
					break;
				case 4:
					m_from = new IntVector3(6, 0, 0);
					m_to = new IntVector3(37, 15, 0);
					break;
				default:
					return;
			}

			ClearMap();
			DoAStar(m_from, m_to);
			m_state = 3;
		}

		public bool Step { get; set; }
		AutoResetEvent m_contEvent = new AutoResetEvent(false);

		Point m_oldDragPos;

		void OnDragStarted(Point pos)
		{
			m_oldDragPos = pos;
			Cursor = Cursors.ScrollAll;
		}

		void OnDragEnded(Point pos)
		{
			ClearValue(UserControl.CursorProperty);
		}

		void OnDragging(Point pos)
		{
			var v = m_oldDragPos - pos;
			m_oldDragPos = pos;

			var tileOffset = v / this.TileSize;
			this.ScreenCenterPos += new DoubleVector3(tileOffset.X, tileOffset.Y, 0);
		}

		void OnDragAborted()
		{
			ClearValue(UserControl.CursorProperty);
		}


		void Notify(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		class MyTarget : IAStarTarget
		{
			const int COST_DIAGONAL = 14;
			const int COST_STRAIGHT = 10;

			Map m_env;
			IntVector3 m_destination;
			DirectionSet m_positioning;

			public MyTarget(Map env, IntVector3 destination, DirectionSet positioning)
			{
				m_env = env;
				m_destination = destination;
				m_positioning = positioning;
			}

			bool IAStarTarget.GetIsTarget(IntVector3 p)
			{
				return p.IsAdjacentTo(m_destination, m_positioning);
			}

			ushort IAStarTarget.GetHeuristic(IntVector3 p)
			{
				var v = m_destination - p;

				int hDiagonal = Math.Min(Math.Min(Math.Abs(v.X), Math.Abs(v.Y)), Math.Abs(v.Z));
				int hStraight = v.ManhattanLength;
				int h = COST_DIAGONAL * hDiagonal + COST_STRAIGHT * (hStraight - 2 * hDiagonal);

				return (ushort)h;
			}

			ushort IAStarTarget.GetCostBetween(IntVector3 src, IntVector3 dst)
			{
				ushort cost = (src - dst).ManhattanLength == 1 ? (ushort)COST_STRAIGHT : (ushort)COST_DIAGONAL;
				cost += (ushort)m_env.GetWeight(dst);
				return cost;

			}

			IEnumerable<Direction> IAStarTarget.GetValidDirs(IntVector3 p)
			{
				var map = m_env;

				foreach (var d in DirectionExtensions.PlanarDirections)
				{
					var l = p + d;
					if (map.Bounds.Contains(l) && map.GetBlocked(l) == false)
						yield return d;
				}

				var stairs = map.GetStairs(p);

				if (stairs == Stairs.Up || stairs == Stairs.UpDown)
					yield return Direction.Up;

				if (stairs == Stairs.Down || stairs == Stairs.UpDown)
					yield return Direction.Down;
			}
		}
	}
}
