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
using System.Drawing;
using System.Windows.Forms;

namespace Gobchat.Core.Util
{
    internal class ButtonClassThingy
    {
        public static DialogResult DialogBox(string title, string promptText, ref string value, string button1 = "OK", string button2 = "Cancel", string button3 = null)
        {
            using (Form form = new Form())
            {
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button button_1 = new Button();
                Button button_2 = new Button();
                Button button_3 = new Button();

                int buttonStartPos = 228; //Standard two button position

                if (button3 != null)
                    buttonStartPos = 228 - 81;
                else
                {
                    button_3.Visible = false;
                    button_3.Enabled = false;
                }

                form.Text = title;

                // Label
                label.Text = promptText;
                label.SetBounds(9, 20, 372, 13);
                label.Font = new Font("Microsoft Tai Le", 10, FontStyle.Regular);

                // TextBox
                if (value == null)
                {
                }
                else
                {
                    textBox.Text = value;
                    textBox.SetBounds(12, 36, 372, 20);
                    textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                }

                button_1.Text = button1;
                button_2.Text = button2;
                button_3.Text = button3 ?? string.Empty;
                button_1.DialogResult = DialogResult.OK;
                button_2.DialogResult = DialogResult.Cancel;
                button_3.DialogResult = DialogResult.Yes;

                button_1.SetBounds(buttonStartPos, 72, 75, 23);
                button_2.SetBounds(buttonStartPos + 81, 72, 75, 23);
                button_3.SetBounds(buttonStartPos + (2 * 81), 72, 75, 23);

                label.AutoSize = true;
                button_1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                button_2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                button_3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, button_1, button_2 });
                if (button3 != null)
                    form.Controls.Add(button_3);
                if (value != null)
                    form.Controls.Add(textBox);

                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = button_1;
                form.CancelButton = button_2;

                DialogResult dialogResult = form.ShowDialog();
                value = textBox.Text;
                return dialogResult;
            }
        }
    }
}