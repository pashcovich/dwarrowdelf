﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MyGame
{
	[DataContract]
	public struct IntVector3D : IEquatable<IntVector3D>
	{
		[DataMember(Name = "X")]
		readonly int m_x;
		[DataMember(Name = "Y")]
		readonly int m_y;
		[DataMember(Name = "Z")]
		readonly int m_z;

		public int X { get { return m_x; } }
		public int Y { get { return m_y; } }
		public int Z { get { return m_z; } }

		public IntVector3D(int x, int y, int z)
		{
			m_x = x;
			m_y = y;
			m_z = z;
		}

		#region IEquatable<IntVector3D> Members

		public bool Equals(IntVector3D other)
		{
			return ((other.X == this.X) && (other.Y == this.Y) && (other.Z == this.Z));
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (!(obj is IntVector3D))
				return false;

			IntVector3D l = (IntVector3D)obj;
			return ((l.X == this.X) && (l.Y == this.Y) && (l.Z == this.Z));
		}

		public double Length
		{
			get { return Math.Pow(this.X * this.X + this.Y * this.Y + this.Z * this.Z, 1.0 / 3.0); }
		}

		public int ManhattanLength
		{
			get { return Math.Abs(this.X) + Math.Abs(this.Y) + Math.Abs(this.Z); }
		}

		public IntVector3D Normalize()
		{
			double len = this.Length;
			var x = (int)Math.Round(this.X / len);
			var y = (int)Math.Round(this.Y / len);
			var z = (int)Math.Round(this.Z / len);
			return new IntVector3D(x, y, z);
		}

		public static bool operator ==(IntVector3D left, IntVector3D right)
		{
			return ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z));
		}

		public static bool operator !=(IntVector3D left, IntVector3D right)
		{
			return !(left == right);
		}

		public static IntVector3D operator +(IntVector3D left, IntVector3D right)
		{
			return new IntVector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static IntVector3D operator -(IntVector3D left, IntVector3D right)
		{
			return new IntVector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static IntVector3D operator *(IntVector3D left, int number)
		{
			return new IntVector3D(left.X * number, left.Y * number, left.Z * number);
		}

		public override int GetHashCode()
		{
			return (this.X << 20) | (this.Y << 10) | this.Z;
		}

		public override string ToString()
		{
			return String.Format(System.Globalization.CultureInfo.InvariantCulture,
				"IntVector3D({0}, {1}, {2})", X, Y, Z);
		}

		public static explicit operator IntVector3D(IntPoint3D point)
		{
			return new IntVector3D(point.X, point.Y, point.Z);
		}

		public Direction ToDirection()
		{
			IntVector3D v = Normalize();

			Direction dir = 0;

			if (v.X > 0)
				dir |= Direction.East;
			else if (v.X < 0)
				dir |= Direction.West;

			if (v.Y > 0)
				dir |= Direction.North;
			else if (v.Y < 0)
				dir |= Direction.South;

			if (v.Z > 0)
				dir |= Direction.Up;
			else if (v.Z < 0)
				dir |= Direction.Down;

			return dir;
		}

		public static IntVector3D FromDirection(Direction dir)
		{
			int x, y, z;

			x = ((int)dir >> DirectionConsts.XShift) & DirectionConsts.Mask;
			y = ((int)dir >> DirectionConsts.YShift) & DirectionConsts.Mask;
			z = ((int)dir >> DirectionConsts.ZShift) & DirectionConsts.Mask;

			if (x == DirectionConsts.DirNeg)
				x = -1;
			if (y == DirectionConsts.DirNeg)
				y = -1;
			if (z == DirectionConsts.DirNeg)
				z = -1;

			return new IntVector3D(x, y, z);
		}

		public IntVector3D Reverse()
		{
			return new IntVector3D(-m_x, -m_y, -m_z);
		}

		static int FastCos(int rot)
		{
			rot %= 8;
			if (rot < 0)
				rot += 8;
			if (rot == 0 || rot == 1 || rot == 7)
				return 1;
			if (rot == 2 || rot == 6)
				return 0;
			return -1;
		}

		static int FastSin(int rot)
		{
			rot %= 8;
			if (rot < 0)
				rot += 8;
			if (rot == 1 || rot == 2 || rot == 3)
				return 1;
			if (rot == 0 || rot == 4)
				return 0;
			return -1;
		}

		static int FastMul(int a, int b)
		{
			if (a == 0 || b == 0)
				return 0;
			if (a == b)
				return 1;
			return -1;
		}

		/// <summary>
		/// Rotate unit vector in 45 degree steps
		/// </summary>
		/// <param name="rotate">Rotation units, in 45 degree steps</param>
		public IntVector3D FastRotate(int rotate)
		{
			int x = FastMul(FastCos(rotate), this.X) - FastMul(FastSin(rotate), this.Y);
			int y = FastMul(FastSin(rotate), this.X) + FastMul(FastCos(rotate), this.Y);

			var ix = x > 1 ? 1 : (x < -1 ? -1 : x);
			var iy = y > 1 ? 1 : (y < -1 ? -1 : y);

			return new IntVector3D(ix, iy, this.Z);
		}

		public static Direction RotateDir(Direction dir, int rotate)
		{
			int x = ((int)dir >> DirectionConsts.XShift) & DirectionConsts.Mask;
			int y = ((int)dir >> DirectionConsts.YShift) & DirectionConsts.Mask;

			if (x == DirectionConsts.DirNeg)
				x = -1;

			if (y == DirectionConsts.DirNeg)
				y = -1;

			var v = new IntVector3D(x, y, 0);
			v.FastRotate(rotate);
			return v.ToDirection();
		}

		// XXX needs better name
		public bool IsAdjacent
		{
			get
			{
				if (this.X <= 1 && this.X >= -1 && this.Y <= 1 && this.Y >= -1 && this.Z <= 1 && this.Z >= -1)
					return true;
				else
					return false;
			}
		}

		// XXX needs better name
		public bool IsAdjacent2D
		{
			get
			{
				if (this.X <= 1 && this.X >= -1 && this.Y <= 1 && this.Y >= -1 && this.Z == 0)
					return true;
				else
					return false;
			}
		}

		public bool IsNull
		{
			get
			{
				return this.X == 0 && this.Y == 0 && this.Z == 0;
			}
		}
	}
}
