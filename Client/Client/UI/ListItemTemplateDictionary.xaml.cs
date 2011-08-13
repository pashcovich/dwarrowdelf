﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;

namespace Dwarrowdelf.Client.UI
{
	partial class ListItemTemplateDictionary
	{
		public void Button_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;

			var ob = (BaseGameObject)button.DataContext;

			var contentControl = new ContentControl();
			contentControl.Resources = new ResourceDictionary() { Source = new Uri("/UI/ContentTemplateDictionary.xaml", UriKind.Relative) };
			contentControl.Content = ob;

			var dockableContent = new DockableContent()
			{
				Title = ob.ToString(),
				HideOnClose = false,
				IsCloseable = true,
				Content = contentControl,
			};

			// XXX for some reason the DockableContent window seems to stay even after closed. This leads to the object being referenced.
			// This hack lets at least the object to be collected, although the window will stay in memory.
			dockableContent.Closed += (s2, e2) =>
				{
					var s = (DockableContent)s2;
					s.Content = null;
				};

			dockableContent.ShowAsFloatingWindow(GameData.Data.MainWindow.Dock, true);
		}
	}
}