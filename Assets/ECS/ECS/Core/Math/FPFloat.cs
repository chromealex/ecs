namespace ME.ECS {

    #if ECS_COMPILE_IL2CPP_OPTIONS
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOptionAttribute(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
    #endif
    [System.Serializable]
    #if MESSAGE_PACK_SUPPORT
    [MessagePack.MessagePackObjectAttribute()]
    #endif
    public partial struct pfloat : System.IEquatable<pfloat>, System.IComparable<pfloat> {

        #if MESSAGE_PACK_SUPPORT
        [MessagePack.Key(0)]
        #endif
        public readonly long v;

        // Precision of this type is 2^-32, that is 2,3283064365386962890625E-10
        public static readonly decimal Precision = (decimal)new pfloat(1L); //0.00000000023283064365386962890625m;
        public static readonly pfloat MaxValue = new pfloat(pfloat.MAX_VALUE);
        public static readonly pfloat MinValue = new pfloat(pfloat.MIN_VALUE);
        public static readonly pfloat One = new pfloat(pfloat.ONE);
        public static readonly pfloat Zero = new pfloat();
        /// <summary>
        /// The value of Pi
        /// </summary>
        public static readonly pfloat Pi = new pfloat(pfloat.PI);
        public static readonly pfloat PiOver2 = new pfloat(pfloat.PI_OVER_2);
        public static readonly pfloat PiTimes2 = new pfloat(pfloat.PI_TIMES_2);
        public static readonly pfloat PiInv = (pfloat)0.3183098861837906715377675267M;
        public static readonly pfloat PiOver2Inv = (pfloat)0.6366197723675813430755350535M;
        private static readonly pfloat Log2Max = new pfloat(pfloat.LOG2MAX);
        private static readonly pfloat Log2Min = new pfloat(pfloat.LOG2MIN);
        private static readonly pfloat Ln2 = new pfloat(pfloat.LN2);

        private static readonly pfloat LutInterval = (pfloat)(pfloat.LUT_SIZE - 1) / pfloat.PiOver2;
        private const long MAX_VALUE = long.MaxValue;
        private const long MIN_VALUE = long.MinValue;
        private const int NUM_BITS = 64;
        private const int FRACTIONAL_PLACES = 32;
        private const long ONE = 1L << pfloat.FRACTIONAL_PLACES;
        private const long PI_TIMES_2 = 0x6487ED511;
        private const long PI = 0x3243F6A88;
        private const long PI_OVER_2 = 0x1921FB544;
        private const long LN2 = 0xB17217F7;
        private const long LOG2MAX = 0x1F00000000;
        private const long LOG2MIN = -0x2000000000;
        private const int LUT_SIZE = (int)(pfloat.PI_OVER_2 >> 15);

        /// <summary>
        /// Returns a number indicating the sign of a Fix64 number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        public static int Sign(pfloat value) {
            return
                value.v < 0 ? -1 :
                value.v > 0 ? 1 :
                0;
        }


        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// Note: Abs(Fix64.MinValue) == Fix64.MaxValue.
        /// </summary>
        public static pfloat Abs(pfloat value) {
            if (value.v == pfloat.MIN_VALUE) {
                return pfloat.MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.v >> 63;
            return new pfloat((value.v + mask) ^ mask);
        }

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// FastAbs(Fix64.MinValue) is undefined.
        /// </summary>
        public static pfloat FastAbs(pfloat value) {
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.v >> 63;
            return new pfloat((value.v + mask) ^ mask);
        }


        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        public static pfloat Floor(pfloat value) {
            // Just zero out the fractional part
            return new pfloat((long)((ulong)value.v & 0xFFFFFFFF00000000));
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        public static pfloat Ceiling(pfloat value) {
            var hasFractionalPart = (value.v & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? pfloat.Floor(value) + pfloat.One : value;
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        public static pfloat Round(pfloat value) {
            var fractionalPart = value.v & 0x00000000FFFFFFFF;
            var integralPart = pfloat.Floor(value);
            if (fractionalPart < 0x80000000) {
                return integralPart;
            }

            if (fractionalPart > 0x80000000) {
                return integralPart + pfloat.One;
            }

            // if number is halfway between two values, round to the nearest even number
            // this is the method used by System.Math.Round().
            return (integralPart.v & pfloat.ONE) == 0
                       ? integralPart
                       : integralPart + pfloat.One;
        }

        /// <summary>
        /// Adds x and y. Performs saturating addition, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static pfloat operator +(pfloat x, pfloat y) {
            var xl = x.v;
            var yl = y.v;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if ((~(xl ^ yl) & (xl ^ sum) & pfloat.MIN_VALUE) != 0) {
                sum = xl > 0 ? pfloat.MAX_VALUE : pfloat.MIN_VALUE;
            }

            return new pfloat(sum);
        }

        /// <summary>
        /// Adds x and y witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static pfloat FastAdd(pfloat x, pfloat y) {
            return new pfloat(x.v + y.v);
        }

        /// <summary>
        /// Subtracts y from x. Performs saturating substraction, i.e. in case of overflow, 
        /// rounds to MinValue or MaxValue depending on sign of operands.
        /// </summary>
        public static pfloat operator -(pfloat x, pfloat y) {
            var xl = x.v;
            var yl = y.v;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if (((xl ^ yl) & (xl ^ diff) & pfloat.MIN_VALUE) != 0) {
                diff = xl < 0 ? pfloat.MIN_VALUE : pfloat.MAX_VALUE;
            }

            return new pfloat(diff);
        }

        /// <summary>
        /// Subtracts y from x witout performing overflow checking. Should be inlined by the CLR.
        /// </summary>
        public static pfloat FastSub(pfloat x, pfloat y) {
            return new pfloat(x.v - y.v);
        }

        private static long AddOverflowHelper(long x, long y, ref bool overflow) {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & pfloat.MIN_VALUE) != 0;
            return sum;
        }

        public static pfloat operator *(pfloat x, pfloat y) {

            var xl = x.v;
            var yl = y.v;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> pfloat.FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> pfloat.FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> pfloat.FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << pfloat.FRACTIONAL_PLACES;

            var overflow = false;
            var sum = pfloat.AddOverflowHelper((long)loResult, midResult1, ref overflow);
            sum = pfloat.AddOverflowHelper(sum, midResult2, ref overflow);
            sum = pfloat.AddOverflowHelper(sum, hiResult, ref overflow);

            var opSignsEqual = ((xl ^ yl) & pfloat.MIN_VALUE) == 0;

            // if signs of operands are equal and sign of result is negative,
            // then multiplication overflowed positively
            // the reverse is also true
            if (opSignsEqual) {
                if (sum < 0 || overflow && xl > 0) {
                    return pfloat.MaxValue;
                }
            } else {
                if (sum > 0) {
                    return pfloat.MinValue;
                }
            }

            // if the top 32 bits of hihi (unused in the result) are neither all 0s or 1s,
            // then this means the result overflowed.
            var topCarry = hihi >> pfloat.FRACTIONAL_PLACES;
            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/) {
                return opSignsEqual ? pfloat.MaxValue : pfloat.MinValue;
            }

            // If signs differ, both operands' magnitudes are greater than 1,
            // and the result is greater than the negative operand, then there was negative overflow.
            if (!opSignsEqual) {
                long posOp, negOp;
                if (xl > yl) {
                    posOp = xl;
                    negOp = yl;
                } else {
                    posOp = yl;
                    negOp = xl;
                }

                if (sum > negOp && negOp < -pfloat.ONE && posOp > pfloat.ONE) {
                    return pfloat.MinValue;
                }
            }

            return new pfloat(sum);
        }

        /// <summary>
        /// Performs multiplication without checking for overflow.
        /// Useful for performance-critical code where the values are guaranteed not to cause overflow
        /// </summary>
        public static pfloat FastMul(pfloat x, pfloat y) {

            var xl = x.v;
            var yl = y.v;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> pfloat.FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> pfloat.FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> pfloat.FRACTIONAL_PLACES;
            var midResult1 = lohi;
            var midResult2 = hilo;
            var hiResult = hihi << pfloat.FRACTIONAL_PLACES;

            var sum = (long)loResult + midResult1 + midResult2 + hiResult;
            return new pfloat(sum);
        }

        [System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int CountLeadingZeroes(ulong x) {
            var result = 0;
            while ((x & 0xF000000000000000) == 0) {
                result += 4;
                x <<= 4;
            }

            while ((x & 0x8000000000000000) == 0) {
                result += 1;
                x <<= 1;
            }

            return result;
        }

        public static pfloat operator /(pfloat x, pfloat y) {
            var xl = x.v;
            var yl = y.v;

            if (yl == 0) {
                throw new System.DivideByZeroException();
            }

            var remainder = (ulong)(xl >= 0 ? xl : -xl);
            var divider = (ulong)(yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = pfloat.NUM_BITS / 2 + 1;


            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4) {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0) {
                var shift = pfloat.CountLeadingZeroes(remainder);
                if (shift > bitPos) {
                    shift = bitPos;
                }

                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                // Detect overflow
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0) {
                    return ((xl ^ yl) & pfloat.MIN_VALUE) == 0 ? pfloat.MaxValue : pfloat.MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // rounding
            ++quotient;
            var result = (long)(quotient >> 1);
            if (((xl ^ yl) & pfloat.MIN_VALUE) != 0) {
                result = -result;
            }

            return new pfloat(result);
        }

        public static pfloat operator %(pfloat x, pfloat y) {
            return new pfloat(
                (x.v == pfloat.MIN_VALUE) & (y.v == -1) ? 0 : x.v % y.v);
        }

        /// <summary>
        /// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
        /// Use the operator (%) for a more reliable but slower modulo.
        /// </summary>
        public static pfloat FastMod(pfloat x, pfloat y) {
            return new pfloat(x.v % y.v);
        }

        public static pfloat operator -(pfloat x) {
            return x.v == pfloat.MIN_VALUE ? pfloat.MaxValue : new pfloat(-x.v);
        }

        public static bool operator ==(pfloat x, pfloat y) {
            return x.v == y.v;
        }

        public static bool operator !=(pfloat x, pfloat y) {
            return x.v != y.v;
        }

        public static bool operator >(pfloat x, pfloat y) {
            return x.v > y.v;
        }

        public static bool operator <(pfloat x, pfloat y) {
            return x.v < y.v;
        }

        public static bool operator >=(pfloat x, pfloat y) {
            return x.v >= y.v;
        }

        public static bool operator <=(pfloat x, pfloat y) {
            return x.v <= y.v;
        }

        /// <summary>
        /// Returns 2 raised to the specified power.
        /// Provides at least 6 decimals of accuracy.
        /// </summary>
        internal static pfloat Pow2(pfloat x) {
            if (x.v == 0) {
                return pfloat.One;
            }

            // Avoid negative arguments by exploiting that exp(-x) = 1/exp(x).
            var neg = x.v < 0;
            if (neg) {
                x = -x;
            }

            if (x == pfloat.One) {
                return neg ? pfloat.One / (pfloat)2 : (pfloat)2;
            }

            if (x >= pfloat.Log2Max) {
                return neg ? pfloat.One / pfloat.MaxValue : pfloat.MaxValue;
            }

            if (x <= pfloat.Log2Min) {
                return neg ? pfloat.MaxValue : pfloat.Zero;
            }

            /* The algorithm is based on the power series for exp(x):
             * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
             * 
             * From term n, we get term n+1 by multiplying with x/n.
             * When the sum term drops to zero, we can stop summing.
             */

            var integerPart = (int)pfloat.Floor(x);
            // Take fractional part of exponent
            x = new pfloat(x.v & 0x00000000FFFFFFFF);

            var result = pfloat.One;
            var term = pfloat.One;
            var i = 1;
            while (term.v != 0) {
                term = pfloat.FastMul(pfloat.FastMul(x, term), pfloat.Ln2) / (pfloat)i;
                result += term;
                i++;
            }

            result = pfloat.FromRaw(result.v << integerPart);
            if (neg) {
                result = pfloat.One / result;
            }

            return result;
        }

        /// <summary>
        /// Returns the base-2 logarithm of a specified number.
        /// Provides at least 9 decimals of accuracy.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        internal static pfloat Log2(pfloat x) {
            if (x.v <= 0) {
                throw new System.ArgumentOutOfRangeException("Non-positive value passed to Ln", "x");
            }

            // This implementation is based on Clay. S. Turner's fast binary logarithm
            // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
            //     Processing Mag., pp. 124,140, Sep. 2010.)

            long b = 1U << (pfloat.FRACTIONAL_PLACES - 1);
            long y = 0;

            var rawX = x.v;
            while (rawX < pfloat.ONE) {
                rawX <<= 1;
                y -= pfloat.ONE;
            }

            while (rawX >= pfloat.ONE << 1) {
                rawX >>= 1;
                y += pfloat.ONE;
            }

            var z = new pfloat(rawX);

            for (var i = 0; i < pfloat.FRACTIONAL_PLACES; i++) {
                z = pfloat.FastMul(z, z);
                if (z.v >= pfloat.ONE << 1) {
                    z = new pfloat(z.v >> 1);
                    y += b;
                }

                b >>= 1;
            }

            return new pfloat(y);
        }

        /// <summary>
        /// Returns the natural logarithm of a specified number.
        /// Provides at least 7 decimals of accuracy.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static pfloat Ln(pfloat x) {
            return pfloat.FastMul(pfloat.Log2(x), pfloat.Ln2);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// Provides about 5 digits of accuracy for the result.
        /// </summary>
        /// <exception cref="System.DivideByZeroException">
        /// The base was zero, with a negative exponent
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The base was negative, with a non-zero exponent
        /// </exception>
        public static pfloat Pow(pfloat b, pfloat exp) {
            if (b == pfloat.One) {
                return pfloat.One;
            }

            if (exp.v == 0) {
                return pfloat.One;
            }

            if (b.v == 0) {
                if (exp.v < 0) {
                    throw new System.DivideByZeroException();
                }

                return pfloat.Zero;
            }

            var log2 = pfloat.Log2(b);
            return pfloat.Pow2(exp * log2);
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The argument was negative.
        /// </exception>
        public static pfloat Sqrt(pfloat x) {
            var xl = x.v;
            if (xl < 0) {
                // We cannot represent infinities like Single and Double, and Sqrt is
                // mathematically undefined for x < 0. So we just throw an exception.
                throw new System.ArgumentOutOfRangeException("Negative value passed to Sqrt", "x");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << (pfloat.NUM_BITS - 2);

            while (bit > num) {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i) {
                // First we get the top 48 bits of the answer.
                while (bit != 0) {
                    if (num >= result + bit) {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    } else {
                        result = result >> 1;
                    }

                    bit >>= 2;
                }

                if (i == 0) {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (pfloat.NUM_BITS / 2)) - 1) {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (pfloat.NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (pfloat.NUM_BITS / 2)) + 0x80000000UL;
                    } else {
                        num <<= pfloat.NUM_BITS / 2;
                        result <<= pfloat.NUM_BITS / 2;
                    }

                    bit = 1UL << (pfloat.NUM_BITS / 2 - 2);
                }
            }

            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result) {
                ++result;
            }

            return new pfloat((long)result);
        }

        /// <summary>
        /// Returns the Sine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static pfloat Sin(pfloat x) {
            var clampedL = pfloat.ClampSinValue(x.v, out var flipHorizontal, out var flipVertical);
            var clamped = new pfloat(clampedL);

            // Find the two closest values in the LUT and perform linear interpolation
            // This is what kills the performance of this function on x86 - x64 is fine though
            var rawIndex = pfloat.FastMul(clamped, pfloat.LutInterval);
            var roundedIndex = pfloat.Round(rawIndex);
            var indexError = pfloat.FastSub(rawIndex, roundedIndex);

            var nearestValue = new pfloat(FPMathTables.SinLut[flipHorizontal ? FPMathTables.SinLut.Length - 1 - (int)roundedIndex : (int)roundedIndex]);
            var secondNearestValue =
                new pfloat(FPMathTables.SinLut[
                               flipHorizontal ? FPMathTables.SinLut.Length - 1 - (int)roundedIndex - pfloat.Sign(indexError) : (int)roundedIndex + pfloat.Sign(indexError)]);

            var delta = pfloat.FastMul(indexError, pfloat.FastAbs(pfloat.FastSub(nearestValue, secondNearestValue))).v;
            var interpolatedValue = nearestValue.v + (flipHorizontal ? -delta : delta);
            var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;
            return new pfloat(finalValue);
        }

        /// <summary>
        /// Returns a rough approximation of the Sine of x.
        /// This is at least 3 times faster than Sin() on x86 and slightly faster than Math.Sin(),
        /// however its accuracy is limited to 4-5 decimals, for small enough values of x.
        /// </summary>
        public static pfloat FastSin(pfloat x) {
            var clampedL = pfloat.ClampSinValue(x.v, out var flipHorizontal, out var flipVertical);

            // Here we use the fact that the SinLut table has a number of entries
            // equal to (PI_OVER_2 >> 15) to use the angle to index directly into it
            var rawIndex = (uint)(clampedL >> 15);
            if (rawIndex >= pfloat.LUT_SIZE) {
                rawIndex = pfloat.LUT_SIZE - 1;
            }

            var nearestValue = FPMathTables.SinLut[flipHorizontal ? FPMathTables.SinLut.Length - 1 - (int)rawIndex : (int)rawIndex];
            return new pfloat(flipVertical ? -nearestValue : nearestValue);
        }


        private static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical) {
            var largePI = 7244019458077122842;
            // Obtained from ((Fix64)1686629713.065252369824872831112M).m_rawValue
            // This is (2^29)*PI, where 29 is the largest N such that (2^N)*PI < MaxValue.
            // The idea is that this number contains way more precision than PI_TIMES_2,
            // and (((x % (2^29*PI)) % (2^28*PI)) % ... (2^1*PI) = x % (2 * PI)
            // In practice this gives us an error of about 1,25e-9 in the worst case scenario (Sin(MaxValue))
            // Whereas simply doing x % PI_TIMES_2 is the 2e-3 range.

            var clamped2Pi = angle;
            for (var i = 0; i < 29; ++i) {
                clamped2Pi %= largePI >> i;
            }

            if (angle < 0) {
                clamped2Pi += pfloat.PI_TIMES_2;
            }

            // The LUT contains values for 0 - PiOver2; every other value must be obtained by
            // vertical or horizontal mirroring
            flipVertical = clamped2Pi >= pfloat.PI;
            // obtain (angle % PI) from (angle % 2PI) - much faster than doing another modulo
            var clampedPi = clamped2Pi;
            while (clampedPi >= pfloat.PI) {
                clampedPi -= pfloat.PI;
            }

            flipHorizontal = clampedPi >= pfloat.PI_OVER_2;
            // obtain (angle % PI_OVER_2) from (angle % PI) - much faster than doing another modulo
            var clampedPiOver2 = clampedPi;
            if (clampedPiOver2 >= pfloat.PI_OVER_2) {
                clampedPiOver2 -= pfloat.PI_OVER_2;
            }

            return clampedPiOver2;
        }

        /// <summary>
        /// Returns the cosine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static pfloat Cos(pfloat x) {
            var xl = x.v;
            var rawAngle = xl + (xl > 0 ? -pfloat.PI - pfloat.PI_OVER_2 : pfloat.PI_OVER_2);
            return pfloat.Sin(new pfloat(rawAngle));
        }

        /// <summary>
        /// Returns a rough approximation of the cosine of x.
        /// See FastSin for more details.
        /// </summary>
        public static pfloat FastCos(pfloat x) {
            var xl = x.v;
            var rawAngle = xl + (xl > 0 ? -pfloat.PI - pfloat.PI_OVER_2 : pfloat.PI_OVER_2);
            return pfloat.FastSin(new pfloat(rawAngle));
        }

        /// <summary>
        /// Returns the tangent of x.
        /// </summary>
        /// <remarks>
        /// This function is not well-tested. It may be wildly inaccurate.
        /// </remarks>
        public static pfloat Tan(pfloat x) {
            var clampedPi = x.v % pfloat.PI;
            var flip = false;
            if (clampedPi < 0) {
                clampedPi = -clampedPi;
                flip = true;
            }

            if (clampedPi > pfloat.PI_OVER_2) {
                flip = !flip;
                clampedPi = pfloat.PI_OVER_2 - (clampedPi - pfloat.PI_OVER_2);
            }

            var clamped = new pfloat(clampedPi);

            // Find the two closest values in the LUT and perform linear interpolation
            var rawIndex = pfloat.FastMul(clamped, pfloat.LutInterval);
            var roundedIndex = pfloat.Round(rawIndex);
            var indexError = pfloat.FastSub(rawIndex, roundedIndex);

            var nearestValue = new pfloat(FPMathTables.TanLut[(int)roundedIndex]);
            var secondNearestValue = new pfloat(FPMathTables.TanLut[(int)roundedIndex + pfloat.Sign(indexError)]);

            var delta = pfloat.FastMul(indexError, pfloat.FastAbs(pfloat.FastSub(nearestValue, secondNearestValue))).v;
            var interpolatedValue = nearestValue.v + delta;
            var finalValue = flip ? -interpolatedValue : interpolatedValue;
            return new pfloat(finalValue);
        }

        /// <summary>
        /// Returns the arccos of of the specified number, calculated using Atan and Sqrt
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static pfloat Acos(pfloat x) {
            if (x < -pfloat.One || x > pfloat.One) {
                throw new System.ArgumentOutOfRangeException(nameof(x));
            }

            if (x.v == 0) {
                return pfloat.PiOver2;
            }

            var result = pfloat.Atan(pfloat.Sqrt(pfloat.One - x * x) / x);
            return x.v < 0 ? result + pfloat.Pi : result;
        }

        /// <summary>
        /// Returns the arctan of of the specified number, calculated using Euler series
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static pfloat Atan(pfloat z) {
            if (z.v == 0) {
                return pfloat.Zero;
            }

            // Force positive values for argument
            // Atan(-z) = -Atan(z).
            var neg = z.v < 0;
            if (neg) {
                z = -z;
            }

            pfloat result;
            var two = (pfloat)2;
            var three = (pfloat)3;

            var invert = z > pfloat.One;
            if (invert) {
                z = pfloat.One / z;
            }

            result = pfloat.One;
            var term = pfloat.One;

            var zSq = z * z;
            var zSq2 = zSq * two;
            var zSqPlusOne = zSq + pfloat.One;
            var zSq12 = zSqPlusOne * two;
            var dividend = zSq2;
            var divisor = zSqPlusOne * three;

            for (var i = 2; i < 30; ++i) {
                term *= dividend / divisor;
                result += term;

                dividend += zSq2;
                divisor += zSq12;

                if (term.v == 0) {
                    break;
                }
            }

            result = result * z / zSqPlusOne;

            if (invert) {
                result = pfloat.PiOver2 - result;
            }

            if (neg) {
                result = -result;
            }

            return result;
        }

        public static pfloat Atan2(pfloat y, pfloat x) {
            var yl = y.v;
            var xl = x.v;
            if (xl == 0) {
                if (yl > 0) {
                    return pfloat.PiOver2;
                }

                if (yl == 0) {
                    return pfloat.Zero;
                }

                return -pfloat.PiOver2;
            }

            pfloat atan;
            var z = y / x;

            // Deal with overflow
            if (pfloat.One + (pfloat)0.28M * z * z == pfloat.MaxValue) {
                return y < pfloat.Zero ? -pfloat.PiOver2 : pfloat.PiOver2;
            }

            if (pfloat.Abs(z) < pfloat.One) {
                atan = z / (pfloat.One + (pfloat)0.28M * z * z);
                if (xl < 0) {
                    if (yl < 0) {
                        return atan - pfloat.Pi;
                    }

                    return atan + pfloat.Pi;
                }
            } else {
                atan = pfloat.PiOver2 - z / (z * z + (pfloat)0.28M);
                if (yl < 0) {
                    return atan - pfloat.Pi;
                }
            }

            return atan;
        }


        public static implicit operator pfloat(long value) {
            return new pfloat(value * pfloat.ONE);
        }

        public static implicit operator long(pfloat value) {
            return value.v >> pfloat.FRACTIONAL_PLACES;
        }

        public static implicit operator pfloat(float value) {
            return new pfloat((long)(value * pfloat.ONE));
        }

        public static implicit operator float(pfloat value) {
            return (float)value.v / pfloat.ONE;
        }

        public static implicit operator pfloat(double value) {
            return new pfloat((long)(value * pfloat.ONE));
        }

        public static implicit operator double(pfloat value) {
            return (double)value.v / pfloat.ONE;
        }

        public static implicit operator pfloat(decimal value) {
            return new pfloat((long)(value * pfloat.ONE));
        }

        public static implicit operator decimal(pfloat value) {
            return (decimal)value.v / pfloat.ONE;
        }

        public override bool Equals(object obj) {
            return obj is pfloat && ((pfloat)obj).v == this.v;
        }

        public override int GetHashCode() {
            return this.v.GetHashCode();
        }

        public bool Equals(pfloat other) {
            return this.v == other.v;
        }

        public int CompareTo(pfloat other) {
            return this.v.CompareTo(other.v);
        }

        public override string ToString() {
            // Up to 10 decimal places
            return ((decimal)this).ToString("0.##########");
            //return this.v.ToString();
        }

        public static pfloat FromRaw(long rawValue) {
            return new pfloat(rawValue);
        }

        internal static void GenerateSinLut() {
            using (var writer = new System.IO.StreamWriter("Fix64SinLut.cs")) {
                writer.Write(
                    @"namespace FixMath.NET 
{
    partial struct Fix64 
    {
        public static readonly long[] SinLut = new[] 
        {");
                var lineCounter = 0;
                for (var i = 0; i < pfloat.LUT_SIZE; ++i) {
                    var angle = i * System.Math.PI * 0.5 / (pfloat.LUT_SIZE - 1);
                    if (lineCounter++ % 8 == 0) {
                        writer.WriteLine();
                        writer.Write("            ");
                    }

                    var sin = System.Math.Sin(angle);
                    var rawValue = ((pfloat)sin).v;
                    writer.Write(string.Format("0x{0:X}L, ", rawValue));
                }

                writer.Write(
                    @"
        };
    }
}");
            }
        }

        internal static void GenerateTanLut() {
            using (var writer = new System.IO.StreamWriter("Fix64TanLut.cs")) {
                writer.Write(
                    @"namespace FixMath.NET 
{
    partial struct Fix64 
    {
        public static readonly long[] TanLut = new[] 
        {");
                var lineCounter = 0;
                for (var i = 0; i < pfloat.LUT_SIZE; ++i) {
                    var angle = i * System.Math.PI * 0.5 / (pfloat.LUT_SIZE - 1);
                    if (lineCounter++ % 8 == 0) {
                        writer.WriteLine();
                        writer.Write("            ");
                    }

                    var tan = System.Math.Tan(angle);
                    if (tan > (double)pfloat.MaxValue || tan < 0.0) {
                        tan = (double)pfloat.MaxValue;
                    }

                    var rawValue = ((decimal)tan > (decimal)pfloat.MaxValue || tan < 0.0 ? pfloat.MaxValue : (pfloat)tan).v;
                    writer.Write(string.Format("0x{0:X}L, ", rawValue));
                }

                writer.Write(
                    @"
        };
    }
}");
            }
        }

        // turn into a Console Application and use this to generate the look-up tables
        //static void Main(string[] args)
        //{
        //    GenerateSinLut();
        //    GenerateTanLut();
        //}

        /// <summary>
        /// The underlying integer representation
        /// </summary>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.IgnoreMemberAttribute]
        #endif
        public long RawValue {
            get {
                return this.v;
            }
        }

        /// <summary>
        /// This is the constructor from raw value; it can only be used interally.
        /// </summary>
        /// <param name="rawValue"></param>
        #if MESSAGE_PACK_SUPPORT
        [MessagePack.SerializationConstructorAttribute]
        #endif
        public pfloat(long rawValue) {
            this.v = rawValue;
        }

        public pfloat(int value) {
            this.v = value * pfloat.ONE;
        }

        public pfloat(pfloat value) {
            this.v = value.v;
        }

        public pfloat(float value) {
            this.v = ((pfloat)value).v;
        }

    }

}