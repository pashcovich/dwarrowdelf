﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyGame;
using MyGame.Server;
using Environment = MyGame.Server.Environment;

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

		IntPoint3D m_portalLoc = new IntPoint3D(2, 4, 9);
		IntPoint3D m_portalLoc2 = new IntPoint3D(1, 1, 0);

		bool ActionHandler(ServerGameObject ob, GameAction action)
		{
			if (ob.Environment == m_map1 && ob.Location == m_portalLoc)
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
			else if (ob.Environment == m_map2 && ob.Location == m_portalLoc2)
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
			int sizeExp = 5;
			int size = (int)Math.Pow(2, sizeExp);
			var terrainGen = new TerrainGen(sizeExp, 10, 5, 0.75);


			// XXX some size limit with the size in WCF
			var env = new Environment(world, size, size, 20, VisibilityMode.LOS);
			env.Name = "map1";

			Random r = new Random(123);

			var stone = world.AreaData.Materials.GetMaterialInfo("Stone").ID;
			var steel = world.AreaData.Materials.GetMaterialInfo("Steel").ID;
			var diamond = world.AreaData.Materials.GetMaterialInfo("Diamond").ID;
			var wood = world.AreaData.Materials.GetMaterialInfo("Wood").ID;

			foreach (var p in env.Bounds.Range())
			{
				double d = terrainGen.Grid[p.TwoD];

				if (d > p.Z)
				{
					env.SetInterior(p, InteriorID.NaturalWall, stone);
					env.SetFloor(p, FloorID.NaturalFloor, stone);
				}
				else
				{
					env.SetInteriorID(p, InteriorID.Empty);
					if (env.GetInteriorID(p + new IntVector3D(0, 0, -1)) != InteriorID.Empty)
						env.SetFloor(p, FloorID.NaturalFloor, stone);
					else
						env.SetFloor(p, FloorID.Empty, stone);
				}

				if (p.X == 2 && p.Y == 2 && p.Z > 0)
					env.SetInterior(p, (p.Z % 2) == 0 ? InteriorID.StairsDown : InteriorID.StairsUp, stone);
				else if (p.X == 8 && p.Y == 8)
					env.SetInterior(p, (p.Z % 2) != 0 ? InteriorID.StairsDown : InteriorID.StairsUp, stone);
			}

			env.SetInterior(m_portalLoc, InteriorID.Portal, steel);
			env.SetActionHandler(m_portalLoc, ActionHandler);

			var syms = world.AreaData.Symbols;

			var rand = new Random();
			for (int i = 0; i < 1; ++i)
			{
				// Add a monster
				var monster = new Living(world);
				monster.SymbolID = syms.Single(o => o.Name == "Monster").ID;
				monster.Name = String.Format("monsu{0}", i);
				if (monster.MoveTo(env, new IntPoint3D(6, 6, 9)) == false)
					throw new Exception();
				var monsterAI = new MonsterActor(monster);
				monster.Actor = monsterAI;
				monster.Color = new GameColor((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
			}

			// Add items
			for (int i = 0; i < 10; ++i)
			{
				IntPoint3D p;
				do
				{
					p = new IntPoint3D(rand.Next(env.Width), rand.Next(env.Height), 9);
				} while (env.GetInteriorID(p) != InteriorID.Empty);

				var item = new ItemObject(world)
				{
					SymbolID = syms.Single(o => o.Name == "Gem").ID,
					Name = "gem" + i.ToString(),
					Color = new GameColor((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256)),
					MaterialID = diamond,
				};

				item.MoveTo(env, p);
			}

			{
				// Add an item
				var item = new ItemObject(world)
				{
					SymbolID = syms.Single(o => o.Name == "Gem").ID,
					Name = "red gem",
					Color = GameColors.Red,
					MaterialID = diamond,
				};
				item.MoveTo(env, new IntPoint3D(3, 1, 9));

				item = new ItemObject(world)
				{
					SymbolID = syms.Single(o => o.Name == "Gem").ID,
					Name = "gem",
					MaterialID = diamond,
				};
				item.MoveTo(env, new IntPoint3D(2, 1, 9));

				item = new ItemObject(world)
				{
					SymbolID = syms.Single(o => o.Name == "Tree").ID,
					Name = "puu",
					MaterialID = wood,
				};
				item.MoveTo(env, new IntPoint3D(1, 3, 9));
			}

			var building = new BuildingData(world, BuildingID.Smith) { Area = new IntRect(2, 4, 2, 2), Z = 9 };
			env.AddBuilding(building);

			return env;
		}

		Environment CreateMap2(World world)
		{
			var env = new Environment(world, 20, 20, 1, VisibilityMode.SimpleFOV);
			env.Name = "map2";

			var stone = world.AreaData.Materials.GetMaterialInfo("Stone").ID;
			var steel = world.AreaData.Materials.GetMaterialInfo("Steel").ID;

			foreach (var p in env.Bounds.Range())
			{
				env.SetInteriorID(p, InteriorID.Empty);
				env.SetFloor(p, FloorID.NaturalFloor, stone);
			}

			env.SetInterior(m_portalLoc2, InteriorID.Portal, steel);
			env.SetActionHandler(m_portalLoc2, ActionHandler);

			return env;
		}
	}
}
