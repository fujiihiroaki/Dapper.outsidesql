#region copyright

// /*
//  * Copyright 2018-2018 Hiroaki Fujii  All rights reserved. 
//  *
//  * Licensed under the Apache License, Version 2.0 (the "License");
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *     http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
//  * either express or implied. See the License for the specific language
//  * governing permissions and limitations under the License.
//  */

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using Hnx8.ReadJEnc;
using Jiifureit.Dapper.OutsideSql.Impl;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Jiifureit.Dapper.OutsideSql.Utility;
using NLog;

#endregion

namespace Jiifureit.Dapper.OutsideSql
{
    /// <summary>
    ///     Dapper Extension for using Outside SQL file.
    /// </summary>
    public static class DapperOutsideSqlExtension
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Execute parameterized SQL.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteOutsideSql(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.Execute(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell selected as <see cref="object" />.</returns>
        public static object ExecuteScalarOutsideSql(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.ExecuteScalar(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static T ExecuteScalarOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.ExecuteScalar<T>(sql, newParam, transaction, commandTimeout, commandType);
        }


        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL to execute.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static IDataReader ExecuteReaderOutsideSql(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.ExecuteReader(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static IEnumerable<dynamic> QueryOutsideSql(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryOutsideSql<dynamic>(cnn, filepath, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOutsideSql(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstOutsideSql<dynamic>(cnn, filepath, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOrDefaultOutsideSql(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QueryFirstOrDefaultOutsideSql<dynamic>(cnn, filepath, param, transaction, commandTimeout,
                commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOutsideSql(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleOutsideSql<dynamic>(cnn, filepath, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOrDefaultOutsideSql(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            return QuerySingleOrDefaultOutsideSql<dynamic>(cnn, filepath, param, transaction, commandTimeout,
                commandType);
        }

        /// <summary>
        ///     Executes a query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static IEnumerable<T> QueryOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.Query<T>(sql, newParam, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var type = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, type);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QueryFirst<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOrDefaultOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefault<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QuerySingle<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOrDefaultOutsideSql<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefault<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static IEnumerable<object> QueryOutsideSql(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.Query(type, sql, newParam, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QueryFirstOutsideSql(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QueryFirst(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QueryFirstOrDefaultOutsideSql(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefault(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QuerySingleOutsideSql(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QuerySingle(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static object QuerySingleOrDefaultOutsideSql(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefault(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static SqlMapper.GridReader QueryMultipleOutsideSql(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = _CreateDynamicParameters(param);
            return cnn.QueryMultiple(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Read sql file.
        /// </summary>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="type">Bind Viarables type.</param>
        /// <returns>sql</returns>
        private static string _ParseFile(string filepath, object param,
            BindVariableType type = BindVariableType.Question)
        {
            string sql;

            // Read file.
            var fileInfo = new FileInfo(filepath);
            using (var fileReader = new FileReader(fileInfo))
            {
                var charcode = fileReader.Read(fileInfo);

                using (TextReader reader = new StreamReader(filepath, charcode.GetEncoding()))
                {
                    sql = reader.ReadToEnd();
                }
            }

            // parse file content.
            var parser = new Parser(sql);
            var rootNode = parser.Parse();

            ICommandContext ctx = new CommandContextImpl(BindVariableType.Question)
            {
                BindVariableType = type
            };

            if (param != null)
            {
                // hold parameters.
                if (!(param is IEnumerable<KeyValuePair<string, object>> dictionary))
                {
                    var proprties = param.GetType().GetProperties();
                    proprties.AsList().ForEach(p =>
                    {
                        ctx.AddArg(p.Name, p.GetValue(param), p.GetValue(param).GetType());
                    });
                }
                else
                {
                    foreach (var keyValue in dictionary)
                        ctx.AddArg(keyValue.Key, keyValue.Value, keyValue.Value.GetType());
                }
            }

            rootNode.Accept(ctx);

            // log sql.
            logger.Debug(ctx.SqlWithValue);

            return ctx.Sql;
        }

        private static object _CreateDynamicParameters(object param)
        {
            // IEnumerable<> Check
            bool IsGenericEnumerable(Type type)
            {
                return type.GetInterfaces()
                    .Any(t => t.IsGenericType &&
                              (t.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                               t.GetGenericTypeDefinition() == typeof(IList<>)));
            }

            if (param == null)
                return null;

            if (param is IEnumerable<KeyValuePair<string, object>>)
                return param;

            // Convert parameters;
            var newParam = new DynamicParameters();
            var properties = param.GetType().GetProperties();
            foreach (var info in properties)
            {
                if (info.PropertyType.GetInterface("System.Collections.ICollection") != null)
                {
                    var list = (ICollection) info.GetValue(param);
                    var i = 1;
                    foreach (var o in list)
                    {
                        newParam.Add(info.Name + i, o);
                        i++;
                    }
                }
                else if (info.PropertyType.GetInterface("System.Collections.IList") != null)
                {
                    var list = (IList) info.GetValue(param);
                    var i = 1;
                    foreach (var o in list)
                    {
                        newParam.Add(info.Name + i, o);
                        i++;
                    }
                }
                else if (IsGenericEnumerable(info.PropertyType))
                {
                    var val = (IList) info.GetValue(param);
                    if (val != null)
                    {
                        for (var i = 0; i < val.Count; i++)
                            newParam.Add(info.Name + (i + 1), val[i]);
                    }
                    else
                    {
                        var val2 = (ICollection) info.GetValue(param);
                        var i = 1;
                        foreach (var o in val2)
                        {
                            newParam.Add(info.Name + i, o);
                            i++;
                        }

                    }
                }
                else if (info.PropertyType.IsArray)
                {
                    var rank = param.GetType().GetArrayRank();
                    for (var i = 0; i < rank; i++)
                    {
                        var val = info.GetValue(param, new object[] {i});
                        newParam.Add(info.Name + (i + 1), val);
                    }
                }
                else
                {
                    var val = info.GetValue(param);
                    newParam.Add(info.Name, val);
                }
            }

            return newParam;
        }
    }
}