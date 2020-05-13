/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gobchat.LogConverter
{
    internal sealed class LogConverter
    {
        private ILogParser _parser;
        private ILogFormater _formater;

        public void ConvertLog(string file, LogConverterOptions options, ProgressMonitorAdapter monitor)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (monitor == null) throw new ArgumentNullException(nameof(monitor));

            monitor.Progress = 0;
            monitor.Log("Read file");

            var lines = File.ReadAllLines(file);
            monitor.Log($"{lines.Length} lines read");
            if (lines.Length == 0)
            {
                monitor.Log("File empty");
                return;
            }

            var startIdx = 0;
            var loggerId = "ACTv1";

            if (HasLoggerId(lines[0]))
            {
                startIdx = 1;
                loggerId = GetLoggerId(lines[0]);
            }

            if (loggerId.Equals(options.ConvertTo))
            {
                monitor.Log("Log already converted");
                return;
            }

            _parser = GetParser(loggerId);
            _formater = GetFormater(options.ConvertTo);

            if (_parser == null)
            {
                monitor.Log($"No parser for logger type: {loggerId}");
                return;
            }

            if (_formater == null)
            {
                monitor.Log($"No formater for logger type: {options.ConvertTo}");
                return;
            }

            monitor.Log($"Converting from {loggerId} to {options.ConvertTo} ...");
            var workRemaining = lines.Length - startIdx;
            var workDone = 0;
            var result = new List<string>()
            {
                $"Chatlogger Id: {options.ConvertTo}"
            };

            var lineIdx = 0;
            foreach (var line in lines.Skip(startIdx))
            {
                try
                {
                    _parser.Read(line);
                    workDone += 1;

                    if (!_parser.NeedMore)
                    {
                        var parsedLines = _parser.GetResults();
                        foreach (var parsedLine in parsedLines)
                        {
                            var linesToWrite = _formater.Format(parsedLine);
                            result.Add(linesToWrite);
                        }
                    }

                    monitor.Progress = workDone / workRemaining;
                    lineIdx += 1;
                }
                catch (Exception ex)
                {
                    monitor.Log($"Error in line {lineIdx}: {line}");
                    throw;
                }
            }

            if (_parser.NeedMore)
                monitor.Log("Log incomplete");

            var targetFile = file;
            if (!options.ReplaceOldLog)
            {
                var idx = targetFile.LastIndexOf(".");
                if (idx < 0)
                    targetFile += $".{options.ConvertTo}.log";
                else
                    targetFile = targetFile.Substring(0, idx) + $".{options.ConvertTo}" + targetFile.Substring(idx);
            }

            monitor.Log($"Saving log: {targetFile}");
            File.WriteAllLines(targetFile, result, System.Text.Encoding.UTF8);
        }

        private bool HasLoggerId(string line)
        {
            return line.StartsWith("Chatlogger Id:");
        }

        private string GetLoggerId(string line)
        {
            var idx = line.IndexOf(":");
            return line.Substring(idx + 1).Trim();
        }

        private ILogParser GetParser(string id)
        {
            switch (id)
            {
                case "ACTv1":
                    return new ACTv1LogParser();

                case "FCLv1":
                    return new FCLv1LogParser();

                default:
                    return null;
            }
        }

        private ILogFormater GetFormater(string id)
        {
            switch (id)
            {
                case "ACTv1":
                    return new ACTv1Formater();

                case "FCLv1":
                    return new FCLv1Formater();

                default:
                    return null;
            }
        }
    }

    internal sealed class LogConverterOptions
    {
        public bool ReplaceOldLog { get; set; } = false;
        public string ConvertTo { get; set; } = "FCLv1";
    }

    internal sealed class Entry
    {
        public DateTime Time { get; set; }

        public ChatChannel Channel { get; set; } = ChatChannel.NONE;

        public string Source { get; set; } = "";

        public string Message { get; set; } = "";
    }

    #region parser

    internal interface ILogParser
    {
        bool NeedMore { get; }

        void Read(string line);

        IEnumerable<Entry> GetResults();
    }

    internal sealed class ACTv1LogParser : ILogParser
    {
        private List<Entry> _results = new List<Entry>();
        private readonly Regex _regex = new Regex(@"^\d{2}\|(?<time>.+)\|(?<channel>[0-9a-fA-F]+)\|(?<source>.+)?\|(?<msg>.+)?\|$");

        public bool NeedMore { get; private set; } = false;

        public IEnumerable<Entry> GetResults()
        {
            var results = _results;
            _results = new List<Entry>();
            return results;
        }

        public void Read(string line)
        {
            var match = _regex.Match(line);
            if (!match.Success)
                return;

            var timeGroup = match.Groups["time"];
            var channelGroup = match.Groups["channel"];
            var sourceGroup = match.Groups["source"];
            var messageGroup = match.Groups["msg"];

            var entry = new Entry()
            {
                Time = DateTime.ParseExact(timeGroup.Value, "o", CultureInfo.InvariantCulture),
                Channel = (ChatChannel)Int32.Parse(channelGroup.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            };

            if (sourceGroup.Success)
                entry.Source = sourceGroup.Value;
            if (messageGroup.Success)
                entry.Message = messageGroup.Value;

            _results.Add(entry);
        }
    }

    internal sealed class FCLv1LogParser : ILogParser
    {
        private List<Entry> _results = new List<Entry>();
        private Entry _entry;

        private readonly Regex _headerRegex = new Regex(@"^(?<channel>[a-zA-Z0-9_]+)\s+\[(?<time>.*)\]\s+(?<source>.*)?:$");

        public bool NeedMore { get => _entry != null; }

        public IEnumerable<Entry> GetResults()
        {
            var results = _results;
            _results = new List<Entry>();
            return results;
        }

        public void Read(string line)
        {
            if (_entry == null)
            {
                var match = _headerRegex.Match(line);
                if (!match.Success)
                    return;

                var timeGroup = match.Groups["time"];
                var channelGroup = match.Groups["channel"];
                var sourceGroup = match.Groups["source"];

                _entry = new Entry()
                {
                    Time = DateTime.ParseExact(timeGroup.Value, "yyyy'-'MM'-'dd' 'HH':'mm':'ssK", CultureInfo.InvariantCulture),
                    Channel = (ChatChannel)Enum.Parse(typeof(ChatChannel), channelGroup.Value, true)
                };

                if (sourceGroup.Success)
                    _entry.Source = sourceGroup.Value;
            }
            else
            {
                _entry.Message = line;
                _results.Add(_entry);
                _entry = null;
            }
        }
    }

    #endregion parser

    #region formater

    internal interface ILogFormater
    {
        string Format(Entry entry);
    }

    internal class FCLv1Formater : ILogFormater
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public string Format(Entry entry)
        {
            try
            {
                _builder.Append(entry.Channel).Append(" ");
                var timeConverted = entry.Time.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ssK", CultureInfo.InvariantCulture);
                _builder.Append("[").Append(timeConverted).Append("] ");
                _builder.Append(entry.Source).AppendLine(":");
                _builder.AppendLine(entry.Message);
                var formatedLine = _builder.ToString();
                return formatedLine;
            }
            finally
            {
                _builder.Clear();
            }
        }
    }

    internal class ACTv1Formater : ILogFormater
    {
        public string Format(Entry entry)
        {
            return $"00|{entry.Time.ToString("o", CultureInfo.InvariantCulture)}|{((int)entry.Channel).ToString("x4", CultureInfo.InvariantCulture)}|{entry.Source}|{entry.Message}|";
        }
    }

    #endregion formater
}