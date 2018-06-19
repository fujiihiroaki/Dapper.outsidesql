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
using Jiifureit.Dapper.OutsideSql.Exception;
using Jint;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Utility
{
    /// <summary>
    ///     Rosylnでコードを実行します
    /// </summary>
    public sealed class CSharpScriptUtil
    {
        public static object Evaluate(string exp, Hashtable ctx, object root)
        {
            if (exp.Contains("\r")) exp = exp.Replace("\r", "\\r");
            if (exp.Contains("\n")) exp = exp.Replace("\n", "\\n");

            var arg = exp.Replace("self.", "").Replace("'", "\"");
            try
            {
                var engine = new Engine(cfg => cfg.AllowClr());

                var ret = engine
                    .SetValue("GetArg", new Func<string, object>(name => ((ICommandContext) ctx["self"]).GetArg(name)))
                    .Execute(arg)
                    .GetCompletionValue()
                    .ToObject();

                return ret;
            }
            catch (System.Exception ex)
            {
                throw new ScriptEvaluateRuntimeException(arg, ex);
            }
        }

        public static object Evaluate(string exp, object root)
        {
            try
            {
                return Evaluate(exp, new Hashtable(), root);
            }
            catch (System.Exception ex)
            {
                throw new ScriptEvaluateRuntimeException(exp, ex);
            }
        }
    }
}