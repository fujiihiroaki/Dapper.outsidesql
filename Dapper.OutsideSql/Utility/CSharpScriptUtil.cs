// /* 
// *  Copyright (c) 2018-2018  Hiroaki Fujii All rights reserved. Licensed under the MIT license. 
// *  See LICENSE in the source repository root for complete license information. 
// */

#region using

using System;
using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Seasar.Framework.Exceptions;

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
                var result = CSharpScript.RunAsync(arg, globals: ctx["self"]).Result;
                return result.ReturnValue;
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                throw new ScriptEvaluateRuntimeException(exp, ex);
            }
        }
    }
}