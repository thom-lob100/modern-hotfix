using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 소형 통계 카드.
    /// - Title: 숫자 위에 표시되는 캡션 (예: "조회 건수")
    /// - Value: 강조할 숫자/텍스트
    /// - TopLabel: 값 바로 위의 작은 글씨(10 크기, 보조 색 — 라벨·버튼·그리드 헤더와 같은 규칙) — 비면 숨김.
    /// - BottomLabel: 값 바로 아래의 작은 글씨(예: 공정·설비, TopLabel 과 같은 크기·색) — 비면 숨김.
    /// - Tone: 카드 색조("#RRGGBB") — 그 색을 표면 쪽으로 살짝 섞어(원색 비중 85%)
    ///   배경을 만든다. Palette 색은 이미 파스텔이라 결과도 연하다. 카드 여러 장을
    ///   나란히 둘 때 자리를 구분하는 용도이고,
    ///   색에 의미를 싣지 않는다(의미 있는 상태색은 SemanticColors 몫).
    /// - TrendValues: 값 오른쪽에 붙는 초미니 추세선(스파크라인) 데이터 —
    ///   2개 미만이면 숨김. 축/눈금 없이 잉크만으로 추세를 전달한다.
    /// </summary>
    public partial class ModernKpiCardControl : UserControl
    {
        /// <summary>값 위에 표시되는 캡션.</summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata("Title"));

        /// <summary>강조 표시되는 값 텍스트.</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata("0", OnValueChanged));

        /// <summary>
        /// 값이 바뀔 때 값 글자 위로 밝은 띠가 왼쪽에서 오른쪽으로 한 번 지나가게 할지(기본 false).
        /// 값에서 다른 값으로 바뀔 때만 재생한다 — 빈 값("-" 같은 자리 표시 포함)에서 처음 채워질 때와
        /// 같은 값으로 다시 대입될 때는 재생하지 않는다.
        /// </summary>
        public static readonly DependencyProperty HighlightValueChangeProperty =
            DependencyProperty.Register(
                "HighlightValueChange",
                typeof(bool),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(false));

        /// <summary>값 바로 위의 작은 글씨. 빈 문자열이면 숨긴다.</summary>
        public static readonly DependencyProperty TopLabelProperty =
            DependencyProperty.Register(
                "TopLabel",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(string.Empty));

        /// <summary>값 아래 한 줄로 붙는 보조 문구. 빈 문자열이면 숨긴다.</summary>
        public static readonly DependencyProperty BottomLabelProperty =
            DependencyProperty.Register(
                "BottomLabel",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(string.Empty));

        /// <summary>카드 색조("#RRGGBB"). 빈 문자열이면 기본 카드 표면 그대로.</summary>
        public static readonly DependencyProperty ToneProperty =
            DependencyProperty.Register(
                "Tone",
                typeof(string),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(string.Empty, OnToneSourceChanged));

        /// <summary>공용 카드 패널 위에서 쓰도록 카드 크롬(테두리/배경/패딩)을 제거한다.</summary>
        public static readonly DependencyProperty FlatProperty =
            DependencyProperty.Register(
                "Flat",
                typeof(bool),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(false, OnToneSourceChanged));

        /// <summary>스파크라인 데이터 — 숫자 시퀀스(2개 미만이면 스파크라인 숨김).</summary>
        public static readonly DependencyProperty TrendValuesProperty =
            DependencyProperty.Register(
                "TrendValues",
                typeof(IEnumerable),
                typeof(ModernKpiCardControl),
                new PropertyMetadata(null, OnTrendValuesChanged));

        // 색조가 표면 쪽으로 섞이는 비율(1 = 표면 그대로). 0.15 — 원색을 85% 남긴다.
        //
        // "85%면 너무 진한 것 아닌가"는 오해다. Palette가 주는 색은 이미 HSL 명도 0.92의
        // 파스텔이라, 85%를 남겨도 배경은 #E0F0FC 수준으로 연하고 값 글자 대비는 14:1을
        // 넘는다. 반대로 이 값을 키우면 이 속성의 유일한 목적인 "카드 여러 장의 자리 구분"이
        // 먼저 사라진다 — 색조 없는 흰 카드와의 대비가 원색 85%에서 1.095, 45%에서 1.052,
        // 14%에서 1.018까지 떨어진다(1.000 = 구분 불가. Light 팔레트 4색 실측 2026-08-31).
        // 그래서 진하기는 눈이 아니라 이 구분 대비가 정한다.
        //
        // 테두리는 색조를 중간 회색 쪽으로 45% 섞어(원색 55%) 배경과 뚜렷이 다른 색으로
        // 둔다 — 카드 경계가 읽히게 하는 것이 목적이다. Palette 파스텔에서는 배경보다
        // 진하게 나오지만(밝기 차 +10~15), 회색보다 어두운 색조를 넣으면 오히려 연해진다
        // (#3A7BD5 → -23). "언제나 진하다"가 아니라 "언제나 구분된다"가 이 값의 계약이고,
        // 계약 검사가 그 둘을 나눠서 지킨다. 진한 색조를 넣었을 때 글자색이 따라가지
        // 않는 것과 같은 갈래의 한계다(별도 과제).
        private const double surfaceMix = 0.15d;
        private const double borderMix = 0.45d;

        // 제목·작은 글씨가 색조 배경 위에서 읽히는 최소 대비 — 보조 글자(UI 요소) 기준 3:1. 라이트 테마의 보조 회색은
        // 파스텔 위에서 약 4.2:1 이라 그대로 두고, CrimsonGray(약 1.3:1)만 유도한다.
        private const double minimumCaptionContrast = 3d;

        // 값 변경 강조 띠 — 폭(글자 폭 대비 비율)과 한 번 지나가는 시간.
        private const double sweepBandWidth = 0.45d;
        private const double sweepMilliseconds = 700d;

        // 띠가 지나가는 횟수, 띠가 끝난 뒤 글자가 원래 색으로 돌아오는 시간.
        private const double sweepPasses = 2d;
        private const double settleMilliseconds = 600d;

        // 띠의 밝기(액센트를 흰색 쪽으로 섞는 비율).
        private const double sweepShineMix = 0.8d;
        // 강조 재생 전의 값 글자 전경 — 재생이 끝나면 이 브러시로 되돌린다.
        private Brush valueRestBrush;

        public ModernKpiCardControl()
        {
            this.InitializeComponent();
        }

        /// <summary>값 위에 표시되는 캡션.</summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>강조 표시되는 값 텍스트.</summary>
        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>공용 카드 패널 위에서 쓰도록 카드 크롬을 제거한다.</summary>
        public bool Flat
        {
            get { return (bool)this.GetValue(FlatProperty); }
            set { this.SetValue(FlatProperty, value); }
        }

        /// <summary>값 바로 위의 작은 글씨. 빈 문자열이면 숨긴다.</summary>
        public string TopLabel
        {
            get { return (string)this.GetValue(TopLabelProperty); }
            set { this.SetValue(TopLabelProperty, value); }
        }

        /// <summary>값 아래 한 줄로 붙는 보조 문구. 빈 문자열이면 숨긴다.</summary>
        public string BottomLabel
        {
            get { return (string)this.GetValue(BottomLabelProperty); }
            set { this.SetValue(BottomLabelProperty, value); }
        }

        /// <summary>카드 색조("#RRGGBB"). 빈 문자열이면 기본 카드 표면 그대로.</summary>
        public string Tone
        {
            get { return (string)this.GetValue(ToneProperty); }
            set { this.SetValue(ToneProperty, value); }
        }

        /// <summary>스파크라인 데이터 — 숫자 시퀀스. null/2개 미만이면 숨긴다.</summary>
        public IEnumerable TrendValues
        {
            get { return (IEnumerable)this.GetValue(TrendValuesProperty); }
            set { this.SetValue(TrendValuesProperty, value); }
        }

        /// <summary>값이 바뀔 때 값 글자 위로 밝은 띠가 한 번 지나가게 할지.</summary>
        public bool HighlightValueChange
        {
            get { return (bool)this.GetValue(HighlightValueChangeProperty); }
            set { this.SetValue(HighlightValueChangeProperty, value); }
        }

        /// <summary>지금까지 값 변경 강조가 재생된 횟수 — 자체 검사가 "언제 재생되는가"를 확인하는 데 쓴다.</summary>
        public int ValueSweepCount { get; private set; }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ModernKpiCardControl card = (ModernKpiCardControl)d;

            if (card.HighlightValueChange && HasVisibleText(e.OldValue as string) && HasVisibleText(e.NewValue as string))
            {
                card.PlayValueSweep();
            }
        }

        // 글자나 숫자가 하나라도 있어야 "값"이다 — 빈 문자열과 "-" 같은 자리 표시는 값이 아니다.
        private static bool HasVisibleText(string text)
        {
            if (text == null)
            {
                return false;
            }

            foreach (char character in text)
            {
                if (char.IsLetterOrDigit(character))
                {
                    return true;
                }
            }

            return false;
        }

        // 값 변경 강조 — "이 카드의 값이 방금 바뀌었다"를 알리는 신호다.
        //   ① 값 글자가 액센트색으로 바뀌고 그 위로 밝은 띠가 왼쪽에서 오른쪽으로 두 번 지나간다.
        //   ② 띠가 끝나면 글자가 액센트색에서 원래 색으로 서서히 돌아온다.
        // 처음 넣은 것(본문색 글자 위로 액센트 띠 한 번)은 눈에 띄지 않았고, 카드 배경까지 켜는 것은 뺐다(2026-10-03 사용자 확인).
        private void PlayValueSweep()
        {
            this.ValueSweepCount = this.ValueSweepCount + 1;

            if (this.valueRestBrush == null)
            {
                this.valueRestBrush = this.ValueText.Foreground;
            }

            int serial = this.ValueSweepCount;
            Color rest = this.ResourceColor("Brush.TextPrimary", Colors.Black);
            Color accent = this.ResourceColor("Brush.Accent", Colors.DodgerBlue);
            Color shine = ChipColorHelper.Blend(accent, Colors.White, sweepShineMix);

            GradientStop before = new GradientStop(accent, -sweepBandWidth * 2d);
            GradientStop peak = new GradientStop(shine, -sweepBandWidth);
            GradientStop after = new GradientStop(accent, 0d);

            LinearGradientBrush sweep = new LinearGradientBrush();
            sweep.StartPoint = new Point(0d, 0.5d);
            sweep.EndPoint = new Point(1d, 0.5d);
            sweep.GradientStops.Add(before);
            sweep.GradientStops.Add(peak);
            sweep.GradientStops.Add(after);
            this.ValueText.Foreground = sweep;

            double travel = 1d + sweepBandWidth * 2d;
            Duration pass = new Duration(TimeSpan.FromMilliseconds(sweepMilliseconds));
            System.Windows.Media.Animation.RepeatBehavior twice =
                    new System.Windows.Media.Animation.RepeatBehavior(sweepPasses);

            System.Windows.Media.Animation.DoubleAnimation lead = new System.Windows.Media.Animation.DoubleAnimation(
                    before.Offset, before.Offset + travel, pass);
            System.Windows.Media.Animation.DoubleAnimation middle = new System.Windows.Media.Animation.DoubleAnimation(
                    peak.Offset, peak.Offset + travel, pass);
            System.Windows.Media.Animation.DoubleAnimation tail = new System.Windows.Media.Animation.DoubleAnimation(
                    after.Offset, after.Offset + travel, pass);
            lead.RepeatBehavior = twice;
            middle.RepeatBehavior = twice;
            tail.RepeatBehavior = twice;

            // 띠가 끝나면 액센트색에서 원래 색으로 서서히 — 재생 중에 값이 또 바뀌어 새 강조가 시작됐으면 건드리지 않는다.
            tail.Completed += delegate
            {
                if (serial != this.ValueSweepCount)
                {
                    return;
                }

                SolidColorBrush settle = new SolidColorBrush(accent);
                this.ValueText.Foreground = settle;

                System.Windows.Media.Animation.ColorAnimation fade = new System.Windows.Media.Animation.ColorAnimation(
                        rest, new Duration(TimeSpan.FromMilliseconds(settleMilliseconds)));
                fade.Completed += delegate
                {
                    if (serial == this.ValueSweepCount)
                    {
                        this.ValueText.Foreground = this.valueRestBrush;
                    }
                };
                settle.BeginAnimation(SolidColorBrush.ColorProperty, fade);
            };

            before.BeginAnimation(GradientStop.OffsetProperty, lead);
            peak.BeginAnimation(GradientStop.OffsetProperty, middle);
            after.BeginAnimation(GradientStop.OffsetProperty, tail);
        }

        private static void OnTrendValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernKpiCardControl)d).RebuildTrend();
        }

        private static void OnToneSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ModernKpiCardControl)d).ApplyTone();
        }

        // 색조를 카드 표면에 얹는다 — 지정색을 표면색 쪽으로 크게 섞어 "가장 연한"
        // 배경을 만들고, 테두리만 한 단계 진하게 해 카드 경계가 읽히게 한다.
        // 색이 없거나 Flat(크롬 제거)이면 지역값을 걷어 공용 카드 스타일이 그대로
        // 살아나게 한다 — 지역값은 스타일 트리거보다 우선하므로 반드시 ClearValue다.
        private void ApplyTone()
        {
            Color tone;

            if (this.Flat || !ChipColorHelper.TryParseColor(this.Tone, out tone))
            {
                this.Surface.ClearValue(Border.BackgroundProperty);
                this.Surface.ClearValue(Border.BorderBrushProperty);
                this.TitleText.ClearValue(TextBlock.ForegroundProperty);
                this.TopLabelText.ClearValue(TextBlock.ForegroundProperty);
                this.BottomLabelText.ClearValue(TextBlock.ForegroundProperty);
                return;
            }

            Color surface = this.ResourceColor("Brush.Surface", Colors.White);
            Color border = this.ResourceColor("Brush.Border", Colors.Gray);
            Color background = ChipColorHelper.Blend(tone, surface, surfaceMix);

            this.Surface.Background = Frozen(background);
            this.Surface.BorderBrush = Frozen(ChipColorHelper.Blend(tone, border, borderMix));

            // 제목·위·아래 작은 글씨는 보조 색(TextSecondary)이라 색조 배경과 겹치면 묻힌다 — 어두운 테마의
            // 밝은 파스텔 색조(CrimsonGray)에서 연회색 글자가 안 보였다(2026-10-15 사용자 보고). 보조 색의 대비가
            // 3:1 에 못 미칠 때만 배지와 같은 규칙으로 배경에서 글자색을 유도한다 — 라이트 테마처럼 이미 읽히는
            // 곳은 그대로다. 큰 값 글자는 TextPrimary 그대로다.
            Color secondary = this.ResourceColor("Brush.TextSecondary", Colors.Gray);

            if (ChipColorHelper.WcagContrast(secondary, background) >= minimumCaptionContrast)
            {
                this.TitleText.ClearValue(TextBlock.ForegroundProperty);
                this.TopLabelText.ClearValue(TextBlock.ForegroundProperty);
                this.BottomLabelText.ClearValue(TextBlock.ForegroundProperty);
                return;
            }

            SolidColorBrush caption = Frozen(ChipColorHelper.DeriveForeground(background));
            this.TitleText.Foreground = caption;
            this.TopLabelText.Foreground = caption;
            this.BottomLabelText.Foreground = caption;
        }

        // 토큰 브러시에서 색을 읽는다 — 컨트롤 리소스에 병합된 테마 사전이 먼저 잡히므로
        // 테마를 바꾸면 색조도 그 테마의 표면색 기준으로 다시 섞인다.
        private Color ResourceColor(string key, Color fallback)
        {
            SolidColorBrush brush = this.TryFindResource(key) as SolidColorBrush;
            return brush == null ? fallback : brush.Color;
        }

        private static SolidColorBrush Frozen(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        // 시퀀스를 캔버스 좌표로 변환해 추세선과 끝점 도트를 다시 그린다.
        // min~max를 세로로 정규화하고(전부 같은 값이면 가운데 평평한 선),
        // 가로는 등간격 — 축/눈금 없는 스파크라인 문법이다.
        private void RebuildTrend()
        {
            List<double> numbers = ReadNumbers(this.TrendValues);

            if (numbers.Count < 2)
            {
                this.TrendCanvas.Visibility = Visibility.Collapsed;
                this.TrendLine.Points.Clear();
                return;
            }

            double width = this.TrendCanvas.Width;
            double height = this.TrendCanvas.Height;

            // 선 굵기(1.5)와 끝점 도트(5)가 캔버스 밖으로 잘리지 않게 안쪽 여백을 둔다.
            const double pad = 3d;

            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (double number in numbers)
            {
                if (number < min) { min = number; }
                if (number > max) { max = number; }
            }

            double span = max - min;
            double stepX = (width - pad * 2) / (numbers.Count - 1);

            this.TrendLine.Points.Clear();

            for (int index = 0; index < numbers.Count; index++)
            {
                double x = pad + stepX * index;
                double y = span > 0d
                        ? pad + (1d - (numbers[index] - min) / span) * (height - pad * 2)
                        : height / 2d;

                this.TrendLine.Points.Add(new Point(x, y));
            }

            // 끝점 도트 — "지금 값"이 어디인지 시선을 끌어 준다.
            Point last = this.TrendLine.Points[this.TrendLine.Points.Count - 1];
            Canvas.SetLeft(this.TrendDot, last.X - this.TrendDot.Width / 2d);
            Canvas.SetTop(this.TrendDot, last.Y - this.TrendDot.Height / 2d);

            this.TrendCanvas.Visibility = Visibility.Visible;
        }

        // IEnumerable의 요소를 숫자로 읽는다 — int/decimal/double은 물론
        // 숫자 문자열도 수용하고, 해석 불가 요소는 건너뛴다(예외 금지).
        private static List<double> ReadNumbers(IEnumerable source)
        {
            List<double> numbers = new List<double>();

            if (source == null)
            {
                return numbers;
            }

            foreach (object item in source)
            {
                if (item == null)
                {
                    continue;
                }

                if (item is double || item is float || item is int || item is long
                        || item is short || item is byte || item is decimal)
                {
                    numbers.Add(Convert.ToDouble(item, CultureInfo.InvariantCulture));
                    continue;
                }

                double parsed;
                if (double.TryParse(
                        Convert.ToString(item, CultureInfo.InvariantCulture),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
                {
                    numbers.Add(parsed);
                }
            }

            return numbers;
        }
    }
}
