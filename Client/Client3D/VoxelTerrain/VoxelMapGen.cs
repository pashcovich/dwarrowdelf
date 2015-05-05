﻿using Dwarrowdelf;
using SharpNoise;
using SharpNoise.Builders;
using SharpNoise.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwarrowdelf.Client
{
	static class VoxelMapGen
	{
		public static NoiseMap CreateTerrainNoiseMap(Module noise, IntSize2 size)
		{
			var map = new NoiseMap();

			var build = new SharpNoise.Builders.PlaneNoiseMapBuilder()
			{
				DestNoiseMap = map,
				EnableSeamless = false,
				SourceModule = noise,
			};

			build.SetDestSize(size.Width, size.Height);
			build.SetBounds(0.5, 1.5, 0.5, 1.5);
			build.Build();

			//map.BorderValue = 1;
			//map = NoiseMap.BilinearFilter(map, this.Width, this.Height);

			return map;
		}

		public static Module CreateTerrainNoise()
		{
			var mountainTerrain = new RidgedMulti()
			{

			};

			var baseFlatTerrain = new Billow()
			{
				Frequency = 2,
			};

			var flatTerrain = new ScaleBias()
			{
				Source0 = baseFlatTerrain,
				Scale = 0.125,
				Bias = -0.75,
			};

			var terrainType = new Perlin()
			{
				Frequency = 0.5,
				Persistence = 0.25,
			};

			var terrainSelector = new Select()
			{
				Source0 = flatTerrain,
				Source1 = mountainTerrain,
				Control = terrainType,
				LowerBound = 0,
				UpperBound = 1000,
				EdgeFalloff = 0.125,
			};

			var finalTerrain = new Turbulence()
			{
				Source0 = terrainSelector,
				Frequency = 4,
				Power = 0.125,
			};

			return finalTerrain;
		}
	}
}
