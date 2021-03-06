﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Dwarrowdelf.Client
{
	public sealed class ChangeHandler
	{
		static Dictionary<Type, Action<ChangeHandler, ChangeData>> s_changeHandlerMap;

		static ChangeHandler()
		{
			var changeTypes = Helpers.GetNonabstractSubclasses(typeof(ChangeData));

			s_changeHandlerMap = new Dictionary<Type, Action<ChangeHandler, ChangeData>>(changeTypes.Count());

			foreach (var type in changeTypes)
			{
				var method = WrapperGenerator.CreateActionWrapper<ChangeHandler, ChangeData>("HandleChange", type);
				if (method == null)
					throw new NotImplementedException(String.Format("No HandleChange method found for {0}", type.Name));
				s_changeHandlerMap[type] = method;
			}
		}

		World m_world;

		public ChangeHandler(World world)
		{
			m_world = world;
		}

		public void HandleChangeMessage(Dwarrowdelf.Messages.ChangeMessage msg)
		{
			var change = msg.ChangeData;
			var method = s_changeHandlerMap[change.GetType()];
			method(this, change);
		}

		void HandleChange(ObjectCreatedChangeData change)
		{
			// Ignore. Client creates objects when receiving data for them.
		}

		void HandleChange(ObjectMoveChangeData change)
		{
			var ob = m_world.FindObject<MovableObject>(change.ObjectID);

			if (ob == null)
			{
				/* There's a special case where we don't get objectinfo, but we do get
				 * ObjectMove: If the object moves from tile, that just came visible to us, 
				 * to a tile that we cannot see. So let's not throw exception, but exit
				 * silently */
				// XXX is this still valid?
				return;
			}

			Debug.Assert(ob.IsInitialized);

			ContainerObject env = null;
			if (change.DestinationID != ObjectID.NullObjectID)
				env = m_world.GetObject<ContainerObject>(change.DestinationID);

			ob.MoveTo(env, change.DestinationLocation);
		}

		void HandleChange(ObjectMoveLocationChangeData change)
		{
			var ob = m_world.FindObject<MovableObject>(change.ObjectID);

			if (ob == null)
			{
				/* There's a special case where we don't get objectinfo, but we do get
				 * ObjectMove: If the object moves from tile, that just came visible to us, 
				 * to a tile that we cannot see. So let's not throw exception, but exit
				 * silently */
				// XXX is this still valid?
				return;
			}

			Debug.Assert(ob.IsInitialized);

			ob.MoveTo(change.DestinationLocation);
		}

		void HandlePropertyChange(ObjectID objectID, PropertyID propertyID, object value)
		{
			var ob = m_world.GetObject<BaseObject>(objectID);

			Debug.Assert(ob.IsInitialized);

			ob.SetProperty(propertyID, value);
		}

		void HandleChange(PropertyValueChangeData change)
		{
			HandlePropertyChange(change.ObjectID, change.PropertyID, change.Value);
		}

		void HandleChange(PropertyIntChangeData change)
		{
			HandlePropertyChange(change.ObjectID, change.PropertyID, change.Value);
		}

		void HandleChange(PropertyStringChangeData change)
		{
			HandlePropertyChange(change.ObjectID, change.PropertyID, change.Value);
		}

		void HandleChange(SkillChangeData change)
		{
			var ob = m_world.GetObject<LivingObject>(change.ObjectID);

			Debug.Assert(ob.IsInitialized);

			ob.SetSkillLevel(change.SkillID, change.Level);
		}

		void HandleChange(ObjectDestructedChangeData change)
		{
			var ob = m_world.FindObject<BaseObject>(change.ObjectID);

			if (ob == null)
				return;

			Debug.Assert(ob.IsInitialized);

			ob.Destruct();
		}

		void HandleChange(MapChangeData change)
		{
			var env = m_world.GetObject<EnvironmentObject>(change.EnvironmentID);

			Debug.Assert(env.IsInitialized);

			env.SetTileData(change.Location, change.TileData);
		}

		void HandleChange(TickStartChangeData change)
		{
			m_world.HandleChange(change);
		}

		void HandleChange(TurnStartChangeData change)
		{
			m_world.HandleChange(change);
		}

		void HandleChange(TurnEndChangeData change)
		{
			m_world.HandleChange(change);
		}

		void HandleChange(GameDateChangeData change)
		{
			m_world.HandleChange(change);
		}

		void HandleChange(ActionStartedChangeData change)
		{
			//Debug.WriteLine("ActionStartedChange({0})", change.ObjectID);

			var ob = m_world.GetObject<LivingObject>(change.ObjectID);

			Debug.Assert(ob.IsInitialized);

			ob.HandleActionStartEvent(change.ActionStartEvent);
		}

		void HandleChange(ActionProgressChangeData change)
		{
			var ob = m_world.GetObject<LivingObject>(change.ObjectID);

			Debug.Assert(ob.IsInitialized);

			ob.HandleActionProgressEvent(change.ActionProgressEvent);
		}

		void HandleChange(ActionDoneChangeData change)
		{
			var ob = m_world.GetObject<LivingObject>(change.ObjectID);

			Debug.Assert(ob.IsInitialized);

			ob.HandleActionDone(change.ActionDoneEvent);
		}
	}
}
