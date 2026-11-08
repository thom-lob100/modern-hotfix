using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Input;

namespace Modern.Lab.Hosting.Reporting
{
    internal sealed class ReportConditionDrawer : Form
    {
        private const int MaxVisibleRows = 10;
        private const int AnchorGap = 4;
        private const int PaddingX = 12;
        private const int PaddingY = 8;
        private const int SuppressStaleMs = 1000;

        private readonly ReportFormBase report;
        private readonly List<ReportCondition> conditions;
        private readonly Panel body;
        private Control anchor;
        private bool suppressNextToggle;
        private int suppressSetAt;

        internal ReportConditionDrawer(ReportFormBase report, List<ReportCondition> conditions)
        {
            this.report = report;
            this.conditions = conditions;

            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.AutoScaleMode = AutoScaleMode.None;
            this.KeyPreview = true;
            this.Font = report.Font;
            this.BackColor = ModernTheme.Surface;
            this.Padding = new Padding(1);

            this.body = new Panel();
            this.body.Dock = DockStyle.Fill;
            this.body.AutoScroll = true;
            this.Controls.Add(this.body);

            this.LayoutRows();
        }

        internal void ShowBelow(Control anchorControl, ReportCondition focus)
        {
            this.anchor = anchorControl;

            Point below = anchorControl.PointToScreen(new Point(anchorControl.Width, anchorControl.Height + this.Px(AnchorGap)));
            Rectangle area = Screen.FromControl(anchorControl).WorkingArea;
            int x = Math.Max(area.Left, below.X - this.Width);
            int y = below.Y + this.Height > area.Bottom ? Math.Max(area.Top, area.Bottom - this.Height) : below.Y;
            this.Location = new Point(x, y);

            if (this.Visible)
            {
                this.Activate();
            }
            else
            {
                this.Show(anchorControl.TopLevelControl as Form);
            }

            ReportCondition target = focus ?? (this.conditions.Count > 0 ? this.conditions[0] : null);

            if (target != null)
            {
                this.body.ScrollControlIntoView(target.Editor);
                target.FocusEditor();
            }
        }

        internal void HideDrawer(bool activateOwner)
        {
            if (!this.Visible)
            {
                return;
            }

            this.Hide();

            if (activateOwner && this.Owner != null)
            {
                this.Owner.Activate();
            }
        }

        internal void HideFromOutside()
        {
            if (!this.Visible)
            {
                return;
            }

            if (this.anchor != null && this.anchor.Visible && this.anchor.ClientRectangle.Contains(this.anchor.PointToClient(Cursor.Position)))
            {
                this.suppressNextToggle = true;
                this.suppressSetAt = Environment.TickCount;
            }

            this.Hide();
        }

        internal bool ConsumeSuppressedToggle()
        {
            bool suppressed = this.suppressNextToggle && Environment.TickCount - this.suppressSetAt < SuppressStaleMs;
            this.suppressNextToggle = false;
            return suppressed;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(this.HideIfAnotherFormIsActive));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.HideDrawer(true);
                return true;
            }

            if (keyData == Keys.Enter && DeepActiveControl(this) is ModernDateRangePicker)
            {
                this.report.OnConditionEnter();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen pen = new Pen(ModernTheme.Border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void HideIfAnotherFormIsActive()
        {
            Form active = Form.ActiveForm;

            if (active != null && active != this)
            {
                this.HideFromOutside();
            }
        }

        private void LayoutRows()
        {
            int y = this.Px(PaddingY);
            int editorWidth = 0;

            foreach (ReportCondition condition in this.conditions)
            {
                condition.Label.SetBounds(this.Px(PaddingX), y, this.Px(ReportFormBase.LabelWidth), this.Px(ReportFormBase.FieldHeight));
                condition.Editor.SetBounds(
                        this.Px(PaddingX + ReportFormBase.LabelWidth + ReportFormBase.LabelGap),
                        y,
                        this.Px(condition.EditorWidth),
                        this.Px(ReportFormBase.FieldHeight));
                this.body.Controls.Add(condition.Label);
                this.body.Controls.Add(condition.Editor);
                editorWidth = Math.Max(editorWidth, condition.EditorWidth);
                y += this.Px(ReportFormBase.RowHeight);
            }

            int rows = this.conditions.Count;
            int visibleRows = Math.Min(rows, MaxVisibleRows);
            int width = this.Px(PaddingX * 2 + ReportFormBase.LabelWidth + ReportFormBase.LabelGap + editorWidth) + this.Padding.Horizontal;

            if (rows > MaxVisibleRows)
            {
                width += SystemInformation.VerticalScrollBarWidth;
            }

            this.body.AutoScrollMinSize = new Size(0, y + this.Px(PaddingY) - this.Px(ReportFormBase.RowHeight - ReportFormBase.FieldHeight));
            this.ClientSize = new Size(width, this.Px(PaddingY * 2) + visibleRows * this.Px(ReportFormBase.RowHeight) - this.Px(ReportFormBase.RowHeight - ReportFormBase.FieldHeight) + this.Padding.Vertical);
        }

        private int Px(int logical)
        {
            return Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Scale(this, logical);
        }

        private static Control DeepActiveControl(ContainerControl container)
        {
            Control active = container.ActiveControl;

            while (active is ContainerControl && ((ContainerControl)active).ActiveControl != null)
            {
                active = ((ContainerControl)active).ActiveControl;
            }

            return active;
        }
    }
}
