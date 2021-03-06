﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dwarrowdelf.TerrainGen;

namespace Dwarrowdelf.Server.Fortress
{
	public class DungeonWorldCreator
	{
		World m_world;
		EnvironmentObject m_env;
		Dictionary<int, IntGrid2[]> m_rooms;
		TerrainData m_terrainData;

		LivingInfo[] m_livingInfos;

		public EnvironmentObject MainEnv { get { return m_env; } }

		public DungeonWorldCreator(World world)
		{
			m_world = world;

			m_livingInfos = Livings.GetLivingInfos(LivingCategory.Monster | LivingCategory.Herbivore | LivingCategory.Carnivore).ToArray();
		}

		public void InitializeWorld(World world, IntSize3 size)
		{
			CreateTerrain(size);

			IntVector3? stairs = null;

			foreach (var p2 in m_terrainData.Size.Plane.Range())
			{
				var p = new IntVector3(p2, m_terrainData.Size.Depth - 1);

				var td = m_terrainData.GetTileData(p.Down);
				if (td.ID == TileID.Stairs)
				{
					stairs = p;
					break;
				}
			}

			if (stairs.HasValue == false)
				throw new Exception();

			m_env = EnvironmentObject.Create(world, m_terrainData, VisibilityMode.LivingLOS, stairs.Value);

			CreateMonsters();

			CreateDebugMonsterAtEntry();
		}

		void CreateDebugMonsterAtEntry()
		{
			var pn = GetLocNearEntry(m_env);
			if (pn.HasValue)
			{
				var living = CreateRandomLiving(1);
				living.MoveToMustSucceed(m_env, pn.Value);
			}
		}

		void CreateMonsters()
		{
			foreach (var kvp in m_rooms)
			{
				int z = kvp.Key;
				var rooms = kvp.Value;

				for (int i = 0; i < 10; ++i)
				{
					var room = new IntGrid2Z(rooms[Helpers.GetRandomInt(rooms.Length)], z);

					var pn = GetRandomRoomLoc(m_env, ref room);
					if (pn.HasValue == false)
						continue;

					var p = pn.Value;

					var living = CreateRandomLiving(z);
					living.MoveToMustSucceed(m_env, p);
				}
			}
		}

		static IntVector3? GetRandomRoomLoc(EnvironmentObject env, ref IntGrid2Z grid)
		{
			int x = grid.X + Helpers.GetRandomInt(grid.Columns);
			int y = grid.Y + Helpers.GetRandomInt(grid.Rows);

			foreach (var p in IntVector2.SquareSpiral(new IntVector2(x, y), Math.Max(grid.Columns, grid.Rows)))
			{
				if (env.Size.Plane.Contains(p) == false)
					continue;

				var p3 = new IntVector3(p, grid.Z);

				if (env.CanEnter(p3) == false)
					continue;

				return p3;
			}

			return null;
		}

		LivingObject CreateRandomLiving(int z)
		{
			var li = m_livingInfos[Helpers.GetRandomInt(m_livingInfos.Length)];

			var livingBuilder = new LivingObjectBuilder(li.ID);
			var living = livingBuilder.Create(m_world);
			living.SetAI(new Dwarrowdelf.AI.MonsterAI(living, m_world.PlayerID));

			Helpers.AddBattleGear(living);

			return living;
		}

		static IntVector3? GetLocNearEntry(EnvironmentObject env)
		{
			foreach (var p in IntVector2.SquareSpiral(env.StartLocation.ToIntVector2(), env.Width / 2))
			{
				if (env.Size.Plane.Contains(p) == false)
					continue;

				var p3 = env.GetSurfaceLocation(p);

				if (env.CanEnter(p3))
					return p3;
			}

			return null;
		}

		void CreateTerrain(IntSize3 size)
		{
			var random = Helpers.Random;

			var terrain = new TerrainData(size);

			var tg = new DungeonTerrainGenerator(terrain, random);

			tg.Generate(1);

			TerrainHelpers.CreateSoil(terrain, 9999);
			TerrainHelpers.CreateVegetation(terrain, random, 9999);

			m_rooms = tg.Rooms;
			m_terrainData = terrain;
		}
	}
}
