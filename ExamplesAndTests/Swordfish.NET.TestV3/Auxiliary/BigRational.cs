// Code taken from https://github.com/AdamWhiteHat/BigRational on 10th July 2020
// Using this code for performance comparisions in the test code
// MIT License
// Copyright(c) 2019 Adam White

using System;
using System.Linq;
using System.Numerics;
using System.Globalization;
using System.Collections.Generic;

namespace ExtendedNumerics
{
	public class BigRational : IComparable, IComparable<BigRational>, IEquatable<BigRational>
	{
		#region Constructors

		public BigRational(int value)
			: this((BigInteger)value, Fraction.Zero)
		{
		}

		public BigRational(BigInteger value)
			: this(value, Fraction.Zero)
		{
		}

		public BigRational(Fraction fraction)
			: this(BigInteger.Zero, fraction)
		{
		}

		public BigRational(BigInteger whole, Fraction fraction)
			: this(whole, fraction.Numerator, fraction.Denominator)
		{
		}

		public BigRational(BigInteger whole, BigInteger numerator, BigInteger denominator)
		{
			WholePart = whole;
			FractionalPart = new Fraction(numerator, denominator);
		}

		public BigRational(float value)
		{
			if (!CheckForWholeValues(value))
			{
				WholePart = (BigInteger)Math.Truncate(value);
				float fract = Math.Abs(value) % 1;
				FractionalPart = (fract == 0) ? Fraction.Zero : new Fraction(fract);
			}
		}

		public BigRational(double value)
		{
			if (!CheckForWholeValues(value))
			{
				WholePart = (BigInteger)Math.Truncate(value);
				double fract = Math.Abs(value) % 1;
				FractionalPart = (fract == 0) ? Fraction.Zero : new Fraction(fract);
			}
		}

		public BigRational(decimal value)
		{
			if (!CheckForWholeValues((double)value))
			{
				WholePart = (BigInteger)Math.Truncate(value);
				decimal fract = Math.Abs(value) % 1;
				FractionalPart = (fract == 0) ? Fraction.Zero : new Fraction(fract);
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
				WholePart = BigInteger.Zero;
				FractionalPart = Fraction.Zero;
				return true;
			}
			else if (value == 1)
			{
				WholePart = BigInteger.Zero;
				FractionalPart = Fraction.One;
				return true;
			}
			else if (value == -1)
			{
				WholePart = BigInteger.Zero;
				FractionalPart = Fraction.MinusOne;
				return true;
			}
			return false;
		}


		#endregion

		#region Properties

		public BigInteger WholePart { get; private set; }
		public Fraction FractionalPart { get; private set; }

		public int Sign { get { return NormalizeSign(this).WholePart.Sign; } }
		public bool IsZero { get { return (WholePart.IsZero && FractionalPart.IsZero); } }

		#region Static Properties

		public static BigRational One { get { return _one; } }
		public static BigRational Zero { get { return _zero; } }
		public static BigRational MinusOne { get { return _minusOne; } }

		private static BigRational _one { get { return new BigRational(BigInteger.One); } }
		private static BigRational _zero { get { return new BigRational(BigInteger.Zero); } }
		private static BigRational _minusOne { get { return new BigRational(BigInteger.MinusOne); } }

		#endregion

		#endregion

		#region Arithmetic Methods

		public static BigRational Add(BigRational augend, BigRational addend)
		{
			Fraction fracAugend = augend.GetImproperFraction();
			Fraction fracAddend = addend.GetImproperFraction();

			BigRational result = Add(fracAugend, fracAddend);
			BigRational reduced = BigRational.Reduce(result);
			return reduced;
		}

		public static BigRational Subtract(BigRational minuend, BigRational subtrahend)
		{
			Fraction fracMinuend = minuend.GetImproperFraction();
			Fraction fracSubtrahend = subtrahend.GetImproperFraction();

			BigRational result = Subtract(fracMinuend, fracSubtrahend);
			BigRational reduced = BigRational.Reduce(result);
			return reduced;
		}

