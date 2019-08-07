using System;
using System.Text;

namespace Features_Tasks
{
    /// <summary>
    /// Decimal number allowing a maximum value of 4,294,967,296 with up to 4,294,967,296 decimals.
    /// </summary>
    public class SuperDecimal
    {
        private readonly UInt32[] _superDecimal;
        private readonly int _decimalCount = 0;
        private readonly int _arraySize = 0;

        /// <summary>
        /// Gets the array of elements.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
        public UInt32[] Array
        {
            get { return _superDecimal; }
        }

        /// <summary>
        /// Gets the decimal count.
        /// </summary>
        /// <value>
        /// The decimal count.
        /// </value>
        public int DecimalCount
        {
            get { return _decimalCount; }
        }

        /// <summary>
        /// Gets the size of the array.
        /// </summary>
        /// <value>
        /// The size of the array.
        /// </value>
        public int ArraySize
        {
            get { return _arraySize; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperDecimal"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="decimalCount">The decimal count.</param>
        public SuperDecimal(UInt32 initialValue, int decimalCount)
        {
            _decimalCount = decimalCount;
            _arraySize = (int)Math.Ceiling((float)decimalCount * 0.104) + 2; // Array size based on 2e32 base
            _superDecimal = new UInt32[_arraySize];

            _superDecimal[0] = initialValue;
        }

        /// <summary>
        /// Assigns the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="Exception">SuperDecimal numbers provided cannot operate together.</exception>
        public void AssignValue(SuperDecimal value)
        {
            if (!IsValidForOperators(value))
                throw new Exception("SuperDecimal numbers provided cannot operate together.");

            for (int i = 0; i < _arraySize; i++)
            {
                _superDecimal[i] = value.Array[i];
            }
        }

        /// <summary>
        /// Determines whether a SuperDecimal can apply operations against another.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is valid for operators] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidForOperators(SuperDecimal value)
        {
            // Cannot operate with itself
            if (Object.ReferenceEquals(this, value))
                return false;

            // Have to be the same size
            if (value.DecimalCount != _decimalCount)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether this instance is zero.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is zero; otherwise, <c>false</c>.
        /// </returns>
        public bool IsZero()
        {
            foreach (UInt32 item in _superDecimal)
            {
                if (item != 0)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            SuperDecimal temp = new SuperDecimal(0, _decimalCount);
            temp.AssignValue(this);

            StringBuilder sb = new StringBuilder();
            sb.Append(temp.Array[0]);
            sb.Append(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);

            int digitCount = 0;
            while (digitCount < _decimalCount)
            {
                temp.Array[0] = 0;
                temp = temp * 100000;
                sb.AppendFormat("{0:D5}", temp.Array[0]);
                digitCount += 5;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Compute the arc tangent result.
        /// </summary>
        /// <param name="multiplicand">The multiplicand.</param>
        /// <param name="reciprocal">The reciprocal.</param>
        public SuperDecimal ArcTan(UInt32 multiplicand, UInt32 reciprocal)
        {
            SuperDecimal result = new SuperDecimal(0, _decimalCount);

            SuperDecimal X = new SuperDecimal(multiplicand, _decimalCount);
            X = X / reciprocal;
            reciprocal *= reciprocal;

            result.AssignValue(X);

            SuperDecimal term = new SuperDecimal(0, _decimalCount);
            UInt32 divisor = 1;
            bool subtractTerm = true;
            while (true)
            {
                X = X / reciprocal;
                term.AssignValue(X);

                divisor += 2;

                term = term / divisor;
                if (term.IsZero())
                    break;

                if (subtractTerm)
                    result = result - term;
                else
                    result = result + term;

                subtractTerm = !subtractTerm;
            }

            return result;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="valueA">The value a.</param>
        /// <param name="valueB">The value b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <exception cref="Exception">SuperDecimal numbers provided cannot operate together.</exception>
        public static SuperDecimal operator + (SuperDecimal valueA, SuperDecimal valueB)
        {
            if (!valueA.IsValidForOperators(valueB))
                throw new Exception("SuperDecimal numbers provided cannot operate together.");

            int index = valueA.ArraySize - 1;
            while (index >= 0 && valueB.Array[index] == 0)
                index--;

            UInt32 carry = 0;
            while (index >= 0)
            {
                UInt64 result = (UInt64)valueA.Array[index] + valueB.Array[index] + carry;
                valueA.Array[index] = (UInt32)result;
                if (result >= 0x100000000U)
                    carry = 1;
                else
                    carry = 0;
                index--;
            }

            return valueA;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="valueA">The value a.</param>
        /// <param name="valueB">The value b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <exception cref="Exception">SuperDecimal numbers provided cannot operate together.</exception>
        public static SuperDecimal operator -(SuperDecimal valueA, SuperDecimal valueB)
        {
            if (!valueA.IsValidForOperators(valueB))
                throw new Exception("SuperDecimal numbers provided cannot operate together.");

            int index = valueA.ArraySize - 1;
            while (index >= 0 && valueB.Array[index] == 0)
                index--;

            UInt32 borrow = 0;
            while (index >= 0)
            {
                UInt64 result = 0x100000000U + (UInt64)valueA.Array[index] - valueB.Array[index] - borrow;
                valueA.Array[index] = (UInt32)result;
                if (result >= 0x100000000U)
                    borrow = 0;
                else
                    borrow = 1;
                index--;
            }

            return valueA;
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="valueA">The value a.</param>
        /// <param name="valueB">The value b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static SuperDecimal operator *(SuperDecimal valueA, UInt32 valueB)
        {
            int index = valueA.ArraySize - 1;
            while (index >= 0 && valueA.Array[index] == 0)
                index--;

            UInt32 carry = 0;
            while (index >= 0)
            {
                UInt64 result = (UInt64)valueA.Array[index] * valueB + carry;
                valueA.Array[index] = (UInt32)result;
                carry = (UInt32)(result >> 32);

                index--;
            }

            return valueA;
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="valueA">The value a.</param>
        /// <param name="valueB">The value b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static SuperDecimal operator /(SuperDecimal valueA, UInt32 valueB)
        {
            int index = 0;
            while (index < valueA.ArraySize && valueA.Array[index] == 0)
                index++;

            UInt32 carry = 0;
            while (index < valueA.ArraySize)
            {
                UInt64 result = valueA.Array[index] + ((UInt64)carry << 32);
                valueA.Array[index] = (UInt32)(result / (UInt64)valueB);
                carry = (UInt32)(result % (UInt64)valueB);

                index++;
            }

            return valueA;
        }
    }
}
