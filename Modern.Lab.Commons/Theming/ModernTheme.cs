using System.Drawing;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// 테마 선택 플래그 + GDI+ 컨트롤용 중앙 팔레트.
    ///
    /// 두 파이프라인을 하나의 플래그로 묶는다:
    ///   - WPF(ElementHost) 컨트롤: XAML의 StaticResource가 <c>Themes/Tokens.xaml</c>를
    ///     쓰는데, Light가 아니면 <c>SharedResourceDictionary</c>가
    ///     <c>Tokens.&lt;테마&gt;.xaml</c> 오버라이드를 뒤에 병합해 테마 값을 집게 한다.
    ///   - 순수 GDI+ 컨트롤(ModernLabel/StatusBadge/CardPanel/GroupBox/SplitContainer):
    ///     XAML을 읽을 수 없으므로 여기 팔레트 색을 읽는다.
    ///
    /// 팔레트 구성:
    ///   - Light: 연한 블루 그레이 뉴트럴(#F3F4F6 계열) + 액센트 #0078D4 램프
    ///     (2026-07-14 Win11 순정화 이전의 소프트 팔레트로 복원).
    ///   - Dark: #202020/#2B2B2B, 액센트 #4CC2FF(+검정 OnAccent — WPF 쪽 토큰).
    ///   - OrangeBlue: 웜 미드 그레이(#574F4A) 면 + 밝은 오렌지(#F08A3E) 액센트,
    ///     선택 강조는 블루 틴트 (CrimsonGray와 같은 밝기의 중간톤, 2026-10-19).
    ///   - GreenTomato: 그린 미드 그레이(#4C5A52) 면 + 밝은 그린(#7FD69A) 액센트,
    ///     선택 강조는 토마토 틴트 (중간톤, 2026-10-19).
    ///   - CrimsonGray: 미드 그레이 모노톤(#555555) + 라이트 크림슨(#F2919E) 액센트.
    ///   - LightPurple: Fluent 퍼플(#5C2E91) 액센트 + 라벤더 파스텔 뉴트럴.
    ///   - HighContrast: 접근성/현장 시인성 고대비 — 순검정 면 + 순백 텍스트/테두리,
    ///     밝은 시안(#6FE0FF) 액센트, 시맨틱은 고채도 밝은 색.
    ///   - SolarizedLight: Solarized Light 정본 — 크림 카드(#FDF6E3) + 크림 배경
    ///     (#EEE8D5), 블루(#1A6598) 액센트 (저채도·저대비, 장시간 모니터링용).
    ///   - SolarizedDark: Solarized Dark 정본 — 딥 틸(#002B36/#073642) 면 +
    ///     밝힌 블루(#4BA3DE) 액센트 + base03 OnAccent.
    ///   - Nord: nordtheme.com 정본 — Polar Night 청회색(#2E3440~) 면 +
    ///     Frost 시안(#88C0D0) 액센트 (Dark보다 덜 차가운 다크).
    ///
    /// <see cref="Mode"/>는 <b>앱 시작 시 첫 컨트롤 생성 전에 한 번</b> 설정한다
    /// (런타임 토글은 지원하지 않는다 — WPF StaticResource가 로드 시 확정되기 때문).
    /// 설정하지 않으면 라이트라서, 이 라이브러리를 쓰는 다른 시스템은 영향받지 않는다.
    /// </summary>
    public static class ModernTheme
    {
        /// <summary>
        /// 테마 종류.
        ///
        /// 번호는 명시적으로 고정한다 — enum 상수는 소비자 바이너리의 IL에 숫자로
        /// 박히므로, 멤버를 지우거나 순서를 바꿔도 기존 번호가 다른 의미로 재사용되면
        /// DLL 교체만 한 옛 프로그램이 조용히 다른 테마로 오동작한다.
        /// 5(구 Blue)·7(구 Mono)은 2026-08-07 테마 삭제로 영구 결번 — 재사용 금지.
        /// 정의 없는 값이 들어오면 모든 소비 지점의 default 분기가 Light로 저하한다.
        /// </summary>
        public enum ThemeMode
        {
            /// <summary>기본값 — 연한 라이트 Fluent (액센트 #0078D4).</summary>
            Light = 0,

            /// <summary>Win11 다크 (#202020, 액센트 #4CC2FF).</summary>
            Dark = 1,

            /// <summary>웜 미드 그레이 면 + 밝은 오렌지 액센트, 블루 선택 강조 테마 (어두운 계열 중간톤).</summary>
            OrangeBlue = 2,

            /// <summary>그린 미드 그레이 면 + 밝은 그린 액센트, 토마토 선택 강조 테마 (어두운 계열 중간톤).</summary>
            GreenTomato = 3,

            /// <summary>미드 그레이 모노톤 + 라이트 크림슨 액센트 테마 (어두운 계열).</summary>
            CrimsonGray = 4,

            /// <summary>Fluent 퍼플(#5C2E91) 액센트 + 라벤더 파스텔 테마 (라이트 기반).</summary>
            LightPurple = 6,

            /// <summary>접근성/현장 시인성 고대비 — 순검정 면 + 순백 텍스트/테두리 + 밝은 시안(#6FE0FF) 액센트 (어두운 계열).</summary>
            HighContrast = 8,

            /// <summary>Solarized Light 정본 — 크림 카드(#FDF6E3)·크림 배경(#EEE8D5) + 블루 액센트 (라이트 기반).</summary>
            SolarizedLight = 9,

            /// <summary>Solarized Dark 정본 — 딥 틸(#002B36/#073642) 면 + 블루 액센트 (어두운 계열).</summary>
            SolarizedDark = 10,

            /// <summary>Nord 정본 — Polar Night 청회색(#2E3440~) 면 + Frost 시안(#88C0D0) 액센트 (어두운 계열).</summary>
            Nord = 11
        }

        /// <summary>현재 테마. 앱 시작 시 한 번 설정한다.</summary>
        public static ThemeMode Mode { get; set; } = ThemeMode.Light;

        // ---- 장평 (글자 가로 비율) ----

        /// <summary>장평 허용 하한.</summary>
        public const double MinFontWidthRatio = 0.8;

        /// <summary>장평 허용 상한.</summary>
        public const double MaxFontWidthRatio = 1.2;

        private static double fontWidthRatio = 1.0;

        /// <summary>
        /// 전역 장평(글자 가로 비율). 1.0 = 원래 폭, 0.9 = 축소, 1.1 = 확대.
        /// 허용 범위(0.8~1.2) 밖의 값은 경계로 잘린다.
        ///
        /// <see cref="Mode"/>와 마찬가지로 <b>앱 시작 시 첫 컨트롤 생성 전에 한 번</b>
        /// 설정한다 — WPF 쪽(그리드 셀 등)은 컬럼/템플릿 구성 시점에 값이 굳는다.
        /// 기본 1.0이므로 설정하지 않으면 기존 시스템은 영향받지 않는다.
        /// 컨트롤별 FontWidthRatio 속성(0 = 전역 사용)으로 개별 재정의할 수 있다.
        /// </summary>
        public static double FontWidthRatio
        {
            get { return fontWidthRatio; }
            set { fontWidthRatio = ClampFontWidthRatio(value); }
        }

        /// <summary>
        /// 컨트롤별 재정의 값을 유효 장평으로 해석한다 —
        /// 0 이하(기본)는 전역 <see cref="FontWidthRatio"/>, 양수는 클램프해 사용.
        /// </summary>
        public static double ResolveFontWidthRatio(double localOverride)
        {
            return localOverride > 0d ? ClampFontWidthRatio(localOverride) : fontWidthRatio;
        }

        /// <summary>장평 값을 허용 범위(0.8~1.2)로 자른다.</summary>
        public static double ClampFontWidthRatio(double value)
        {
            if (value < MinFontWidthRatio)
            {
                return MinFontWidthRatio;
            }

            if (value > MaxFontWidthRatio)
            {
                return MaxFontWidthRatio;
            }

            return value;
        }

        /// <summary>다크 테마 여부 (Dark 전용 플래그).</summary>
        public static bool IsDark
        {
            get { return Mode == ThemeMode.Dark; }
        }

        /// <summary>
        /// 어두운 표면 계열 테마 여부 (Dark, OrangeBlue, GreenTomato, CrimsonGray, HighContrast,
        /// SolarizedDark, Nord) — 다크 타이틀바, 밝은 텍스트 등 "어두운 화면" 공통 처리가 걸리는 기준.
        /// </summary>
        public static bool IsDarkBased
        {
            get
            {
                return Mode == ThemeMode.Dark
                        || Mode == ThemeMode.OrangeBlue
                        || Mode == ThemeMode.GreenTomato
                        || Mode == ThemeMode.CrimsonGray
                        || Mode == ThemeMode.HighContrast
                        || Mode == ThemeMode.SolarizedDark
                        || Mode == ThemeMode.Nord;
            }
        }

        // ---- GDI+ 팔레트 (Themes/Tokens.xaml의 색을 미러링; 테마별 대응 값) ----

        /// <summary>카드/입력 표면 (Brush.Surface)</summary>
        public static Color Surface
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(43, 43, 43);
                    case ThemeMode.OrangeBlue: return Rgb(99, 90, 84);
                    case ThemeMode.GreenTomato: return Rgb(87, 102, 93);
                    case ThemeMode.CrimsonGray: return Rgb(97, 97, 97);
                    case ThemeMode.HighContrast: return Rgb(0, 0, 0);
                    // Solarized 카드는 순백이 아니라 크림(base3 #FDF6E3)이다.
                    case ThemeMode.SolarizedLight: return Rgb(253, 246, 227);
                    case ThemeMode.SolarizedDark: return Rgb(7, 54, 66);
                    case ThemeMode.Nord: return Rgb(59, 66, 82);
                    default: return Rgb(255, 255, 255);
                }
            }
        }

        /// <summary>폼/페이지 바탕 (Brush.Background) — 테마 톤이 가장 크게 갈리는 곳.</summary>
        public static Color Background
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(32, 32, 32);
                    case ThemeMode.CrimsonGray: return Rgb(85, 85, 85);
                    case ThemeMode.OrangeBlue: return Rgb(87, 79, 74);
                    case ThemeMode.GreenTomato: return Rgb(76, 90, 82);
                    case ThemeMode.LightPurple: return Rgb(239, 232, 248);
                    case ThemeMode.HighContrast: return Rgb(0, 0, 0);
                    case ThemeMode.SolarizedLight: return Rgb(238, 232, 213);
                    case ThemeMode.SolarizedDark: return Rgb(0, 43, 54);
                    case ThemeMode.Nord: return Rgb(46, 52, 64);
                    default: return Rgb(243, 244, 246);
                }
            }
        }

        /// <summary>기본 테두리 (Brush.Border)</summary>
        public static Color Border
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(65, 65, 65);
                    case ThemeMode.CrimsonGray: return Rgb(122, 122, 122);
                    case ThemeMode.OrangeBlue: return Rgb(126, 116, 108);
                    case ThemeMode.GreenTomato: return Rgb(116, 133, 122);
                    case ThemeMode.LightPurple: return Rgb(211, 194, 236);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.SolarizedLight: return Rgb(211, 203, 183);
                    case ThemeMode.SolarizedDark: return Rgb(30, 84, 98);
                    case ThemeMode.Nord: return Rgb(76, 86, 106);
                    default: return Rgb(209, 213, 219);
                }
            }
        }

        /// <summary>옅은 테두리/구분선 (Brush.BorderSubtle)</summary>
        public static Color BorderSubtle
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(56, 56, 56);
                    case ThemeMode.CrimsonGray: return Rgb(112, 112, 112);
                    case ThemeMode.OrangeBlue: return Rgb(116, 106, 99);
                    case ThemeMode.GreenTomato: return Rgb(106, 122, 112);
                    case ThemeMode.LightPurple: return Rgb(226, 214, 243);
                    case ThemeMode.HighContrast: return Rgb(157, 157, 157);
                    case ThemeMode.SolarizedLight: return Rgb(228, 221, 201);
                    case ThemeMode.SolarizedDark: return Rgb(18, 65, 79);
                    case ThemeMode.Nord: return Rgb(67, 77, 96);
                    default: return Rgb(229, 231, 235);
                }
            }
        }

        /// <summary>본문/제목 텍스트 (Brush.TextPrimary)</summary>
        public static Color TextPrimary
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(240, 240, 240);
                    case ThemeMode.CrimsonGray: return Rgb(245, 245, 245);
                    case ThemeMode.OrangeBlue: return Rgb(247, 243, 240);
                    case ThemeMode.GreenTomato: return Rgb(242, 247, 243);
                    case ThemeMode.LightPurple: return Rgb(27, 27, 27);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.SolarizedLight: return Rgb(7, 54, 66);
                    case ThemeMode.SolarizedDark: return Rgb(159, 178, 181);
                    case ThemeMode.Nord: return Rgb(236, 239, 244);
                    default: return Rgb(17, 24, 39);
                }
            }
        }

        /// <summary>
        /// 오류를 말하는 **진한 채움색** (Brush.ErrorBorder). 배지 강조에 쓴다 —
        /// 주변 배지가 전부 옅은 파스텔이라, 같은 파스텔로는 눈에 걸리지 않는다.
        /// 채도와 무게가 다른 칩이라야 종류가 다른 신호로 읽힌다.
        ///
        /// 글자색은 이 색에서 유도한다(검정/흰색 자동) — 그래야 테마마다 대비가
        /// 보장된다. 어두운 테마의 값은 밝은 빨강이므로 글자가 검정이 된다.
        /// </summary>
        public static Color ErrorFill
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(255, 122, 133);
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.CrimsonGray: return Rgb(255, 151, 160);
                    case ThemeMode.HighContrast: return Rgb(255, 80, 80);
                    case ThemeMode.SolarizedLight: return Rgb(220, 50, 47);
                    case ThemeMode.SolarizedDark: return Rgb(238, 106, 98);
                    case ThemeMode.Nord: return Rgb(210, 121, 131);
                    case ThemeMode.LightPurple: return Rgb(196, 43, 28);
                    default: return Rgb(220, 38, 38);
                }
            }
        }

        /// <summary>보조 텍스트 (Brush.TextSecondary)</summary>
        public static Color TextSecondary
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(160, 160, 160);
                    case ThemeMode.CrimsonGray: return Rgb(204, 204, 204);
                    case ThemeMode.OrangeBlue: return Rgb(212, 204, 198);
                    case ThemeMode.GreenTomato: return Rgb(203, 214, 206);
                    case ThemeMode.LightPurple: return Rgb(93, 93, 93);
                    case ThemeMode.HighContrast: return Rgb(224, 224, 224);
                    case ThemeMode.SolarizedLight: return Rgb(101, 123, 131);
                    case ThemeMode.SolarizedDark: return Rgb(124, 145, 152);
                    case ThemeMode.Nord: return Rgb(194, 202, 216);
                    default: return Rgb(107, 114, 128);
                }
            }
        }

        /// <summary>비활성 텍스트 (Brush.DisabledText)</summary>
        public static Color DisabledText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(110, 110, 110);
                    case ThemeMode.CrimsonGray: return Rgb(156, 156, 156);
                    case ThemeMode.OrangeBlue: return Rgb(161, 152, 143);
                    case ThemeMode.GreenTomato: return Rgb(151, 165, 155);
                    case ThemeMode.LightPurple: return Rgb(157, 157, 157);
                    case ThemeMode.HighContrast: return Rgb(141, 141, 141);
                    case ThemeMode.SolarizedLight: return Rgb(161, 153, 138);
                    case ThemeMode.SolarizedDark: return Rgb(78, 101, 109);
                    case ThemeMode.Nord: return Rgb(112, 122, 140);
                    default: return Rgb(156, 163, 175);
                }
            }
        }

        /// <summary>액센트 (Brush.Accent) — 테마의 주 색. 어두운 계열에서는 밝게.</summary>
        public static Color Accent
        {
            get { return AccentOf(Mode); }
        }

        /// <summary>
        /// 지정 테마의 액센트 — 현재 테마와 무관하게 조회한다.
        /// Palette가 시작 색상(Hue)을 여기서 유도하므로, 테마 액센트를 바꾸면
        /// 배지 팔레트의 시작색도 자동으로 따라온다 (값 이중 관리 금지).
        /// </summary>
        internal static Color AccentOf(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.Dark: return Rgb(76, 194, 255);
                case ThemeMode.CrimsonGray: return Rgb(242, 145, 158);
                case ThemeMode.OrangeBlue: return Rgb(240, 138, 62);
                case ThemeMode.GreenTomato: return Rgb(127, 214, 154);
                case ThemeMode.LightPurple: return Rgb(92, 46, 145);
                case ThemeMode.HighContrast: return Rgb(111, 224, 255);
                case ThemeMode.SolarizedLight: return Rgb(26, 101, 152);
                case ThemeMode.SolarizedDark: return Rgb(75, 163, 222);
                case ThemeMode.Nord: return Rgb(136, 192, 208);
                default: return Rgb(0, 120, 212);
            }
        }

        /// <summary>필수 표시/위험 빨강 (Brush.ErrorBorder)</summary>
        public static Color RequiredRed
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark:
                    case ThemeMode.OrangeBlue:
                    case ThemeMode.GreenTomato:
                    case ThemeMode.CrimsonGray: return Rgb(255, 138, 148);
                    case ThemeMode.LightPurple: return Rgb(196, 43, 28);
                    case ThemeMode.HighContrast: return Rgb(255, 80, 80);
                    case ThemeMode.SolarizedLight: return Rgb(220, 50, 47);
                    case ThemeMode.SolarizedDark: return Rgb(238, 106, 98);
                    case ThemeMode.Nord: return Rgb(210, 121, 131);
                    default: return Rgb(220, 38, 38);
                }
            }
        }

        /// <summary>중립 배지 배경 (Brush.NeutralBackground)</summary>
        public static Color NeutralBackground
        {
            get { return NeutralBackgroundOf(Mode); }
        }

        /// <summary>
        /// 지정 테마의 중립 배지 배경 — 현재 테마와 무관하게 조회한다.
        /// SemanticColors.Neutral이 여기서 유도하므로, 이 값을 바꾸면 화면들이
        /// 데이터로 넣는 중립 배지색도 자동으로 따라온다 (값 이중 관리 금지).
        /// </summary>
        internal static Color NeutralBackgroundOf(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.Dark: return Rgb(56, 56, 56);
                case ThemeMode.CrimsonGray: return Rgb(112, 112, 112);
                case ThemeMode.OrangeBlue: return Rgb(114, 104, 98);
                case ThemeMode.GreenTomato: return Rgb(107, 122, 113);
                case ThemeMode.LightPurple: return Rgb(240, 240, 240);
                case ThemeMode.HighContrast: return Rgb(30, 30, 30);
                case ThemeMode.SolarizedLight: return Rgb(233, 226, 205);
                case ThemeMode.SolarizedDark: return Rgb(17, 69, 80);
                case ThemeMode.Nord: return Rgb(67, 76, 94);
                default: return Rgb(243, 244, 246);
            }
        }

        /// <summary>중립 배지 텍스트 (Brush.NeutralText)</summary>
        public static Color NeutralText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(208, 208, 208);
                    case ThemeMode.CrimsonGray: return Rgb(228, 226, 224);
                    case ThemeMode.OrangeBlue: return Rgb(226, 219, 214);
                    case ThemeMode.GreenTomato: return Rgb(221, 229, 223);
                    case ThemeMode.LightPurple: return Rgb(59, 59, 59);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.SolarizedLight: return Rgb(91, 110, 118);
                    case ThemeMode.SolarizedDark: return Rgb(168, 188, 192);
                    case ThemeMode.Nord: return Rgb(216, 222, 233);
                    default: return Rgb(55, 65, 81);
                }
            }
        }

        /// <summary>선택 강조 배경 (Brush.SelectedBackground)</summary>
        public static Color SelectionBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(30, 73, 100);
                    case ThemeMode.CrimsonGray: return Rgb(133, 90, 98);
                    case ThemeMode.OrangeBlue: return Rgb(79, 106, 134);
                    case ThemeMode.GreenTomato: return Rgb(138, 90, 80);
                    case ThemeMode.LightPurple: return Rgb(220, 204, 240);
                    case ThemeMode.HighContrast: return Rgb(111, 224, 255);
                    case ThemeMode.SolarizedLight: return Rgb(197, 222, 240);
                    case ThemeMode.SolarizedDark: return Rgb(18, 71, 95);
                    case ThemeMode.Nord: return Rgb(63, 88, 114);
                    default: return Rgb(182, 217, 242);
                }
            }
        }

        /// <summary>보조 표면 — 카드 위 살짝 다른 톤 (Brush.SurfaceAlt / HeaderBackground)</summary>
        public static Color SurfaceAlt
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(51, 51, 51);
                    case ThemeMode.CrimsonGray: return Rgb(104, 104, 104);
                    case ThemeMode.OrangeBlue: return Rgb(106, 97, 91);
                    case ThemeMode.GreenTomato: return Rgb(94, 109, 100);
                    case ThemeMode.LightPurple: return Rgb(246, 241, 251);
                    case ThemeMode.HighContrast: return Rgb(16, 16, 16);
                    case ThemeMode.SolarizedLight: return Rgb(247, 241, 222);
                    case ThemeMode.SolarizedDark: return Rgb(11, 61, 74);
                    case ThemeMode.Nord: return Rgb(67, 76, 94);
                    default: return Rgb(249, 250, 251);
                }
            }
        }

        /// <summary>그리드 열 헤더 배경 (Brush.GridHeaderBackground)</summary>
        public static Color GridHeaderBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(54, 54, 54);
                    case ThemeMode.CrimsonGray: return Rgb(110, 110, 110);
                    case ThemeMode.OrangeBlue: return Rgb(113, 103, 96);
                    case ThemeMode.GreenTomato: return Rgb(101, 117, 107);
                    case ThemeMode.LightPurple: return Rgb(227, 216, 244);
                    case ThemeMode.HighContrast: return Rgb(20, 20, 20);
                    case ThemeMode.SolarizedLight: return Rgb(230, 223, 200);
                    case ThemeMode.SolarizedDark: return Rgb(17, 70, 82);
                    case ThemeMode.Nord: return Rgb(71, 81, 100);
                    default: return Rgb(239, 243, 250);
                }
            }
        }

        /// <summary>그리드 교차 행 배경 (Brush.GridRowAlt)</summary>
        public static Color GridRowAlt
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(48, 48, 48);
                    case ThemeMode.CrimsonGray: return Rgb(102, 102, 102);
                    case ThemeMode.OrangeBlue: return Rgb(104, 95, 89);
                    case ThemeMode.GreenTomato: return Rgb(92, 107, 98);
                    case ThemeMode.LightPurple: return Rgb(248, 245, 252);
                    case ThemeMode.HighContrast: return Rgb(10, 10, 10);
                    case ThemeMode.SolarizedLight: return Rgb(246, 240, 221);
                    case ThemeMode.SolarizedDark: return Rgb(6, 50, 61);
                    case ThemeMode.Nord: return Rgb(54, 62, 76);
                    default: return Rgb(246, 247, 249);
                }
            }
        }

        /// <summary>선택 강조 텍스트 (Brush.SelectedText) — SelectionBackground 위 글자색.</summary>
        public static Color SelectedText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(183, 227, 255);
                    case ThemeMode.CrimsonGray: return Rgb(255, 237, 239);
                    case ThemeMode.OrangeBlue: return Rgb(220, 235, 250);
                    case ThemeMode.GreenTomato: return Rgb(255, 228, 222);
                    case ThemeMode.LightPurple: return Rgb(70, 34, 110);
                    case ThemeMode.HighContrast: return Rgb(0, 0, 0);
                    case ThemeMode.SolarizedLight: return Rgb(23, 94, 146);
                    case ThemeMode.SolarizedDark: return Rgb(168, 214, 242);
                    case ThemeMode.Nord: return Rgb(195, 226, 236);
                    default: return Rgb(0, 90, 158);
                }
            }
        }

        private static Color Rgb(int r, int g, int b)
        {
            return Color.FromArgb(255, r, g, b);
        }
    }
}
