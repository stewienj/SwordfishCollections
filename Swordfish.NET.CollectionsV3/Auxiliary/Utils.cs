using System;
using System.Diagnostics;

namespace Swordfish.NET.Collections.Auxiliary
{
    internal static class Utils
    {

        //--------------------------------------------------------------------------------------------------------

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException"/> if the
        ///     provided string is null.
        ///     Throws an <see cref="ArgumentOutOfRangeException"/> if the
        ///     provided string is empty.
        /// </summary>
        /// <param name="stringParameter">The object to test for null and empty.</param>
        /// <param name="parameterName">The string for the ArgumentException parameter, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireNotNullOrEmpty(string stringParameter, string parameterName)
        {
            if (stringParameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            else if (stringParameter.Length == 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Throws an <see cref="ArgumentNullException"/> if the
        ///     provided object is null.
        /// </summary>
        /// <param name="obj">The object to test for null.</param>
        /// <param name="parameterName">The string for the ArgumentNullException parameter, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireNotNull(object obj, string parameterName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Throws an <see cref="ArgumentException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="parameterName">The string for <see cref="ArgumentException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgument(bool truth, string parameterName)
        {
            RequireNotNullOrEmpty(parameterName, "parameterName");

            if (!truth)
            {
                throw new ArgumentException(parameterName);
            }
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Throws an <see cref="ArgumentException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="paramName">The paramName for the <see cref="ArgumentException"/>, if thrown.</param>
        /// <param name="message">The message for <see cref="ArgumentException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgument(bool truth, string paramName, string message)
        {
            RequireNotNullOrEmpty(paramName, "paramName");
            RequireNotNullOrEmpty(message, "message");

            if (!truth)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="parameterName">The string for <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgumentRange(bool truth, string parameterName)
        {
            RequireNotNullOrEmpty(parameterName, "parameterName");

            if (!truth)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        //--------------------------------------------------------------------------------------------

        /// <summary>
        ///     Throws an <see cref="ArgumentOutOfRangeException"/> if the provided truth is false.
        /// </summary>
        /// <param name="truth">The value assumed to be true.</param>
        /// <param name="paramName">The paramName for the <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
        /// <param name="message">The message for <see cref="ArgumentOutOfRangeException"/>, if thrown.</param>
        [DebuggerStepThrough]
        public static void RequireArgumentRange(bool truth, string paramName, string message)
        {
            RequireNotNullOrEmpty(paramName, "paramName");
            RequireNotNullOrEmpty(message, "message");

            if (!truth)
            {
                throw new ArgumentOutOfRangeException(message, paramName);
            }
        }

    }
}
