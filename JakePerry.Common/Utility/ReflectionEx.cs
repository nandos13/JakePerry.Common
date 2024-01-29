using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry
{
    public static class ReflectionEx
    {
        private static readonly Stack<object[]>[] _rentalArrays;

        static ReflectionEx()
        {
            _rentalArrays = new Stack<object[]>[4];
            for (int i = 0; i < 4; ++i)
            {
                _rentalArrays[i] = new Stack<object[]>(capacity: 8);
            }
        }

        /// <summary>
        /// Rent an array with the given length in range [1..4].
        /// </summary>
        /// <remarks>
        /// Intended for use as an arguments array in a reflection call.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static object[] RentArray(int length)
        {
            if (length < 1 || length > 4) throw new ArgumentOutOfRangeException(nameof(length));

            var stack = _rentalArrays[length - 1];
            var array = stack.Count > 0 ? stack.Pop() : new object[length];

            return array;
        }

        /// <summary>
        /// Rent an array containing the given argument(s).
        /// </summary>
        /// <remarks>
        /// Intended for use as an arguments array in a reflection call.
        /// </remarks>
        public static object[] RentArrayWithArguments(object arg0)
        {
            var array = RentArray(1);
            array[0] = arg0;

            return array;
        }

        /// <inheritdoc cref="RentArrayWithArguments(object)"/>
        public static object[] RentArrayWithArguments(object arg0, object arg1)
        {
            var array = RentArray(2);
            array[0] = arg0;
            array[1] = arg1;

            return array;
        }

        /// <inheritdoc cref="RentArrayWithArguments(object)"/>
        public static object[] RentArrayWithArguments(object arg0, object arg1, object arg2)
        {
            var array = RentArray(3);
            array[0] = arg0;
            array[1] = arg1;
            array[2] = arg2;

            return array;
        }

        /// <inheritdoc cref="RentArrayWithArguments(object)"/>
        public static object[] RentArrayWithArguments(object arg0, object arg1, object arg2, object arg3)
        {
            var array = RentArray(4);
            array[0] = arg0;
            array[1] = arg1;
            array[2] = arg2;
            array[3] = arg3;

            return array;
        }

        /// <summary>
        /// Clear and return a rented array to the pool.
        /// </summary>
        public static void ReturnArray(object[] array)
        {
            if (array is null) return;

            int length = array.Length;
            if (length < 1 || length > 4) return;

            Array.Clear(array, 0, length);
            _rentalArrays[length - 1].Push(array);
        }
    }
}