		public static BigRational Multiply(BigRational multiplicand, BigRational multiplier)
		{
			Fraction fracMultiplicand = multiplicand.GetImproperFraction();
			Fraction fracMultiplier = multiplier.GetImproperFraction();

			BigRational result = Fraction.ReduceToProperFraction(Fraction.Multiply(fracMultiplicand, fracMultiplier));
			BigRational reduced = BigRational.Reduce(result);
			return reduced;
		}

		public static BigRational Divide(BigInteger dividend, BigInteger divisor)
		{
			BigInteger remainder = new BigInteger(-1);
			BigInteger quotient = BigInteger.DivRem(dividend, divisor, out remainder);

			BigRational result = new BigRational(
					quotient,
					new Fraction(remainder, divisor)
				);

			return result;
		}

		public static BigRational Divide(BigRational dividend, BigRational divisor)
		{
			// a/b / c/d  == (ad)/(bc)			
			Fraction l = dividend.GetImproperFraction();
			Fraction r = divisor.GetImproperFraction();

			BigInteger ad = BigInteger.Multiply(l.Numerator, r.Denominator);
			BigInteger bc = BigInteger.Multiply(l.Denominator, r.Numerator);

			Fraction newFraction = new Fraction(ad, bc);
			BigRational result = Fraction.ReduceToProperFraction(newFraction);
			return result;
		}

		public static BigRational Remainder(BigInteger dividend, BigInteger divisor)
		{
			BigInteger remainder = (dividend % divisor);
			return new BigRational(BigInteger.Zero, new Fraction(remainder, divisor));
		}

		public static BigRational Mod(BigRational number, BigRational mod)
		{
			Fraction num = number.GetImproperFraction();
			Fraction modulus = mod.GetImproperFraction();

			return new BigRational(Fraction.Remainder(num, modulus));
		}

		public static BigRational Pow(BigRational baseValue, BigInteger exponent)
		{
			Fraction fractPow = Fraction.Pow(baseValue.GetImproperFraction(), exponent);
			return new BigRational(fractPow);
		}

		public static double Log(BigRational rational)
		{
			return Fraction.Log(rational.GetImproperFraction());
		}

		public static BigRational Abs(BigRational rational)
		{
			BigRational input = BigRational.Reduce(rational);
			return new BigRational(BigInteger.Abs(input.WholePart), input.FractionalPart);
		}

		public static BigRational Negate(BigRational rational)
		{
			BigRational input = BigRational.Reduce(rational);
			return new BigRational(BigInteger.Negate(input.WholePart), input.FractionalPart);
		}

		public static BigRational Add(Fraction augend, Fraction addend)
		{
			return new BigRational(BigInteger.Zero, Fraction.Add(augend, addend));
		}

		public static BigRational Subtract(Fraction minuend, Fraction subtrahend)
		{
			return new BigRational(BigInteger.Zero, Fraction.Subtract(minuend, subtrahend));
		}

		public static BigRational Multiply(Fraction multiplicand, Fraction multiplier)
		{
			return new BigRational(BigInteger.Zero, Fraction.Multiply(multiplicand, multiplier));
		}

		public static BigRational Divide(Fraction dividend, Fraction divisor)
		{
			return new BigRational(BigInteger.Zero, Fraction.Divide(dividend, divisor));
		}

		#region GCD & LCM

		public static BigRational LeastCommonDenominator(BigRational left, BigRational right)
		{
			Fraction leftFrac = left.GetImproperFraction();
			Fraction rightFrac = right.GetImproperFraction();

			return BigRational.Reduce(new BigRational(Fraction.LeastCommonDenominator(leftFrac, rightFrac)));
		}

		public static BigRational GreatestCommonDivisor(BigRational left, BigRational right)
		{
			Fraction leftFrac = left.GetImproperFraction();
			Fraction rightFrac = right.GetImproperFraction();

			return BigRational.Reduce(new BigRational(Fraction.GreatestCommonDivisor(leftFrac, rightFrac)));
		}

		#endregion

		#endregion

		#region Arithmetic Operators

		public static BigRational operator +(BigRational augend, BigRational addend) => Add(augend, addend);
		public static BigRational operator -(BigRational minuend, BigRational subtrahend) => Subtract(minuend, subtrahend);
		public static BigRational operator *(BigRational multiplicand, BigRational multiplier) => Multiply(multiplicand, multiplier);
		public static BigRational operator /(BigRational dividend, BigRational divisor) => Divide(dividend, divisor);
		// Unitary operators
		public static BigRational operator +(BigRational rational) => Abs(rational);
		public static BigRational operator -(BigRational rational) => Negate(rational);
		public static BigRational operator ++(BigRational rational) => Add(rational, BigRational.One);
		public static BigRational operator --(BigRational rational) => Subtract(rational, BigRational.One);

