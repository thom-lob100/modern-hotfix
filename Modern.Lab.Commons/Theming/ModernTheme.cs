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
    ///   - OrangeBlue: 라이트 오렌지(#F7D9C2) 배경 + 흰 카드 + 진한 오렌지(#9E440A) 액센트,
    ///     선택 강조는 진한 블루 + 흰 글자 (채도 있는 라이트 테마, 2026-10-22 복귀).
    ///   - GreenTomato: 연한 그린(#D8EFE0) 배경 + 흰 카드 + 토마토(#B3401F) 액센트,
    ///     선택 강조는 진한 그린(#2E7D64) + 흰 글자, 오류는 크림슨 (라이트, 2026-10-21).
    ///   - CrimsonGray: 미드 그레이 모노톤(#585858) + 라이트 크림슨(#FFDCE0) 액센트, 보조 글자 #E8E8E8 (2026-10-22 가독성 조정).
    ///   - DarkGold: 웜 차콜(#1A1814/#24211B) 면 + 골드(#E0B45A) 액센트 + 딥 틸(#1F4F5A) 선택,
    ///     경고는 골드와 겹치지 않게 오렌지 (2026-10-21).
    ///   - LightPurple: Fluent 퍼플(#5C2E91) 액센트 + 라벤더 파스텔 뉴트럴.
    ///   - Mocha: Catppuccin Mocha — 네이비퍼플(#1E1E2E/#313244) 면 + 라벤더(#CBA6F7) 액센트 + 딥 모브(#5B4A8C) 선택 (2026-10-22).
    ///   - Forest: Everforest Dark — 그린그레이(#2D353B/#343F44) 면 + 세이지 그린(#A7C080) 액센트 + 딥 그린(#3B5A47) 선택, 베이지 글자 (2026-10-22).
    ///   - TokyoNight: Tokyo Night — 딥 네이비(#1A1B26/#24283B) 면 + 블루(#7AA2F7) 액센트 + 블루(#3D59A1) 선택, 라벤더화이트 글자 (2026-10-22).
    ///   - TokyoDay: Tokyo Night Day — 페리윙클(#E1E2E7) 배경 + 흰 카드 + 블루 글자(#2F54B5) + 블루(#2468CF) 액센트, TokyoNight 의 라이트 짝 (2026-10-22).
    ///   - Mint: Radix Teal 라이트 — 민트(#CCF3EA) 배경 + 흰 카드 + 딥 틸 글자(#0D3D38) + 틸(#00705F) 액센트 (2026-10-22).
    ///   - LightPink: LightPurple 과 같은 구조의 핑크 — 라즈베리(#BD1558) 액센트 + 핑크 파스텔(#F6D9E6) 뉴트럴 (2026-10-22).
    ///   - HighContrast: 접근성/현장 시인성 고대비 — 순검정 면 + 순백 텍스트/테두리,
    ///     밝은 시안(#6FE0FF) 액센트, 시맨틱은 고채도 밝은 색.
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

            /// <summary>라이트 오렌지(#F7D9C2) 면 + 흰 카드 + 진한 오렌지(#9E440A) 액센트, 진한 블루 선택 강조 (채도 있는 라이트 계열).</summary>
            OrangeBlue = 2,

            /// <summary>연한 그린(#D8EFE0) 면 + 흰 카드 + 토마토(#B3401F) 액센트 + 진한 그린 선택 (라이트 기반).</summary>
            GreenTomato = 3,

            /// <summary>미드 그레이 모노톤 + 라이트 크림슨 액센트 테마 (어두운 계열).</summary>
            CrimsonGray = 4,

            /// <summary>Fluent 퍼플(#5C2E91) 액센트 + 라벤더 파스텔 테마 (라이트 기반).</summary>
            LightPurple = 6,

            /// <summary>접근성/현장 시인성 고대비 — 순검정 면 + 순백 텍스트/테두리 + 밝은 시안(#6FE0FF) 액센트 (어두운 계열).</summary>
            HighContrast = 8,

            /// <summary>웜 차콜(#1A1814/#24211B) 면 + 골드(#E0B45A) 액센트 + 딥 틸 선택 (어두운 계열).</summary>
            DarkGold = 12,

            /// <summary>라즈베리 핑크(#BD1558) 액센트 + 핑크 파스텔(#F6D9E6) 테마 — LightPurple 과 같은 구조 (라이트 기반).</summary>
            LightPink = 13,

            /// <summary>Catppuccin Mocha — 네이비퍼플(#1E1E2E/#313244) 면 + 라벤더(#CBA6F7) 액센트 + 딥 모브 선택 (어두운 계열).</summary>
            Mocha = 14,

            /// <summary>Everforest Dark — 그린그레이(#2D353B/#343F44) 면 + 세이지 그린(#A7C080) 액센트 + 딥 그린 선택, 베이지 글자 (어두운 계열).</summary>
            Forest = 15,

            /// <summary>Tokyo Night — 딥 네이비(#1A1B26/#24283B) 면 + 블루(#7AA2F7) 액센트 + 블루 선택, 라벤더화이트 글자 (어두운 계열).</summary>
            TokyoNight = 16,

            /// <summary>Tokyo Night Day — 페리윙클(#E1E2E7) 배경 + 흰 카드 + 블루 글자(#2F54B5) + 블루(#2468CF) 액센트 (라이트 기반).</summary>
            TokyoDay = 17,

            /// <summary>Radix Teal 라이트 — 민트(#CCF3EA) 배경 + 흰 카드 + 딥 틸 글자(#0D3D38) + 틸(#00705F) 액센트 (라이트 기반).</summary>
            Mint = 18
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
        /// 어두운 표면 계열 테마 여부 (Dark, CrimsonGray, HighContrast,
        /// DarkGold, Mocha, Forest, TokyoNight) — 다크 타이틀바, 밝은 텍스트 등 "어두운 화면" 공통 처리가 걸리는 기준.
        /// </summary>
        public static bool IsDarkBased
        {
            get
            {
                return Mode == ThemeMode.Dark
                        || Mode == ThemeMode.CrimsonGray
                        || Mode == ThemeMode.HighContrast
                        || Mode == ThemeMode.DarkGold
                        || Mode == ThemeMode.Mocha
                        || Mode == ThemeMode.Forest
                        || Mode == ThemeMode.TokyoNight;
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
                    case ThemeMode.OrangeBlue: return Rgb(255, 255, 255);
                    case ThemeMode.GreenTomato: return Rgb(255, 255, 255);
                    case ThemeMode.CrimsonGray: return Rgb(88, 88, 88);
                    case ThemeMode.HighContrast: return Rgb(0, 0, 0);
                    case ThemeMode.DarkGold: return Rgb(36, 33, 27);
                    case ThemeMode.Mocha: return Rgb(49, 50, 68);
                    case ThemeMode.Forest: return Rgb(52, 63, 68);
                    case ThemeMode.TokyoNight: return Rgb(36, 40, 59);
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
                    case ThemeMode.CrimsonGray: return Rgb(74, 74, 74);
                    case ThemeMode.OrangeBlue: return Rgb(247, 217, 194);
                    case ThemeMode.GreenTomato: return Rgb(216, 239, 224);
                    case ThemeMode.LightPurple: return Rgb(239, 232, 248);
                    case ThemeMode.LightPink: return Rgb(246, 217, 230);
                    case ThemeMode.TokyoDay: return Rgb(225, 226, 231);
                    case ThemeMode.Mint: return Rgb(204, 243, 234);
                    case ThemeMode.HighContrast: return Rgb(0, 0, 0);
                    case ThemeMode.DarkGold: return Rgb(26, 24, 20);
                    case ThemeMode.Mocha: return Rgb(30, 30, 46);
                    case ThemeMode.Forest: return Rgb(45, 53, 59);
                    case ThemeMode.TokyoNight: return Rgb(26, 27, 38);
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
                    case ThemeMode.CrimsonGray: return Rgb(140, 140, 140);
                    case ThemeMode.OrangeBlue: return Rgb(232, 167, 118);
                    case ThemeMode.GreenTomato: return Rgb(127, 198, 149);
                    case ThemeMode.LightPurple: return Rgb(211, 194, 236);
                    case ThemeMode.LightPink: return Rgb(230, 169, 198);
                    case ThemeMode.TokyoDay: return Rgb(180, 181, 185);
                    case ThemeMode.Mint: return Rgb(131, 205, 193);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.DarkGold: return Rgb(68, 61, 48);
                    case ThemeMode.Mocha: return Rgb(88, 91, 112);
                    case ThemeMode.Forest: return Rgb(79, 88, 94);
                    case ThemeMode.TokyoNight: return Rgb(59, 66, 97);
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
                    case ThemeMode.CrimsonGray: return Rgb(119, 119, 119);
                    case ThemeMode.OrangeBlue: return Rgb(242, 201, 172);
                    case ThemeMode.GreenTomato: return Rgb(182, 224, 196);
                    case ThemeMode.LightPurple: return Rgb(226, 214, 243);
                    case ThemeMode.LightPink: return Rgb(242, 207, 223);
                    case ThemeMode.TokyoDay: return Rgb(200, 204, 218);
                    case ThemeMode.Mint: return Rgb(161, 222, 210);
                    case ThemeMode.HighContrast: return Rgb(157, 157, 157);
                    case ThemeMode.DarkGold: return Rgb(54, 48, 42);
                    case ThemeMode.Mocha: return Rgb(69, 71, 90);
                    case ThemeMode.Forest: return Rgb(67, 77, 83);
                    case ThemeMode.TokyoNight: return Rgb(47, 53, 80);
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
                    case ThemeMode.OrangeBlue: return Rgb(27, 27, 27);
                    case ThemeMode.GreenTomato: return Rgb(27, 27, 27);
                    case ThemeMode.LightPurple: return Rgb(27, 27, 27);
                    case ThemeMode.LightPink: return Rgb(27, 27, 27);
                    case ThemeMode.TokyoDay: return Rgb(47, 84, 181);
                    case ThemeMode.Mint: return Rgb(13, 61, 56);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.DarkGold: return Rgb(243, 237, 224);
                    case ThemeMode.Mocha: return Rgb(205, 214, 244);
                    case ThemeMode.Forest: return Rgb(211, 198, 170);
                    case ThemeMode.TokyoNight: return Rgb(192, 202, 245);
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
                    case ThemeMode.GreenTomato: return Rgb(155, 28, 46);
                    case ThemeMode.OrangeBlue: return Rgb(196, 43, 28);
                    case ThemeMode.CrimsonGray: return Rgb(255, 151, 160);
                    case ThemeMode.HighContrast: return Rgb(255, 80, 80);
                    case ThemeMode.DarkGold: return Rgb(255, 138, 138);
                    case ThemeMode.Mocha: return Rgb(243, 139, 168);
                    case ThemeMode.Forest: return Rgb(234, 138, 140);
                    case ThemeMode.TokyoNight: return Rgb(247, 118, 142);
                    case ThemeMode.LightPurple: return Rgb(196, 43, 28);
                    case ThemeMode.LightPink: return Rgb(196, 43, 28);
                    case ThemeMode.TokyoDay: return Rgb(200, 18, 75);
                    case ThemeMode.Mint: return Rgb(196, 43, 28);
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
                    case ThemeMode.CrimsonGray: return Rgb(232, 232, 232);
                    case ThemeMode.OrangeBlue: return Rgb(93, 93, 93);
                    case ThemeMode.GreenTomato: return Rgb(93, 93, 93);
                    case ThemeMode.LightPurple: return Rgb(93, 93, 93);
                    case ThemeMode.LightPink: return Rgb(93, 93, 93);
                    case ThemeMode.TokyoDay: return Rgb(78, 94, 158);
                    case ThemeMode.Mint: return Rgb(47, 107, 99);
                    case ThemeMode.HighContrast: return Rgb(224, 224, 224);
                    case ThemeMode.DarkGold: return Rgb(201, 191, 168);
                    case ThemeMode.Mocha: return Rgb(186, 194, 222);
                    case ThemeMode.Forest: return Rgb(187, 181, 164);
                    case ThemeMode.TokyoNight: return Rgb(169, 177, 214);
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
                    case ThemeMode.CrimsonGray: return Rgb(180, 180, 180);
                    case ThemeMode.OrangeBlue: return Rgb(157, 157, 157);
                    case ThemeMode.GreenTomato: return Rgb(157, 157, 157);
                    case ThemeMode.LightPurple: return Rgb(157, 157, 157);
                    case ThemeMode.LightPink: return Rgb(157, 157, 157);
                    case ThemeMode.TokyoDay: return Rgb(154, 160, 188);
                    case ThemeMode.Mint: return Rgb(143, 177, 171);
                    case ThemeMode.HighContrast: return Rgb(141, 141, 141);
                    case ThemeMode.DarkGold: return Rgb(126, 116, 98);
                    case ThemeMode.Mocha: return Rgb(108, 112, 134);
                    case ThemeMode.Forest: return Rgb(127, 138, 130);
                    case ThemeMode.TokyoNight: return Rgb(107, 114, 154);
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
                case ThemeMode.CrimsonGray: return Rgb(255, 220, 224);
                case ThemeMode.OrangeBlue: return Rgb(158, 68, 10);
                case ThemeMode.GreenTomato: return Rgb(179, 64, 31);
                case ThemeMode.LightPurple: return Rgb(92, 46, 145);
                case ThemeMode.LightPink: return Rgb(189, 21, 88);
                case ThemeMode.TokyoDay: return Rgb(36, 104, 207);
                case ThemeMode.Mint: return Rgb(0, 112, 95);
                case ThemeMode.HighContrast: return Rgb(111, 224, 255);
                case ThemeMode.DarkGold: return Rgb(224, 180, 90);
                case ThemeMode.Mocha: return Rgb(203, 166, 247);
                case ThemeMode.Forest: return Rgb(167, 192, 128);
                case ThemeMode.TokyoNight: return Rgb(122, 162, 247);
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
                    case ThemeMode.GreenTomato: return Rgb(155, 28, 46);
                    case ThemeMode.OrangeBlue: return Rgb(196, 43, 28);
                    case ThemeMode.Dark:
                    case ThemeMode.CrimsonGray: return Rgb(251, 196, 203);
                    case ThemeMode.LightPurple: return Rgb(196, 43, 28);
                    case ThemeMode.LightPink: return Rgb(196, 43, 28);
                    case ThemeMode.TokyoDay: return Rgb(200, 18, 75);
                    case ThemeMode.Mint: return Rgb(196, 43, 28);
                    case ThemeMode.HighContrast: return Rgb(255, 80, 80);
                    case ThemeMode.DarkGold: return Rgb(255, 138, 138);
                    case ThemeMode.Mocha: return Rgb(243, 139, 168);
                    case ThemeMode.Forest: return Rgb(234, 138, 140);
                    case ThemeMode.TokyoNight: return Rgb(247, 118, 142);
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
                case ThemeMode.CrimsonGray: return Rgb(102, 102, 102);
                case ThemeMode.OrangeBlue: return Rgb(240, 240, 240);
                case ThemeMode.GreenTomato: return Rgb(240, 240, 240);
                case ThemeMode.LightPurple: return Rgb(240, 240, 240);
                case ThemeMode.LightPink: return Rgb(240, 240, 240);
                case ThemeMode.TokyoDay: return Rgb(228, 231, 240);
                case ThemeMode.Mint: return Rgb(230, 241, 238);
                case ThemeMode.HighContrast: return Rgb(30, 30, 30);
                case ThemeMode.DarkGold: return Rgb(58, 53, 43);
                case ThemeMode.Mocha: return Rgb(69, 71, 90);
                case ThemeMode.Forest: return Rgb(71, 82, 88);
                case ThemeMode.TokyoNight: return Rgb(59, 66, 97);
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
                    case ThemeMode.CrimsonGray: return Rgb(242, 242, 242);
                    case ThemeMode.OrangeBlue: return Rgb(59, 59, 59);
                    case ThemeMode.GreenTomato: return Rgb(59, 59, 59);
                    case ThemeMode.LightPurple: return Rgb(59, 59, 59);
                    case ThemeMode.LightPink: return Rgb(59, 59, 59);
                    case ThemeMode.TokyoDay: return Rgb(59, 74, 122);
                    case ThemeMode.Mint: return Rgb(47, 76, 71);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.DarkGold: return Rgb(230, 223, 208);
                    case ThemeMode.Mocha: return Rgb(226, 230, 245);
                    case ThemeMode.Forest: return Rgb(224, 217, 196);
                    case ThemeMode.TokyoNight: return Rgb(216, 222, 248);
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
                    case ThemeMode.CrimsonGray: return Rgb(156, 63, 78);
                    case ThemeMode.OrangeBlue: return Rgb(46, 109, 180);
                    case ThemeMode.GreenTomato: return Rgb(46, 125, 100);
                    case ThemeMode.LightPurple: return Rgb(220, 204, 240);
                    case ThemeMode.LightPink: return Rgb(246, 193, 214);
                    case ThemeMode.TokyoDay: return Rgb(61, 89, 161);
                    case ThemeMode.Mint: return Rgb(14, 95, 85);
                    case ThemeMode.HighContrast: return Rgb(111, 224, 255);
                    case ThemeMode.DarkGold: return Rgb(31, 79, 90);
                    case ThemeMode.Mocha: return Rgb(91, 74, 140);
                    case ThemeMode.Forest: return Rgb(59, 90, 71);
                    case ThemeMode.TokyoNight: return Rgb(61, 89, 161);
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
                    case ThemeMode.CrimsonGray: return Rgb(99, 99, 99);
                    case ThemeMode.OrangeBlue: return Rgb(253, 241, 232);
                    case ThemeMode.GreenTomato: return Rgb(240, 249, 243);
                    case ThemeMode.LightPurple: return Rgb(246, 241, 251);
                    case ThemeMode.LightPink: return Rgb(251, 237, 243);
                    case ThemeMode.TokyoDay: return Rgb(244, 245, 249);
                    case ThemeMode.Mint: return Rgb(243, 251, 249);
                    case ThemeMode.HighContrast: return Rgb(16, 16, 16);
                    case ThemeMode.DarkGold: return Rgb(43, 39, 32);
                    case ThemeMode.Mocha: return Rgb(56, 58, 78);
                    case ThemeMode.Forest: return Rgb(58, 70, 75);
                    case ThemeMode.TokyoNight: return Rgb(42, 47, 69);
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
                    case ThemeMode.CrimsonGray: return Rgb(102, 102, 102);
                    case ThemeMode.OrangeBlue: return Rgb(242, 194, 156);
                    case ThemeMode.GreenTomato: return Rgb(183, 224, 198);
                    case ThemeMode.LightPurple: return Rgb(227, 216, 244);
                    case ThemeMode.LightPink: return Rgb(240, 196, 216);
                    case ThemeMode.TokyoDay: return Rgb(216, 220, 232);
                    case ThemeMode.Mint: return Rgb(184, 234, 224);
                    case ThemeMode.HighContrast: return Rgb(20, 20, 20);
                    case ThemeMode.DarkGold: return Rgb(46, 42, 34);
                    case ThemeMode.Mocha: return Rgb(69, 71, 90);
                    case ThemeMode.Forest: return Rgb(61, 72, 77);
                    case ThemeMode.TokyoNight: return Rgb(41, 46, 66);
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
                    case ThemeMode.CrimsonGray: return Rgb(94, 94, 94);
                    case ThemeMode.OrangeBlue: return Rgb(253, 243, 234);
                    case ThemeMode.GreenTomato: return Rgb(242, 250, 245);
                    case ThemeMode.LightPurple: return Rgb(248, 245, 252);
                    case ThemeMode.LightPink: return Rgb(252, 243, 247);
                    case ThemeMode.TokyoDay: return Rgb(246, 247, 250);
                    case ThemeMode.Mint: return Rgb(246, 252, 250);
                    case ThemeMode.HighContrast: return Rgb(10, 10, 10);
                    case ThemeMode.DarkGold: return Rgb(40, 36, 30);
                    case ThemeMode.Mocha: return Rgb(53, 54, 73);
                    case ThemeMode.Forest: return Rgb(56, 67, 74);
                    case ThemeMode.TokyoNight: return Rgb(39, 43, 64);
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
                    case ThemeMode.CrimsonGray: return Rgb(255, 241, 243);
                    case ThemeMode.OrangeBlue: return Rgb(255, 255, 255);
                    case ThemeMode.GreenTomato: return Rgb(255, 255, 255);
                    case ThemeMode.LightPurple: return Rgb(70, 34, 110);
                    case ThemeMode.LightPink: return Rgb(122, 16, 56);
                    case ThemeMode.TokyoDay: return Rgb(255, 255, 255);
                    case ThemeMode.Mint: return Rgb(255, 255, 255);
                    case ThemeMode.HighContrast: return Rgb(0, 0, 0);
                    case ThemeMode.DarkGold: return Rgb(255, 255, 255);
                    case ThemeMode.Mocha: return Rgb(255, 255, 255);
                    case ThemeMode.Forest: return Rgb(255, 255, 255);
                    case ThemeMode.TokyoNight: return Rgb(255, 255, 255);
                    default: return Rgb(0, 90, 158);
                }
            }
        }

        /// <summary>누름(pressed) 배경 — 보조 버튼·토글·메뉴 항목을 누르는 순간 (Brush.PressedBackground, 2026-10-21).
        /// 선택 배경을 빌려 쓰던 자리였는데 선택색이 진한 테마에서 글자가 묻혔다. <see cref="PressedText"/>와 짝이다.</summary>
        public static Color PressedBackground
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(58, 111, 138);
                    case ThemeMode.OrangeBlue: return Rgb(234, 214, 201);
                    case ThemeMode.GreenTomato: return Rgb(205, 230, 214);
                    case ThemeMode.CrimsonGray: return Rgb(112, 104, 105);
                    case ThemeMode.LightPurple: return Rgb(219, 209, 231);
                    case ThemeMode.LightPink: return Rgb(235, 201, 216);
                    case ThemeMode.TokyoDay: return Rgb(208, 213, 227);
                    case ThemeMode.Mint: return Rgb(161, 222, 210);
                    case ThemeMode.HighContrast: return Rgb(50, 101, 115);
                    case ThemeMode.DarkGold: return Rgb(58, 53, 43);
                    case ThemeMode.Mocha: return Rgb(69, 71, 90);
                    case ThemeMode.Forest: return Rgb(71, 82, 88);
                    case ThemeMode.TokyoNight: return Rgb(51, 57, 90);
                    default: return Rgb(199, 225, 246);
                }
            }
        }

        /// <summary>누름(pressed) 글자 — <see cref="PressedBackground"/> 위에서 4.5:1 이상 (Brush.PressedText).</summary>
        public static Color PressedText
        {
            get
            {
                switch (Mode)
                {
                    case ThemeMode.Dark: return Rgb(240, 240, 240);
                    case ThemeMode.OrangeBlue: return Rgb(127, 54, 8);
                    case ThemeMode.GreenTomato: return Rgb(143, 51, 24);
                    case ThemeMode.CrimsonGray: return Rgb(245, 245, 245);
                    case ThemeMode.LightPurple: return Rgb(75, 37, 120);
                    case ThemeMode.LightPink: return Rgb(142, 21, 68);
                    case ThemeMode.TokyoDay: return Rgb(42, 74, 153);
                    case ThemeMode.Mint: return Rgb(13, 61, 56);
                    case ThemeMode.HighContrast: return Rgb(255, 255, 255);
                    case ThemeMode.DarkGold: return Rgb(243, 237, 224);
                    case ThemeMode.Mocha: return Rgb(205, 214, 244);
                    case ThemeMode.Forest: return Rgb(211, 198, 170);
                    case ThemeMode.TokyoNight: return Rgb(192, 202, 245);
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
