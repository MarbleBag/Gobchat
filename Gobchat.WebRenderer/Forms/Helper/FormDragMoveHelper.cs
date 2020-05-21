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
using System.Drawing;
using System.Windows.Forms;

namespace Gobchat.UI.Forms.Helper
{
    internal sealed class FormDragMoveHelper : IDisposable
    {
        private bool _mouseDown;
        private bool _noficiationSent;
        private Point _lastLocation;

        public Form TargetForm { get; set; }
        public bool AllowToMove { get; set; }
        public bool IsMoving { get { return _mouseDown && AllowToMove; } }

        public event System.EventHandler<FormMoveEventArgs> FormMove;

        public FormDragMoveHelper(Form target)
        {
            this.TargetForm = target;
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            _mouseDown = false;
            _noficiationSent = false;
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            _mouseDown = true;
            _noficiationSent = false;
            _lastLocation = e.Location;
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseDown && AllowToMove)
            {
                var newPosition = new Point((TargetForm.Location.X - _lastLocation.X) + e.X, (TargetForm.Location.Y - _lastLocation.Y) + e.Y);

                if (!_noficiationSent)
                {
                    _noficiationSent = true;
                    FormMove?.Invoke(e, new FormMoveEventArgs(newPosition, TargetForm));
                }

                TargetForm.Location = newPosition;
                TargetForm.Update();
            }
        }

        public void Dispose()
        {
            TargetForm = null;
            FormMove = null;
        }

        public sealed class FormMoveEventArgs : System.EventArgs
        {
            public Point NewPosition { get; }
            public Form TargetForm { get; }

            public FormMoveEventArgs(Point newPosition, Form targetForm)
            {
                NewPosition = newPosition;
                TargetForm = targetForm;
            }
        }
    }
}