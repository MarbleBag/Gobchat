using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.Core.UI
{
    public sealed class TrayIcon : IDisposable
    {
        private bool _isDisposed = false;
        private readonly NotifyIcon _icon;

        public string DefaultGroup { get; private set; }
        public IEnumerable<string> Groups { get; private set; }

        public interface ICommand
        {
            string Label { get; }
            bool IsEnabled { get; }
            bool IsVisible { get; }

            void Invoke();
        }

        public TrayIcon()
        {
            _icon = new NotifyIcon();
            _icon.ContextMenuStrip = new ContextMenuStrip();
            _icon.ContextMenuStrip.Opening += ContextMenu_Opening;
            _icon.ContextMenuStrip.Closed += ContextMenu_Closed;
        }

        public void AddCommand(string groupId, ICommand command)
        {
        }

        public void AddCommand(ICommand command)
        {
            AddCommand(DefaultGroup, command);
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs cancelEventArgs)
        {
            // var eventArgs = new TrayIconOpenCMEventArgs(Groups.ToList());
            // OnContextMenuOpen?.Invoke(eventArgs);

            // var groups =  eventArgs.Groups;
            // var commandsByGroups = ;

            /*
            if (OnContextMenuOpen != null)
            {
                foreach (var handler in OnContextMenuOpen.GetInvocationList())
                {
                    var eventArgs = new TrayIconOpenCMEventArgs();
                    handler.Method.Invoke(handler.Target, new object[] { this, eventArgs });
                }
            }
            */

            bool isFirstGroup = true;
            foreach (var group in Groups)
            {
                if (!isFirstGroup)
                    _icon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                isFirstGroup = false;

                var commands = GetCommandsForGroup(group);

                foreach (var command in commands)
                {
                    if (!command.IsVisible)
                        continue;
                    var item = new ToolStripMenuItem(command.Label);
                    item.Click += (s, e) => command.Invoke();
                    _icon.ContextMenuStrip.Items.Add(item);
                }
            }
        }

        private List<ICommand> GetCommandsForGroup(string group)
        {
            return new List<ICommand>(); //TODO
        }

        private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            var items = (sender as ContextMenuStrip).Items;
            foreach (var item in items)
            {
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }
            items.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;

            _icon.Dispose();
        }

        public event EventHandler<TrayIconOpenCMEventArgs> OnContextMenuOpen;

        public sealed class TrayIconOpenCMEventArgs : EventArgs
        {
            public void AddItem()
            {
            }
        }
    }
}