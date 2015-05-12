﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Dwarrowdelf.Client.UI
{
	public interface IMapControl : IDisposable
	{
		event Action<MapSelection> GotSelection;
		MapSelectionMode SelectionMode { get; set; }
		MapSelection Selection { get; set; }

		void GoTo(MovableObject ob);
		void GoTo(EnvironmentObject env, IntVector3 p);
		void ScrollTo(MovableObject ob);
		void ScrollTo(EnvironmentObject env, IntVector3 p);

		EnvironmentObject Environment { get; }

		TileAreaView HoverTileView { get; }
		TileAreaView SelectionTileAreaView { get; }

		Rect GetPlacementRect(IntVector3 ml);
	}
}
