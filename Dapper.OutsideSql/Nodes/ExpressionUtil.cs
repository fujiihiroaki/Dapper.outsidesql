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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Jiifureit.Dapper.OutsideSql.Nodes
{
    public class ExpressionUtil
    {
        private readonly Regex _reLit;
        private readonly Regex _reOps;
        private readonly Regex _reSym;
        private IToken _current;
        private int _start;
        private string _text;

        public ExpressionUtil()
        {
            _reOps = new Regex(@"^\s*(&&|\|\||<=|>=|==|!=|[=+\-*/^()!<>])", RegexOptions.Compiled);
            _reSym = new Regex(@"^\s*(\-?\b*[^=+\-*/^()!<>\s]*)", RegexOptions.Compiled);
            _reLit = new Regex(@"^\s*([-+]?[0-9]+(\.[0-9]+)?)", RegexOptions.Compiled);
        }

        public string ParseExpression(string expression)
        {
            _current = null;
            _text = expression;
            var sb = new StringBuilder(255);
            while (!Eof())
            {
                var token = NextToken();
                sb.Append(token.Value + " ");
            }

            return sb.ToString().TrimEnd(' ');
        }

        protected bool Eof()
        {
            if (_current is Eof) return true;
            return false;
        }

        protected IToken NextToken()
        {
            var match = _reLit.Match(_text);
            if (match.Length != 0)
            {
                _SetNumberLiteralToken(match);
            }
            else
            {
                match = _reOps.Match(_text);
                if (match.Length != 0)
                {
                    _SetOperatorToken(match);
                }
                else
                {
                    match = _reSym.Match(_text);
                    if (match.Length != 0)
                        _SetSymbolToken(match);
                    else
                        _current = new Eof();
                }
            }

            return _current;
        }

        private void _SetNumberLiteralToken(Match match)
        {
            _start += match.Length;
            _text = _text.Substring(match.Length);
            IToken token = new NumberLiteral
            {
                Value = match.Groups[1].Value
            };
            _current = token;
        }

        private void _SetSymbolToken(Match match)
        {
            _start += match.Length;
            _text = _text.Substring(match.Length);
            IToken token = new Symbol
            {
                Value = match.Groups[1].Value
            };
            _current = token;
        }

        private void _SetOperatorToken(Match match)
        {
            _start += match.Length;
            _text = _text.Substring(match.Length);
            IToken token = new Operator
            {
                Value = match.Groups[1].Value
            };
            _current = token;
        }
    }

    #region Token

    public interface IToken
    {
        object Value { get; set; }
    }

    public class Eof : IToken
    {
        public object Value
        {
            get => null;
            set => throw new NotImplementedException();
        }
    }

    public class Symbol : IToken
    {
        private readonly string[] _escapes = {"null", "true", "false"};
        private string _value;

        public object Value
        {
            get => _GetArgValue();
            set => _value = (string) value;
        }

        private string _GetArgValue()
        {
            if (_value.StartsWith("'") && _value.EndsWith("'"))
                return _value;

            foreach (string escape in _escapes)
                if (_value.ToLower() == escape)
                    return _value.ToLower();

            return "self.GetArg('" + _value + "')";
        }

        public override string ToString()
        {
            return _value;
        }
    }

    public class NumberLiteral : IToken
    {
        private float _value;

        public object Value
        {
            get => _value;
            set => _value = (float) double.Parse(value.ToString());
        }

        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }
    }

    public class Operator : IToken
    {
        private string _value;

        public object Value
        {
            get => _value;
            set => _value = (string) value;
        }

        public override string ToString()
        {
            return _value;
        }
    }

    #endregion
}