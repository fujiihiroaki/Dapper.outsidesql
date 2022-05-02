#region copyright

// /*
//  * Copyright 2018-2022 Hiroaki Fujii  All rights reserved. 
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

#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Jiifureit.Dapper.OutsideSql.Utility;

#endregion


namespace Jiifureit.Dapper.OutsideSql
{
    /// <summary>
    ///     Dapper Extension for using Outside SQL file.
    ///     Execute a query asynchronously using Task.
    /// </summary>
    public static partial class DapperOutsideSqlExtension
    {
        /// <summary>
        ///     Execute a query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Task<IEnumerable<dynamic>> QueryOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Task<IEnumerable<dynamic>> QueryOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of <typeparamref name="T" />; if a basic type (int, string, etc) is queried then the data from
        ///     the first column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static Task<IEnumerable<T>> QueryOutsideSqlAsync<T>(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of results to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>
        ///     A sequence of data of <typeparamref name="T" />; if a basic type (int, string, etc) is queried then the data from
        ///     the first column in assumed, otherwise an instance is
        ///     created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        public static Task<IEnumerable<T>> QueryOutsideSqlAsync<T>(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QueryFirstOutsideSqlAsync<T>(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QueryFirstOutsideSqlAsync<T>(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QueryFirstOrDefaultOutsideSqlAsync<T>(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QueryFirstOrDefaultOutsideSqlAsync<T>(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QuerySingleOutsideSqlAsync<T>(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QuerySingleOutsideSqlAsync<T>(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QuerySingleOrDefaultOutsideSqlAsync<T>(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<T> QuerySingleOrDefaultOutsideSqlAsync<T>(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QueryFirstOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QueryFirstOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QueryFirstOrDefaultOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QueryFirstOrDefaultOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QuerySingleOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QuerySingleOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QuerySingleOrDefaultOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        public static Task<dynamic> QuerySingleOrDefaultOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync<dynamic>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<IEnumerable<object>> QueryOutsideSqlAsync(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<IEnumerable<object>> QueryOutsideSqlAsync(this IDbConnection cnn, Type type,
            Stream sqlStream, Encoding encoding = null, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QueryFirstOutsideSqlAsync(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QueryFirstOutsideSqlAsync(this IDbConnection cnn, Type type, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QueryFirstOrDefaultOutsideSqlAsync(this IDbConnection cnn, Type type,
            string filepath, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QueryFirstOrDefaultOutsideSqlAsync(this IDbConnection cnn, Type type,
            Stream sqlStream, Encoding encoding = null, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryFirstOrDefaultAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QuerySingleOutsideSqlAsync(this IDbConnection cnn, Type type, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QuerySingleOutsideSqlAsync(this IDbConnection cnn, Type type, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QuerySingleOrDefaultOutsideSqlAsync(this IDbConnection cnn, Type type,
            string filepath, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync(type, sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="type">The type to return.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> is <c>null</c>.</exception>
        public static Task<object> QuerySingleOrDefaultOutsideSqlAsync(this IDbConnection cnn, Type type,
            Stream sqlStream, Encoding encoding = null, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            if (type == null) throw new ArgumentNullException(nameof(type));
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QuerySingleOrDefaultAsync(type, sql, newParam, transaction, commandTimeout, commandType);
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
        public static Task<SqlMapper.GridReader> QueryMultipleOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryMultipleAsync(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        public static Task<SqlMapper.GridReader> QueryMultipleOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.QueryMultipleAsync(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static Task<int> ExecuteOutsideSqlAsync(this IDbConnection cnn, string filepath, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteAsync(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute a command asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static Task<int> ExecuteOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null,
            IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteAsync(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An <see cref="IDataReader" /> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        ///     This is typically used when the results of a query are not processed by Dapper, for example, used to fill a
        ///     <see cref="DataTable" />
        ///     or <see cref="T:DataSet" />.
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// DataTable table = new DataTable("MyTable");
        /// using (var reader = ExecuteReader(cnn, sql, param))
        /// {
        ///     table.Load(reader);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Task<IDataReader> ExecuteReaderOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteReaderAsync(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL and return an <see cref="IDataReader" />.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An <see cref="IDataReader" /> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        ///     This is typically used when the results of a query are not processed by Dapper, for example, used to fill a
        ///     <see cref="DataTable" />
        ///     or <see cref="T:DataSet" />.
        /// </remarks>
        /// <example>
        ///     <code>
        /// <![CDATA[
        /// DataTable table = new DataTable("MyTable");
        /// using (var reader = ExecuteReader(cnn, sql, param))
        /// {
        ///     table.Load(reader);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static Task<IDataReader> ExecuteReaderOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteReaderAsync(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <see cref="object" />.</returns>
        public static Task<object> ExecuteScalarOutsideSqlAsync(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalarAsync<object>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <see cref="object" />.</returns>
        public static Task<object> ExecuteScalarOutsideSqlAsync(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalarAsync<object>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="filepath">The SQL file to execute for this query.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static Task<T> ExecuteScalarOutsideSqlAsync<T>(this IDbConnection cnn, string filepath,
            object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseFile(filepath, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalarAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }

        /// <summary>
        ///     Execute parameterized SQL that selects a single value.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="sqlStream">The SQL file to execute for this query.</param>
        /// <param name="encoding">The encoding of Stream.</param>
        /// <param name="param">The parameters to use for this command.</param>
        /// <param name="transaction">The transaction to use for this command.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The first cell returned, as <typeparamref name="T" />.</returns>
        public static Task<T> ExecuteScalarOutsideSqlAsync<T>(this IDbConnection cnn, Stream sqlStream,
            Encoding encoding = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bindType = DataProviderUtil.GetBindVariableType(cnn);
            var sql = _ParseStream(sqlStream, encoding, param, bindType);
            var newParam = DynamicParameterUtil.CreateDynamicParameters(param);
            return cnn.ExecuteScalarAsync<T>(sql, newParam, transaction, commandTimeout, commandType);
        }
    }
}