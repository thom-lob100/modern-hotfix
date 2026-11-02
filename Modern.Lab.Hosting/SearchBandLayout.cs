using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.Hosting
{
    /// <summary>
    /// 조회 띠 배치 규격 — 업무 화면의 조회 카드 안 라벨 · 편집기 · 버튼을 한 규격으로 놓는다(2026-10-30, 사용자
    /// "조회조건이 올려놓으면 꽤 간격을 정확히 맞춰도 뭔가 정렬되어 있는 느낌이 아닌 경우가 많아"). 리포트 틀
    /// (<c>Modern.Lab.Report</c>)의 조회 띠와 같은 규격이다.
    ///
    /// - 라벨은 한 줄 왼쪽 정렬이고 같은 띠의 라벨은 같은 폭(가장 긴 글자에 맞추되 최소 88)이다. 띠 단위로 오른쪽 정렬을 고를 수 있다
    ///   (<see cref="Attach(Control, IList{Field}, IList{Control}, HorizontalAlignment)"/> — Lot Management, 2026-11-02 사용자 선택: 라벨이 제 칸 안에서 편집기에 붙는다). 위·아래 작은 글씨를 둔 라벨
    ///   (사용자 지정 예외 — Lot Management, 2026-11-02)은 글자가 다 보이는 높이(<c>MeasureTextHeight</c>)로 편집기 줄 가운데에 놓는다.
    ///   라벨 앞에 체크박스를 둔 조건(<see cref="Pair(Control, Control, Control)"/> — 켜면 그 조건을 쓴다, Lot Management 의 Create Date)은
    ///   체크박스가 라벨 칸 왼쪽 끝에 서고 라벨은 그 뒤(체크 폭 + 8)부터 같은 칸 안에 놓인다.
    /// - 라벨–편집기 8, 조건 사이 24, 줄 간격 40, 편집기 높이 32. 편집기 폭은 화면이 디자이너에 둔 값 그대로다.
    ///   높이가 32 가 아닌 것(배지 · 체크박스)은 줄 가운데에 놓는다.
    /// - 오른쪽 버튼은 같은 폭 120 으로 한 줄(사이 8)에 조건 첫 줄과 같은 높이에 서고, 마지막 버튼이 카드 오른쪽 끝이다.
    /// - 폭이 모자라면 다음 줄로 내려가고 카드 높이가 줄 수를 따라 자란다(카드 여백 + 줄 수 × 40). 버튼은 첫 줄만
    ///   막으므로 둘째 줄부터는 카드 오른쪽 끝까지 쓴다.
    /// - 숨긴 조건(자동 새로고침 배지처럼 때에 따라 보이는 것)도 자리를 지킨다 — 보였다 숨을 때 다른 조건이 뛰지 않게.
    ///
    /// 화면은 생성자에서 <see cref="Attach"/> 한 줄만 적는다. 첫 배치는 폼 Load 때(배율 보정이 끝난 뒤)이고
    /// 그 뒤는 카드 크기가 바뀔 때마다 다시 놓는다. 디자이너의 좌표는 그대로 두므로 디자이너 화면과 실행 화면이
    /// 다를 수 있다 — 조건의 순서는 <see cref="Attach"/> 에 넘긴 목록이 정한다.
    /// </summary>
    public static class SearchBandLayout
    {
        public const int LabelMinWidth = 88;
        public const int LabelGap = 8;
        public const int FieldGap = 24;
        public const int RowHeight = 40;
        public const int FieldHeight = 32;
        public const int ButtonWidth = 120;
        public const int ButtonGap = 8;

        private const int RowInset = 4;
        private const int LabelSlack = 4;

        /// <summary>조건 하나 — 라벨 앞 체크박스(없으면 null) · 라벨(없으면 null) · 편집기.</summary>
        public sealed class Field
        {
            private readonly Control check;
            private readonly Control label;
            private readonly Control editor;

            public Field(Control label, Control editor) : this(null, label, editor)
            {
            }

            public Field(Control check, Control label, Control editor)
            {
                if (editor == null)
                {
                    throw new ArgumentNullException("editor");
                }

                if (check != null && label == null)
                {
                    throw new ArgumentException("A check box needs a label to stand in front of.", "label");
                }

                this.check = check;
                this.label = label;
                this.editor = editor;
            }

            public Control Check
            {
                get { return this.check; }
            }

            public Control Label
            {
                get { return this.label; }
            }

            public Control Editor
            {
                get { return this.editor; }
            }
        }

        /// <summary>라벨이 있는 조건.</summary>
        public static Field Pair(Control label, Control editor)
        {
            return new Field(label, editor);
        }

        /// <summary>라벨 앞에 체크박스가 서는 조건 — 켜면 그 조건을 쓴다(Create Date). 체크박스는 글자 없이 상자만 둔다.</summary>
        public static Field Pair(Control check, Control label, Control editor)
        {
            if (check == null)
            {
                throw new ArgumentNullException("check");
            }

            return new Field(check, label, editor);
        }

        /// <summary>라벨 없이 편집기만 있는 조건(체크박스 · 배지).</summary>
        public static Field Alone(Control editor)
        {
            return new Field(null, editor);
        }

        /// <summary>
        /// 조회 카드에 규격을 붙인다. <paramref name="fields"/> 의 순서가 조건의 순서이고, <paramref name="buttons"/> 는
        /// 오른쪽에 한 줄로 왼쪽부터 차례로 놓인다(예: Reset · Refresh).
        /// </summary>
        public static void Attach(Control band, IList<Field> fields, IList<Control> buttons)
        {
            Attach(band, fields, buttons, HorizontalAlignment.Left);
        }

        /// <summary>라벨 글자 정렬을 고르는 형 — 위·아래 작은 글씨도 같은 쪽으로 맞춘다.</summary>
        public static void Attach(Control band, IList<Field> fields, IList<Control> buttons, HorizontalAlignment labelAlignment)
        {
            if (band == null)
            {
                throw new ArgumentNullException("band");
            }

            Binding binding = new Binding(band, fields ?? new Field[0], buttons ?? new Control[0], labelAlignment);
            binding.Start();
        }

        private sealed class Binding
        {
            private readonly Control band;
            private readonly IList<Field> fields;
            private readonly IList<Control> buttons;
            private readonly HorizontalAlignment labelAlignment;
            private bool started;
            private bool laying;

            internal Binding(Control band, IList<Field> fields, IList<Control> buttons, HorizontalAlignment labelAlignment)
            {
                this.band = band;
                this.fields = fields;
                this.buttons = buttons;
                this.labelAlignment = labelAlignment;
            }

            internal void Start()
            {
                foreach (Field field in this.fields)
                {
                    if (field.Check != null)
                    {
                        field.Check.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    }

                    if (field.Label != null)
                    {
                        field.Label.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    }

                    field.Editor.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                }

                foreach (Control button in this.buttons)
                {
                    button.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                }

                Form form = this.band.FindForm();

                if (form != null && !form.Visible)
                {
                    form.Load += this.OnFormLoad;
                    return;
                }

                this.Begin();
            }

            private void OnFormLoad(object sender, EventArgs e)
            {
                ((Form)sender).Load -= this.OnFormLoad;
                this.Begin();
            }

            private void Begin()
            {
                if (this.started)
                {
                    return;
                }

                this.started = true;
                this.band.Resize += this.OnBandResize;
                this.Layout();
            }

            private void OnBandResize(object sender, EventArgs e)
            {
                this.Layout();
            }

            private void Layout()
            {
                if (this.laying || this.band.IsDisposed)
                {
                    return;
                }

                this.laying = true;

                try
                {
                    this.LayoutCore();
                }
                finally
                {
                    this.laying = false;
                }
            }

            private void LayoutCore()
            {
                Padding inset = this.band.Padding;
                int left = inset.Left;
                int top = inset.Top + this.Px(RowInset);
                int right = this.band.ClientSize.Width - inset.Right;
                int rowHeight = this.Px(RowHeight);
                int fieldHeight = this.Px(FieldHeight);
                int labelWidth = this.LabelColumnWidth();

                int buttonCount = this.buttons.Count;
                int buttonRows = buttonCount > 0 ? 1 : 0;
                int buttonsWidth = buttonCount == 0
                        ? 0
                        : buttonCount * this.Px(ButtonWidth) + (buttonCount - 1) * this.Px(ButtonGap);
                int fieldsRight = buttonsWidth == 0 ? right : right - buttonsWidth - this.Px(FieldGap);

                int x = left;
                int y = top;
                int rows = 1;
                int rowRight = RowRight(rows, buttonRows, fieldsRight, right);

                foreach (Field field in this.fields)
                {
                    int editorWidth = field.Editor.Width;
                    int width = (field.Label != null ? labelWidth + this.Px(LabelGap) : 0) + editorWidth;

                    if (x > left && x + width > rowRight)
                    {
                        x = left;
                        y += rowHeight;
                        rows++;
                        rowRight = RowRight(rows, buttonRows, fieldsRight, right);
                    }

                    if (field.Label != null)
                    {
                        ModernLabel modernLabel = field.Label as ModernLabel;
                        int labelHeight = fieldHeight;
                        int labelLeft = x;
                        int labelOnlyWidth = labelWidth;

                        if (modernLabel != null)
                        {
                            modernLabel.TextAlignment = this.labelAlignment;
                            modernLabel.TopLabelAlignment = this.labelAlignment;
                            modernLabel.BottomLabelAlignment = this.labelAlignment;
                            labelHeight = Math.Max(fieldHeight, modernLabel.MeasureTextHeight());
                        }

                        if (field.Check != null)
                        {
                            int checkWidth = field.Check.Width;
                            int checkHeight = field.Check.Height;
                            int checkLeft = x;

                            // 오른쪽 정렬이면 체크박스도 글자 바로 왼쪽에 붙는다 — 글자 폭을 재서 그만큼 뒤로 민다.
                            if (this.labelAlignment == HorizontalAlignment.Right && modernLabel != null)
                            {
                                int textWidth = Math.Min(modernLabel.MeasureTextWidth(), labelWidth - checkWidth - this.Px(LabelGap));
                                checkLeft = Math.Max(x, x + labelWidth - textWidth - this.Px(LabelGap) - checkWidth);
                            }

                            field.Check.SetBounds(checkLeft, y + (fieldHeight - checkHeight) / 2, checkWidth, checkHeight);
                            labelLeft = checkLeft + checkWidth + this.Px(LabelGap);
                            labelOnlyWidth = Math.Max(0, x + labelWidth - labelLeft);
                        }

                        field.Label.SetBounds(labelLeft, y + (fieldHeight - labelHeight) / 2, labelOnlyWidth, labelHeight);
                        x += labelWidth + this.Px(LabelGap);
                    }

                    int editorHeight = field.Editor.Height;
                    field.Editor.SetBounds(x, y + (fieldHeight - editorHeight) / 2, editorWidth, editorHeight);
                    x += editorWidth + this.Px(FieldGap);
                }

                for (int index = 0; index < buttonCount; index++)
                {
                    int buttonLeft = right - buttonsWidth + index * (this.Px(ButtonWidth) + this.Px(ButtonGap));
                    this.buttons[index].SetBounds(buttonLeft, top, this.Px(ButtonWidth), fieldHeight);
                }

                int totalRows = Math.Max(rows, buttonRows);
                int height = inset.Vertical + totalRows * rowHeight;

                if (this.band.Height != height)
                {
                    this.band.Height = height;
                }
            }

            // 버튼은 첫 줄만 막는다 — 둘째 줄부터는 카드 오른쪽 끝까지 쓴다.
            private static int RowRight(int row, int buttonRows, int fieldsRight, int right)
            {
                return row <= buttonRows ? fieldsRight : right;
            }

            // 같은 띠의 라벨은 같은 폭 — 가장 긴 글자에 맞추되 최소 88 이다.
            private int LabelColumnWidth()
            {
                int width = this.Px(LabelMinWidth);

                foreach (Field field in this.fields)
                {
                    if (field.Label == null || string.IsNullOrEmpty(field.Label.Text))
                    {
                        continue;
                    }

                    Size measured = TextRenderer.MeasureText(field.Label.Text, field.Label.Font);
                    int lead = field.Check == null ? 0 : field.Check.Width + this.Px(LabelGap);
                    width = Math.Max(width, lead + measured.Width + this.Px(LabelSlack));
                }

                return width;
            }

            private int Px(int logical)
            {
                return ScreenDpi.Scale(this.band, logical);
            }
        }
    }
}
