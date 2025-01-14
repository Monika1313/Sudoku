namespace System.Numerics;

partial class BitOperationsExtensions
{
	/// <summary>
	/// Find all offsets of set bits of the binary representation of a specified value.
	/// </summary>
	/// <param name="this">The value.</param>
	/// <returns>All offsets.</returns>
	public static partial Bits GetAllSets(this sbyte @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((uint)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(sbyte) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this byte @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((uint)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(byte) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this short @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((uint)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(short) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this ushort @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((uint)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(ushort) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this int @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((uint)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(int) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this uint @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount(@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(uint) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this long @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((ulong)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(long) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static partial Bits GetAllSets(this ulong @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount(@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(ulong) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static unsafe partial Bits GetAllSets(this nint @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount((nuint)@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(nint) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}

	/// <inheritdoc cref="GetAllSets(sbyte)"/>
	public static unsafe partial Bits GetAllSets(this nuint @this)
	{
		if (@this == 0)
		{
			return Bits.Empty;
		}

		var length = PopCount(@this);
		var result = new int[length];
		for (byte i = 0, p = 0; i < sizeof(nuint) << 3; i++, @this >>= 1)
		{
			if ((@this & 1) != 0)
			{
				result[p++] = i;
			}
		}

		return result;
	}
}
