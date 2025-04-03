/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Module.Language.Internal
{
    internal sealed class MessageManager
    {
        private IDictionary<Type, object> _map;

        public M GetMessages<M>()
        {
            if (_map.TryGetValue(typeof(M), out var stored))
                return (M)stored;
            var msg = CreateMessages<M>();
            _map.Add(typeof(M), msg);
            return msg;
        }

        public void PopulateMessages(object msg)
        {
            var type = msg.GetType();
            //TODO
            type.GetCustomAttribute(typeof(MessageAttribute));
        }

        private M CreateMessages<M>()
        {
            M msg = default;

            //TODO make class
            PopulateMessages(msg);
            return msg;
        }

        public MessageRegistry<M> CreateRegistry<M>(M messages)
        {
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class MessagesAttribute : Attribute
    {
        public Uri Uri { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class MessageAttribute : Attribute
    {
        public string Key { get; set; }
    }

    [Messages]
    public sealed class SomeLocalizedMessages
    {
        [Message(Key = "ui.tray.app.close")]
        public readonly string ui_tray_app_close;

        public readonly string ui_tray_app_hide;
        public readonly string ui_tray_app_reload;
    }

    public delegate void MessageChanged<M>(M message);

    public interface IMessageConsumer
    {
        void Consume(string msg);
    }

    public interface IMessageProducer
    {
        string Produce();
    }

    public interface IMessageRegistry<M> : IDisposable
    {
        event MessageChanged<M> OnMessagesChanged;

        M Messages { get; }

        void Register(Action<string> consumer, Func<M, string> supplier);

        void Register(Action<string> consumer, string key);

        IMessageProducer CreateProducer(string key);
    }

    public sealed class MessageChangedEventArgs<M> : EventArgs
    {
        public M Messages { get; }
    }

    public class MessageRegistry<M> : IMessageRegistry<M>
    {
        private sealed class MessageConsumerImpl1 : IMessageConsumer
        {
            private readonly MessageRegistry<M> _registry;
            private readonly Action<string> _consumer;

            public MessageConsumerImpl1(MessageRegistry<M> registry, Action<string> consumer)
            {
                this._registry = registry ?? throw new ArgumentNullException(nameof(registry));
                this._consumer = consumer ?? throw new ArgumentNullException(nameof(registry));
            }

            public void Consume(string msg)
            {
                try
                {
                    _consumer(msg);
                }
                catch (Exception ex)
                {
                    _registry.RemoveConsumer(this);
                }
            }
        }

        private sealed class MessageProducerImpl1 : IMessageProducer
        {
            private readonly MessageRegistry<M> _registry;
            private readonly Func<M, string> _supplier;

            public MessageProducerImpl1(MessageRegistry<M> registry, Func<M, string> supplier)
            {
                this._registry = registry ?? throw new ArgumentNullException(nameof(registry));
                this._supplier = supplier ?? throw new ArgumentNullException(nameof(supplier));
            }

            public string Produce()
            {
                try
                {
                    return _supplier(_registry.Messages);
                }
                catch (Exception ex)
                {
                    _registry.RemoveProducer(this);
                    return null;
                }
            }
        }

        private sealed class MessageProducerImpl2 : IMessageProducer
        {
            private readonly MessageRegistry<M> _registry;
            private readonly FieldInfo _field;

            public MessageProducerImpl2(MessageRegistry<M> registry, FieldInfo field)
            {
                this._registry = registry ?? throw new ArgumentNullException(nameof(registry));
                this._field = field ?? throw new ArgumentNullException(nameof(field));
            }

            public string Produce()
            {
                try
                {
                    return (string)_field.GetValue(_registry.Messages);
                }
                catch (Exception ex)
                {
                    _registry.RemoveProducer(this);
                    return null;
                }
            }
        }

        private readonly IDictionary<IMessageConsumer, IMessageProducer> _store;

        private event MessageChanged<M> _messageEvent;

        public event MessageChanged<M> OnMessagesChanged
        {
            add
            {
                _messageEvent += value;
                value(message: Messages);
            }
            remove
            {
                _messageEvent -= value;
            }
        }

        public M Messages { get; private set; }

        public void Register(Action<string> consumer, Func<M, string> supplier)
        {
            Register(new MessageConsumerImpl1(this, consumer), new MessageProducerImpl1(this, supplier));
        }

        public void Register(Action<string> consumer, string key)
        {
            var producer = CreateProducer(key);
            if (producer == null)
                throw new ArgumentNullException($"key not found: {key}");
            Register(new MessageConsumerImpl1(this, consumer), producer);
        }

        public void UpdateMessages(M messages)
        {
            this.Messages = messages;
            foreach (var key in _store.Keys.ToList())
                key.Consume(_store[key].Produce());
        }

        protected void Register(IMessageConsumer consumer, IMessageProducer producer)
        {
            consumer.Consume(producer.Produce());
            _store.Add(consumer, producer);
        }

        public IMessageProducer CreateProducer(string key)
        {
            var type = typeof(M);
            var field = type.GetField(key);
            if (field == null)
                return null;
            return new MessageProducerImpl2(this, field);
        }

        private void RemoveProducer(IMessageProducer producer)
        {
            foreach (var key in _store.Keys.ToList())
            {
                if (Object.ReferenceEquals(_store[key], producer))
                    _store.Remove(key);
            }
        }

        private void RemoveConsumer(IMessageConsumer consumer)
        {
            _store.Remove(consumer);
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _store.Clear();
                    _messageEvent = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MessageRegistry()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
    }
}