		#endregion

		#region Comparison Operators

		public static bool operator ==(BigRational left, BigRational right) { return Compare(left, right) == 0; }
		public static bool operator !=(BigRational left, BigRational right) { return Compare(left, right) != 0; }
		public static bool operator <(BigRational left, BigRational right) { return Compare(left, right) < 0; }
		public static bool operator <=(BigRational left, BigRational right) { return Compare(left, right) <= 0; }
		public static bool operator >(BigRational left, BigRational right) { return Compare(left, right) > 0; }
		public static bool operator >=(BigRational left, BigRational right) { return Compare(left, right) >= 0; }

		#endregion

		#region Compare

		public static int Compare(BigRational left, BigRational right)
		{
			BigRational leftRed = BigRational.Reduce(left);
			BigRational rightRed = BigRational.Reduce(right);

			if (leftRed.WholePart == rightRed.WholePart)
			{
				return Fraction.Compare(leftRed.FractionalPart, rightRed.FractionalPart);
			}
			else
			{
				return BigInteger.Compare(leftRed.WholePart, rightRed.WholePart);
			}
		}

		// IComparable
		int IComparable.CompareTo(Object obj)
		{
			if (obj == null) { return 1; }
			if (!(obj is BigRational)) { throw new ArgumentException($"Argument must be of type {nameof(BigRational)}", nameof(obj)); }
			return Compare(this, (BigRational)obj);
		}

		// IComparable<Fraction>
		public int CompareTo(BigRational other)
		{
			return Compare(this, other);
		}

		#endregion

		#region Conversion

