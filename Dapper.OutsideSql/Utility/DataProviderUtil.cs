#region copyright

// /*
//  * Copyright 2018-2021 Hiroaki Fujii  All rights reserved. 
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

using System.Data;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Utility
{
    public static class DataProviderUtil
    {
        /// <summary>
        ///     Get kind of Bind Variables from Connection object.
        /// </summary>
        /// <param name="connection">DbConnection object</param>
        /// <returns>Bind Virables type</returns>
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