#nullable enable
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;
// ReSharper disable CompareNonConstrainedGenericWithNull

namespace Floydcom.ForEachAsync
{
    /// <summary>
    /// Common runtime checks that throw <see cref="ArgumentException"/> upon failure.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class Requires
    {

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL), NoEnumeration]
            [System.Diagnostics.CodeAnalysis.NotNull]
            T value, [InvokerParameterName] string parameterName, string? message)
            where T : class? // ensures value-types aren't passed to a null checking method
        {
            if (value == null)
            {
                FailArgumentNullException(parameterName, message);
            }

        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotNull<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL), NoEnumeration]
            [System.Diagnostics.CodeAnalysis.NotNull]
            T value, [InvokerParameterName] string parameterName)
            where T : class? // ensures value-types aren't passed to a null checking method
        {
            if (value == null)
            {
                FailArgumentNullException(parameterName);
            }

        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null, empty or whitespace.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <param name="message">The message to include in any thrown exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>String.Empty</c> or all whitespace</exception>
        /// <returns>The original <paramref name="value"/></returns>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotNullOrEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            [System.Diagnostics.CodeAnalysis.NotNull]
            string? value, [InvokerParameterName] string parameterName, string? message = null)
        {
            NotNull(value, parameterName, message);
            Argument(!string.IsNullOrWhiteSpace(value), parameterName, message ?? $"{parameterName} is Required");
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null, empty or whitespace.
        /// </summary>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <param name="message">The message to include in any thrown exception.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>String.Empty</c> or all whitespace</exception>
        /// <returns>The original <paramref name="value"/></returns>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)]
            [System.Diagnostics.CodeAnalysis.NotNull]
            string? value, [InvokerParameterName] string parameterName, string message)
        {
            Argument(!string.IsNullOrWhiteSpace(value), parameterName, message);
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.  It passes through the specified value back as a return value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <returns>The value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        [DebuggerStepThrough, AssertionMethod, NotNull, Pure]
        [return: NotNullIfNotNull("value")]
        public static T NotNullPassthrough<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL), NoEnumeration]
            [System.Diagnostics.CodeAnalysis.NotNull]
            T value, [InvokerParameterName] string parameterName)
            where T : class? // ensures value-types aren't passed to a null checking method
        {
            NotNull(value, parameterName);
            return value;
        }

        /// <summary>
        /// Throws an exception if the specified parameter's value is null.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="value">The value of the argument.</param>
        /// <param name="parameterName">The name of the parameter to include in any thrown exception.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c></exception>
        /// <remarks>
        /// This method exists for callers who themselves only know the type as a generic parameter which
        /// may or may not be a class, but certainly cannot be null.
        /// </remarks>
        [DebuggerStepThrough, AssertionMethod]
        public static void NotNullAllowStructs<T>([AssertionCondition(AssertionConditionType.IS_NOT_NULL), NoEnumeration]
            [System.Diagnostics.CodeAnalysis.NotNull]
            T value, [InvokerParameterName] string parameterName)
        {
            if (null == value)
            {
                FailArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter that was null.</param>
        /// <param name="message">The message.</param>
        [DebuggerStepThrough, ContractAnnotation("=> halt"), DoesNotReturn]
        private static void FailArgumentNullException(string parameterName, string? message = null)
        {
            // Separating out this throwing operation helps with inlining of the caller
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(parameterName);

            throw new ArgumentNullException(parameterName, message);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException" /> if a condition does not evaluate to true.
        /// </summary>
        /// <param name="condition">if <c>false</c> the exception will be thrown.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="condition"/> is <c>false</c></exception>
        [DebuggerStepThrough, AssertionMethod]
        public static void Range(
            [AssertionCondition(AssertionConditionType.IS_TRUE),
             DoesNotReturnIf(false)]
            bool condition, [InvokerParameterName] string parameterName, string? message = null)
        {
            if (!condition)
            {
                FailRange(parameterName, message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        [DebuggerStepThrough, ContractAnnotation("=> halt"), DoesNotReturn]
        private static void FailRange(string parameterName, string? message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if a condition does not evaluate to true.
        /// </summary>
        /// <param name="condition">if <c>false</c> the exception will be thrown.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentException"><paramref name="condition"/> is <c>false</c></exception>
        [DebuggerStepThrough, AssertionMethod]
        public static void Argument(
            [AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool condition, [InvokerParameterName] string parameterName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if a condition does not evaluate to true.
        /// </summary>
        /// <param name="condition">if <c>false</c> the exception will be thrown.</param>
        /// <exception cref="ArgumentException"><paramref name="condition"/> is <c>false</c></exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [DebuggerStepThrough, AssertionMethod]
        public static void Argument(
            [AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)]
            bool condition)
        {
            if (!condition)
            {
                throw new ArgumentException();
            }
        }

    }
}
