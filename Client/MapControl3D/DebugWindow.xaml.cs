﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Dwarrowdelf.Client
{
	public partial class DebugWindow : Window
	{
		DebugWindowData m_data;
		DispatcherTimer m_timer;
		MyGame m_game;

		public DebugWindow()
		{
			InitializeComponent();
		}

		internal void SetGame(MyGame game)
		{
			m_game = game;

			m_data = new DebugWindowData(game);
			this.DataContext = m_data;

			m_timer = new DispatcherTimer();
			m_timer.Tick += m_data.Update;
			m_timer.Interval = TimeSpan.FromSeconds(1);
			m_timer.IsEnabled = true;

			var m_scene = m_game.TerrainRenderer;

			cbBorders.Checked += (s, e) => m_scene.Effect.DisableBorders = true;
			cbBorders.Unchecked += (s, e) => m_scene.Effect.DisableBorders = false;

			cbLight.Checked += (s, e) => m_scene.Effect.DisableLight = true;
			cbLight.Unchecked += (s, e) => m_scene.Effect.DisableLight = false;

			cbOcclusion.Checked += (s, e) => m_scene.Effect.DisableOcclusion = true;
			cbOcclusion.Unchecked += (s, e) => m_scene.Effect.DisableOcclusion = false;

			cbTexture.Checked += (s, e) => m_scene.Effect.DisableTexture = true;
			cbTexture.Unchecked += (s, e) => m_scene.Effect.DisableTexture = false;

			/*
			cbVsync.Checked += (s, e) => m_game.GraphicsDevice.Presenter.Description.PresentationInterval = SharpDX.Toolkit.Graphics.PresentInterval.Immediate;
			cbVsync.Unchecked += (s, e) => m_game.GraphicsDevice.Presenter.Description.PresentationInterval = SharpDX.Toolkit.Graphics.PresentInterval.One;
			*/

			cbWireframe.Checked += OnRenderStateCheckBoxChanged;
			cbWireframe.Unchecked += OnRenderStateCheckBoxChanged;
			cbCulling.Checked += OnRenderStateCheckBoxChanged;
			cbCulling.Unchecked += OnRenderStateCheckBoxChanged;
		}

		void OnRenderStateCheckBoxChanged(object sender, EventArgs e)
		{
			bool disableCull = cbCulling.IsChecked.Value;
			bool wire = cbWireframe.IsChecked.Value;

			SharpDX.Toolkit.Graphics.RasterizerState state;

			if (!disableCull && !wire)
				state = m_game.GraphicsDevice.RasterizerStates.CullBack;
			else if (disableCull && !wire)
				state = m_game.GraphicsDevice.RasterizerStates.CullNone;
			else if (!disableCull && wire)
				state = m_game.GraphicsDevice.RasterizerStates.WireFrame;
			else if (disableCull && wire)
				state = m_game.GraphicsDevice.RasterizerStates.WireFrameCullNone;
			else
				throw new Exception();

			m_game.RasterizerState = state;
		}

	}

	class DebugWindowData : INotifyPropertyChanged
	{
		MyGame m_game;

		public DebugWindowData(MyGame game)
		{
			m_game = game;

			m_game.ViewGrid.ViewGridCornerChanged += (o, n) => Notify("");
		}

		public string CameraPos
		{
			get
			{
				var campos = m_game.Camera.Position;
				var chunkpos = (campos / Chunk.CHUNK_SIZE).ToFloorIntVector3();
				return String.Format("{0:F2}/{1:F2}/{2:F2} (Chunk {3}/{4}/{5})",
					campos.X, campos.Y, campos.Z,
					chunkpos.X, chunkpos.Y, chunkpos.Z);
			}
		}

		public int Vertices { get { return m_game.TerrainRenderer.VerticesRendered; } }
		public string Chunks { get { return m_game.TerrainRenderer.ChunkManager.ChunkCountDebug; } }
		public int ChunkRecalcs { get { return m_game.TerrainRenderer.ChunkRecalcs; } }
		public string GC { get; set; }

		public IntVector3 ViewCorner1 { get { return m_game.ViewGrid.ViewCorner1; } }
		public IntVector3 ViewCorner2 { get { return m_game.ViewGrid.ViewCorner2; } }

		public int ViewMaxX
		{
			get { return m_game.Environment != null ? m_game.Environment.Width : 0; }
		}

		public int ViewMaxY
		{
			get { return m_game.Environment != null ? m_game.Environment.Height : 0; }
		}

		public int ViewMaxZ
		{
			get { return m_game.Environment != null ? m_game.Environment.Depth : 0; }
		}

		public int ViewZ
		{
			get { return m_game.ViewGrid.ViewCorner2.Z; }
			set { m_game.ViewGrid.ViewCorner2 = m_game.ViewGrid.ViewCorner2.SetZ(value); }
		}

		public int ViewX1
		{
			get { return m_game.ViewGrid.ViewCorner1.X; }
			set { m_game.ViewGrid.ViewCorner1 = m_game.ViewGrid.ViewCorner1.SetX(value); }
		}

		public int ViewX2
		{
			get { return m_game.ViewGrid.ViewCorner2.X; }
			set { m_game.ViewGrid.ViewCorner2 = m_game.ViewGrid.ViewCorner2.SetX(value); }
		}

		public int ViewY1
		{
			get { return m_game.ViewGrid.ViewCorner1.Y; }
			set { m_game.ViewGrid.ViewCorner1 = m_game.ViewGrid.ViewCorner1.SetY(value); }
		}

		public int ViewY2
		{
			get { return m_game.ViewGrid.ViewCorner2.Y; }
			set { m_game.ViewGrid.ViewCorner2 = m_game.ViewGrid.ViewCorner2.SetY(value); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		int m_c0;
		int m_c1;
		int m_c2;

		public void Update(object sender, EventArgs e)
		{
			var c0 = System.GC.CollectionCount(0);
			var c1 = System.GC.CollectionCount(1);
			var c2 = System.GC.CollectionCount(2);

			var d0 = c0 - m_c0;
			var d1 = c1 - m_c1;
			var d2 = c2 - m_c2;

			this.GC = String.Format("GC0 {0}/s, GC1 {1}/s, GC2 {2}/s",
				d0, d1, d2);

			m_c0 = c0;
			m_c1 = c1;
			m_c2 = c2;

			Notify("");

			m_game.TerrainRenderer.ChunkRecalcs = 0;
		}

		void Notify(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}