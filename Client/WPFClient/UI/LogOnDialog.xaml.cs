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

namespace Dwarrowdelf.Client
{
	/// <summary>
	/// Interaction logic for LogOnDialog.xaml
	/// </summary>
	public partial class LogOnDialog : Window
	{
		public LogOnDialog()
		{
			InitializeComponent();
		}

		public void SetText(string text)
		{
			label.Content = text;
		}
	}
}