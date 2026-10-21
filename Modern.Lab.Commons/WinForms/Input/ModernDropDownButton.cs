using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 드롭다운 버튼 (Button + ContextMenuStrip 조합을 대체;
    /// WPF ModernDropDownButtonControl을 ElementHost로 호스팅).
    ///
    /// 버튼을 클릭하면 메뉴가 열리고, 항목 클릭 시 ItemClicked가 발생한다.
    /// 메뉴 항목은 코드/명칭 계약(DataSource/DisplayMember/ValueMember)으로
    /// 할당한다 — 엑셀 내보내기 옵션처럼 한 버튼에 여러 동작을 묶을 때 사용.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("ItemClicked")]
    public class ModernDropDownButton : WpfElementHostBase<Modern.Lab.Controls.Wpf.Input.ModernDropDownButtonControl>
    {
        private object dataSource;

        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackText;
        private string fallbackDisplayMember;
        private string fallbackValueMember;
        private string fallbackEnabledMember;
        private string fallbackGlyphMember = string.Empty;
        private string fallbackAccentTextMember = string.Empty;
        private string fallbackIconGlyph = string.Empty;
        private Modern.Lab.Controls.Wpf.Input.ButtonKind fallbackKind;
        private string fallbackTopLabel = string.Empty;
        private string fallbackBottomLabel = string.Empty;
        private HorizontalAlignment fallbackTopLabelAlignment = HorizontalAlignment.Center;
        private HorizontalAlignment fallbackBottomLabelAlignment = HorizontalAlignment.Center;

        /// <summary>메뉴 항목이 클릭될 때 발생한다.</summary>
        public event EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs> ItemClicked;

        /// <summary>비활성(회색) 항목을 눌렀을 때 발생한다 — 항목은 실행되지 않고 메뉴만 닫힌다.
        /// 폼 베이스가 "왜 닫혔는지" 안내 창을 여는 데 쓴다. 구분선은 올리지 않는다.</summary>
        public event EventHandler<Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs> DisabledItemClicked;

        /// <summary>버튼 시각 종류 — ModernButton과 같은 문법
        /// (Secondary 기본, Execute = Success 초록 채움).</summary>
        [Category("Modern")]
        [Description("Button visual kind (Secondary default, Execute = green fill)")]
        [DefaultValue(Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary)]
        public Modern.Lab.Controls.Wpf.Input.ButtonKind Kind
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Kind;
                }

                return this.fallbackKind;
            }
            set
            {
                this.fallbackKind = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Kind = value;
                }
            }
        }

        /// <summary>캡션 위에 붙는 아주 작은 상단 라벨(비어 있으면 숨김) — ModernButton 의 TopLabel 과 같은 규칙.
        /// 값이 있으면 버튼이 두 줄이 되므로 컨트롤 높이를 그만큼 잡아 준다.</summary>
        [Category("Modern")]
        [Description("Small label above the caption (hidden when empty)")]
        [DefaultValue("")]
        public string TopLabel
        {
            get
            {
                return this.Wpf != null ? this.Wpf.TopLabel : this.fallbackTopLabel;
            }
            set
            {
                this.fallbackTopLabel = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.TopLabel = this.fallbackTopLabel;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>캡션 아래에 붙는 아주 작은 하단 라벨(비어 있으면 숨김). 상단 라벨과 같은 규칙이고 함께 쓸 수 있다.</summary>
        [Category("Modern")]
        [Description("Small label below the caption (hidden when empty)")]
        [DefaultValue("")]
        public string BottomLabel
        {
            get
            {
                return this.Wpf != null ? this.Wpf.BottomLabel : this.fallbackBottomLabel;
            }
            set
            {
                this.fallbackBottomLabel = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.BottomLabel = this.fallbackBottomLabel;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>상단 라벨의 가로 정렬(기본 Center). Left·Right 는 캡션 덩어리의 왼쪽·오른쪽 끝에 맞춘다.</summary>
        [Category("Modern")]
        [Description("Horizontal alignment of the top label")]
        [DefaultValue(HorizontalAlignment.Center)]
        public HorizontalAlignment TopLabelAlignment
        {
            get
            {
                return this.Wpf != null ? ToWinForms(this.Wpf.TopLabelAlignment) : this.fallbackTopLabelAlignment;
            }
            set
            {
                this.fallbackTopLabelAlignment = value;

                if (this.Wpf != null)
                {
                    this.Wpf.TopLabelAlignment = ToWpf(value);
                }
            }
        }

        /// <summary>하단 라벨의 가로 정렬(기본 Center).</summary>
        [Category("Modern")]
        [Description("Horizontal alignment of the bottom label")]
        [DefaultValue(HorizontalAlignment.Center)]
        public HorizontalAlignment BottomLabelAlignment
        {
            get
            {
                return this.Wpf != null ? ToWinForms(this.Wpf.BottomLabelAlignment) : this.fallbackBottomLabelAlignment;
            }
            set
            {
                this.fallbackBottomLabelAlignment = value;

                if (this.Wpf != null)
                {
                    this.Wpf.BottomLabelAlignment = ToWpf(value);
                }
            }
        }

        /// <summary>메뉴 팝업이 현재 열려 있는지 여부.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDropDownOpen
        {
            get { return this.Wpf != null && this.Wpf.IsDropDownOpen; }
        }

        /// <summary>캡션 앞에 표시되는 아이콘 글리프(Segoe MDL2 Assets) — ModernButton.IconGlyph 와 같은 규칙. 비우면 숨긴다.</summary>
        [Category("Modern")]
        [Description("Icon glyph before the button text (Segoe MDL2 Assets)")]
        [DefaultValue("")]
        public string IconGlyph
        {
            get
            {
                return this.Wpf != null ? this.Wpf.IconGlyph : this.fallbackIconGlyph;
            }
            set
            {
                this.fallbackIconGlyph = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.IconGlyph = this.fallbackIconGlyph;
                }
            }
        }

        /// <summary>
        /// 항목 왼쪽 글리프로 쓸 컬럼/속성 이름(Segoe MDL2 Assets 문자 — <see cref="Modern.Lab.WinForms.Rendering.ModernMenuGlyphs"/> 상수).
        /// 비우면 글리프 칸이 없고, 지정하면 값이 빈 항목도 칸을 비워 두어 글자 시작이 맞는다 — 컨텍스트 메뉴의 글리프 여백과 같은 뜻.
        /// </summary>
        [Category("Modern")]
        [Description("Column/property name for the glyph drawn left of each item (empty = no glyph column)")]
        [DefaultValue("")]
        public string GlyphMember
        {
            get
            {
                return this.Wpf != null ? this.Wpf.GlyphMemberPath : this.fallbackGlyphMember;
            }
            set
            {
                this.fallbackGlyphMember = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.GlyphMemberPath = this.fallbackGlyphMember;
                }
            }
        }

        /// <summary>
        /// 강조색(토큰 Brush.ErrorText, 빨강)으로 그릴 끝 단어의 컬럼/속성 이름 — 값이 항목 글자의 끝 단어와 같을 때만 그 단어를
        /// 빨갛게 그린다. 컨텍스트 메뉴 <see cref="Modern.Lab.WinForms.Rendering.ModernMenuItem.AccentText"/> 와 같은 규칙(… Cancel).
        /// </summary>
        [Category("Modern")]
        [Description("Column/property name for the trailing word drawn in the error color (empty = none)")]
        [DefaultValue("")]
        public string AccentTextMember
        {
            get
            {
                return this.Wpf != null ? this.Wpf.AccentTextMemberPath : this.fallbackAccentTextMember;
            }
            set
            {
                this.fallbackAccentTextMember = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.AccentTextMemberPath = this.fallbackAccentTextMember;
                }
            }
        }

        /// <summary>
        /// 항목 실행 가능 여부로 쓸 컬럼/속성 이름 (bool 또는 "Y"/"true"/"1").
        /// 비우면 모든 항목이 활성이다. 비활성 항목은 회색으로 표시되고
        /// 클릭되지 않는다 — 컨텍스트 메뉴의 비활성 표시와 같은 의미.
        /// </summary>
        [Category("Modern")]
        [Description("Column/property name for per-item enabled state (empty = all enabled)")]
        [DefaultValue("")]
        public string EnabledMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.EnabledMemberPath;
                }

                return this.fallbackEnabledMember;
            }
            set
            {
                this.fallbackEnabledMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.EnabledMemberPath = value;
                }
            }
        }

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernDropDownButton()
        {
            this.Size = new Size(110, 32);
            this.fallbackText = "Menu";
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackEnabledMember = string.Empty;
            this.fallbackKind = Modern.Lab.Controls.Wpf.Input.ButtonKind.Secondary;

            if (this.Wpf != null)
            {
                this.Wpf.Text = this.fallbackText;
                this.Wpf.ItemClicked += this.OnWpfItemClicked;
                this.Wpf.DisabledItemClicked += this.OnWpfDisabledItemClicked;
            }
        }

        /// <summary>버튼 캡션 (셰브런은 자동으로 붙는다).</summary>
        [Category("Modern")]
        [Description("Text shown on the button")]
        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue("Menu")]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Text;
                }

                return this.fallbackText;
            }
            set
            {
                this.fallbackText = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Text = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 메뉴 항목 목록: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// null이면 메뉴를 비운다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);
                }
            }
        }

        /// <summary>메뉴 표시 텍스트로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("Modern")]
        [Description("Column/property name used as the menu display text")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.DisplayMemberPath;
                }

                return this.fallbackDisplayMember;
            }
            set
            {
                this.fallbackDisplayMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.DisplayMemberPath = value;
                }
            }
        }

        /// <summary>항목 값으로 사용할 컬럼/속성 이름(WinForms 호환 이름).</summary>
        [Category("Modern")]
        [Description("Column/property name used as the item value")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ValueMemberPath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ValueMemberPath = value;
                }
            }
        }

        // WinForms 정렬 값 ↔ WPF 정렬 값 — ModernButton 의 TopLabelAlignment 와 같은 세 값만 쓴다.
        private static System.Windows.HorizontalAlignment ToWpf(HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    return System.Windows.HorizontalAlignment.Left;
                case HorizontalAlignment.Right:
                    return System.Windows.HorizontalAlignment.Right;
                default:
                    return System.Windows.HorizontalAlignment.Center;
            }
        }

        private static HorizontalAlignment ToWinForms(System.Windows.HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case System.Windows.HorizontalAlignment.Left:
                    return HorizontalAlignment.Left;
                case System.Windows.HorizontalAlignment.Right:
                    return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Center;
            }
        }

        private void OnWpfItemClicked(object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            if (this.ItemClicked != null)
            {
                this.ItemClicked(this, e);
            }
        }

        private void OnWpfDisabledItemClicked(object sender, Modern.Lab.Controls.Wpf.Input.DropDownItemClickedEventArgs e)
        {
            if (this.DisabledItemClicked != null)
            {
                this.DisabledItemClicked(this, e);
            }
        }
    }
}
