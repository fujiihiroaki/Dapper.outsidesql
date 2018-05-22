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

namespace Seasar.Dao.Parser
{
    public class SqlTokenizerImpl : ISqlTokenizer
    {
        private readonly string _sql;
        private TokenType _nextTokenType = TokenType.SQL;
        private int _bindVariableNum;

        public SqlTokenizerImpl(string sql)
        {
            _sql = sql;
        }

        #region ISqlTokenizer ƒƒ“ƒo

        public string Token { get; private set; }

        public string Before => _sql.Substring(0, Position);

        public string After => _sql.Substring(Position);

        public int Position { get; private set; }

        public TokenType TokenType { get; private set; } = TokenType.SQL;

        public TokenType NextTokenType
        {
            get { return NextTokenType; }
        }

        public TokenType Next()
        {
            if (Position >= _sql.Length)
            {
                Token = null;
                TokenType = TokenType.EOF;
                _nextTokenType = TokenType.EOF;
                return TokenType;
            }
            switch (_nextTokenType)
            {
                case TokenType.SQL:
                    ParseSql();
                    break;
                case TokenType.COMMENT:
                    ParseComment();
                    break;
                case TokenType.ELSE:
                    ParseElse();
                    break;
                case TokenType.BIND_VARIABLE:
                    ParseBindVariable();
                    break;
                default:
                    ParseEof();
                    break;
            }
            return TokenType;
        }

        public string SkipToken()
        {
            var index = _sql.Length;
            var quote = Position < _sql.Length ? _sql.ToCharArray()[Position] : '\0';
            var quoting = quote == '\'' || quote == '(';
            if (quote == '(') quote = ')';

            for (var i = quoting ? Position + 1 : Position; i < _sql.Length; ++i)
            {
                var c = _sql.ToCharArray()[i];
                if ((char.IsWhiteSpace(c) || c == ',' || c == ')' || c == '(')
                    && !quoting)
                {
                    index = i;
                    break;
                }
                else if (c == '/' && i + 1 < _sql.Length
                    && _sql.ToCharArray()[i + 1] == '*')
                {
                    index = i;
                    break;
                }
                else if (c == '-' && i + 1 < _sql.Length
                    && _sql.ToCharArray()[i + 1] == '-')
                {
                    index = i;
                    break;
                }
                else if (quoting && quote == '\'' && c == '\''
                    && (i + 1 >= _sql.Length || _sql.ToCharArray()[i + 1] != '\''))
                {
                    index = i + 1;
                    break;
                }
                else if (quoting && c == quote)
                {
                    index = i + 1;
                    break;
                }
            }
            Token = _sql.Substring(Position, (index - Position));
            TokenType = TokenType.SQL;
            _nextTokenType = TokenType.SQL;
            Position = index;
            return Token;
        }

        public string SkipWhitespace()
        {
            var index = SkipWhitespace(Position);
            Token = _sql.Substring(Position, (index - Position));
            Position = index;
            return Token;
        }

        #endregion

        protected void ParseSql()
        {
            var commentStartPos = _sql.IndexOf("/*", Position);
            var lineCommentStartPos = _sql.IndexOf("--", Position);
            var bindVariableStartPos = _sql.IndexOf("?", Position);
            var elseCommentStartPos = -1;
            var elseCommentLength = -1;

            if (bindVariableStartPos < 0)
            {
                bindVariableStartPos = _sql.IndexOf("?", Position);
            }
            if (lineCommentStartPos >= 0)
            {
                var skipPos = SkipWhitespace(lineCommentStartPos + 2);
                if (skipPos + 4 < _sql.Length
                    && "ELSE" == _sql.Substring(skipPos, skipPos + 4 - skipPos))
                {
                    elseCommentStartPos = lineCommentStartPos;
                    elseCommentLength = skipPos + 4 - lineCommentStartPos;
                }
            }
            int nextStartPos = GetNextStartPos(commentStartPos,
                elseCommentStartPos, bindVariableStartPos);
            if (nextStartPos < 0)
            {
                Token = _sql.Substring(Position);
                _nextTokenType = TokenType.EOF;
                Position = _sql.Length;
                TokenType = TokenType.SQL;
            }
            else
            {
                Token = _sql.Substring(Position, nextStartPos - Position);
                TokenType = TokenType.SQL;
                bool needNext = nextStartPos == Position;
                if (nextStartPos == commentStartPos)
                {
                    _nextTokenType = TokenType.COMMENT;
                    Position = commentStartPos + 2;
                }
                else if (nextStartPos == elseCommentStartPos)
                {
                    _nextTokenType = TokenType.ELSE;
                    Position = elseCommentStartPos + elseCommentLength;
                }
                else if (nextStartPos == bindVariableStartPos)
                {
                    _nextTokenType = TokenType.BIND_VARIABLE;
                    Position = bindVariableStartPos;
                }
                if (needNext) Next();
            }
        }

        protected int GetNextStartPos(int commentStartPos, int elseCommentStartPos,
            int bindVariableStartPos)
        {
            var nextStartPos = -1;
            if (commentStartPos >= 0)
                nextStartPos = commentStartPos;

            if (elseCommentStartPos >= 0
                && (nextStartPos < 0 || elseCommentStartPos < nextStartPos))
                nextStartPos = elseCommentStartPos;

            if (bindVariableStartPos >= 0
                && (nextStartPos < 0 || bindVariableStartPos < nextStartPos))
                nextStartPos = bindVariableStartPos;

            return nextStartPos;
        }

        protected string NextBindVariableName => "$" + ++_bindVariableNum;

        protected void ParseComment()
        {
            int commentEndPos = _sql.IndexOf("*/", Position);
            if (commentEndPos < 0)
                throw new TokenNotClosedRuntimeException("*/",
                    _sql.Substring(Position));

            Token = _sql.Substring(Position, (commentEndPos - Position));
            _nextTokenType = TokenType.SQL;
            Position = commentEndPos + 2;
            TokenType = TokenType.COMMENT;
        }

        protected void ParseBindVariable()
        {
            Token = NextBindVariableName;
            _nextTokenType = TokenType.SQL;
            Position++;
            TokenType = TokenType.BIND_VARIABLE;
        }

        protected void ParseElse()
        {
            Token = null;
            _nextTokenType = TokenType.SQL;
            TokenType = TokenType.ELSE;
        }

        protected void ParseEof()
        {
            Token = null;
            TokenType = TokenType.EOF;
            _nextTokenType = TokenType.EOF;
        }

        private int SkipWhitespace(int position)
        {
            int index = _sql.Length;
            for (var i = position; i < _sql.Length; ++i)
            {
                var c = _sql.ToCharArray()[i];
                if (!char.IsWhiteSpace(c))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
