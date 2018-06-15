#region Copyright

/*
 * Copyright 2005-2015 the Seasar Foundation and the Others.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */

#endregion

#region using

using System;
using System.Runtime.Serialization;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     CSharpScript Evaluation Runtime Exception
    /// </summary>
    [Serializable]
    public sealed class ScriptEvaluateRuntimeException : SRuntimeException
    {
        public ScriptEvaluateRuntimeException(string expression, System.Exception cause)
            : base("ESSR0073", new object[] {expression, cause}, cause)
        {
            Expression = expression;
        }

        public ScriptEvaluateRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Expression = info.GetString("_expression");
        }

        public string Expression { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_expression", Expression, typeof(string));
            base.GetObjectData(info, context);
        }
    }
}