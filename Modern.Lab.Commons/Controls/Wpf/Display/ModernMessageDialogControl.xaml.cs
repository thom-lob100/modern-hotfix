using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 메시지 다이얼로그의 <b>본문</b> — 종류별 색 아이콘 + 제목 + 본문이다.
    /// 창·버튼·결과 처리는 호스트 폼
    /// (<c>Modern.Lab.WinForms.Controls.Dialogs.ModernMessageDialog</c>)이 담당하고
    /// 이 컨트롤은 내용만 그린다(토스트와 같은 분담).
    ///
    /// 본문을 읽기 전용 <see cref="RichTextBox"/>로 둔 이유는 둘이다 — <b>긴 서버 오류
    /// 문구를 드래그해 복사</b>할 수 있어야 하고(<see cref="TextBlock"/>은 선택이 안 된다),
    /// <c>**단어**</c>로 감싼 구간을 <b>액센트색 SemiBold로 강조</b>해 "무엇으로 바꾸는가"가
    /// 한눈에 보여야 하기 때문이다(2026-08-29). 테두리·배경을 없애서 보기에는 글자만 남는다.
    /// </summary>
    public partial class ModernMessageDialogControl : UserControl
    {
        private const string CopyGlyphText = "";
        private const string CopiedGlyphText = "";

        internal event EventHandler CopyRequested;

        /// <summary>제목 (비어 있으면 본문만 표시).</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ModernMessageDialogControl),
                new PropertyMetadata(string.Empty, OnTitleChanged));

        /// <summary>
        /// 본문 — 여러 줄·긴 문구를 그대로 넣는다(줄바꿈·스크롤·선택 가능).
        /// <see cref="EmphasisMarker"/>(<c>**</c>)로 감싼 구간은 액센트 SemiBold로 그린다.
        /// 한 줄을 탭 문자로 나누면(<c>라벨&lt;탭&gt;값</c>) 앞이 라벨, 뒤가 값이고, 그런 줄이 이어지면 값이 같은 세로선에서 시작한다(2026-11-15).
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "Message",
                typeof(string),
                typeof(ModernMessageDialogControl),
                new PropertyMetadata(string.Empty, OnMessageChanged));

        /// <summary>강조 구간 표기 — 이 문자열로 감싼 낱말이 액센트 SemiBold로 그려진다.</summary>
        public const string EmphasisMarker = "**";

        // 라벨 · 값 구분 표기 — 본문의 한 줄을 이 문자(탭)로 나누면 앞이 라벨, 뒤가 값이고, 그런 줄이 이어지면 값이 같은 세로선에서 시작한다.
        private const char FieldSeparator = '\t';

        /// <summary>메시지 종류 (아이콘·색 결정).</summary>
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(
                "Kind",
                typeof(ModernMessageKind),
                typeof(ModernMessageDialogControl),
                new PropertyMetadata(ModernMessageKind.Information));

        /// <summary>컨트롤을 생성한다.</summary>
        public ModernMessageDialogControl()
        {
            this.InitializeComponent();
            ApplyTitleVisibility(this);
            this.BuildDocument();
        }

        /// <summary>
        /// 강조 표기를 걷어낸 순수 본문 — 복사·높이 계산·로그가 쓴다
        /// ("Change A to **B**?" → "Change A to B?").
        /// </summary>
        public static string StripEmphasis(string message)
        {
            return (message ?? string.Empty).Replace(EmphasisMarker, string.Empty);
        }

        internal void ShowCopyConfirmation()
        {
            this.CopyGlyph.Text = CopiedGlyphText;
            Brush accent = this.TryFindResource("Brush.Accent") as Brush;

            if (accent != null)
            {
                this.CopyGlyph.Foreground = accent;
            }
        }

        internal void ResetCopyConfirmation()
        {
            this.CopyGlyph.Text = CopyGlyphText;
            Brush foreground = this.TryFindResource("Brush.TextSecondary") as Brush;

            if (foreground != null)
            {
                this.CopyGlyph.Foreground = foreground;
            }
        }

        private void OnCopyButtonClick(object sender, RoutedEventArgs e)
        {
            EventHandler handler = this.CopyRequested;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private static void OnMessageChanged(
                DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ModernMessageDialogControl control = source as ModernMessageDialogControl;

            if (control != null)
            {
                control.BuildDocument();
            }
        }

        // Message → FlowDocument. 마커 사이는 강조 Run(액센트·SemiBold), 그 밖은 일반 Run.
        // 줄바꿈은 LineBreak로 옮긴다. 마커가 짝이 안 맞으면 남은 꼬리는 일반 글자다.
        // 탭이 있는 줄("라벨<탭>값")이 이어지면 그 묶음은 두 칸 표로 그린다 — 라벨 칸은 묶음에서 가장 긴 라벨의 폭이라
        // 값이 같은 세로선에서 시작한다(2026-11-15). 탭이 없는 본문은 예전과 같은 문단 하나다.
        private void BuildDocument()
        {
            FlowDocument document = new FlowDocument();
            document.PagePadding = new Thickness(0d);
            Brush accent = this.TryFindResource("Brush.Accent") as Brush;
            List<List<MessagePiece>> lines = SplitLines(this.Message ?? string.Empty);
            int index = 0;

            while (index < lines.Count)
            {
                bool fields = HasFieldSeparator(lines[index]);
                int end = index;

                while (end < lines.Count && HasFieldSeparator(lines[end]) == fields)
                {
                    end++;
                }

                if (fields)
                {
                    document.Blocks.Add(this.BuildFieldTable(lines, index, end, accent));
                }
                else
                {
                    document.Blocks.Add(BuildParagraph(lines, index, end, accent));
                }

                index = end;
            }

            this.MessageBox.Document = document;
        }

        // 본문을 줄마다 조각 목록으로 나눈다 — 강조 여부는 본문 전체에서 마커 짝으로 정한다(줄을 넘는 강조도 그대로다).
        private static List<List<MessagePiece>> SplitLines(string message)
        {
            List<List<MessagePiece>> lines = new List<List<MessagePiece>>();
            List<MessagePiece> current = new List<MessagePiece>();
            lines.Add(current);
            string[] pieces = message.Split(new string[] { EmphasisMarker }, StringSplitOptions.None);

            for (int index = 0; index < pieces.Length; index++)
            {
                bool emphasized = index % 2 == 1 && index < pieces.Length - 1;
                string[] parts = pieces[index].Replace("\r\n", "\n").Split('\n');

                for (int part = 0; part < parts.Length; part++)
                {
                    if (part > 0)
                    {
                        current = new List<MessagePiece>();
                        lines.Add(current);
                    }

                    if (parts[part].Length > 0)
                    {
                        current.Add(new MessagePiece(parts[part], emphasized));
                    }
                }
            }

            return lines;
        }

        private static bool HasFieldSeparator(List<MessagePiece> line)
        {
            foreach (MessagePiece piece in line)
            {
                if (piece.Text.IndexOf(FieldSeparator) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Paragraph BuildParagraph(List<List<MessagePiece>> lines, int start, int end, Brush accent)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0d);

            for (int index = start; index < end; index++)
            {
                if (index > start)
                {
                    paragraph.Inlines.Add(new LineBreak());
                }

                AppendPieces(paragraph, lines[index], accent);
            }

            return paragraph;
        }

        // 라벨 · 값 두 칸 표 — 첫 탭 앞이 라벨, 뒤가 값이다. 라벨 칸 폭은 이 묶음의 가장 긴 라벨 + 간격이고 값 칸이 남은 폭을 쓴다
        // (긴 값은 값 칸 안에서 줄이 바뀐다). 표 · 칸 · 문단의 여백을 모두 0 으로 두어 줄 간격이 문단과 같다.
        // 값 칸에는 폭을 주지 않는다 — 폭이 없는 칸이 남은 폭을 받고, 비율 폭(Star)을 주면 RichTextBox 안에서 값 칸이 한 글자 폭으로 줄어든다.
        private Table BuildFieldTable(List<List<MessagePiece>> lines, int start, int end, Brush accent)
        {
            Table table = new Table();
            table.Margin = new Thickness(0d);
            table.CellSpacing = 0d;
            TableColumn labels = new TableColumn();
            table.Columns.Add(labels);
            table.Columns.Add(new TableColumn());
            TableRowGroup group = new TableRowGroup();
            table.RowGroups.Add(group);
            double widest = 0d;

            for (int index = start; index < end; index++)
            {
                List<MessagePiece> label = new List<MessagePiece>();
                List<MessagePiece> value = new List<MessagePiece>();
                SplitField(lines[index], label, value);
                widest = Math.Max(widest, this.MeasurePieces(label));
                TableRow row = new TableRow();
                row.Cells.Add(BuildFieldCell(label, accent));
                row.Cells.Add(BuildFieldCell(value, accent));
                group.Rows.Add(row);
            }

            object gap = this.TryFindResource("Space.Md");
            labels.Width = new GridLength(Math.Ceiling(widest) + (gap is double ? (double)gap : 12d));
            return table;
        }

        private static void SplitField(List<MessagePiece> line, List<MessagePiece> label, List<MessagePiece> value)
        {
            bool separated = false;

            foreach (MessagePiece piece in line)
            {
                int tab = separated ? -1 : piece.Text.IndexOf(FieldSeparator);

                if (separated)
                {
                    value.Add(piece);
                    continue;
                }

                if (tab < 0)
                {
                    label.Add(piece);
                    continue;
                }

                separated = true;

                if (tab > 0)
                {
                    label.Add(new MessagePiece(piece.Text.Substring(0, tab), piece.Emphasized));
                }

                if (tab < piece.Text.Length - 1)
                {
                    value.Add(new MessagePiece(piece.Text.Substring(tab + 1), piece.Emphasized));
                }
            }
        }

        private static TableCell BuildFieldCell(List<MessagePiece> pieces, Brush accent)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Margin = new Thickness(0d);
            AppendPieces(paragraph, pieces, accent);
            TableCell cell = new TableCell(paragraph);
            cell.Padding = new Thickness(0d);
            return cell;
        }

        // 라벨 한 줄의 폭 — 본문 글꼴로 조각마다 재서 더한다(강조 조각은 SemiBold).
        private double MeasurePieces(List<MessagePiece> pieces)
        {
            double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            double width = 0d;

            foreach (MessagePiece piece in pieces)
            {
                Typeface typeface = new Typeface(
                        this.MessageBox.FontFamily,
                        FontStyles.Normal,
                        piece.Emphasized ? FontWeights.SemiBold : this.MessageBox.FontWeight,
                        FontStretches.Normal);
                FormattedText formatted = new FormattedText(
                        piece.Text,
                        CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight,
                        typeface,
                        this.MessageBox.FontSize,
                        Brushes.Black,
                        pixelsPerDip);
                width += formatted.WidthIncludingTrailingWhitespace;
            }

            return width;
        }

        private static void AppendPieces(Paragraph paragraph, List<MessagePiece> pieces, Brush accent)
        {
            foreach (MessagePiece piece in pieces)
            {
                Run run = new Run(piece.Text);

                if (piece.Emphasized)
                {
                    run.FontWeight = FontWeights.SemiBold;

                    if (accent != null)
                    {
                        run.Foreground = accent;
                    }
                }

                paragraph.Inlines.Add(run);
            }
        }

        // 본문 한 줄 안의 글자 조각 — 강조 구간인지 함께 든다.
        private sealed class MessagePiece
        {
            public MessagePiece(string text, bool emphasized)
            {
                this.Text = text;
                this.Emphasized = emphasized;
            }

            public string Text { get; private set; }

            public bool Emphasized { get; private set; }
        }

        /// <summary>제목.</summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>본문.</summary>
        public string Message
        {
            get { return (string)this.GetValue(MessageProperty); }
            set { this.SetValue(MessageProperty, value); }
        }

        /// <summary>메시지 종류.</summary>
        public ModernMessageKind Kind
        {
            get { return (ModernMessageKind)this.GetValue(KindProperty); }
            set { this.SetValue(KindProperty, value); }
        }

        // 제목이 비면 줄을 접는다 — 빈 제목이 본문 위에 여백만 남기지 않게 한다.
        private static void OnTitleChanged(
                DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            ApplyTitleVisibility(source as ModernMessageDialogControl);
        }

        private static void ApplyTitleVisibility(ModernMessageDialogControl control)
        {
            if (control == null || control.TitleText == null)
            {
                return;
            }

            control.TitleText.Visibility = string.IsNullOrEmpty(control.Title)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
    }
}
