/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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
using System.Windows.Forms;

namespace Gobchat.LogConverter
{
    public sealed class LogConverter
    {
        private readonly LogConverterManager _manager;

        private string _file;
        private ProgressMonitorAdapter _monitor;

        private LogParserContainer _parserContainer;
        private LogFormaterContainer _formaterContainer;

        private IList<string> _result;

        public LogConverter(LogConverterManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public IEnumerable<string> ConvertLog(string file, LogFormaterContainer formater, ProgressMonitorAdapter monitor)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _formaterContainer = formater ?? throw new ArgumentNullException(nameof(formater));

            try
            {
                ConvertLog();
                var result = _result;
                return result;
            }
            finally
            {
                _file = null;
                _monitor = null;
                _parserContainer = null;
                _formaterContainer = null;
                _result = null;
            }
        }

        private void ConvertLog()
        {
            _result = new List<string>();
            _monitor.Progress = 0;

            var lines = ReadFile();
            if (lines.Length == 0)
                return;

            _result.Add($"Chatlogger Id: {_formaterContainer.Id}");
            _parserContainer = _manager.GetParser("ACTv1"); //act logs don't have an id, so this will be the default parser.

            for (var lineIdx = 0; lineIdx < lines.Length; ++lineIdx)
            {
                _monitor.Progress = lineIdx / lines.Length;
                var line = lines[lineIdx];

                try
                {
                    if (CheckForParseCommands(line))
                        continue;

                    if (_parserContainer == null)
                        continue;                    

                    var parser = _parserContainer.Parser;
                    var formater = _formaterContainer.Formater;

                    parser.Read(line);

                    if (!parser.NeedMore)                    
                        foreach (var parsedLine in parser.GetResults().Select(e => formater.Format(e)).SelectMany(e => e))
                            _result.Add(parsedLine);                    

                    if (parser.ReparseLines > 0)
                        lineIdx = Math.Max(0, lineIdx - parser.ReparseLines);
                }
                catch (Exception ex)
                {
                    _monitor.Log($"Error in line {lineIdx}: {line}");
                    _monitor.Log(ex.Message);
                    throw;
                }
            }

            if (_parserContainer.Parser.NeedMore)
                _monitor.Log("Log incomplete");

            _monitor.Progress = 1;
        }

        private string[] ReadFile()
        {
            _monitor.Progress = 0;
            _monitor.Log("Read file");
            var fileLines = File.ReadAllLines(_file);
            _monitor.Log($"{fileLines.Length} lines read");
            if (fileLines.Length == 0)            
                _monitor.Log("File empty");
            return fileLines;
        }

        private bool CheckForParseCommands(string line)
        {
            if (line.StartsWith("Chatlogger Id:"))
            {
                var parserId = line.Substring(line.IndexOf(":") + 1).Trim();
                _parserContainer = _manager.GetParser(parserId);
                if (_parserContainer == null)                
                    _monitor.Log($"No parser for chatlogger id: {parserId}");
                else
                    _monitor.Log($"Converting from {_formaterContainer.Id} to {_parserContainer.Id} ...");
                return true;
            }

            return false;
        }
    }

    public sealed class LogConverterManager
    {
        private readonly IDictionary<string, Func<IParser>> _parsers = new Dictionary<string, Func<IParser>>();
        private readonly IDictionary<string, Func<IFormater>> _formaters = new Dictionary<string, Func<IFormater>>();

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
                if(typeof(IFormater).IsAssignableFrom(value.Type))
                {
                    var factory = Expression.Lambda<Func<IFormater>>(Expression.New(value.Type.GetConstructor(Type.EmptyTypes))).Compile();
                    foreach (var attribute in value.Attributes)
                        _formaters.Add(attribute.LoggerId, factory);                    
                }
                if(typeof(IParser).IsAssignableFrom(value.Type))
                {
                    var factory = Expression.Lambda<Func<IParser>>(Expression.New(value.Type.GetConstructor(Type.EmptyTypes))).Compile();
                    foreach (var attribute in value.Attributes)
                        _parsers.Add(attribute.LoggerId, factory);
                }
            }
        }

        public string[] GetFormaters()
        {
            return _formaters.Keys.ToArray();
        }

        public LogFormaterContainer GetFormater(string id)
        {
            if (_formaters.TryGetValue(id, out var factory))
            {
                var formater = factory.Invoke();
                return new LogFormaterContainer()
                {
                    Formater = formater,
                    Id = id,
                    Settings = (formater as IFormaterWithSettings)?.GetSettingForm()
                };
            }
            return null;            
        }

        public LogParserContainer GetParser(string id)
        {
            if (_parsers.TryGetValue(id, out var factory))
            {
                return new LogParserContainer()
                {
                    Parser = factory.Invoke(),
                    Id = id
                };
            }
            return null;
        }

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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LogSettingAttribute : Attribute
    {
        public Type Control { get; }
        public LogSettingAttribute(Type control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
        }
    }

    public interface IFormater
    {
        IEnumerable<string> Format(Entry entry);
    }

    public interface IFormaterWithSettings : IFormater
    {
        System.Windows.Forms.Control GetSettingForm();
    }

    public sealed class LogFormaterContainer
    {
        public IFormater Formater { get; internal set; }
        public string Id { get; internal set; }
        public Control Settings { get; internal set; }
    }

    public interface IParser
    {
        bool NeedMore { get; }

        int ReparseLines { get; }

        void Read(string line);

        IEnumerable<Entry> GetResults();
    }

    public sealed class LogParserContainer
    {
        public IParser Parser { get; internal set; }

        public string Id { get; internal set; }
    }






}