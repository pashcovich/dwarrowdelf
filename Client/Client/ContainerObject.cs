﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dwarrowdelf.Client
{
	abstract class ContainerObject : BaseObject, IContainerObject
	{
		GameObjectCollection m_inventory;
		public ReadOnlyGameObjectCollection Inventory { get; private set; }

		public ContainerObject(World world, ObjectID objectID)
			: base(world, objectID)
		{
			m_inventory = new GameObjectCollection();
			this.Inventory = new ReadOnlyGameObjectCollection(m_inventory);
		}

		protected virtual void ChildAdded(MovableObject child) { }
		protected virtual void ChildRemoved(MovableObject child) { }
		protected virtual void ChildMoved(MovableObject child, IntPoint3D from, IntPoint3D to) { }

		public void AddChild(MovableObject ob)
		{
			m_inventory.Add(ob);
			ChildAdded(ob);
		}

		public void RemoveChild(MovableObject ob)
		{
			m_inventory.Remove(ob);
			ChildRemoved(ob);
		}

		public void MoveChild(MovableObject ob, IntPoint3D from, IntPoint3D to)
		{
			ChildMoved(ob, from, to);
		}
	}
}