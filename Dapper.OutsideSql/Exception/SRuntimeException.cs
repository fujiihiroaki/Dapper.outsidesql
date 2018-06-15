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
using Jiifureit.Dapper.OutsideSql.Utility;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     Seasarの実行時例外のベースとなるクラスです。
    ///     メッセージコードによって例外を詳細に特定できます。
    /// </summary>
    [Serializable]
    public class SRuntimeException : ApplicationException
    {
        private readonly string _message;

        public SRuntimeException(string messageCode)
            : this(messageCode, null, null)
        {
        }

        public SRuntimeException(string messageCode, object[] args)
            : this(messageCode, args, null)
        {
        }

        public SRuntimeException(string messageCode, object[] args, System.Exception cause)
            : base(messageCode, cause)
        {
            MessageCode = messageCode;
            Args = args;
            SimpleMessage = MessageFormatter.GetSimpleMessage(MessageCode, Args);
            _message = "[" + messageCode + "]" + SimpleMessage;
        }

        public SRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MessageCode = info.GetString("_messageCode");
            Args = info.GetValue("_args", typeof(object[])) as object[];
            _message = info.GetString("_message");
            SimpleMessage = info.GetString("_simpleMessage");
        }

        public string MessageCode { get; }

        public object[] Args { get; }

        public override string Message => _message;

        public string SimpleMessage { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_messageCode", MessageCode, typeof(string));
            info.AddValue("_args", Args, typeof(object[]));
            info.AddValue("_message", _message, typeof(string));
            info.AddValue("_simpleMessage", SimpleMessage, typeof(string));
            base.GetObjectData(info, context);
        }
    }
}