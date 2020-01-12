#region

using System;
using Microsoft.Extensions.Logging;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Log
{
    /// <summary>
    ///     Class for Microsoft.Extension.Logging
    /// </summary>
    public static class Logger
    {
        /// <summary>
        ///     Category. Fully qualified class name.
        /// </summary>
        public static string Category { get; set; }

        /// <summary>
        ///     Log Factory.
        /// </summary>
        public static ILoggerFactory Factory { get; set; } = new LoggerFactory(); 


        /// <summary>
        ///     Create logger based on category.
        /// </summary>
        /// <returns></returns>
        public static ILogger Create()
        {
            if (String.IsNullOrEmpty(Category))
                Category = "System.Object";

            return Factory.CreateLogger(Category);
        }

        /// <summary>
        ///     Create logger based on T.
        /// </summary>
        /// <returns></returns>
        public static ILogger<T> CreateLogger<T>()
        {
            if (String.IsNullOrEmpty(Category))
                Category = "System.Object";

            return Factory.CreateLogger<T>();
        }
    }
}