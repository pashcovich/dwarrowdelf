﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Dwarrowdelf.TerrainGen
{
	public sealed class TerrainData
	{
		public IntSize3 Size { get; private set; }
		TileData[, ,] m_tileGrid;
		byte[,] m_levelMap;

		public int Width { get { return this.Size.Width; } }
		public int Height { get { return this.Size.Height; } }
		public int Depth { get { return this.Size.Depth; } }

		public TerrainData(IntSize3 size)
		{
			this.Size = size;
			m_levelMap = new byte[size.Height, size.Width];
			m_tileGrid = new TileData[size.Depth, size.Height, size.Width];
		}

		public void GetData(out TileData[, ,] tileGrid, out byte[,] levelMap)
		{
			tileGrid = m_tileGrid;
			levelMap = m_levelMap;
		}

		public void RescanLevelMap()
		{
			var levelMap = m_levelMap;

			Parallel.ForEach(this.Size.Plane.Range(), p =>
			{
				for (int z = this.Size.Depth - 1; z >= 0; --z)
				{
					if (GetTileData(p.X, p.Y, z).IsWall)
					{
						levelMap[p.Y, p.X] = (byte)(z + 1);
						break;
					}
				}
			});
		}

		public bool Contains(IntVector3 p)
		{
			return p.X >= 0 && p.Y >= 0 && p.Z >= 0 && p.X < this.Width && p.Y < this.Height && p.Z < this.Depth;
		}

		public int GetSurfaceLevel(int x, int y)
		{
			return m_levelMap[y, x];
		}

		public int GetSurfaceLevel(IntVector2 p)
		{
			return m_levelMap[p.Y, p.X];
		}

		public void SetSurfaceLevel(int x, int y, int level)
		{
			m_levelMap[y, x] = (byte)level;
		}

		public IntVector3 GetSurfaceLocation(int x, int y)
		{
			return new IntVector3(x, y, GetSurfaceLevel(x, y));
		}

		public IntVector3 GetSurfaceLocation(IntVector2 p)
		{
			return new IntVector3(p, GetSurfaceLevel(p));
		}

		public TileID GetTileID(IntVector3 p)
		{
			return GetTileData(p).ID;
		}

		public MaterialID GetMaterialID(IntVector3 p)
		{
			return GetTileData(p).MaterialID;
		}

		public MaterialInfo GetMaterial(IntVector3 p)
		{
			return Materials.GetMaterial(GetMaterialID(p));
		}

		public TileData GetTileData(int x, int y, int z)
		{
			return m_tileGrid[z, y, x];
		}

		public TileData GetTileData(IntVector3 p)
		{
			return m_tileGrid[p.Z, p.Y, p.X];
		}

		public byte GetWaterLevel(IntVector3 p)
		{
			return GetTileData(p).WaterLevel;
		}

		public void SetTileData(IntVector3 p, TileData data)
		{
			int oldLevel = GetSurfaceLevel(p.X, p.Y);

			if (data.IsWall && oldLevel <= p.Z)
			{
				// Surface level has risen
				Debug.Assert(p.Z >= 0 && p.Z < 256);
				SetSurfaceLevel(p.X, p.Y, p.Z + 1);
			}
			else if (data.IsWall == false && oldLevel == p.Z + 1)
			{
				// Surface level has possibly lowered
				if (p.Z == 0)
					throw new Exception();

				for (int z = p.Z - 1; z >= 0; --z)
				{
					if (GetTileData(p.X, p.Y, z).IsWall)
					{
						Debug.Assert(z >= 0 && z < 256);
						SetSurfaceLevel(p.X, p.Y, z + 1);
						break;
					}
				}
			}

			SetTileDataNoHeight(p, data);
		}

		public void SetTileDataNoHeight(IntVector3 p, TileData data)
		{
			data.Flags |= TileFlags.Error; // ZZZ
			m_tileGrid[p.Z, p.Y, p.X] = data;
		}

		public unsafe void SaveTerrain(string path, string name)
		{
			using (var stream = File.Create(path))
			{
				using (var bw = new BinaryWriter(stream, Encoding.Default, true))
				{
					bw.Write(name);

					bw.Write(this.Size.Width);
					bw.Write(this.Size.Height);
					bw.Write(this.Size.Depth);
				}

				fixed (TileData* v = this.m_tileGrid)
				{
					byte* p = (byte*)v;
					using (var memStream = new UnmanagedMemoryStream(p, this.Size.Volume * sizeof(TileData)))
						memStream.CopyTo(stream);
				}

				fixed (byte* v = this.m_levelMap)
				{
					byte* p = (byte*)v;
					using (var memStream = new UnmanagedMemoryStream(p, this.Width * this.Height * sizeof(byte)))
						memStream.CopyTo(stream);
				}
			}
		}

		public unsafe static TerrainData LoadTerrain(string path, string expectedName, IntSize3 expectedSize)
		{
			if (File.Exists(path) == false)
				return null;

			using (var stream = File.OpenRead(path))
			{
				TerrainData terrain;

				using (var br = new BinaryReader(stream, Encoding.Default, true))
				{
					var name = br.ReadString();

					if (name != expectedName)
						return null;

					int w = br.ReadInt32();
					int h = br.ReadInt32();
					int d = br.ReadInt32();

					var size = new IntSize3(w, h, d);

					if (size != expectedSize)
						return null;

					terrain = new TerrainData(size);
				}

				fixed (TileData* v = terrain.m_tileGrid)
				{
					byte* p = (byte*)v;

					int len = terrain.Size.Volume * sizeof(TileData);

					using (var memStream = new UnmanagedMemoryStream(p, 0, len, FileAccess.Write))
						CopyTo(stream, memStream, len);
				}

				fixed (byte* p = terrain.m_levelMap)
				{
					int len = terrain.Size.Plane.Area * sizeof(byte);

					using (var memStream = new UnmanagedMemoryStream(p, 0, len, FileAccess.Write))
						CopyTo(stream, memStream, len);
				}

				return terrain;
			}
		}

		static void CopyTo(Stream input, Stream output, int len)
		{
			var arr = new byte[4096 * 8];

			while (len > 0)
			{
				int l = input.Read(arr, 0, Math.Min(len, arr.Length));

				if (l == 0)
					throw new EndOfStreamException();

				output.Write(arr, 0, l);

				len -= l;
			}
		}
	}
}
