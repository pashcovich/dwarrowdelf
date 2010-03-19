﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows;

namespace MyGame.Client
{
	class LivingCollection : ObservableCollection<Living> { }

	class Living : ClientGameObject
	{
		// XXX not re-entrant
		static ILOSAlgo s_losAlgo = new LOSShadowCast1();

		uint m_losMapVersion;
		IntPoint3D m_losLocation;
		int m_visionRange;
		Grid2D<bool> m_visionMap;

		public int VisionRange
		{
			get { return m_visionRange; }
			set { m_visionRange = value; m_visionMap = null; }
		}

		public AI AI { get; private set; }

		public Living(World world, ObjectID objectID)
			: base(world, objectID)
		{
			this.AI = new AI(this);
			this.IsLiving = true;
		}

		DependencyProperty PropertyIDToDependencyProperty(PropertyID propertyID)
		{
			switch (propertyID)
			{
				case PropertyID.HitPoints:
					return HitPointsProperty;
				case PropertyID.SpellPoints:
					return SpellPointsProperty;

				case PropertyID.Strength:
					return StrengthProperty;
				case PropertyID.Dexterity:
					return DexterityProperty;
				case PropertyID.Constitution:
					return ConstitutionProperty;
				case PropertyID.Intelligence:
					return IntelligenceProperty;
				case PropertyID.Wisdom:
					return WisdomProperty;
				case PropertyID.Charisma:
					return CharismaProperty;

				default:
					throw new Exception();
			}
		}

		public void SetProperty(PropertyID propertyID, object value)
		{
			if (propertyID == PropertyID.Color)
			{
				this.Color = ((GameColor)value).ToColor();
				return;
			}

			var prop = PropertyIDToDependencyProperty(propertyID);
			SetValue(prop, value);
		}


		public static readonly DependencyProperty HitPointsProperty =
			DependencyProperty.Register("HitPoints", typeof(int), typeof(Living), new UIPropertyMetadata(0));
		public static readonly DependencyProperty SpellPointsProperty =
			DependencyProperty.Register("SpellPoints", typeof(int), typeof(Living), new UIPropertyMetadata(0));

		public static readonly DependencyProperty StrengthProperty =
			DependencyProperty.Register("Strength", typeof(int), typeof(Living), new UIPropertyMetadata(0));
		public static readonly DependencyProperty DexterityProperty =
			DependencyProperty.Register("Dexterity", typeof(int), typeof(Living), new UIPropertyMetadata(0));
		public static readonly DependencyProperty ConstitutionProperty =
			DependencyProperty.Register("Constitution", typeof(int), typeof(Living), new UIPropertyMetadata(0));
		public static readonly DependencyProperty IntelligenceProperty =
			DependencyProperty.Register("Intelligence", typeof(int), typeof(Living), new UIPropertyMetadata(0));
		public static readonly DependencyProperty WisdomProperty =
			DependencyProperty.Register("Wisdom", typeof(int), typeof(Living), new UIPropertyMetadata(0));
		public static readonly DependencyProperty CharismaProperty =
			DependencyProperty.Register("Charisma", typeof(int), typeof(Living), new UIPropertyMetadata(0));

		public int HitPoints
		{
			get { return (int)GetValue(HitPointsProperty); }
			set { SetValue(HitPointsProperty, value); }
		}

		public int SpellPoints
		{
			get { return (int)GetValue(SpellPointsProperty); }
			set { SetValue(SpellPointsProperty, value); }
		}

		public int Strength
		{
			get { return (int)GetValue(StrengthProperty); }
			set { SetValue(StrengthProperty, value); }
		}

		public int Dexterity
		{
			get { return (int)GetValue(DexterityProperty); }
			set { SetValue(DexterityProperty, value); }
		}

		public int Constitution
		{
			get { return (int)GetValue(ConstitutionProperty); }
			set { SetValue(ConstitutionProperty, value); }
		}

		public int Intelligence
		{
			get { return (int)GetValue(IntelligenceProperty); }
			set { SetValue(IntelligenceProperty, value); }
		}

		public int Wisdom
		{
			get { return (int)GetValue(WisdomProperty); }
			set { SetValue(WisdomProperty, value); }
		}

		public int Charisma
		{
			get { return (int)GetValue(CharismaProperty); }
			set { SetValue(CharismaProperty, value); }
		}



		public void EnqueueAction(GameAction action)
		{
			action.ActorObjectID = this.ObjectID;
			MyDebug.WriteLine("DoAction({0}: {1})", this, action);
			GameData.Data.ActionCollection.Add(action);
			GameData.Data.Connection.EnqueueAction(action);
		}

		public void ActionDone(GameAction action)
		{
			MyDebug.WriteLine("ActionDone({0}: {1})", this, action);
			GameData.Data.ActionCollection.Remove(action);
		}

		public Grid2D<bool> VisionMap
		{
			get
			{
				UpdateLOS();
				return m_visionMap;
			}
		}

		void UpdateLOS()
		{
			Debug.Assert(this.Environment.VisibilityMode == VisibilityMode.LOS);

			if (this.Environment == null)
				return;

			if (m_losLocation == this.Location && m_losMapVersion == this.Environment.Version)
				return;

			if (m_visionMap == null)
			{
				m_visionMap = new Grid2D<bool>(m_visionRange * 2 + 1, m_visionRange * 2 + 1,
					m_visionRange, m_visionRange);
				m_losMapVersion = 0;
			}

			var level = this.Environment.GetLevel(this.Location.Z);

			s_losAlgo.Calculate(this.Location.ToIntPoint(), m_visionRange,
				m_visionMap, level.Bounds,
				l => Interiors.GetInterior(level.GetInteriorID(l)).Blocker);

			m_losMapVersion = this.Environment.Version;
			m_losLocation = this.Location;
		}

	}
}
