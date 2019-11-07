using Gobchat.Memory.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core
{
    internal class ChatlogParser
    {
        public void Process(ChatlogItem item)
        {
            if (!IsChatlogItemValid(item))
                return; //TODO


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatlogItem"></param>
        /// <returns>true when the chatlog is valid</returns>
        private bool IsChatlogItemValid(ChatlogItem chatlogItem)
        {
            var tokens = chatlogItem.Tokens;
            if (tokens.Count == 0) return false;
            if (tokens[0] is TextToken txtToken)
            {
                var txt = txtToken.GetText();
                if (txt == null || txt.Length == 0)
                    return false;
                return txt.StartsWith(":", System.StringComparison.InvariantCulture);
            }
            else
            {
                return false;
            }
        }

        private void ExtractSource(TokenReader tokenReader)
        {
            var tokenIdx = tokenReader.LastTokenIndex;
            var tokens = tokenReader.Tokens;

            if (!(tokens[tokenIdx] is TextToken firstToken))
                throw new ArgumentException(""); //TODO invalid chatlog

            var text = firstToken.GetText();
            if (text.Length == 0 || !text.StartsWith(":", System.StringComparison.InvariantCulture))
                throw new ArgumentException(""); //TODO invalid chatlog

            tokenReader.LastReadIndex += 1;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            do
            {
                if (tokens.Count <= tokenReader.LastTokenIndex)
                    break; //end

                var token = tokens[tokenReader.LastTokenIndex];

                if (token is TextToken txtToken)
                {
                    var txt = txtToken.GetText();
                    var idx = txt.IndexOf(":", tokenReader.LastReadIndex, System.StringComparison.InvariantCulture);

                    if (idx < 0)
                        builder.Append(txt, tokenReader.LastReadIndex, txt.Length - tokenReader.LastReadIndex);
                    else
                        builder.Append(txt, tokenReader.LastReadIndex, idx - tokenReader.LastReadIndex);

                    tokenReader.LastReadIndex = idx;
                    if (txt.Length <= tokenReader.LastReadIndex)
                    {
                        tokenReader.LastReadIndex = 0;
                        tokenReader.LastTokenIndex += 1;
                    }
                    else
                        break; //end of header
                }

            } while (true);

        }

        private void bla(List<IChatlogToken> tokens, int lastTokenIdx, out int nextTokenidx, Func<IChatlogToken, bool> f)
        {
            for (; lastTokenIdx < tokens.Count; ++lastTokenIdx)
            {
                var token = tokens[lastTokenIdx];
                if (!f(token))
                    break;
            }
            nextTokenidx = lastTokenIdx + 1;
        }

        private class TokenReader
        {
            public List<IChatlogToken> Tokens;
            public int LastTokenIndex;
            public int LastReadIndex;

            public IChatlogToken NextToken { get { return Tokens[LastTokenIndex++]; } }

            public TokenReader(List<IChatlogToken> tokens)
            {
                Tokens = tokens;
                LastTokenIndex = 0;
                LastReadIndex = 0;
            }
        }
    }
}
