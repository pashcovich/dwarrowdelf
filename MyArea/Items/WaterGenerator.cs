﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dwarrowdelf;
using Dwarrowdelf.Server;

namespace MyArea
{
	[SaveGameObject(UseRef = true)]
	public class WaterGenerator : ItemObject
	{
		public static WaterGenerator Create(World world)
		{
			var builder = new ItemObjectBuilder(ItemID.Custom, MaterialID.Diamond)
			{
				Name = "Water Generator",
				SymbolID = SymbolID.Contraption,
				Color = GameColor.Blue,
			};

			var item = new WaterGenerator(builder);
			item.Initialize(world);
			return item;
		}

		WaterGenerator(ItemObjectBuilder builder)
			: base(builder)
		{
		}

		WaterGenerator(SaveGameContext ctx)
			: base(ctx)
		{
			this.World.TickStarting += OnTickStart;
		}

		public override void Initialize(World world)
		{
			base.Initialize(world);

			world.TickStarting += OnTickStart;
		}

		public override void Destruct()
		{
			this.World.TickStarting -= OnTickStart;

			base.Destruct();
		}

		void OnTickStart()
		{
			if (this.Environment == null)
				return;

			this.Environment.SetWaterLevel(this.Location, TileData.MaxWaterLevel);
		}
	}
}
