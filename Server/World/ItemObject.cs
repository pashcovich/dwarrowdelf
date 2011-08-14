﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Dwarrowdelf.Server
{
	[SaveGameObject(UseRef = true)]
	public class ItemObject : LocatableGameObject, IItemObject
	{
		internal static ItemObject Create(World world, ItemObjectBuilder builder)
		{
			var ob = new ItemObject(builder);
			ob.Initialize(world);
			return ob;
		}

		protected ItemObject(SaveGameContext ctx)
			: base(ctx, ObjectType.Item)
		{
		}

		protected ItemObject(ItemObjectBuilder builder)
			: base(ObjectType.Item, builder)
		{
			Debug.Assert(builder.ItemID != Dwarrowdelf.ItemID.Undefined);
			Debug.Assert(builder.MaterialID != Dwarrowdelf.MaterialID.Undefined);

			this.ItemID = builder.ItemID;
			m_nutritionalValue = builder.NutritionalValue;
			m_refreshmentValue = builder.RefreshmentValue;
		}

		[SaveGameProperty]
		public ItemID ItemID { get; private set; }
		public ItemInfo ItemInfo { get { return Dwarrowdelf.Items.GetItem(this.ItemID); } }
		public ItemCategory ItemCategory { get { return this.ItemInfo.Category; } }

		public object ReservedBy { get; set; }

		[SaveGameProperty("NutritionalValue")]
		int m_nutritionalValue;
		public int NutritionalValue
		{
			get { return m_nutritionalValue; }
			set { if (m_nutritionalValue == value) return; m_nutritionalValue = value; NotifyInt(PropertyID.NutritionalValue, value); }
		}

		[SaveGameProperty("RefreshmentValue")]
		int m_refreshmentValue;
		public int RefreshmentValue
		{
			get { return m_refreshmentValue; }
			set { if (m_refreshmentValue == value) return; m_refreshmentValue = value; NotifyInt(PropertyID.RefreshmentValue, value); }
		}

		protected override void SerializeTo(BaseGameObjectData data, ObjectVisibility visibility)
		{
			base.SerializeTo(data, visibility);

			SerializeToInternal((ItemData)data, visibility);
		}

		void SerializeToInternal(ItemData data, ObjectVisibility visibility)
		{
			data.ItemID = this.ItemID;
		}

		public override void SendTo(IPlayer player, ObjectVisibility visibility)
		{
			var data = new ItemData();

			SerializeTo(data, visibility);

			player.Send(new Messages.ObjectDataMessage() { ObjectData = data });

			base.SendTo(player, visibility);
		}

		protected override Dictionary<PropertyID, object> SerializeProperties(ObjectVisibility visibility)
		{
			var props = base.SerializeProperties(visibility);
			if (visibility == ObjectVisibility.All)
			{
				props[PropertyID.NutritionalValue] = m_nutritionalValue;
				props[PropertyID.RefreshmentValue] = m_refreshmentValue;
			}
			return props;
		}

		public override string ToString()
		{
			return String.Format("ItemObject({0}/{1})", this.Name, this.ObjectID);
		}
	}

	public class ItemObjectBuilder : LocatableGameObjectBuilder
	{
		public ItemID ItemID { get; set; }
		public int NutritionalValue { get; set; }
		public int RefreshmentValue { get; set; }

		public ItemObjectBuilder(ItemID itemID, MaterialID materialID)
		{
			this.ItemID = itemID;
			this.MaterialID = materialID;
		}

		public ItemObject Create(World world)
		{
			return ItemObject.Create(world, this);
		}
	}
}
