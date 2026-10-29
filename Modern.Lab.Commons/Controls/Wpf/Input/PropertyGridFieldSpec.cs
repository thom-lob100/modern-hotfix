namespace Modern.Lab.Controls.Wpf.Input
{
    /// <summary>
    /// 기준정보 편집기의 입력칸 하나를 적는 명세 — 컬럼 이름 · 입력 종류 · 라벨(위·본·아래) · 키 여부 · 콤보/숫자 설정.
    /// 호스트(상위 Common 의 <c>ModernPropertyGrid</c>)가 응답 컬럼마다 하나씩 만들어
    /// <see cref="ModernPropertyGridControl.SetFields"/> 에 넘긴다. 값·읽기 전용·필수는 명세가 아니라 컨트롤의 메서드로 바꾼다.
    /// </summary>
    public sealed class PropertyGridFieldSpec
    {
        /// <summary>컬럼 이름으로 명세를 만든다. 종류는 텍스트, 라벨은 비어 있다.</summary>
        public PropertyGridFieldSpec(string column)
        {
            this.Column = column ?? string.Empty;
            this.Kind = PropertyEditorKind.Text;
            this.LabelText = string.Empty;
            this.TopLabel = string.Empty;
            this.BottomLabel = string.Empty;
            this.ValueMember = "CODE";
            this.DisplayMember = "NAME";
        }

        /// <summary>응답 컬럼 이름 — 값을 읽고 쓰는 키다.</summary>
        public string Column { get; private set; }

        /// <summary>입력 종류.</summary>
        public PropertyEditorKind Kind { get; set; }

        /// <summary>라벨 본문.</summary>
        public string LabelText { get; set; }

        /// <summary>라벨 본문 위의 작은 글씨 — 비면 줄이 사라진다.</summary>
        public string TopLabel { get; set; }

        /// <summary>라벨 본문 아래의 작은 글씨 — 비면 줄이 사라진다.</summary>
        public string BottomLabel { get; set; }

        /// <summary>키 컬럼인가 — 표시만을 위한 정보다(잠금은 호스트가 <see cref="ModernPropertyGridControl.SetReadOnly"/> 로 정한다).</summary>
        public bool IsKey { get; set; }

        /// <summary>숫자 입력의 소수 자릿수.</summary>
        public int DecimalPlaces { get; set; }

        /// <summary>콤보 항목의 저장 값 컬럼.</summary>
        public string ValueMember { get; set; }

        /// <summary>콤보 항목의 표시 글자 컬럼.</summary>
        public string DisplayMember { get; set; }
    }
}
