﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Dwarrowdelf.Client.TileControl
{
	public interface ITileRenderer : IDisposable
	{
		void Render(DrawingContext dc, Size renderSize, TileRenderContext ctx);
	}
}