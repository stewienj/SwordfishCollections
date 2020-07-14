// Code taken from https://github.com/AdamWhiteHat/BigRational on 14th July 2020
// Using this code for performance comparisions in the test code
// MIT License
// Copyright(c) 2019 Adam White
//
// Modifications:

using System;
using System.Linq;
using System.Numerics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Swordfish.NET.Collections.Auxiliary
{
	public class Fraction : IComparable, IComparable<Fraction>, IEquatable<Fraction>, IEqualityComparer<Fraction>
	{
		#region Constructors

		public Fraction()
			: this(BigInteger.Zero, BigInteger.One)
		{
		}

		public Fraction(Fraction fraction)
			: this(fraction.Numerator, fraction.Denominator)
		{
		}

		public Fraction(int value)
			: this((BigInteger)value, BigInteger.One)
		{
		}

		public Fraction(BigInteger value)
			: this(value, BigInteger.One)
		{
		}

		public Fraction(BigInteger numerator, BigInteger denominator)
		{
			Numerator = numerator;
			Denominator = denominator;
		}

		public Fraction(float value)
		{
			Initialize(value, 7);
		}

		public Fraction(double value)
		{
			Initialize(value, 13);
		}

		private void Initialize(double value, int precision)
		{
			if (!CheckForWholeValues(value))
			{
				int sign = Math.Sign(value);
				int exponent = value.ToString(CultureInfo.InvariantCulture)
										.TrimEnd('0')
										.SkipWhile(c => c != '.').Skip(1)
										.Count();

				double oneOver = Math.Round(1 / Math.Abs(value), precision);

				bool isWholeNumber = false;
				BigInteger denom;

				if (precision == 7)
				{
					float floatVal = (float)oneOver;
					isWholeNumber = (floatVal % 1 == 0);
					denom = (BigInteger)floatVal;
				}
				else
				{
					isWholeNumber = (oneOver % 1 == 0);
					denom = (BigInteger)oneOver;
				}

				if (isWholeNumber)
				{
					Numerator = sign;
					Denominator = denom;
					return;
				}

				if (exponent > 0)
				{
					double pow = value * Math.Pow(10, exponent);
					Fraction notReduced = new Fraction((BigInteger)pow, BigInteger.Pow(10, (int)exponent));
					Fraction reduced = Simplify(notReduced);
					Numerator = reduced.Numerator;
					Denominator = reduced.Denominator;
				}
				else
				{
					Numerator = new BigInteger(value);
					Denominator = BigInteger.One;
				}
			}
		}

		public Fraction(decimal value)
		{
			int[] bits = decimal.GetBits(value);
			if (bits == null || bits.Length != 4 || (bits[3] & ~(DecimalSignMask | DecimalScaleMask)) != 0 || (bits[3] & DecimalScaleMask) > (28 << 16))
			{
				throw new ArgumentException("invalid decimal", "value");
			}

			if (!CheckForWholeValues((double)value))
			{
				// build up the numerator
				ulong ul = (((ulong)(uint)bits[2]) << 32) | ((ulong)(uint)bits[1]);  // (hi    << 32) | (mid)
				BigInteger numerator = (new BigInteger(ul) << 32) | (uint)bits[0];   // (hiMid << 32) | (low)

				bool isNegative = (bits[3] & DecimalSignMask) != 0;
				if (isNegative)
				{
					numerator = BigInteger.Negate(numerator);
				}

				// build up the denominator
				int scale = (bits[3] & DecimalScaleMask) >> 16;     // 0-28, power of 10 to divide numerator by
				BigInteger denominator = BigInteger.Pow(10, scale);

				Fraction notReduced = new Fraction(numerator, denominator);
				Fraction reduced = Simplify(notReduced);
				Numerator = reduced.Numerator;
				Denominator = reduced.Denominator;
			}
		}

		private bool CheckForWholeValues(double value)
		{
			if (double.IsNaN(value))
			{
				throw new ArgumentException("Value is not a number", nameof(value));
			}
			if (double.IsInfinity(value))
			{
				throw new ArgumentException("Cannot represent infinity", nameof(value));
			}

			if (value == 0)
			{
				Numerator = BigInteger.Zero;
				Denominator = BigInteger.One;
				return true;
			}
			else if (value == 1)
			{
				Numerator = BigInteger.One;
				Denominator = BigInteger.One;
				return true;
			}
			else if (value == -1)
			{
				Numerator = BigInteger.MinusOne;
				Denominator = BigInteger.One;
				return true;
			}
			else if (value % 1 == 0)
			{
				Numerator = (BigInteger)value;
				Denominator = BigInteger.One;
				return true;
			}
			return false;
		}

		#endregion

		#region Properties

		public BigInteger Numerator { get; private set; }
		public BigInteger Denominator { get; private set; }

		public int Sign { get { return Fraction.NormalizeSign(this).Numerator.Sign; } }
		public bool IsZero { get { return (this == Fraction.Zero); } }
		public bool IsOne { get { return (this == Fraction.One); } }

		#region Static Properties

		public static Fraction Zero { get { return _zero; } }
		public static Fraction One { get { return _one; } }
		public static Fraction MinusOne { get { return _minusOne; } }
		public static Fraction OneHalf { get { return _oneHalf; } }

		private static readonly Fraction _zero = new Fraction(BigInteger.Zero, BigInteger.One);
		private static readonly Fraction _one = new Fraction(BigInteger.One, BigInteger.One);
		private static readonly Fraction _minusOne = new Fraction(BigInteger.MinusOne, BigInteger.One);
		private static readonly Fraction _oneHalf = new Fraction(new BigInteger(1), new BigInteger(2));

		#endregion

		#endregion

		#region Arithmetic Methods

		public static Fraction Add(Fraction augend, Fraction addend)
		{
			// a/b + c/d  == (ad + bc)/bd
			return new Fraction(
					BigInteger.Add(
						BigInteger.Multiply(augend.Numerator, addend.Denominator),
						BigInteger.Multiply(augend.Denominator, addend.Numerator)
					),
					BigInteger.Multiply(augend.Denominator, addend.Denominator)
				);
		}

		public static Fraction Subtract(Fraction minuend, Fraction subtrahend)
		{
			// a/b - c/d  == (ad - bc)/bd
			return new Fraction(
					BigInteger.Subtract(
						BigInteger.Multiply(minuend.Numerator, subtrahend.Denominator),
						BigInteger.Multiply(minuend.Denominator, subtrahend.Numerator)
					),
					BigInteger.Multiply(minuend.Denominator, subtrahend.Denominator)
				);
		}

		public static Fraction Multiply(Fraction multiplicand, Fraction multiplier)
		{
			Fraction frac1 =
			   new Fraction(
				   BigInteger.Multiply(multiplicand.Numerator, multiplier.Numerator),
				   BigInteger.Multiply(multiplicand.Denominator, multiplier.Denominator)
			   );

			Fraction frac2 = Simplify(frac1);

			if (frac1 != frac2)
			{
				throw new ArithmeticException("Multiply methods needs to simplify result. Please add this behavior to this method.");
			}


			return frac1;
		}

		public static Fraction Divide(Fraction dividend, Fraction divisor)
		{
			return Simplify(Multiply(dividend, Reciprocal(divisor)));
		}

		public static Fraction Remainder(BigInteger dividend, BigInteger divisor)
		{
			BigInteger remainder = dividend % divisor;
			return new Fraction(remainder, divisor);
		}

		public static Fraction Remainder(Fraction dividend, Fraction divisor)
		{
			return new Fraction(
				BigInteger.Multiply(dividend.Numerator, divisor.Denominator) % BigInteger.Multiply(dividend.Denominator, divisor.Numerator),
				BigInteger.Multiply(dividend.Denominator, divisor.Denominator)
			);
		}

		public static Fraction DivRem(Fraction dividend, Fraction divisor, out Fraction remainder)
		{
			// a/b / c/d  == (ad)/(bc) ; a/b % c/d  == (ad % bc)/bd
			BigInteger ad = dividend.Numerator * divisor.Denominator;
			BigInteger bc = dividend.Denominator * divisor.Numerator;
			BigInteger bd = dividend.Denominator * divisor.Denominator;
			remainder = new Fraction((ad % bc), bd);
			return new Fraction(ad, bc);
		}

		public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out Fraction remainder)
		{
			BigInteger remaind = new BigInteger(-1);
			BigInteger quotient = BigInteger.DivRem(dividend, divisor, out remaind);

			remainder = new Fraction(remaind, divisor);
			return quotient;
		}

		public static Fraction Pow(Fraction value, BigInteger exponent)
		{
			if (exponent.Sign == 0)
			{
				return Fraction.One;
			}

			Fraction inputValue;
			BigInteger inputExponent;

			if (exponent.Sign < 0)
			{
				if (value == Fraction.Zero)
				{
					throw new ArgumentException("Cannot raise zero to a negative power", nameof(value));
				}
				// n^(-e) -> (1/n)^e
				inputValue = Reciprocal(value);
				inputExponent = BigInteger.Negate(exponent);
			}
			else
			{
				inputValue = new Fraction(value);
				inputExponent = exponent;
			}

			Fraction result = inputValue;
			while (inputExponent > BigInteger.One)
			{
				result = Multiply(result, inputValue);
				inputExponent--;
			}

			return result;
		}

		public static double Log(Fraction fraction)
		{
			double a = BigInteger.Log(fraction.Numerator);
			double b = BigInteger.Log(fraction.Denominator);
			return (a - b);
		}

		public static Fraction Reciprocal(Fraction fraction)
		{
			Fraction result = new Fraction(fraction.Denominator, fraction.Numerator);
			Fraction simplified = Fraction.Simplify(result);
			return simplified;
		}

		public static Fraction Abs(Fraction fraction)
		{
			return (fraction.Numerator.Sign < 0 ? new Fraction(BigInteger.Abs(fraction.Numerator), fraction.Denominator) : fraction);
		}

		public static Fraction Negate(Fraction fraction)
		{
			return new Fraction(BigInteger.Negate(fraction.Numerator), fraction.Denominator);
		}

		#region GCD & LCM

		public static Fraction LeastCommonDenominator(Fraction left, Fraction right)
		{
			return new Fraction((left.Denominator * right.Denominator), BigInteger.GreatestCommonDivisor(left.Denominator, right.Denominator));
		}

		public static Fraction GreatestCommonDivisor(Fraction left, Fraction right)
		{
			Fraction leftFrac = Fraction.Simplify(left);
			Fraction rightFrac = Fraction.Simplify(right);

			BigInteger gcd = BigInteger.GreatestCommonDivisor(left.Numerator, right.Numerator);
			BigInteger lcm = LCM(left.Denominator, right.Denominator);

			return new Fraction(gcd, lcm);
		}

		private static BigInteger LCM(BigInteger value1, BigInteger value2)
		{
			BigInteger absValue1 = BigInteger.Abs(value1);
			BigInteger absValue2 = BigInteger.Abs(value2);
			return (absValue1 * absValue2) / BigInteger.GreatestCommonDivisor(absValue1, absValue2);
		}

		#endregion

		#endregion

		#region Arithmetic Operators

		public static Fraction operator +(Fraction left, Fraction right) => Add(left, right);
		public static Fraction operator -(Fraction left, Fraction right) => Subtract(left, right);
		public static Fraction operator *(Fraction left, Fraction right) => Multiply(left, right);
		public static Fraction operator /(Fraction left, Fraction right) => Divide(left, right);
		public static Fraction operator %(Fraction left, Fraction right) => Remainder(left, right);
		// Unitary operators
		public static Fraction operator +(Fraction fraction) => Abs(fraction);
		public static Fraction operator -(Fraction fraction) => Negate(fraction);
		public static Fraction operator ++(Fraction fraction) => Add(fraction, Fraction.One);
		public static Fraction operator --(Fraction fraction) => Subtract(fraction, Fraction.One);

		#endregion

		#region Comparison Operators

		public static bool operator ==(Fraction left, Fraction right) => Compare(left, right) == 0;
		public static bool operator !=(Fraction left, Fraction right) => Compare(left, right) != 0;
		public static bool operator <(Fraction left, Fraction right) => Compare(left, right) < 0;
		public static bool operator <=(Fraction left, Fraction right) => Compare(left, right) <= 0;
		public static bool operator >(Fraction left, Fraction right) => Compare(left, right) > 0;
		public static bool operator >=(Fraction left, Fraction right) => Compare(left, right) >= 0;

		#endregion

		#region Compare

		public static int Compare(Fraction left, Fraction right)
		{
			var l = BigInteger.Multiply(left.Numerator, right.Denominator);
			var r = BigInteger.Multiply(right.Numerator, left.Denominator);
			return BigInteger.Compare(l, r);
		}

		// IComparable
		int IComparable.CompareTo(Object obj)
		{
			if (obj == null) { return 1; }
			if (!(obj is Fraction)) { throw new ArgumentException($"Argument must be of type {nameof(Fraction)}", nameof(obj)); }
			return Compare(this, (Fraction)obj);
		}

		// IComparable<Fraction>
		public int CompareTo(Fraction other)
		{
			return Compare(this, other);
		}

		#endregion

		#region Conversion Operators

		public static explicit operator BigRational(Fraction value)
		{
			return new BigRational(BigInteger.Zero, value);
		}

		public static explicit operator Fraction(float value)
		{
			return new Fraction(value);
		}

		public static explicit operator Fraction(double value)
		{
			return new Fraction(value);
		}

		public static explicit operator Fraction(decimal value)
		{
			return new Fraction(value);
		}

		public static explicit operator double(Fraction value)
		{
			if (IsInRangeDouble(value.Numerator) && IsInRangeDouble(value.Denominator))
			{
				return (double)value.Numerator / (double)value.Denominator;
			}

			BigInteger scaledup = BigInteger.Multiply(value.Numerator, _doublePrecision) / value.Denominator;
			if (scaledup.IsZero)
			{
				return 0d; // underflow. throw exception here instead?
			}

			bool isDone = false;
			double result = 0;
			int scale = _doubleMaxScale;
			while (scale > 0)
			{
				if (!isDone)
				{
					if (IsInRangeDouble(scaledup))
					{
						result = (double)scaledup;
						isDone = true;
					}
					else
					{
						scaledup = scaledup / 10;
					}
				}

				result = result / 10;
				scale--;
			}

			if (isDone)
			{
				return result;
			}
			else
			{
				return (value.Sign < 0) ? double.NegativeInfinity : double.PositiveInfinity;
			}
		}

		public static explicit operator decimal(Fraction value)
		{
			// The decimal value type represents decimal numbers ranging
			// from +79,228,162,514,264,337,593,543,950,335 to -79,228,162,514,264,337,593,543,950,335
			// the binary representation of a decimal value is of the form, ((-2^96 to 2^96) / 10^(0 to 28))
			if (IsInRangeDecimal(value.Numerator) && IsInRangeDecimal(value.Denominator))
			{
				return (decimal)value.Numerator / (decimal)value.Denominator;
			}

			// scale the numerator to preserve the fraction part through the integer division
			BigInteger denormalized = (value.Numerator * _decimalPrecision) / value.Denominator;
			if (denormalized.IsZero)
			{
				return decimal.Zero; // underflow - fraction is too small to fit in a decimal
			}
			for (int scale = DecimalMaxScale; scale >= 0; scale--)
			{
				if (!IsInRangeDecimal(denormalized))
				{
					denormalized = denormalized / 10;
				}
				else
				{
					DecimalUInt32 dec = new DecimalUInt32();
					dec.dec = (decimal)denormalized;
					dec.flags = (dec.flags & ~DecimalScaleMask) | (scale << 16);
					return dec.dec;
				}
			}
			throw new OverflowException("Value was either too large or too small for a decimal.");
		}

		#region Private Members

		private static bool IsInRangeDouble(BigInteger number)
		{
			return ((BigInteger)double.MinValue < number && number < (BigInteger)double.MaxValue);
		}
		private static readonly int _doubleMaxScale = 308;
		private static readonly BigInteger _doublePrecision = BigInteger.Pow(10, _doubleMaxScale);
		private static readonly BigInteger _decimalPrecision = BigInteger.Pow(10, DecimalMaxScale);
		private static readonly BigInteger _decimalMaxValue = (BigInteger)decimal.MaxValue;
		private static readonly BigInteger _decimalMinValue = (BigInteger)decimal.MinValue;
		private const int DecimalScaleMask = 0x00FF0000;
		private const int DecimalSignMask = unchecked((int)0x80000000);
		private const int DecimalMaxScale = 28;

		private static bool IsInRangeDecimal(BigInteger number)
		{
			return (_decimalMinValue <= number && number <= _decimalMaxValue);
		}

		[StructLayout(LayoutKind.Explicit)]
		internal struct DecimalUInt32
		{
			[FieldOffset(0)]
			public decimal dec;
			[FieldOffset(0)]
			public int flags;
		}

		#endregion

		#endregion

		#region Equality Methods

		public override bool Equals(Object obj)
		{
			if (obj == null) { return false; }
			if (!(obj is Fraction)) { return false; }
			return this.Equals(this, (Fraction)obj);
		}

		public bool Equals(Fraction other)
		{
			return this.Equals(this, other);
		}

		public bool Equals(Fraction left, Fraction right)
		{
			if (left.Denominator == right.Denominator) { return left.Numerator == right.Numerator; }
			else { return (left.Numerator * right.Denominator) == (left.Denominator * right.Numerator); }
		}

		public override int GetHashCode()
		{
			return this.GetHashCode(this);
		}

		public int GetHashCode(Fraction fraction)
		{
			return BigRational.CombineHashCodes(fraction.Numerator.GetHashCode(), fraction.Denominator.GetHashCode());
		}

		#endregion

		#region Transform Methods

		public static BigRational ReduceToProperFraction(Fraction value)
		{
			Fraction input = Fraction.Simplify(value);

			if (input.Numerator.IsZero)
			{
				return new BigRational(BigInteger.Zero, input);
			}
			else if (input.Denominator.IsOne)
			{
				return new BigRational(input.Numerator, Fraction.Zero);
			}
			else
			{
				BigRational result;
				if (BigInteger.Abs(input.Numerator) > BigInteger.Abs(input.Denominator))
				{
					int sign = input.Numerator.Sign;

					BigInteger remainder = new BigInteger(-1);
					BigInteger wholeUnits = BigInteger.DivRem(BigInteger.Abs(input.Numerator), input.Denominator, out remainder);
					if (sign == -1)
					{
						wholeUnits = BigInteger.Negate(wholeUnits);
					}
					result = new BigRational(wholeUnits, new Fraction(remainder, input.Denominator));
					return result;
				}
				else
				{
					result = new BigRational(BigInteger.Zero, input.Numerator, input.Denominator);
					return result;
				}
			}
		}

		public static Fraction Simplify(Fraction value)
		{
			Fraction input = NormalizeSign(value);

			if (input.Numerator.IsZero || input.Numerator.IsOne || input.Numerator == BigInteger.MinusOne)
			{
				return new Fraction(input);
			}

			BigInteger num = input.Numerator;
			BigInteger denom = input.Denominator;
			BigInteger gcd = BigInteger.GreatestCommonDivisor(num, denom);
			if (gcd > BigInteger.One)
			{
				return new Fraction(num / gcd, denom / gcd);
			}

			return new Fraction(input);
		}

		internal static Fraction NormalizeSign(Fraction value)
		{

			BigInteger numer = value.Numerator;
			BigInteger denom = value.Denominator;

			if (numer.Sign == 1 && denom.Sign == 1)
			{
				return value;
			}
			else if (numer.Sign == -1 && denom.Sign == 1)
			{
				return value;
			}
			else if (numer.Sign == 1 && denom.Sign == -1)
			{
				numer = BigInteger.Negate(numer);
				denom = BigInteger.Negate(denom);
			}
			else if (numer.Sign == -1 && denom.Sign == -1)
			{
				numer = BigInteger.Negate(numer);
				denom = BigInteger.Negate(denom);
			}

			return new Fraction(numer, denom);
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return this.ToString(CultureInfo.CurrentCulture);
		}

		public String ToString(String format)
		{
			return this.ToString(format, CultureInfo.CurrentCulture);
		}

		public String ToString(IFormatProvider provider)
		{
			return this.ToString("R", provider);
		}

		public String ToString(String format, IFormatProvider provider)
		{
			NumberFormatInfo numberFormatProvider = (NumberFormatInfo)provider.GetFormat(typeof(NumberFormatInfo));
			if (numberFormatProvider == null)
			{
				numberFormatProvider = CultureInfo.CurrentCulture.NumberFormat;
			}

			string zeroString = numberFormatProvider.NativeDigits[0];
			char zeroChar = zeroString.First();

			if (Numerator.IsZero)
			{
				return zeroString;
			}
			else if (Denominator.IsOne)
			{
				return String.Format(provider, "{0}", Numerator.ToString(format, provider));
			}
			else
			{
				return String.Format(provider, "{0}/{1}", Numerator.ToString(format, provider), Denominator.ToString(format, provider));
			}
		}

		#endregion
	}
}

