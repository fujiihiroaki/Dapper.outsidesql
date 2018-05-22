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

using System;
using System.Runtime.Serialization;

namespace Seasar.Framework.Exceptions
{
    /// <summary>
    /// Microsoft.JScript.Eval.JScriptEvaluate�Ŕ���������s����O�ł��B
    /// </summary>
    [Serializable]
    public sealed class ScriptEvaluateRuntimeException : SRuntimeException
    {
        private readonly string _expression;

        public ScriptEvaluateRuntimeException(string expression, Exception cause)
            : base("ESSR0073", new object[] { expression, cause }, cause)
        {
            _expression = expression;
        }

        public ScriptEvaluateRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _expression = info.GetString("_expression");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_expression", _expression, typeof(string));
            base.GetObjectData(info, context);
        }

        public string Expression
        {
            get { return _expression; }
        }
    }
}