		public static explicit operator BigRational(byte value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(SByte value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(Int16 value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(UInt16 value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(Int32 value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(UInt32 value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(Int64 value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(UInt64 value)
		{
			return new BigRational((BigInteger)value);
		}

		public static explicit operator BigRational(BigInteger value)
		{
			return new BigRational(value);
		}

		public static explicit operator BigRational(float value)
		{
			return new BigRational(value);
		}

		public static explicit operator BigRational(double value)
		{
			return new BigRational(value);
		}

		public static explicit operator BigRational(decimal value)
		{
			return new BigRational(value);
		}

		public static explicit operator double(BigRational value)
		{
			double fract = (double)value.FractionalPart;
			double whole = (double)value.WholePart;
			double result = whole + (fract);
			return result;
		}

		public static explicit operator decimal(BigRational value)
		{
			decimal fract = (decimal)value.FractionalPart;
			decimal whole = (decimal)value.WholePart;
			decimal result = whole + (fract);
			return result;
		}

		public static explicit operator Fraction(BigRational value)
		{
			return Fraction.Simplify(new Fraction(
					BigInteger.Add(value.FractionalPart.Numerator, BigInteger.Multiply(value.WholePart, value.FractionalPart.Denominator)),
					value.FractionalPart.Denominator
				));
		}

		public static BigRational Parse(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException("Argument cannot be null, empty or whitespace.");
			}

			string[] parts = value.Trim().Split('/');
			if (parts.Length == 1)
			{
				BigInteger whole;
				if (!BigInteger.TryParse(parts[0], out whole))
				{
					throw new ArgumentException("Invalid string given for number.");
				}
				return new BigRational(whole);
			}
			else if (parts.Length == 2)
			{
				BigInteger whole = BigInteger.Zero, numerator, denominator;

				string[] firstParts = parts[0].Trim().Split(new char[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (firstParts.Length == 1)
				{
					if (!BigInteger.TryParse(parts[0].Trim(), out numerator))
					{
						throw new ArgumentException("Invalid string given for numerator.");
					}
				}
				else if (firstParts.Length == 2)
				{
					if (!BigInteger.TryParse(firstParts[0].Trim(), out whole))
					{
						throw new ArgumentException("Invalid string given for whole number.");
					}
					if (!BigInteger.TryParse(firstParts[1].Trim(), out numerator))
					{
						throw new ArgumentException("Invalid string given for numerator.");
					}
				}
				else
				{
					throw new ArgumentException("Invalid fraction given as string to parse.");
				}

				if (!BigInteger.TryParse(parts[1].Trim(), out denominator))
				{
					throw new ArgumentException("Invalid string given for denominator.");
				}
				return new BigRational(whole, numerator, denominator);
			}
			else
			{
				throw new ArgumentException("Invalid fraction given as string to parse.");
			}
		}

		#endregion

		#region Equality Methods

		public bool Equals(BigRational other)
		{
			BigRational reducedThis = BigRational.Reduce(this);
			BigRational reducedOther = BigRational.Reduce(other);

			bool result = true;

			result &= reducedThis.WholePart.Equals(reducedOther.WholePart);
			result &= reducedThis.FractionalPart.Numerator.Equals(reducedOther.FractionalPart.Numerator);
			result &= reducedThis.FractionalPart.Denominator.Equals(reducedOther.FractionalPart.Denominator);

			return result;
		}

		public override bool Equals(Object obj)
		{
			if (obj == null) { return false; }
			if (!(obj is BigRational)) { return false; }
			return this.Equals((BigRational)obj);
		}

		public override int GetHashCode()
		{
			return CombineHashCodes(WholePart.GetHashCode(), FractionalPart.GetHashCode());
		}

		internal static int CombineHashCodes(int h1, int h2)
		{
			return (((h1 << 5) + h1) ^ h2);
		}

		#endregion

		#region Transform Methods

		public Fraction GetImproperFraction()
		{
			BigRational input = NormalizeSign(this);

			if (input.WholePart == 0 && input.FractionalPart.Sign == 0)
			{
				return Fraction.Zero;
			}

			if (input.FractionalPart.Sign != 0 || input.FractionalPart.Denominator > 1)
			{
				if (input.WholePart.Sign != 0)
				{
					BigInteger whole = BigInteger.Multiply(input.WholePart, input.FractionalPart.Denominator);

					BigInteger remainder = input.FractionalPart.Numerator;

					if (input.WholePart.Sign == -1)
					{
						remainder = BigInteger.Negate(remainder);
					}

					BigInteger total = BigInteger.Add(whole, remainder);
					Fraction newFractional = new Fraction(total, input.FractionalPart.Denominator);
					return newFractional;
				}
				else
				{
					return input.FractionalPart;
				}
			}
			else
			{
				return new Fraction(input.WholePart, BigInteger.One);
			}
		}

		public static BigRational Reduce(BigRational value)
		{
			BigRational input = NormalizeSign(value);
			BigRational reduced = Fraction.ReduceToProperFraction(input.FractionalPart);
			BigRational result = new BigRational(value.WholePart + reduced.WholePart, reduced.FractionalPart);
			return result;
		}

		private static BigRational NormalizeSign(BigRational value)
		{
			BigInteger whole;
			Fraction fract = Fraction.NormalizeSign(value.FractionalPart);

			if (value.WholePart > 0 && value.WholePart.Sign == 1 && fract.Sign == -1)
			{
				whole = BigInteger.Negate(value.WholePart);
			}
			else
			{
				whole = value.WholePart;
			}

			return new BigRational(whole, fract);
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return this.ToString(CultureInfo.CurrentCulture);
		}

		public String ToString(String format)
		{
			return this.ToString(CultureInfo.CurrentCulture);
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

			BigRational input = BigRational.Reduce(this);

			string first = input.WholePart != 0 ? String.Format(provider, "{0}", input.WholePart.ToString(format, provider)) : string.Empty;
			string second = input.FractionalPart.Numerator != 0 ? String.Format(provider, "{0}", input.FractionalPart.ToString(format, provider)) : string.Empty;
			string join = string.Empty;

			if (!string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(second))
			{
				if (input.WholePart.Sign < 0)
				{
					join = numberFormatProvider.NegativeSign;
				}
				else
				{
					join = numberFormatProvider.PositiveSign;
				}
			}

			if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(join) && string.IsNullOrWhiteSpace(second))
			{
				return zeroString;
			}

			return string.Concat(first, join, second);
		}

		#endregion
	}
}

