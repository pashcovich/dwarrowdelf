﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyGame;
using Environment = MyGame.Environment;

namespace MyArea
{
	public class Area : IArea
	{
		Environment m_map1;
		Environment m_map2;

		public void InitializeWorld(World world, IList<Environment> environments)
		{
			m_map1 = CreateMap1(world);
			environments.Add(m_map1);

			m_map2 = CreateMap2(world);
			environments.Add(m_map2);
		}

		bool ActionHandler(ServerGameObject ob, GameAction action)
		{
			if (ob.Environment == m_map1 && ob.Location == new IntPoint3D(2, 3, 0))
			{
				var a = action as MoveAction;

				if (a == null)
					return false;

				if (a.Direction != Direction.Up && a.Direction != Direction.Down)
					return false;

				var dst = m_map2;
				var dstPos = new IntPoint3D(0, 0, 0);

				var ok = ob.MoveTo(dst, dstPos);

				return ok;
			}
			else if (ob.Environment == m_map2 && ob.Location == new IntPoint3D(2, 3, 0))
			{
				var a = action as MoveAction;

				if (a == null)
					return false;

				if (a.Direction != Direction.Up && a.Direction != Direction.Down)
					return false;

				var dst = m_map1;
				var dstPos = new IntPoint3D(0, 0, 0);

				var ok = ob.MoveTo(dst, dstPos);

				return ok;
			}
			else
			{
				throw new Exception();
			}
		}

		Environment CreateMap1(World world)
		{
			// XXX some size limit with the size in WCF
			var env = new Environment(world, 100, 100, 4, VisibilityMode.AllVisible);
			env.Name = "map1";

			Random r = new Random(123);
			TerrainInfo floor = world.AreaData.Terrains.Single(t => t.Name == "Dungeon Floor");
			TerrainInfo wall = world.AreaData.Terrains.Single(t => t.Name == "Dungeon Wall");
			TerrainInfo down = world.AreaData.Terrains.Single(t => t.Name == "Stairs Down");
			TerrainInfo up = world.AreaData.Terrains.Single(t => t.Name == "Stairs Up");
			TerrainInfo portal = world.AreaData.Terrains.Single(t => t.Name == "Portal");

			foreach (var p in env.Bounds.Range())
			{
				if (p.X == 2 && p.Y == 2)
					env.SetTerrain(p, (p.Z % 2) == 0 ? down.ID : up.ID);
				else if (p.X == 3 && p.Y == 3)
					env.SetTerrain(p, (p.Z % 2) != 0 ? down.ID : up.ID);
				else if (p.X < 7 && p.Y < 7)
					env.SetTerrain(p, floor.ID);
				else if (r.Next() % 8 == 0)
					env.SetTerrain(p, wall.ID);
				else
					env.SetTerrain(p, floor.ID);
			}

			var pp = new IntPoint3D(2, 3, 0);
			env.SetTerrain(pp, portal.ID);
			env.SetActionHandler(pp, ActionHandler);

			var obs = world.AreaData.Objects;

			var rand = new Random();
			for (int i = 0; i < 10; ++i)
			{
				// Add a monster
				var monster = new Living(world);
				monster.SymbolID = obs.Single(o => o.Name == "Monster").SymbolID;
				monster.Name = String.Format("monsu{0}", i);
				if (monster.MoveTo(env, new IntPoint3D(6, 6, 0)) == false)
					throw new Exception();
				var monsterAI = new MonsterActor(monster);
				monster.Actor = monsterAI;
				monster.Color = new GameColor((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
			}

			// Add an item
			var item = new ItemObject(world);
			item.SymbolID = obs.Single(o => o.Name == "Gem").SymbolID; ;
			item.Name = "red gem";
			item.Color = GameColors.Red;
			item.MoveTo(env, new IntPoint3D(3, 0, 0));

			item = new ItemObject(world);
			item.SymbolID = obs.Single(o => o.Name == "Gem").SymbolID; ;
			item.Name = "gem";
			item.MoveTo(env, new IntPoint3D(2, 0, 0));

			item = new ItemObject(world);
			item.SymbolID = obs.Single(o => o.Name == "Tree").SymbolID; ;
			item.Name = "puu";
			item.MoveTo(env, new IntPoint3D(0, 3, 0));

			return env;
		}

		Environment CreateMap2(World world)
		{
			var env = new Environment(world, 20, 20, 1, VisibilityMode.SimpleFOV);
			env.Name = "map2";

			TerrainInfo floor = world.AreaData.Terrains.Single(t => t.Name == "Dungeon Floor");
			TerrainInfo wall = world.AreaData.Terrains.Single(t => t.Name == "Dungeon Wall");
			TerrainInfo down = world.AreaData.Terrains.Single(t => t.Name == "Stairs Down");
			TerrainInfo up = world.AreaData.Terrains.Single(t => t.Name == "Stairs Up");
			TerrainInfo portal = world.AreaData.Terrains.Single(t => t.Name == "Portal");

			foreach (var p in env.Bounds.Range())
				env.SetTerrain(p, floor.ID);

			var pp = new IntPoint3D(2, 3, 0);
			env.SetTerrain(pp, portal.ID);
			env.SetActionHandler(pp, ActionHandler);

			return env;
		}
	}
}
