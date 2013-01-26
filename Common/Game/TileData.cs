﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Dwarrowdelf
{
	/// <summary>
	/// Maintained by EnvironmentObject internally
	/// </summary>
	[Flags]
	public enum TileFlags : ushort
	{
		None = 0,
		ItemBlocks = 1 << 0,	// an item in the tile blocks movement
		Subterranean = 1 << 1,
	}

	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public struct TileData
	{
		[FieldOffset(0)]
		public ulong Raw;
		[NonSerialized]
		[FieldOffset(0)]
		public uint RawTile;	// contains only terrain & interior data

		[NonSerialized]
		[FieldOffset(0)]
		public TerrainID TerrainID;
		[NonSerialized]
		[FieldOffset(1)]
		public MaterialID TerrainMaterialID;

		[NonSerialized]
		[FieldOffset(2)]
		public InteriorID InteriorID;
		[NonSerialized]
		[FieldOffset(3)]
		public MaterialID InteriorMaterialID;

		[NonSerialized]
		[FieldOffset(4)]
		public TileFlags Flags;

		[NonSerialized]
		[FieldOffset(6)]
		public byte WaterLevel;

		[NonSerialized]
		[FieldOffset(7)]
		public byte Unused;

		public const int MinWaterLevel = 1;
		public const int MaxWaterLevel = 7;
		public const int MaxCompress = 1;

		public const int SizeOf = 8;

		static readonly TileData EmptyTileData = new TileData()
		{
			TerrainID = TerrainID.Empty,
			TerrainMaterialID = MaterialID.Undefined,
			InteriorID = InteriorID.Empty,
			InteriorMaterialID = MaterialID.Undefined,
			WaterLevel = 0,
		};

		public static readonly TileData UndefinedTileData = new TileData()
		{
			TerrainID = TerrainID.Undefined,
			TerrainMaterialID = MaterialID.Undefined,
			InteriorID = InteriorID.Undefined,
			InteriorMaterialID = MaterialID.Undefined,
		};

		/// <summary>
		/// Is Interior a sapling or a full grown tree
		/// </summary>
		public bool HasTree { get { return this.InteriorID.IsTree(); } }

		/// <summary>
		/// Check if tile is TerrainID.Empty, MaterialID.Undefined, InteriorID.Empty, MaterialID.Undefined
		/// </summary>
		public bool IsEmpty { get { return this.RawTile == EmptyTileData.RawTile; } }

		/// <summary>
		/// Check if tile is TerrainID.Undefined, MaterialID.Undefined, InteriorID.Undefined, MaterialID.Undefined
		/// </summary>
		public bool IsUndefined { get { return this.RawTile == UndefinedTileData.RawTile; } }

		/// <summary>
		/// Is the tile a floor with empty or soft interior
		/// </summary>
		public bool IsClear { get { return this.TerrainID.IsFloor() && this.InteriorID.IsClear(); } }

		/// <summary>
		/// Can one see through this tile in planar directions
		/// </summary>
		public bool IsSeeThrough
		{
			get
			{
				if (this.IsUndefined)
					throw new Exception();

				var terrain = Terrains.GetTerrain(this.TerrainID);
				var interior = Interiors.GetInterior(this.InteriorID);

				return terrain.IsSeeThrough && interior.IsSeeThrough;
			}
		}

		/// <summary>
		/// Can one see through this tile downwards
		/// </summary>
		public bool IsSeeThroughDown
		{
			get
			{
				if (this.IsUndefined)
					throw new Exception();

				var terrain = Terrains.GetTerrain(this.TerrainID);
				var interior = Interiors.GetInterior(this.InteriorID);

				return terrain.IsSeeThroughDown && interior.IsSeeThrough;
			}
		}
	}
}