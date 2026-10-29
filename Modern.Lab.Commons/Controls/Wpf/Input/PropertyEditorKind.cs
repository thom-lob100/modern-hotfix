namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>기준정보 편집기 입력칸의 종류 — <see cref="ModernPropertyGridControl"/> 가 컬럼마다 어떤 입력 컨트롤을 놓을지 정한다.</summary>
    public enum PropertyEditorKind
    {
        /// <summary>한 줄 텍스트(기본).</summary>
        Text,

        /// <summary>여러 줄 텍스트 — 한 행을 통째로 차지한다.</summary>
        MultilineText,

        /// <summary>숫자 입력.</summary>
        Number,

        /// <summary>날짜 선택(값은 yyyy-MM-dd).</summary>
        Date,

        /// <summary>Y/N 스위치.</summary>
        Toggle,

        /// <summary>콤보박스 — 항목은 호스트가 <see cref="ModernPropertyGridControl.BindComboItems"/> 로 싣는다.</summary>
        Combo
    }
}
