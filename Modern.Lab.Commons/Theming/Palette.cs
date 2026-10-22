using System;
using System.Collections.Generic;
using System.Globalization;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// 범주 색 팔레트 생성기 — 테마와 종류 수를 주면 그 테마에 어울리는
    /// 배경색 문자열("#RRGGBB") 배열을 돌려준다. 배지/칩 배경이 대표 용도이고,
    /// 색으로 종류를 구분하는 곳(범례, 태그, 시리즈 등)이면 어디든 쓸 수 있다.
    ///
    /// 그리드/트리 배지의 <c>*ColorMember</c> 값, <c>ModernStatusBadge.Color</c> 등
    /// "화면이 데이터로 넣는 색" 계약에 그대로 넣을 수 있는 형식이다. 글자색은
    /// 각 컨트롤이 배경색에서 자동 유도하므로(ChipColorHelper 규칙) 배경 배열만 있으면 된다.
    ///
    /// 색 배정 규칙:
    ///   - 유채색 테마: 테마 액센트의 색상(Hue)에서 시작해 황금각(137.508°)씩 회전한다 —
    ///     연속 색이 서로 최대한 벌어지고, 종류 수가 늘어도 앞쪽 색이 바뀌지 않는다
    ///     (prefix 안정 — 상태 종류가 추가돼도 기존 상태의 색이 유지된다).
    ///     채도/명도는 테마 성격을 따른다: 라이트 계열은 파스텔,
    ///     Dark·DarkGold는 어두운 칩, CrimsonGray(미드 그레이
    ///     표면)는 뮤트한 밝은 칩, OrangeBlue·GreenTomato 는 라이트 틴트, HighContrast는 검정 위 밝은 고채도 칩.
    ///   - 값 유도색(<see cref="FromValue(string)"/>): 값 글자의 해시가 색상 9 × 톤 2 = 18칸 중
    ///     하나를 고른다. 색은 값에서만 나오므로 화면·컬럼이 달라도 같은 값은 같은 색이고,
    ///     서로 다른 값이 같은 칸에 떨어질 수는 있다. 상세 규칙은
    ///     <see cref="FromValue(ModernTheme.ThemeMode, string)"/> 참고.
    ///
    /// 정상=초록/오류=빨강처럼 <b>의미가 정해진 상태색은 이 팔레트가 아니라
    /// <see cref="SemanticColors"/>를 쓸 것</b> — 이것은 색에 의미를 싣지 않는
    /// "종류 구분"용 범주 팔레트다.
    /// </summary>
    public static class Palette
    {
        // 황금각(°) — 연속 인덱스의 색상(Hue)이 서로 최대한 벌어지게 하는 회전 간격.
        private const double goldenAngleDegrees = 137.508;

        // 값 유도색(FromValue)의 색상 슬롯 수 — 360/9 = 40° 간격(빨강·주황·노랑·라임·초록·시안·파랑·보라·분홍).
        // 12슬롯(30°)일 때는 라임·초록·에메랄드·틸 네 칸이 파스텔에서 전부 "초록 계열"로, 스카이·파랑과
        // 보라·자홍·분홍도 서로 같은 계열로 읽혔다(2026-10-03 현장 지적: Create · Reserved · JobStart · Hold 가
        // 전부 초록). 눈이 확실히 가르는 색상만 남긴다 — 칸이 줄어 완전히 같은 색이 될 확률은 조금 오르지만,
        // "다른 칸인데 같은 색으로 보이는" 경우가 없어진다.
        private const uint hueSlotCount = 9u;
        // 값 해시의 시드 — 이 저장소 엔티티에 선언된 상태 값 목록(컬럼 23개)과 Job 이벤트 낱말이
        // **한 컬럼 안에서 같은 칸에 떨어지지 않는** 값을 탐색해 고른 것이다(2026-10-03, 0~200,000 중 유일).
        // 시드 없는 해시에서는 Create = Reserved, JobStart = Hold 가 완전히 같은 색이었다.
        // 선언되지 않은 낱말(회사 데이터)은 여전히 겹칠 수 있다 — 값만으로 색을 정하는 방식의 한계다.
        // 이 값을 바꾸면 모든 자동색이 바뀐다.
        private const uint hashSeed = 80035u;

        // 같은 색상 안에서 값을 가르는 톤 티어 수 — 색 가짓수는 색상 9 × 톤 2 = 18개다.
        // 톤을 세 단계로 두면 단계 간격이 좁아져 "거의 같은 분홍"이 나온다(2026-08-12
        // 현장 결함, OKLab ΔE 0.058). 두 단계로 줄이면 같은 색상의 두 톤이 ΔE 0.12 이상
        // 벌어지면서 색은 전부 연한 범위에 남는다 — 계약 검사가 이 간격을 지킨다.
        // 색상(Hue)이 다른 값은 거리가 작아도 눈이 바로 가른다(빨강 vs 파랑 ΔE 0.055가
        // "거의 같은 분홍" 0.058보다 작은데도 명백히 다르게 보인다 — 2026-08-30 실측).
        // 그래서 벌려야 하는 축은 **같은 색상 안의 톤**뿐이다.
        // 티어 간 명도 차는 눈에 확실히 갈리는 크기로 준다(옛 규칙의 미세 지터가
        // "거의 같은 핑크"를 만들었던 것을 대체한다 — 2026-08-12 현장 보고).
        private const int toneTierCount = 2;
        private const double tierLightnessStep = 0.12;
        private const double tierSaturationStep = 0.05;

        // 라이트 계열 테마의 칩 색 — HSL 공식이 아니라 **큐레이션 표**다
        // (2026-08-13 변경). 채도·명도를 하나의 공식으로 12색에 일괄 적용하면
        // HSL의 지각 비균일 때문에 노랑·연두·시안 계열이 탁하게 나온다("파스텔
        // 같은데 파스텔 느낌이 안 난다"는 실사용 지적). 이 표는 색상(Hue)마다
        // 채도·명도를 따로 보정한 파스텔 계열(초기 화면들이 하드코딩해 쓰던
        // #DBEAFE·#DCFCE7과 같은 계열)이고, 티어(충돌 분리)도 같은 계열의 한
        // 단계 진한 색이라 섞여도 탁해지지 않는다.
        //
        // 행 = 색상 슬롯(9개), 열 = 톤 티어(연한 톤 / 진한 톤). 글자색은 ChipColorHelper가 배경에서
        // 유도하므로 배경만 정의한다.
        //
        // 연한 톤은 한 단계 올린 파스텔이다(2026-10-03) — 가장 옅은 단계(#FEE2E2 · #DCFCE7 …)는 채도가 낮아
        // 색상이 달라도 전부 "흰색에 가까운 무언가"로 읽혔다(가장 가까운 두 칸의 OKLab 거리 0.015).
        // 진한 톤은 연한 톤과 OKLab 거리 0.10 이상 벌어지는 가장 연한 색으로 골랐다. 18칸 전체에서
        // 가장 가까운 두 칸의 거리는 0.039(빨강/분홍 연한 톤)다 — 계약 검사가 이 간격을 지킨다.
        private static readonly string[][] lightSlotChips =
        {
            new string[] { "#FECACA", "#FA8B8B" },   // red
            new string[] { "#FED7AA", "#FCA658" },   // orange
            new string[] { "#FEF08A", "#FACC15" },   // yellow
            new string[] { "#D9F99D", "#A3E635" },   // lime
            new string[] { "#BBF7D0", "#68E696" },   // green
            new string[] { "#A5F3FC", "#22D3EE" },   // cyan
            new string[] { "#BFDBFE", "#7AB5FC" },   // blue
            new string[] { "#DDD6FE", "#B6A0FC" },   // violet
            new string[] { "#FBCFE8", "#F68DC5" }    // pink
        };
        /// <summary>현재 테마(<see cref="ModernTheme.Mode"/>) 기준으로 count개의 배경색을 만든다.</summary>
        public static string[] GetColors(int count)
        {
            return GetColors(ModernTheme.Mode, count);
        }

        /// <summary>
        /// 현재 테마와 무관하게 항상 같은 파스텔 톤 배열을 돌려준다
        /// (Light 테마 레시피 고정). 테마가 무엇이든 색 배열을 고정하고
        /// 싶은 화면용이다.
        /// </summary>
        public static string[] GetPastelColors(int count)
        {
            return GetColors(ModernTheme.ThemeMode.Light, count);
        }

        /// <summary>현재 테마(<see cref="ModernTheme.Mode"/>) 기준, 값 문자열에서 유도한 배경색 하나.</summary>
        public static string FromValue(string value)
        {
            return FromValue(ModernTheme.Mode, value);
        }

        /// <summary>
        /// 지정 테마 기준, 값 문자열에서 유도한 배경색 하나 — 색은 **값에서만** 나온다.
        ///
        /// 값 글자의 해시가 색상 9 × 톤 2 = 18칸 중 하나를 고른다. 등록도 전역 배정표도 없어서
        /// 화면·컬럼·실행이 달라도 같은 값이면 언제나 같은 색이다. 대가로 서로 다른 값이 같은 칸에
        /// 떨어질 수 있다 — 한 컬럼에 값이 많을수록 그렇다. 칸끼리는 눈에 확실히 갈리는 색만 둔다
        /// (hueSlotCount · lightSlotChips 주석).
        ///
        /// 색 자체는 테마에 따라 두 경로다: 라이트 계열은 **큐레이션 파스텔 표**
        /// (<see cref="lightSlotChips"/>), 다크 계열은 테마 레시피 공식(색상은 40° 간격).
        /// 칸 번호는 테마와 무관하므로 테마를 바꿔도 값-색 대응은 유지된다.
        ///
        /// 정규화: 앞뒤 공백 제거 + 대소문자 무시("run" == "RUN" == " Run ").
        /// 빈 값/null은 테마의 중립 배지색(<see cref="SemanticColors.Neutral"/>)이다.
        /// </summary>
        public static string FromValue(ModernTheme.ThemeMode mode, string value)
        {
            string normalized = NormalizeValue(value);

            if (normalized.Length == 0)
            {
                return SemanticColors.NeutralOf(mode);
            }

            // 색은 값에서만 나온다 — 등록도, 전역 배정표도 없다. 그래서 화면·프로세스·
            // 실행 파일이 달라도 같은 값이면 언제나 같은 색이고, "칸이 찬다"도 없다.
            // 대가는 서로 다른 값이 같은 셀에 떨어질 수 있다는 것(색상 9 × 톤 2 = 18칸) —
            // 그 겹침은 화면 자체 검사가 실제 데이터로 잡는다.
            uint hash = HashValue(normalized);
            int cell = (int)(hash % (uint)((int)hueSlotCount * toneTierCount));

            int slot = cell % (int)hueSlotCount;
            int tier = cell / (int)hueSlotCount;

            // 라이트 계열은 공식이 아니라 큐레이션 표에서 — lightSlotChips 주석 참고.
            if (UsesCuratedChips(mode))
            {
                return lightSlotChips[slot][tier];
            }

            double startHue;
            double saturation;
            double lightness;
            GetRecipe(mode, out startHue, out saturation, out lightness);

            // 색상(Hue)은 셀의 슬롯에서 — 시작 Hue는 쓰지 않는다(값이 곧 색이어야
            // 테마 안에서 화면·컬럼 무관하게 같은 값 = 같은 색이 성립한다).
            // 40° 슬롯 스냅 이유는 hueSlotCount 상수 주석 참고.
            double hue = slot * (360.0 / hueSlotCount);

            // 톤 티어는 표면에서 멀어지는 한 방향으로만 — 밝은 칩을 더 밝게 올리면
            // 흰 표면에서 사라지고, 어두운 칩을 더 어둡게 내리면 어두운 표면에
            // 묻힌다. 채도는 티어가 깊어질수록 조금 보강해 탁해지지 않게 한다.
            double lightnessDirection = lightness >= 0.5 ? -1.0 : 1.0;
            lightness = Clamp01(lightness + (tier * tierLightnessStep * lightnessDirection));
            saturation = Clamp01(saturation + (tier * tierSaturationStep));

            return ToHex(hue / 360.0, saturation, lightness);
        }

        // 라이트 계열(파스텔 변형 포함)인가 — 이 테마들의 칩은 큐레이션 표에서
        // 나온다. 다크 계열은 각자의 레시피 공식을 유지한다(다크 칩은 공식으로도 탁해 보이지 않는다).
        private static bool UsesCuratedChips(ModernTheme.ThemeMode mode)
        {
            switch (mode)
            {
                case ModernTheme.ThemeMode.Dark:
                case ModernTheme.ThemeMode.CrimsonGray:
                case ModernTheme.ThemeMode.HighContrast:
                case ModernTheme.ThemeMode.DarkGold:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// 값 비교/판정용 정규화 — 앞뒤 공백 제거 + 대문자화(문화권 불변).
        /// FromValue의 색 유도와 그리드 배지의 스핀 값 판정(BadgeSpinValues)이
        /// 같은 규칙을 쓴다.
        /// </summary>
        public static string NormalizeValue(string value)
        {
            return value == null ? string.Empty : value.Trim().ToUpperInvariant();
        }

        // FNV-1a 32비트 — 프레임워크 string.GetHashCode는 프로세스마다 다를 수
        // 있으므로(레거시/코어 차이 포함) 직접 구현으로 영구 결정성을 보장한다.
        private static uint HashValue(string normalized)
        {
            uint hash = 2166136261u ^ hashSeed;

            for (int index = 0; index < normalized.Length; index++)
            {
                hash = (hash ^ normalized[index]) * 16777619u;
            }

            // 마무리 섞기 — 글자 몇 개만 다른 낱말이 같은 칸으로 몰리지 않게 상위 비트를 아래로 내린다.
            hash ^= hash >> 15;
            hash *= 2246822519u;
            hash ^= hash >> 13;

            return hash;
        }

        /// <summary>지정 테마 기준으로 count개의 배경색을 만든다. count가 0 이하면 빈 배열.</summary>
        public static string[] GetColors(ModernTheme.ThemeMode mode, int count)
        {
            if (count <= 0)
            {
                return new string[0];
            }

            string[] colors = new string[count];

            double startHue;
            double saturation;
            double lightness;
            GetRecipe(mode, out startHue, out saturation, out lightness);

            for (int index = 0; index < count; index++)
            {
                double hue = (startHue + (index * goldenAngleDegrees)) % 360.0;
                colors[index] = ToHex(hue / 360.0, saturation, lightness);
            }

            return colors;
        }

        // 테마별 배지 배경 레시피 — 시작 색상은 테마 액센트에서 유도하고(Hue),
        // 채도/명도만 팔레트 고유의 성격값이다. 액센트에서 시작해야 첫 색부터
        // 화면과 한 식구로 보이고, 액센트를 유도하므로 테마 색이 바뀌어도
        // 여기를 같이 고칠 필요가 없다 (시작 Hue의 이중 관리 금지).
        private static void GetRecipe(
            ModernTheme.ThemeMode mode, out double startHue, out double saturation, out double lightness)
        {
            startHue = ModernTheme.AccentOf(mode).GetHue();

            switch (mode)
            {
                case ModernTheme.ThemeMode.Dark:
                    // 다크 표면(#202020) 위 어두운 칩 — 글자색은 밝게 자동 유도된다.
                    saturation = 0.42;
                    lightness = 0.30;
                    return;
                case ModernTheme.ThemeMode.CrimsonGray:
                    // 미드 그레이 면(CrimsonGray) 위 뮤트한 밝은 칩. OrangeBlue · GreenTomato 는 라이트라 기본 레시피를 쓴다.
                    saturation = 0.35;
                    lightness = 0.80;
                    return;
                case ModernTheme.ThemeMode.HighContrast:
                    // 순검정 표면 위 밝은 고채도 칩 — 글자색은 검정으로 자동 유도된다.
                    saturation = 0.85;
                    lightness = 0.66;
                    return;
                case ModernTheme.ThemeMode.DarkGold:
                    // 웜 차콜 표면(#24211B) 위 어두운 칩 — Dark 와 같은 레시피.
                    saturation = 0.40;
                    lightness = 0.32;
                    return;
                default:
                    // Light·파스텔 계열 — 액센트 Hue에서 시작하는 파스텔.
                    saturation = 0.78;
                    lightness = 0.92;
                    return;
            }
        }

        // 톤 지터가 0~1을 넘지 않게 자른다 (지터는 안쪽 방향이라 실제로는 거의
        // 닿지 않지만, 레시피 값이 바뀌어도 변환식이 깨지지 않게 둔다).
        private static double Clamp01(double value)
        {
            if (value < 0.0)
            {
                return 0.0;
            }

            if (value > 1.0)
            {
                return 1.0;
            }

            return value;
        }

        // HSL(0~1) → "#RRGGBB" 문자열 (SemanticColors도 같은 변환으로 칩을 만든다).
        internal static string ToHex(double hue, double saturation, double lightness)
        {
            double r;
            double g;
            double b;

            if (saturation == 0.0)
            {
                r = lightness;
                g = lightness;
                b = lightness;
            }
            else
            {
                double q;

                if (lightness < 0.5)
                {
                    q = lightness * (1.0 + saturation);
                }
                else
                {
                    q = lightness + saturation - (lightness * saturation);
                }

                double p = (2.0 * lightness) - q;
                r = HueToRgbChannel(p, q, hue + (1.0 / 3.0));
                g = HueToRgbChannel(p, q, hue);
                b = HueToRgbChannel(p, q, hue - (1.0 / 3.0));
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "#{0:X2}{1:X2}{2:X2}",
                (int)Math.Round(r * 255.0),
                (int)Math.Round(g * 255.0),
                (int)Math.Round(b * 255.0));
        }

        // HSL 보조: 색상 위치 t에 해당하는 채널 값(0~1)을 구한다
        // (ChipColorHelper/ModernStatusBadge와 같은 변환식).
        private static double HueToRgbChannel(double p, double q, double t)
        {
            if (t < 0.0)
            {
                t = t + 1.0;
            }

            if (t > 1.0)
            {
                t = t - 1.0;
            }

            if (t < 1.0 / 6.0)
            {
                return p + ((q - p) * 6.0 * t);
            }

            if (t < 1.0 / 2.0)
            {
                return q;
            }

            if (t < 2.0 / 3.0)
            {
                return p + ((q - p) * ((2.0 / 3.0) - t) * 6.0);
            }

            return p;
        }
    }
}
