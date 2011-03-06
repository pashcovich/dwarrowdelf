﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Dwarrowdelf
{
	public class IntPointConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is IntPoint) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var rect = (IntPoint)value;
			return rect.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return IntPoint.Parse(source);
		}
	}

	public class IntPoint3DConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is IntPoint3D) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var rect = (IntPoint3D)value;
			return rect.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return IntPoint3D.Parse(source);
		}
	}

	public class IntRectConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is IntRect) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var rect = (IntRect)value;
			return rect.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return IntRect.Parse(source);
		}
	}

	public class IntRect3DConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is IntRect3D) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var rect = (IntRect3D)value;
			return rect.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return IntRect3D.Parse(source);
		}
	}

	public class IntCuboidConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is IntCuboid) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var rect = (IntCuboid)value;
			return rect.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return IntCuboid.Parse(source);
		}
	}

	public class IntSize3DConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is IntSize3D) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var rect = (IntSize3D)value;
			return rect.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return IntSize3D.Parse(source);
		}
	}

	public class ObjectIDConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is ObjectID) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var oid = (ObjectID)value;
			return oid.RawValue.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return new ObjectID(Convert.ToInt32(source, System.Globalization.NumberFormatInfo.InvariantInfo));
		}
	}

	public class TileDataConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == null) || !(value is TileData) || destinationType != typeof(string))
				return base.ConvertTo(context, culture, value, destinationType);

			var td = (TileData)value;
			return td.ConvertToString();
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value == null)
				throw base.GetConvertFromException(value);

			string source = value as string;

			if (source == null)
				return base.ConvertFrom(context, culture, value);

			return TileData.Parse(source);
		}
	}
}
