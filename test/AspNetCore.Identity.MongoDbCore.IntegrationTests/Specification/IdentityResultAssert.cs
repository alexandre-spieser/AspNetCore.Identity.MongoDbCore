// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Test
{
    /// <summary>
    /// Helper for tests to validate identity results.
    /// </summary>
    public static class IdentityResultAssert
    {
        /// <summary>
        /// Asserts that the result has Succeeded.
        /// </summary>
        /// <param name="result"></param>
        public static void IsSuccess(IdentityResult result)
        {
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
        }

        /// <summary>
        /// Asserts that the result has not Succeeded.
        /// </summary>
        public static void IsFailure(IdentityResult result)
        {
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
        }

        /// <summary>
        /// Asserts that the result has not Succeeded and that error is the first Error's Description.
        /// </summary>
        public static void IsFailure(IdentityResult result, string error)
        {
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal(error, result.Errors.First().Description);
        }

        /// <summary>
        /// Asserts that the result has not Succeeded and that first error matches error's code and Description.
        /// </summary>
        public static void IsFailure(IdentityResult result, IdentityError error)
        {
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal(error.Description, result.Errors.First().Description);
            Assert.Equal(error.Code, result.Errors.First().Code);
        }

        /// <summary>
        /// Asserts that the logger contains the expectedLog.
        /// </summary>
        /// <param name="logger">The logger to inspect.</param>
        /// <param name="expectedLog">The expected log message.</param>
        public static void VerifyLogMessage(ILogger logger, string expectedLog)
        {
            var testLogger = logger as ITestLogger;
            if (testLogger != null)
            {
                Assert.True(testLogger.LogMessages.Any(x => x.Contains(expectedLog)), FormatErrorMessage(testLogger.LogMessages, expectedLog));
            }
            else
            {
                Assert.False(true, "No logger registered");
            }
        }

        private static string FormatErrorMessage(IList<string> logMessages, string expectedLog)
        {
            return $"Failed to find in logs\n     Expected: '{expectedLog}'\n     Actual  : '{string.Join("\n     Actual  : '", logMessages)}'";
        }
    }
}