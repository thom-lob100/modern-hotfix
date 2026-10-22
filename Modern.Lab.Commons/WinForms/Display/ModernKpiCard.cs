using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// 소형 통계 카드(수작업 Label 한 쌍을 대체; WPF ModernKpiCardControl을
    /// ElementHost로 호스팅).
    ///
    /// Title은 한 번 설정하고, 매 조회 후 Value를 갱신한다
    /// (예: Value = grid.RowCount.ToString()).
    /// </summary>
    [ToolboxItem(true)]
    public class ModernKpiCard : WpfElementHostBase<Modern.Lab.Controls.Wpf.Display.ModernKpiCardControl>
    {
        // 디자인 타임 WPF 생성이 실패한 경우(Wpf == null)에도 속성 그리드가
        // 동작하도록 하는 폴백 저장소.
        private string fallbackTitle;
        private string fallbackValue;
        private bool fallbackFlat;
        private string fallbackTopLabel;
        private string fallbackBottomLabel;
        private string fallbackTone;
        private bool fallbackHighlightValueChange;
        private bool fallbackBottomLabelEmphasis;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernKpiCard()
        {
            this.Size = new Size(160, 72);
            this.fallbackTitle = "Title";
            this.fallbackValue = "0";
            this.fallbackFlat = false;
            this.fallbackTopLabel = string.Empty;
            this.fallbackBottomLabel = string.Empty;
            this.fallbackTone = string.Empty;
        }

        /// <summary>값 바로 위의 작은 글씨(10 크기, 보조 색 — 라벨·버튼·그리드 헤더의 TopLabel 과 같은 규칙). 비우면 숨긴다.</summary>
        [Category("Modern")]
        [Description("Small line shown right above the value (empty hides it)")]
        [Localizable(true)]
        [DefaultValue("")]
        public string TopLabel
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.TopLabel;
                }

                return this.fallbackTopLabel;
            }
            set
            {
                string text = value ?? string.Empty;
                this.fallbackTopLabel = text;

                if (this.Wpf != null)
                {
                    this.Wpf.TopLabel = text;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>값 바로 아래의 작은 글씨(TopLabel 과 같은 크기·색). 비우면 숨긴다.</summary>
        [Category("Modern")]
        [Description("Small line shown right under the value (empty hides it)")]
        [Localizable(true)]
        [DefaultValue("")]
        public string BottomLabel
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.BottomLabel;
                }

                return this.fallbackBottomLabel;
            }
            set
            {
                string text = value ?? string.Empty;
                this.fallbackBottomLabel = text;

                if (this.Wpf != null)
                {
                    this.Wpf.BottomLabel = text;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>카드 색조("#RRGGBB") — 아주 연한 배경으로 카드끼리 자리를 구분한다.</summary>
        [Category("Modern")]
        [Description("Card tint color (#RRGGBB) — a very light background that tells cards apart")]
        [DefaultValue("")]
        public string Tone
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Tone;
                }

                return this.fallbackTone;
            }
            set
            {
                string text = value ?? string.Empty;
                this.fallbackTone = text;

                if (this.Wpf != null)
                {
                    this.Wpf.Tone = text;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>공용 카드 패널(예: ModernCardPanel) 위에서 쓰도록 카드 크롬을 제거한다.</summary>
        [Category("Modern")]
        [Description("Remove the card border/background — for flat placement on a card panel")]
        [DefaultValue(false)]
        public bool Flat
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Flat;
                }

                return this.fallbackFlat;
            }
            set
            {
                this.fallbackFlat = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Flat = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 값이 바뀔 때 값 글자 위로 밝은 띠가 왼쪽에서 오른쪽으로 한 번 지나가게 할지(기본 false).
        /// 값에서 다른 값으로 바뀔 때만 재생한다 — 빈 값("-" 포함)에서 처음 채워질 때와 같은 값의 재대입은 재생하지 않는다.
        /// </summary>
        [Category("Modern")]
        [Description("Sweep a bright band across the value once when it changes to another value")]
        [DefaultValue(false)]
        public bool HighlightValueChange
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.HighlightValueChange;
                }

                return this.fallbackHighlightValueChange;
            }
            set
            {
                this.fallbackHighlightValueChange = value;

                if (this.Wpf != null)
                {
                    this.Wpf.HighlightValueChange = value;
                }
            }
        }

        /// <summary>값 아래 줄(<see cref="BottomLabel"/>)을 강조할지 — 참이면 BodyLg(14) 세미볼드 본문색(기본 false, 보조 글씨 10).</summary>
        [Category("Modern")]
        [Description("Draw the bottom label as a second value line (larger, semibold) instead of a small caption")]
        [DefaultValue(false)]
        public bool BottomLabelEmphasis
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.BottomLabelEmphasis;
                }

                return this.fallbackBottomLabelEmphasis;
            }
            set
            {
                this.fallbackBottomLabelEmphasis = value;

                if (this.Wpf != null)
                {
                    this.Wpf.BottomLabelEmphasis = value;
                }
            }
        }

        /// <summary>값 글자 위로 빛띠를 한 번 돌린다 — 값은 같지만 아래 줄(자식 장비 등)이 바뀌었을 때 화면이 직접 부른다(2026-10-22).</summary>
        public void PlayValueSweep()
        {
            if (this.Wpf != null)
            {
                this.Wpf.PlayValueSweep();
            }
        }

        /// <summary>값 위에 표시되는 캡션.</summary>
        [Category("Modern")]
        [Description("Title shown above the value")]
        [Localizable(true)]
        [DefaultValue("Title")]
        public string Title
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Title;
                }

                return this.fallbackTitle;
            }
            set
            {
                this.fallbackTitle = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Title = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>
        /// 스파크라인 데이터 — 값 오른쪽에 붙는 초미니 추세선(예: 최근 7일 건수).
        /// 숫자 시퀀스(int/decimal/double 배열, 숫자 문자열 포함)를 조회 후
        /// 할당하고, null이나 2개 미만이면 추세선이 숨는다. 런타임 데이터라
        /// 디자이너 직렬화는 하지 않는다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Collections.IEnumerable TrendValues
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.TrendValues;
                }

                return null;
            }
            set
            {
                if (this.Wpf != null)
                {
                    this.Wpf.TrendValues = value;
                }
            }
        }

        /// <summary>강조 표시되는 값 텍스트.</summary>
        [Category("Modern")]
        [Description("Value text to emphasize")]
        [DefaultValue("0")]
        public string Value
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Value;
                }

                return this.fallbackValue;
            }
            set
            {
                this.fallbackValue = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Value = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }
    }
}
