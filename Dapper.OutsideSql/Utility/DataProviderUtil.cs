// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

#region using

using System.Data;
using Seasar.Framework.Util;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Utility
{
    public sealed class DataProviderUtil
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        private DataProviderUtil()
        {
        }

        /// <summary>
        ///     バインド変数タイプを取得する
        /// </summary>
        /// <param name="connection">コマンドオブジェクト</param>
        /// <returns>バインド変数タイプ</returns>
        public static BindVariableType GetBindVariableType(IDbConnection connection)
        {
            var name = connection.GetType().Name;
            if (name == "SqlConnection" ||
                name == "DB2Connection")
                return BindVariableType.AtmarkWithParam;
            else if (name == "OracleConnection")
                return BindVariableType.ColonWithParam;
            else if (name == "MySqlConnection")
                return BindVariableType.QuestionWithParam;
            else if (name == "NpgsqlConnection")
                return BindVariableType.ColonWithParam;
            else if (name == "FbConnection")
                return BindVariableType.Question;
            else
                return BindVariableType.Question;
        }
    }
}