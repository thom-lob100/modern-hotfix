using System;
using System.Collections.Generic;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// <see cref="ModernToolTip"/>이 그리는 호버 카드의 내용 — 제목 한 줄, 라벨 · 값 줄 몇 개, 맨 아래 안내 한 줄.
    /// 글자 한 줄로는 좁은 칩(자식 장비 등)의 상태를 설명하려고 만들었다(2026-10-22 사용자 요청).
    ///
    /// 색은 전부 토큰 키로 받는다(<c>"Brush.Success"</c> 처럼) — 카드가 뜨는 순간 테마 사전에서 읽으므로
    /// 테마가 무엇이든 맞는 색이 나온다. 값에 색 키를 주지 않으면 본문색이다.
    /// </summary>
    public sealed class ModernToolTipCard
    {
        private readonly List<string> labels = new List<string>();
        private readonly List<string> values = new List<string>();
        private readonly List<string> valueColorTokens = new List<string>();
        private string title = string.Empty;
        private string accentColorToken = string.Empty;
        private string note = string.Empty;

        /// <summary>맨 위 제목(굵게). 비면 제목 줄을 그리지 않는다.</summary>
        public string Title
        {
            get { return this.title; }
            set { this.title = value ?? string.Empty; }
        }

        /// <summary>왼쪽 세로 띠의 색 토큰 키(예: <c>"Brush.InfoText"</c>). 비면 띠가 없다.</summary>
        public string AccentColorToken
        {
            get { return this.accentColorToken; }
            set { this.accentColorToken = value ?? string.Empty; }
        }

        /// <summary>맨 아래 안내 한 줄(보조색) — 꺼진 사유 등. 비면 그리지 않는다.</summary>
        public string Note
        {
            get { return this.note; }
            set { this.note = value ?? string.Empty; }
        }

        /// <summary>라벨 · 값 줄의 수.</summary>
        public int RowCount
        {
            get { return this.labels.Count; }
        }

        /// <summary>라벨 · 값 줄을 하나 더한다. 값은 본문색이다.</summary>
        public ModernToolTipCard AddRow(string label, string value)
        {
            return this.AddRow(label, value, null);
        }

        /// <summary>라벨 · 값 줄을 하나 더한다. 값 글자는 <paramref name="valueColorToken"/> 토큰색(비면 본문색).</summary>
        public ModernToolTipCard AddRow(string label, string value, string valueColorToken)
        {
            this.labels.Add(label ?? string.Empty);
            this.values.Add(value ?? string.Empty);
            this.valueColorTokens.Add(valueColorToken ?? string.Empty);
            return this;
        }

        /// <summary>줄의 라벨.</summary>
        public string LabelAt(int index)
        {
            return this.labels[index];
        }

        /// <summary>줄의 값.</summary>
        public string ValueAt(int index)
        {
            return this.values[index];
        }

        /// <summary>줄 값의 색 토큰 키(비면 본문색).</summary>
        public string ValueColorTokenAt(int index)
        {
            return this.valueColorTokens[index];
        }

        /// <summary>툴팁을 띄우는 열쇠가 되는 한 줄 글자 — 제목이 있으면 제목, 없으면 첫 값, 그것도 없으면 안내.</summary>
        internal string TriggerText()
        {
            if (this.title.Length > 0)
            {
                return this.title;
            }

            if (this.values.Count > 0 && this.values[0].Length > 0)
            {
                return this.values[0];
            }

            return this.note.Length > 0 ? this.note : " ";
        }
    }
}
