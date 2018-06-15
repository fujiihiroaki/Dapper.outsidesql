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

using Jiifureit.Dapper.OutsideSql.Exception;

#endregion

namespace Jiifureit.Dapper.OutsideSql.SqlParser
{
    public class SqlTokenizerImpl : ISqlTokenizer
    {
        private readonly string _sql;
        private int _bindVariableNum;
        private TokenType _nextTokenType = TokenType.Sql;

        public SqlTokenizerImpl(string sql)
        {
            _sql = sql;
        }

        protected string NextBindVariableName => "$" + ++_bindVariableNum;

        protected void ParseSql()
        {
            var commentStartPos = _sql.IndexOf("/*", Position);
            var lineCommentStartPos = _sql.IndexOf("--", Position);
            var bindVariableStartPos = _sql.IndexOf("?", Position);
            var elseCommentStartPos = -1;
            var elseCommentLength = -1;

            if (bindVariableStartPos < 0) bindVariableStartPos = _sql.IndexOf("?", Position);
            if (lineCommentStartPos >= 0)
            {
                var skipPos = _SkipWhitespace(lineCommentStartPos + 2);
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
                _nextTokenType = TokenType.Eof;
                Position = _sql.Length;
                TokenType = TokenType.Sql;
            }
            else
            {
                Token = _sql.Substring(Position, nextStartPos - Position);
                TokenType = TokenType.Sql;
                bool needNext = nextStartPos == Position;
                if (nextStartPos == commentStartPos)
                {
                    _nextTokenType = TokenType.Comment;
                    Position = commentStartPos + 2;
                }
                else if (nextStartPos == elseCommentStartPos)
                {
                    _nextTokenType = TokenType.Else;
                    Position = elseCommentStartPos + elseCommentLength;
                }
                else if (nextStartPos == bindVariableStartPos)
                {
                    _nextTokenType = TokenType.BindVariable;
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

        protected void ParseComment()
        {
            int commentEndPos = _sql.IndexOf("*/", Position);
            if (commentEndPos < 0)
                throw new TokenNotClosedRuntimeException("*/",
                    _sql.Substring(Position));

            Token = _sql.Substring(Position, (commentEndPos - Position));
            _nextTokenType = TokenType.Sql;
            Position = commentEndPos + 2;
            TokenType = TokenType.Comment;
        }

        protected void ParseBindVariable()
        {
            Token = NextBindVariableName;
            _nextTokenType = TokenType.Sql;
            Position++;
            TokenType = TokenType.BindVariable;
        }

        protected void ParseElse()
        {
            Token = null;
            _nextTokenType = TokenType.Sql;
            TokenType = TokenType.Else;
        }

        protected void ParseEof()
        {
            Token = null;
            TokenType = TokenType.Eof;
            _nextTokenType = TokenType.Eof;
        }

        private int _SkipWhitespace(int position)
        {
            var index = _sql.Length;
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

        #region ISqlTokenizer ƒƒ“ƒo

        public string Token { get; private set; }

        public string Before => _sql.Substring(0, Position);

        public string After => _sql.Substring(Position);

        public int Position { get; private set; }

        public TokenType TokenType { get; private set; } = TokenType.Sql;

        public TokenType Next()
        {
            if (Position >= _sql.Length)
            {
                Token = null;
                TokenType = TokenType.Eof;
                _nextTokenType = TokenType.Eof;
                return TokenType;
            }

            switch (_nextTokenType)
            {
                case TokenType.Sql:
                    ParseSql();
                    break;
                case TokenType.Comment:
                    ParseComment();
                    break;
                case TokenType.Else:
                    ParseElse();
                    break;
                case TokenType.BindVariable:
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
            TokenType = TokenType.Sql;
            _nextTokenType = TokenType.Sql;
            Position = index;
            return Token;
        }

        public string _SkipWhitespace()
        {
            var index = _SkipWhitespace(Position);
            Token = _sql.Substring(Position, (index - Position));
            Position = index;
            return Token;
        }

        #endregion
    }
}