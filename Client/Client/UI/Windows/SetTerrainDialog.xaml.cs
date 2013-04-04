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
using System.Windows.Shapes;

namespace Dwarrowdelf.Client.UI
{
	sealed class SetTerrainData
	{
		public TerrainID? TerrainID { get; set; }
		public MaterialID? TerrainMaterialID { get; set; }

		public InteriorID? InteriorID { get; set; }
		public MaterialID? InteriorMaterialID { get; set; }

		public bool? Water { get; set; }
	}

	sealed partial class SetTerrainDialog : Window
	{
		public SetTerrainDialog()
		{
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		public SetTerrainData Data
		{
			get { return (SetTerrainData)GetValue(UserProperty); }
			set { SetValue(UserProperty, value); }
		}

		public static readonly DependencyProperty UserProperty =
			DependencyProperty.Register("Data", typeof(SetTerrainData), typeof(SetTerrainDialog), new PropertyMetadata(new SetTerrainData()));

		private void Button_Click_Preset(object sender, RoutedEventArgs e)
		{
			var b = (Button)sender;
			var s = (string)b.Content;

			var td = new TileData();

			switch (s)
			{
				case "Empty":
					td.TerrainID = TerrainID.Empty;
					td.InteriorID = InteriorID.Empty;
					break;

				case "Wall":
					td.TerrainID = TerrainID.NaturalFloor;
					td.TerrainMaterialID = MaterialID.Granite;
					td.InteriorID = InteriorID.NaturalWall;
					td.InteriorMaterialID = MaterialID.Granite;
					break;

				case "Floor":
					td.TerrainID = TerrainID.NaturalFloor;
					td.TerrainMaterialID = MaterialID.Granite;
					td.InteriorID = InteriorID.Empty;
					break;
			}

			terrainIDListBox.SelectedValue = td.TerrainID;
			terrainMaterialListBox.SelectedValue = td.TerrainMaterialID;
			interiorIDListBox.SelectedValue = td.InteriorID;
			interiorMaterialListBox.SelectedValue = td.InteriorMaterialID;
		}
	}
}
