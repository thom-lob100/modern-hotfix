using System;
using System.Globalization;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// 의미(시맨틱) 배지/칩 배경색 — Palette(의미 없는 종류 구분)의 자매 API.
    /// "정상=초록/오류=빨강"처럼 <b>의미가 정해진 상태색</b>을 모든 화면이 같은
    /// 값으로 쓰게 한다. 반환은 "#RRGGBB" 문자열 — 그리드/트리 <c>*ColorMember</c>,
    /// <c>ModernStatusBadge.Color</c>에 그대로 넣는 형식이고, 글자색은 각 컨트롤이
    /// 배경색에서 자동 유도한다.
    ///
    /// 역할 분담(리뷰 기준): 의미가 있는 색은 SemanticColors, 의미 없이 종류만
    /// 구분하는 색은 Palette — 헥사 문자열을 화면 소스에 직접 쓰지 않는다.
    ///
    /// 테마 인식:
    ///   - 라이트 계열(Light·파스텔): 화면들이 관행으로 쓰던 파스텔 틴트 그대로
    ///     (기존 화면의 외관이 바뀌지 않는다).
    ///   - Dark·DarkGold·Mocha·Forest·TokyoNight·Kanagawa: 어두운 칩 — Palette의 같은 테마 칩과 같은 채도/명도.
    ///   - CrimsonGray(미드 그레이): 뮤트한 밝은 칩. OrangeBlue·GreenTomato 는 라이트 틴트.
    ///   - HighContrast: 검정 위 밝은 고채도 칩.
    ///
    /// Neutral은 새 회색을 만들지 않고 테마의 중립 배지 배경
    /// (<see cref="ModernTheme.NeutralBackground"/>)을 그대로 쓴다 — 컨트롤
    /// 기본 배지와 항상 같은 값이 된다 (값 이중 관리 금지).
    /// </summary>
    public static class SemanticColors
    {
        // 의미별 색상(Hue) — 라이트 관행 틴트(#DCFCE7 등)에서 역산한 값.
        // Dark/CrimsonGray 칩은 이 Hue에 테마 성격의 채도/명도를 입혀 만든다.
        private const double successHue = 141.0;
        private const double warningHue = 48.0;
        private const double errorHue = 0.0;
        private const double infoHue = 214.0;
        private const double agingOrangeHue = 24.0;

        // 라이트 계열 고정 틴트 — 프레젠터들이 손으로 고르던 관행 값 그대로.
        private const string lightSuccess = "#DCFCE7";
        private const string lightWarning = "#FEF3C7";
        private const string lightError = "#FEE2E2";
        private const string lightInfo = "#DBEAFE";
        private const string lightAgingOrange = "#FFE0CC";

        // Palette의 칩 레시피와 같은 채도/명도 — 두 API의 칩이 한 식구로 보인다.
        private const double darkChipSaturation = 0.42;
        private const double darkChipLightness = 0.30;
        private const double crimsonChipSaturation = 0.35;
        private const double crimsonChipLightness = 0.80;
        private const double highContrastChipSaturation = 0.85;
        private const double highContrastChipLightness = 0.66;
        private const double darkGoldChipSaturation = 0.40;
        private const double darkGoldChipLightness = 0.32;
        private const double mochaChipSaturation = 0.42;
        private const double mochaChipLightness = 0.36;
        private const double forestChipSaturation = 0.36;
        private const double forestChipLightness = 0.36;
        private const double tokyoNightChipSaturation = 0.42;
        private const double tokyoNightChipLightness = 0.33;
        private const double kanagawaChipSaturation = 0.36;
        private const double kanagawaChipLightness = 0.33;

        /// <summary>정상·완료·가능 (현재 테마 기준).</summary>
        public static string Success
        {
            get { return SuccessOf(ModernTheme.Mode); }
        }

        /// <summary>주의·개입 중·대기 (현재 테마 기준).</summary>
        public static string Warning
        {
            get { return WarningOf(ModernTheme.Mode); }
        }

        /// <summary>불가·실패·위험 (현재 테마 기준).</summary>
        public static string Error
        {
            get { return ErrorOf(ModernTheme.Mode); }
        }

        /// <summary>진행·정보 (현재 테마 기준).</summary>
        public static string Info
        {
            get { return InfoOf(ModernTheme.Mode); }
        }

        /// <summary>무상태·해당없음 (현재 테마 기준) — 컨트롤 기본 배지와 같은 값.</summary>
        public static string Neutral
        {
            get { return NeutralOf(ModernTheme.Mode); }
        }

        /// <summary>지정 테마의 정상·완료·가능 색.</summary>
        public static string SuccessOf(ModernTheme.ThemeMode mode)
        {
            return Chip(mode, successHue, lightSuccess);
        }

        /// <summary>지정 테마의 주의·개입 중·대기 색.</summary>
        public static string WarningOf(ModernTheme.ThemeMode mode)
        {
            return Chip(mode, warningHue, lightWarning);
        }

        /// <summary>지정 테마의 불가·실패·위험 색.</summary>
        public static string ErrorOf(ModernTheme.ThemeMode mode)
        {
            return Chip(mode, errorHue, lightError);
        }

        /// <summary>지정 테마의 진행·정보 색.</summary>
        public static string InfoOf(ModernTheme.ThemeMode mode)
        {
            return Chip(mode, infoHue, lightInfo);
        }

        /// <summary>지정 테마의 무상태·해당없음 색 — 중립 배지 배경 토큰 그대로.</summary>
        public static string NeutralOf(ModernTheme.ThemeMode mode)
        {
            return ToHexString(ModernTheme.NeutralBackgroundOf(mode));
        }

        /// <summary>
        /// 우선순위 배지색 (현재 테마 기준) — 1순위는 Error, 2~3순위는 Warning,
        /// 그 외(4순위 이하·0/음수 = 미지정)는 Neutral. Equipment 대기 Lot의
        /// 기존 규칙을 그대로 표준화한 것이다.
        /// </summary>
        public static string Priority(int rank)
        {
            return Priority(ModernTheme.Mode, rank);
        }

        /// <summary>지정 테마 기준의 우선순위 배지색.</summary>
        public static string Priority(ModernTheme.ThemeMode mode, int rank)
        {
            if (rank == 1)
            {
                return ErrorOf(mode);
            }

            if (rank == 2 || rank == 3)
            {
                return WarningOf(mode);
            }

            return NeutralOf(mode);
        }

        /// <summary>
        /// 경과/심각도 4단계 램프 (현재 테마 기준) — 가벼움→심각함 순서로
        /// 파랑(Info)→호박(Warning)→주황→빨강(Error). Logistics 경과일 배지의
        /// 기존 규칙을 그대로 표준화한 것이다.
        /// 매 호출 새 배열을 돌려준다 — 받은 쪽이 수정해도 안전하다.
        /// </summary>
        public static string[] GetAgingRamp()
        {
            return GetAgingRamp(ModernTheme.Mode);
        }

        /// <summary>지정 테마 기준의 경과/심각도 4단계 램프.</summary>
        public static string[] GetAgingRamp(ModernTheme.ThemeMode mode)
        {
            return new string[]
            {
                InfoOf(mode),
                WarningOf(mode),
                Chip(mode, agingOrangeHue, lightAgingOrange),
                ErrorOf(mode)
            };
        }

        /// <summary>의미 종류(enum) → 배지색 (현재 테마 기준). None은 Neutral과 같다.</summary>
        public static string Of(SemanticKind kind)
        {
            return Of(ModernTheme.Mode, kind);
        }

        /// <summary>지정 테마 기준의 의미 종류(enum) → 배지색.</summary>
        public static string Of(ModernTheme.ThemeMode mode, SemanticKind kind)
        {
            switch (kind)
            {
                case SemanticKind.Success: return SuccessOf(mode);
                case SemanticKind.Warning: return WarningOf(mode);
                case SemanticKind.Error: return ErrorOf(mode);
                case SemanticKind.Info: return InfoOf(mode);
                default: return NeutralOf(mode);
            }
        }

        /// <summary>
        /// 의미 어휘 문자열 → 배지색 (현재 테마 기준). 쿼리가 판정 결과를 내려주는
        /// 그리드/트리 배지의 <c>BadgeSemanticMember</c>가 이 매핑을 쓴다.
        /// </summary>
        public static string FromName(string name)
        {
            return FromName(ModernTheme.Mode, name);
        }

        /// <summary>
        /// 지정 테마 기준의 의미 어휘 문자열 → 배지색. 어휘(대소문자·공백 무관):
        /// Success = "SUCCESS"/"OK"/"Y"/"YES"/"TRUE"/"1",
        /// Error = "ERROR"/"FAIL"/"DANGER"/"N"/"NO"/"FALSE"/"0",
        /// Warning = "WARNING"/"WARN", Info = "INFO"/"PROGRESS",
        /// 그 외(빈 값·"NEUTRAL"·"NONE"·모르는 어휘)는 전부 Neutral —
        /// 서버 어휘가 흔들려도 화면이 깨지지 않는다(예외 없음).
        /// </summary>
        public static string FromName(ModernTheme.ThemeMode mode, string name)
        {
            return Of(mode, KindFromName(name));
        }

        /// <summary>의미 어휘 문자열 → <see cref="SemanticKind"/> (어휘는 <see cref="FromName(ModernTheme.ThemeMode, string)"/> 참조).</summary>
        public static SemanticKind KindFromName(string name)
        {
            switch (Palette.NormalizeValue(name))
            {
                case "SUCCESS":
                case "OK":
                case "Y":
                case "YES":
                case "TRUE":
                case "1":
                    return SemanticKind.Success;
                case "ERROR":
                case "FAIL":
                case "DANGER":
                case "N":
                case "NO":
                case "FALSE":
                case "0":
                    return SemanticKind.Error;
                case "WARNING":
                case "WARN":
                    return SemanticKind.Warning;
                case "INFO":
                case "PROGRESS":
                    return SemanticKind.Info;
                default:
                    return SemanticKind.Neutral;
            }
        }

        /// <summary>Y/N 이분 상태 배지색 (현재 테마 기준) — Y는 Success, N은 Error.</summary>
        public static string YesNo(bool yes)
        {
            return YesNo(ModernTheme.Mode, yes);
        }

        /// <summary>지정 테마 기준의 Y/N 배지색.</summary>
        public static string YesNo(ModernTheme.ThemeMode mode, bool yes)
        {
            return yes ? SuccessOf(mode) : ErrorOf(mode);
        }

        // 테마 성격에 맞는 칩 하나 — 라이트 계열은 관행 틴트, 어두운 계열은
        // Palette의 같은 테마 칩과 같은 채도/명도의 HSL 생성.
        private static string Chip(
            ModernTheme.ThemeMode mode, double hue, string lightTint)
        {
            switch (mode)
            {
                case ModernTheme.ThemeMode.Dark:
                    return Palette.ToHex(hue / 360.0, darkChipSaturation, darkChipLightness);
                case ModernTheme.ThemeMode.CrimsonGray:
                    return Palette.ToHex(hue / 360.0, crimsonChipSaturation, crimsonChipLightness);
                case ModernTheme.ThemeMode.HighContrast:
                    return Palette.ToHex(
                            hue / 360.0, highContrastChipSaturation, highContrastChipLightness);
                case ModernTheme.ThemeMode.DarkGold:
                    return Palette.ToHex(
                            hue / 360.0, darkGoldChipSaturation, darkGoldChipLightness);
                case ModernTheme.ThemeMode.Mocha:
                    return Palette.ToHex(hue / 360.0, mochaChipSaturation, mochaChipLightness);
                case ModernTheme.ThemeMode.Forest:
                    return Palette.ToHex(hue / 360.0, forestChipSaturation, forestChipLightness);
                case ModernTheme.ThemeMode.TokyoNight:
                    return Palette.ToHex(
                            hue / 360.0, tokyoNightChipSaturation, tokyoNightChipLightness);
                case ModernTheme.ThemeMode.Kanagawa:
                    return Palette.ToHex(hue / 360.0, kanagawaChipSaturation, kanagawaChipLightness);
                default:
                    return lightTint;
            }
        }

        // Color → "#RRGGBB" 문자열 (ModernTheme의 GDI+ 미러 값을 배지 계약 형식으로).
        private static string ToHexString(System.Drawing.Color color)
        {
            return string.Format(
                    CultureInfo.InvariantCulture,
                    "#{0:X2}{1:X2}{2:X2}",
                    color.R,
                    color.G,
                    color.B);
        }
    }
}
