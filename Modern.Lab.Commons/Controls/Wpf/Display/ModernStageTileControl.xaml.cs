using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 단계 타일 — 보드 윗줄에서 단계 하나를 큰 숫자 · 단계 이름 · 한 줄 요약 · 장비 램프로 보이고 오른쪽 끝 셰브론으로 다음 단계와 잇는다(2026-11-04, Lab Monitor).
    /// 값은 전부 폼이 넣는다(데이터 무관). 누르면 <see cref="Click"/> 에 <see cref="Key"/> 를 돌려준다. 색 · 글리프는 디자인 토큰에서만 온다.
    /// </summary>
    public partial class ModernStageTileControl : UserControl
    {
        /// <summary>램프 상태 — 작업 중.</summary>
        public const string LampRun = "Run";
        /// <summary>램프 상태 — 멈춤.</summary>
        public const string LampDown = "Down";
        /// <summary>램프 상태 — 쉼.</summary>
        public const string LampIdle = "Idle";

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(ModernStageTileControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ModernStageTileControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(string), typeof(ModernStageTileControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SummaryProperty =
            DependencyProperty.Register("Summary", typeof(string), typeof(ModernStageTileControl), new PropertyMetadata(string.Empty));

        /// <summary>램프 상태 — Run(초록) · Down(빨강) · Idle(회색). 비면 램프를 그리지 않는다.</summary>
        public static readonly DependencyProperty LampStateProperty =
            DependencyProperty.Register("LampState", typeof(string), typeof(ModernStageTileControl), new PropertyMetadata(string.Empty, OnLookChanged));

        /// <summary>요약 글자의 강조 — 비면 보조색, warning 이면 경고색, error 면 오류색.</summary>
        public static readonly DependencyProperty SummaryKindProperty =
            DependencyProperty.Register("SummaryKind", typeof(string), typeof(ModernStageTileControl), new PropertyMetadata(string.Empty, OnLookChanged));

        public static readonly DependencyProperty ShowChevronProperty =
            DependencyProperty.Register("ShowChevron", typeof(bool), typeof(ModernStageTileControl), new PropertyMetadata(true));

        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(bool), typeof(ModernStageTileControl), new PropertyMetadata(false, OnLookChanged));

        /// <summary>타일을 누르면 발생한다 — 인자에 <see cref="Key"/>.</summary>
        public event EventHandler<StageTileEventArgs> Click;

        public ModernStageTileControl()
        {
            this.InitializeComponent();
            this.ApplyLook();
        }

        public string Key
        {
            get { return (string)this.GetValue(KeyProperty); }
            set { this.SetValue(KeyProperty, value); }
        }

        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        public string Count
        {
            get { return (string)this.GetValue(CountProperty); }
            set { this.SetValue(CountProperty, value); }
        }

        public string Summary
        {
            get { return (string)this.GetValue(SummaryProperty); }
            set { this.SetValue(SummaryProperty, value); }
        }

        public string LampState
        {
            get { return (string)this.GetValue(LampStateProperty); }
            set { this.SetValue(LampStateProperty, value); }
        }

        public string SummaryKind
        {
            get { return (string)this.GetValue(SummaryKindProperty); }
            set { this.SetValue(SummaryKindProperty, value); }
        }

        public bool ShowChevron
        {
            get { return (bool)this.GetValue(ShowChevronProperty); }
            set { this.SetValue(ShowChevronProperty, value); }
        }

        public bool Selected
        {
            get { return (bool)this.GetValue(SelectedProperty); }
            set { this.SetValue(SelectedProperty, value); }
        }

        private static void OnLookChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernStageTileControl)d).ApplyLook();
        }

        // 램프 · 요약 색 · 선택 배경을 토큰에서 다시 고른다.
        private void ApplyLook()
        {
            string lamp = (this.LampState ?? string.Empty).Trim();

            if (lamp.Length == 0)
            {
                this.Lamp.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Lamp.Visibility = Visibility.Visible;
                this.Lamp.Fill = lamp.Equals(LampRun, StringComparison.OrdinalIgnoreCase) ? this.Token("Brush.Success")
                        : lamp.Equals(LampDown, StringComparison.OrdinalIgnoreCase) ? this.Token("Brush.ErrorBorder")
                        : this.Token("Brush.NeutralText");
            }

            string kind = (this.SummaryKind ?? string.Empty).Trim().ToLowerInvariant();
            this.SummaryText.Foreground = kind == "error" ? this.Token("Brush.ErrorText")
                    : kind == "warning" ? this.Token("Brush.WarningText")
                    : this.Token("Brush.TextSecondary");

            this.TileRoot.Background = this.Selected ? this.Token("Brush.SelectedBackground") : this.Token("Brush.Surface");
        }

        private Brush Token(string key)
        {
            Brush brush = this.TryFindResource(key) as Brush;
            return brush ?? Brushes.Transparent;
        }

        private void OnTileMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EventHandler<StageTileEventArgs> handler = this.Click;

            if (handler != null)
            {
                handler(this, new StageTileEventArgs(this.Key));
            }
        }
    }
}
