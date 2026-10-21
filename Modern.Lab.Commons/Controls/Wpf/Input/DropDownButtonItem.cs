using System;

namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>ModernDropDownButtonControl의 메뉴 항목 하나를 나타내는 UI 항목 모델.</summary>
    public class DropDownButtonItem
    {
        public DropDownButtonItem(object value, string displayText, bool isEnabled)
            : this(value, displayText, isEnabled, string.Empty, string.Empty, false)
        {
        }

        /// <param name="glyph">항목 왼쪽에 그릴 글리프(Segoe MDL2 Assets 문자). 비면 그 항목만 비운다.</param>
        /// <param name="accentText">표시 텍스트의 끝 단어 중 강조색(토큰 Brush.ErrorText)으로 그릴 글자 —
        /// 컨텍스트 메뉴 ModernMenuItem.AccentText 와 같은 규칙. 끝 단어가 아니면 무시한다.</param>
        /// <param name="showGlyph">글리프 칸을 비우더라도 자리를 두는가 — 컨트롤에 글리프 컬럼이 지정돼 있으면 참.</param>
        public DropDownButtonItem(
                object value, string displayText, bool isEnabled, string glyph, string accentText, bool showGlyph)
        {
            this.Value = value;
            this.DisplayText = displayText;
            this.IsEnabled = isEnabled;

            // WinForms ToolStrip 관례 — 표시 텍스트 "-"는 항목이 아니라
            // 구분선이다 (컨텍스트 메뉴의 ToolStripSeparator와 같은 의미).
            this.IsSeparator = displayText == "-";
            this.Glyph = glyph ?? string.Empty;
            this.ShowGlyph = showGlyph;

            string text = displayText ?? string.Empty;
            string accent = accentText ?? string.Empty;
            bool accented = accent.Length > 0 && text.Length > accent.Length
                    && text.EndsWith(accent, StringComparison.Ordinal);
            this.AccentText = accented ? accent : string.Empty;
            this.PrefixText = accented ? text.Substring(0, text.Length - accent.Length) : text;
        }

        /// <summary>항목 값 (ValueMemberPath 기준).</summary>
        public object Value { get; private set; }

        /// <summary>메뉴에 표시되는 텍스트.</summary>
        public string DisplayText { get; private set; }

        /// <summary>항목 실행 가능 여부 (EnabledMemberPath 기준) — 비활성이면
        /// 회색으로 표시되고 클릭되지 않는다.</summary>
        public bool IsEnabled { get; private set; }

        /// <summary>구분선 여부 — 표시 텍스트가 "-"인 행. 클릭되지 않고
        /// 가는 가로선으로 그려진다.</summary>
        public bool IsSeparator { get; private set; }

        /// <summary>항목 왼쪽 글리프 (GlyphMemberPath 기준). 비면 그리지 않는다.</summary>
        public string Glyph { get; private set; }

        /// <summary>글리프 칸의 자리를 두는가 — 컨트롤에 GlyphMemberPath 가 있으면 모든 항목이 참이라 글자 시작이 맞는다.</summary>
        public bool ShowGlyph { get; private set; }

        /// <summary>표시 텍스트에서 강조 단어를 뺀 앞부분 — 강조가 없으면 표시 텍스트 전체.</summary>
        public string PrefixText { get; private set; }

        /// <summary>강조색으로 그리는 끝 단어 (AccentTextMemberPath 기준). 없으면 빈 글자.</summary>
        public string AccentText { get; private set; }
    }
}
