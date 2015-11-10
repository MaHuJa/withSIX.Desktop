// <copyright company="SIX Networks GmbH" file="JsonPPStrategyContext.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals.JsonPPStrategies;

namespace SN.withSIX.Core.Extensions.JsonPrettyPrinterInternals
{
    public class JsonPPStrategyContext
    {
        const string Space = " ";
        const string Tab = "\t";
        const string IndentChar = Tab;
        readonly PPScopeState _scopeState = new PPScopeState();
        readonly IDictionary<char, ICharacterStrategy> _strategyCatalog = new Dictionary<char, ICharacterStrategy>();
        char _currentCharacter;
        string _indent = string.Empty;
        StringBuilder _outputBuilder;
        char _previousChar;
        public int IndentAmount = 1;
        public bool IsProcessingVariableAssignment;
        public int SpacesPerIndent = 4;
        public string Indent
        {
            get
            {
                if (IndentAmount == 0)
                    return string.Empty;

                if (_indent == string.Empty)
                    InitializeIndent();

                return _indent;
            }
        }
        public bool IsInArrayScope
        {
            get { return _scopeState.IsTopTypeArray; }
        }
        public bool IsProcessingDoubleQuoteInitiatedString { get; set; }
        public bool IsProcessingSingleQuoteInitiatedString { get; set; }
        public bool IsProcessingString
        {
            get { return IsProcessingDoubleQuoteInitiatedString || IsProcessingSingleQuoteInitiatedString; }
        }
        public bool IsStart
        {
            get { return _outputBuilder.Length == 0; }
        }
        public bool WasLastCharacterABackSlash
        {
            get { return _previousChar == '\\'; }
        }

        void InitializeIndent() {
            for (var iii = 0; iii < IndentAmount; iii++)
                _indent += IndentChar;
        }

        void AppendIndents(int indents) {
            for (var iii = 0; iii < indents; iii++)
                _outputBuilder.Append(Indent);
        }

        public void PrettyPrintCharacter(char curChar, StringBuilder output) {
            _currentCharacter = curChar;

            var strategy = _strategyCatalog.ContainsKey(curChar)
                ? _strategyCatalog[curChar]
                : new DefaultCharacterStrategy();

            _outputBuilder = output;

            strategy.ExecutePrintyPrint(this);

            _previousChar = curChar;
        }

        public void AppendCurrentChar() {
            _outputBuilder.Append(_currentCharacter);
        }

        public void AppendNewLine() {
            _outputBuilder.Append(Environment.NewLine);
        }

        public void BuildContextIndents() {
            AppendNewLine();
            AppendIndents(_scopeState.ScopeDepth);
        }

        public void EnterObjectScope() {
            _scopeState.PushObjectContextOntoStack();
        }

        public void CloseCurrentScope() {
            _scopeState.PopJsonType();
        }

        public void EnterArrayScope() {
            _scopeState.PushJsonArrayType();
        }

        public void AppendSpace() {
            _outputBuilder.Append(Space);
        }

        public void ClearStrategies() {
            _strategyCatalog.Clear();
        }

        public void AddCharacterStrategy(ICharacterStrategy strategy) {
            _strategyCatalog[strategy.ForWhichCharacter] = strategy;
        }
    }
}