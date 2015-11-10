// <copyright company="SIX Networks GmbH" file="ConsoleWriter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class ConsoleWriter
    {
        //readonly StringBuilder _buffer;
        readonly List<ConsoleLine> _buffer;

        public ConsoleWriter() {
            _buffer = new List<ConsoleLine>();
            //_buffer = new StringBuilder();
        }

        /*        public void UpdateOutput(string data) {
            lock (_buffer)
                _buffer.AppendLine(data);
        }

        public override string ToString() {
            lock (_buffer)
                return _buffer.ToString();
        }*/

        // TODO: First need a Process monitor that gives us opportunity to get more than lines..
        // http://stackoverflow.com/questions/7612524/parse-output-from-a-process-that-updates-a-single-console-line

        // Not thread safe to call from multiple threads, but can call ToString safely
        public void UpdateOutput(string data) {
            if (data.EndsWith("\r\n"))
                AddToBuffer(new TerminatedConsoleLine(data.Substring(0, data.Length - 2)));
            else if (data.EndsWith("\n"))
                AddToBuffer(new TerminatedConsoleLine(data.Substring(0, data.Length - 1)));
            else if (data.EndsWith("\r"))
                AddToBuffer(new ConsoleLineR(data.Substring(0, data.Length - 1)));
            else
                AddToBuffer(new ConsoleLine(data));
        }

        void AddToBuffer(ConsoleLine consoleLine) {
            lock (_buffer) {
                var last = _buffer.LastOrDefault();
                // Not exactly how \r works in console, but close enough for our purpose
                if (last != null && !last.Terminated) {
                    _buffer.RemoveAt(_buffer.Count - 1);
                    // If the line didn't have any termination, we should prepend the previous line ...
                    // we still replace the line object because it might be terminated now etc ;-)
                    if (last.Open)
                        consoleLine.Prepend(last);
                }
                _buffer.Add(consoleLine);
            }
        }

        public override string ToString() {
            lock (_buffer)
                return String.Join("\n", _buffer.Select(x => x.Content));
        }
    }

    public class ConsoleLine
    {
        public ConsoleLine(string content) {
            Content = content;
        }

        public string Content { get; private set; }
        public virtual bool Terminated
        {
            get { return false; }
        }
        public virtual bool Open
        {
            get { return true; }
        }

        public void Prepend(ConsoleLine content) {
            Content = content.Content + Content;
        }
    }

    public class ConsoleLineR : ConsoleLine
    {
        public ConsoleLineR(string content) : base(content) {}
        public override bool Open
        {
            get { return false; }
        }
    }

    public class TerminatedConsoleLine : ConsoleLineR
    {
        public TerminatedConsoleLine(string content) : base(content) {}
        public override bool Terminated
        {
            get { return true; }
        }
    }
}