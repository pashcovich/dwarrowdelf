﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dwarrowdelf
{
	public interface IWorld
	{
		int TickNumber { get; }
		event Action TickStarting;
		Random Random { get; }
	}

	public interface IIdentifiable
	{
		ObjectID ObjectID { get; }
	}

	public interface IBaseGameObject : IIdentifiable
	{
		IWorld World { get; }
	}

	public interface ILargeGameObject : IBaseGameObject
	{
		IEnvironment Environment { get; }
		IntRectZ Area { get; }
	}

	public interface IBuildingObject : ILargeGameObject
	{
		BuildingInfo BuildingInfo { get; }
	}

	public interface IGameObject : IBaseGameObject
	{
		IEnvironment Environment { get; }
		IGameObject Parent { get; }
		IntPoint3D Location { get; }
	}

	interface IEnvGameObject : IBaseGameObject
	{
		IEnvironment Environment { get; }
		IntSize Size { get; }
		IntRectZ Area { get; }

	}

	public interface IEnvironment : IGameObject, AStar.IAStarEnvironment
	{
		VisibilityMode VisibilityMode { get; }

		IntCuboid Bounds { get; }
		bool Contains(IntPoint3D p);

		TerrainID GetTerrainID(IntPoint3D l);
		MaterialID GetTerrainMaterialID(IntPoint3D l);

		InteriorID GetInteriorID(IntPoint3D l);
		MaterialID GetInteriorMaterialID(IntPoint3D l);

		TerrainInfo GetTerrain(IntPoint3D l);
		MaterialInfo GetTerrainMaterial(IntPoint3D l);

		InteriorInfo GetInterior(IntPoint3D l);
		MaterialInfo GetInteriorMaterial(IntPoint3D l);

		TileData GetTileData(IntPoint3D l);

		bool GetHidden(IntPoint3D l);
	}

	public interface ILocatableGameObject : IGameObject
	{
		string Name { get; }
		GameColor Color { get; }
		SymbolID SymbolID { get; }
		MaterialClass MaterialClass { get; }
		MaterialID MaterialID { get; }
	}

	public interface ILiving : ILocatableGameObject
	{
		GameAction CurrentAction { get; }
		bool HasAction { get; }
		bool IsDestructed { get; }

		byte GetSkillLevel(SkillID skill);
	}

	public interface IItemObject : ILocatableGameObject
	{
		ItemClass ItemClass { get; }
		ItemID ItemID { get; }
		object ReservedBy { get; set; }
	}

	public interface IPlayer
	{
		bool IsFriendly(IBaseGameObject living);
		void Send(Dwarrowdelf.Messages.ClientMessage message);
		IVisionTracker GetVisionTracker(IEnvironment env);
		ObjectVisibility GetObjectVisibility(IBaseGameObject ob);
	}

	public interface IVisionTracker
	{
		bool Sees(IntPoint3D p);
	}

	public enum ObjectVisibility
	{
		Undefined,
		None,
		Public,
		All,
	}
}
