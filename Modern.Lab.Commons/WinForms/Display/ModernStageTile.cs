using System;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 단계 타일(새 개념으로, 직접 대응하는 WinForms 컨트롤 없음; WPF ModernStageTileControl 을 ElementHost 로 호스팅).
    /// 보드 윗줄에서 단계 하나를 큰 숫자 · 이름 · 한 줄 요약 · 장비 램프 · 셰브론으로 보이고, 누르면 <see cref="Click"/> 에 <see cref="Key"/> 를 돌려준다(2026-11-04).
    /// 값은 전부 폼이 넣는다 — 타일은 판정하지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernStageTile : WpfElementHostBase<ModernStageTileControl>
    {
        private string fallbackKey = string.Empty;
        private string fallbackLabel = string.Empty;
        private string fallbackCount = string.Empty;
        private string fallbackSummary = string.Empty;
        private string fallbackLampState = string.Empty;
        private string fallbackSummaryKind = string.Empty;
        private bool fallbackShowChevron = true;
        private bool fallbackSelected;

        /// <summary>타일을 누르면 발생한다 — 인자에 <see cref="Key"/>.</summary>
        public new event EventHandler<StageTileEventArgs> Click;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernStageTile()
        {
            this.Size = new Size(180, 64);

            if (this.Wpf != null)
            {
                this.Wpf.Click += this.OnInnerClick;
            }
        }

        /// <summary>단계 키 — 클릭 인자로 돌아온다.</summary>
        [Category("Modern")]
        [Description("Stage key returned by Click")]
        [DefaultValue("")]
        public string Key
        {
            get { return this.Wpf != null ? this.Wpf.Key : this.fallbackKey; }
            set { this.fallbackKey = value; if (this.Wpf != null) { this.Wpf.Key = value; } }
        }

        /// <summary>단계 이름.</summary>
        [Category("Modern")]
        [Description("Stage name")]
        [Localizable(true)]
        [DefaultValue("")]
        public string Label
        {
            get { return this.Wpf != null ? this.Wpf.Label : this.fallbackLabel; }
            set { this.fallbackLabel = value; if (this.Wpf != null) { this.Wpf.Label = value; } this.InvalidateDesignTimePreview(); }
        }

        /// <summary>큰 숫자 글자.</summary>
        [Category("Modern")]
        [Description("Big count text")]
        [DefaultValue("")]
        public string Count
        {
            get { return this.Wpf != null ? this.Wpf.Count : this.fallbackCount; }
            set { this.fallbackCount = value; if (this.Wpf != null) { this.Wpf.Count = value; } this.InvalidateDesignTimePreview(); }
        }

        /// <summary>이름 아래 한 줄 요약.</summary>
        [Category("Modern")]
        [Description("One-line summary under the name")]
        [Localizable(true)]
        [DefaultValue("")]
        public string Summary
        {
            get { return this.Wpf != null ? this.Wpf.Summary : this.fallbackSummary; }
            set { this.fallbackSummary = value; if (this.Wpf != null) { this.Wpf.Summary = value; } }
        }

        /// <summary>램프 상태 — Run · Down · Idle. 비면 램프 없음.</summary>
        [Category("Modern")]
        [Description("Lamp state — Run, Down, Idle; empty = no lamp")]
        [DefaultValue("")]
        public string LampState
        {
            get { return this.Wpf != null ? this.Wpf.LampState : this.fallbackLampState; }
            set { this.fallbackLampState = value; if (this.Wpf != null) { this.Wpf.LampState = value; } }
        }

        /// <summary>요약 글자 강조 — 비면 보조색, warning · error.</summary>
        [Category("Modern")]
        [Description("Summary emphasis — empty, warning or error")]
        [DefaultValue("")]
        public string SummaryKind
        {
            get { return this.Wpf != null ? this.Wpf.SummaryKind : this.fallbackSummaryKind; }
            set { this.fallbackSummaryKind = value; if (this.Wpf != null) { this.Wpf.SummaryKind = value; } }
        }

        /// <summary>오른쪽 셰브론 — 마지막 타일은 끈다.</summary>
        [Category("Modern")]
        [Description("Draw the chevron on the right edge (off for the last tile)")]
        [DefaultValue(true)]
        public bool ShowChevron
        {
            get { return this.Wpf != null ? this.Wpf.ShowChevron : this.fallbackShowChevron; }
            set { this.fallbackShowChevron = value; if (this.Wpf != null) { this.Wpf.ShowChevron = value; } this.InvalidateDesignTimePreview(); }
        }

        /// <summary>선택 표시(배경이 선택색).</summary>
        [Category("Modern")]
        [Description("Selected look")]
        [DefaultValue(false)]
        public bool Selected
        {
            get { return this.Wpf != null ? this.Wpf.Selected : this.fallbackSelected; }
            set { this.fallbackSelected = value; if (this.Wpf != null) { this.Wpf.Selected = value; } }
        }

        private void OnInnerClick(object sender, StageTileEventArgs e)
        {
            EventHandler<StageTileEventArgs> handler = this.Click;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
