#region copyright

// /*
//  * Copyright 2018-2020 Hiroaki Fujii  All rights reserved. 
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Dapper;
using Hnx8.ReadJEnc;
using Jiifureit.Dapper.OutsideSql.Exception;
using Jiifureit.Dapper.OutsideSql.Impl;
using Jiifureit.Dapper.OutsideSql.SqlParser;
using Jiifureit.Dapper.OutsideSql.Utility;
using Microsoft.Extensions.Logging;
using Logger = Jiifureit.Dapper.OutsideSql.Log.Logger;

#endregion

namespace Jiifureit.Dapper.OutsideSql
{
    /// <summary>
    ///     Dapper Extension for using Outside SQL file.
    /// </summary>
    public static class DapperOutsideSqlExtension
    {
        private static readonly ILogger logger = Logger.Create();

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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Execute(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static int ExecuteOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalar(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell selected as <see cref="object" />.</returns>
        public static object ExecuteScalarOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalar<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static T ExecuteScalarOutsideSql<T>(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteReader(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static IDataReader ExecuteReaderOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
        ///     Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static IEnumerable<dynamic> QueryOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return QueryOutsideSql<dynamic>(cnn, sqlStream, encoding, param, transaction, buffered, commandTimeout, commandType);
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
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return QueryFirstOutsideSql<dynamic>(cnn, sqlStream, encoding, param, transaction, commandTimeout, commandType);
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
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QueryFirstOrDefaultOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return QueryFirstOrDefaultOutsideSql<dynamic>(cnn, sqlStream, encoding, param, transaction, commandTimeout,
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
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return QuerySingleOutsideSql<dynamic>(cnn, sqlStream, encoding, param, transaction, commandTimeout, commandType);
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
        ///     Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static dynamic QuerySingleOrDefaultOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            return QuerySingleOrDefaultOutsideSql<dynamic>(cnn, sqlStream, encoding, param, transaction, commandTimeout,
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Query<T>(sql, newParam, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
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
        public static IEnumerable<T> QueryOutsideSql<T>(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirst<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOutsideSql<T>(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var type = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, type);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefault<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QueryFirstOrDefaultOutsideSql<T>(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingle<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOutsideSql<T>(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefault<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first
        ///     column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static T QuerySingleOrDefaultOutsideSql<T>(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null, object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.Query(type, sql, newParam, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
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
        public static IEnumerable<object> QueryOutsideSql(this IDbConnection cnn, Type type, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirst(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
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
        public static object QueryFirstOutsideSql(this IDbConnection cnn, Type type, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefault(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
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
        public static object QueryFirstOrDefaultOutsideSql(this IDbConnection cnn, Type type, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingle(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
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
        public static object QuerySingleOutsideSql(this IDbConnection cnn, Type type, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefault(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Executes a single-row query, returning the data typed as <paramref name="type" />.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
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
        public static object QuerySingleOrDefaultOutsideSql(this IDbConnection cnn, Type type, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
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
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryMultiple(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static SqlMapper.GridReader QueryMultipleOutsideSql(this IDbConnection cnn, Stream sqlStream, Encoding encoding = null,
            object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryMultiple(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Read sql file.
        /// </summary>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="type">Bind Variables type.</param>
        /// <returns>sql</returns>
        private static string _ParseFile(string filepath, object param,
            BindVariableType type = BindVariableType.Question)
        {
            if (!File.Exists(filepath))
                throw new SqlFileNotFoundRuntimeException(filepath);

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

            return _ParseRawSql(param, type, sql);
        }

        /// <summary>
        ///     Use raw sql statement.
        /// </summary>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="type">Bind Variables type.</param>
        /// <param name="sql">SQL statement.</param>
        /// <returns>sql</returns>
        private static string _ParseRawSql(object param, BindVariableType type, string sql)
        {
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
                    if (param is DynamicParameters dynamicParam)
                    {
                        var lookup = (SqlMapper.IParameterLookup)dynamicParam;
                        using (var names = dynamicParam.ParameterNames.GetEnumerator())
                        {
                            while (names.MoveNext())
                            {
                                var name = names.Current;
                                var p = lookup[name];
                                if (p != null)
                                    ctx.AddArg(name, p, p.GetType());
                                else
                                    ctx.AddArg(name, null, null);
                            }
                        }
                    }
                    else
                    {
                        var proprties = param.GetType().GetProperties();
                        proprties.AsList().ForEach(p =>
                        {
                            var v = p.GetValue(param);
                            if (v != null)
                                ctx.AddArg(p.Name, v, v.GetType());
                            else
                                ctx.AddArg(p.Name, null, null);
                        });
                    }
                }
                else
                {
                    foreach (var keyValue in dictionary)
                    {
                        var v = keyValue.Value;
                        if (v != null)
                            ctx.AddArg(keyValue.Key, v, v.GetType());
                        else
                            ctx.AddArg(keyValue.Key, null, null);
                    }
                }
            }

            rootNode.Accept(ctx);

            // log sql.
            logger?.LogDebug(ctx.SqlWithValue);

            return ctx.Sql;
        }

        /// <summary>
        ///     Read sql stream.
        /// </summary>
        /// <param name="stream">The SQL stream to execute for this query.</param>
        /// <param name="encoding">The encoding about SQL stream.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="type">Bind Variables type.</param>
        /// <returns>sql</returns>
        private static string _ParseStream(Stream stream, Encoding encoding, object param,
            BindVariableType type = BindVariableType.Question)
        {
            if (stream == null)
            {
                throw new SqlStreamNullException();
            }

            string sql;

            // Read stream.
            using (TextReader reader = new StreamReader(stream, encoding))
            {
                sql = reader.ReadToEnd();
            }

            return _ParseRawSql(param, type, sql);
        }
    }
}