/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Gobchat.LogConverter
{
    public sealed class LogConverter
    {
        private readonly LogConverterManager _manager;

        private ILogParser _parser;
        private ILogFormater _formater;
       
        public LogConverter(LogConverterManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

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

            _parser = _manager.GetParser(loggerId);
            _formater = _manager.GetFormater(options.ConvertTo);

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
    }

    public sealed class LogConverterManager
    {
        private readonly IDictionary<string, Func<ILogParser>> _parsers = new Dictionary<string, Func<ILogParser>>();
        private readonly IDictionary<string, Func<ILogFormater>> _formaters = new Dictionary<string, Func<ILogFormater>>();

        public LogConverterManager()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var selection = from type in assembly.GetTypes().AsParallel()
                            where type.Namespace != null && type.Namespace.StartsWith("Gobchat.LogConverter.Logs")
                            let attributes = type.GetCustomAttributes(typeof(LogAttribute), true)
                            where attributes != null && attributes.Length > 0
                            select new { Type = type, Attributes = attributes.Cast<LogAttribute>() };


            foreach (var value in selection)
            {
                if(typeof(ILogFormater).IsAssignableFrom(value.Type))
                {
                    var factory = Expression.Lambda<Func<ILogFormater>>(Expression.New(value.Type.GetConstructor(Type.EmptyTypes))).Compile();
                    foreach (var attribute in value.Attributes)
                        _formaters.Add(attribute.LoggerId, factory);                    
                }
                if(typeof(ILogParser).IsAssignableFrom(value.Type))
                {
                    var factory = Expression.Lambda<Func<ILogParser>>(Expression.New(value.Type.GetConstructor(Type.EmptyTypes))).Compile();
                    foreach (var attribute in value.Attributes)
                        _parsers.Add(attribute.LoggerId, factory);
                }
            }
        }

        public string[] GetFormaters()
        {
            return _formaters.Keys.ToArray();
        }

        public ILogFormater GetFormater(string id)
        {
            if (_formaters.TryGetValue(id, out var factory))
                return factory.Invoke();
            return null;            
        }

        public ILogParser GetParser(string id)
        {
            if (_parsers.TryGetValue(id, out var factory))
                return factory.Invoke();
            return null;
        }

    }

    public sealed class LogConverterOptions
    {
        public bool ReplaceOldLog { get; set; } = false;
        public string ConvertTo { get; set; } = "FCLv1";
    }

    public sealed class Entry
    {
        public DateTime Time { get; set; }

        public ChatChannel Channel { get; set; } = ChatChannel.None;

        public string Source { get; set; } = "";

        public string Message { get; set; } = "";
    }

   
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class LogAttribute : Attribute
    {
        public string LoggerId { get; }

        public LogAttribute(string loggerId)
        {
            LoggerId = loggerId;
        }
    }

    public interface ILogFormater
    {
        string Format(Entry entry);
    }

    public interface ILogParser
    {
        bool NeedMore { get; }

        void Read(string line);

        IEnumerable<Entry> GetResults();
    }






}