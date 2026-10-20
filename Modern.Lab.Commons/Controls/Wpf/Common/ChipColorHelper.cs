using System;
using System.Windows.Media;

namespace Modern.Lab.Controls.Wpf.Common
{
    /// <summary>
    /// 칩/배지류가 공유하는 색 처리 유틸리티.
    /// 배경색 문자열 파싱과, 배경에서 같은 색상 계열의 글자색을 유도하는
    /// 규칙(밝은 배경 → 진한 톤, 어두운 배경 → 밝은 톤)을 담는다.
    /// </summary>
    internal static class ChipColorHelper
    {
        /// <summary>
        /// "#DBEAFE" 같은 hex 또는 "SkyBlue" 같은 색 이름 문자열을 파싱한다.
        /// 빈 값/파싱 불가면 false를 반환하며 예외를 던지지 않는다.
        /// </summary>
        internal static bool TryParseColor(string text, out Color color)
        {
            color = Colors.Transparent;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            try
            {
                color = (Color)ColorConverter.ConvertFromString(text.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 색 문자열("#0078D4", "Red" 등)을 Freeze된 SolidColorBrush로 해석한다.
        /// 빈 값/해석 불가면 null을 반환하며 예외를 던지지 않는다 — 잘못된 색
        /// 값이 화면 전체를 깨뜨리지 않게 호출 측이 기본색으로 폴백하게 한다.
        /// </summary>
        internal static Brush TryCreateBrush(string text)
        {
            Color color;

            if (TryParseColor(text, out color))
            {
                SolidColorBrush brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }

            return null;
        }

        /// <summary>
        /// 그룹 키 문자열 → 배지 색 문자열 (그리드/트리 배지의 BadgeGroupMember
        /// 공통 규칙): 0 이상의 정수면 Palette 인덱스에 직결(prefix 안정이라
        /// count와 무관하게 같은 색), 그 외 문자열 키는 Palette.FromValue로 유도.
        /// </summary>
        internal static string GroupColorText(string key)
        {
            int index;

            if (int.TryParse(
                    (key ?? string.Empty).Trim(),
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out index)
                && index >= 0)
            {
                return Modern.Lab.Theming.Palette.GetColors(index + 1)[index];
            }

            return Modern.Lab.Theming.Palette.FromValue(key);
        }

        /// <summary>
        /// 배경색에서 글자색을 유도한다: 색상(Hue)은 유지하고 명도만 반대쪽으로.
        /// - 밝은 배경 → 같은 색상의 진한 톤 (채도 보강 + 명도 0.30)
        /// - 어두운 배경 → 같은 색상의 아주 밝은 톤 (명도 0.95)
        /// - 무채색 배경 → 밝기에 따라 중립 진회색 또는 흰색
        ///
        /// 유도 결과가 WCAG 대비 4.5:1에 못 미치면(중간톤 배경 — 같은 Hue 톤으로는
        /// 대비가 안 나온다) 검정/흰색 중 대비가 큰 쪽으로 강등한다. 파스텔·다크
        /// 칩처럼 이미 4.5를 넘는 배경은 기존 결과가 그대로 유지된다.
        /// </summary>
        internal static Color DeriveForeground(Color background)
        {
            double hue;
            double saturation;
            double lightness;
            RgbToHsl(background, out hue, out saturation, out lightness);

            // 상대 휘도(0~1)로 밝은/어두운 배경을 판정한다 (HSL 명도보다 지각에 가깝다).
            double luminance = ((0.2126 * background.R) + (0.7152 * background.G) + (0.0722 * background.B)) / 255.0;

            Color derived;

            if (saturation < 0.15)
            {
                derived = luminance < 0.5 ? Colors.White : Color.FromRgb(0x37, 0x41, 0x51);
            }
            else if (luminance < 0.5)
            {
                derived = HslToRgb(hue, saturation, 0.95);
            }
            else
            {
                derived = HslToRgb(hue, Math.Max(saturation, 0.55), 0.30);
            }

            return EnsureMinimumContrast(derived, background, hue, saturation, luminance < 0.5);
        }

        /// <summary>
        /// 두 색을 지정 비율로 섞는다 (0 = first 그대로, 1 = second 그대로).
        /// 칩 배경을 전경색 쪽으로 조금씩 끌어 버튼 hover/pressed 단계색이나
        /// 중간 톤 테두리를 만들 때 쓴다.
        /// </summary>
        internal static Color Blend(Color first, Color second, double amount)
        {
            double ratio = Math.Max(0.0, Math.Min(1.0, amount));

            return Color.FromRgb(
                (byte)Math.Round(first.R + ((second.R - first.R) * ratio)),
                (byte)Math.Round(first.G + ((second.G - first.G) * ratio)),
                (byte)Math.Round(first.B + ((second.B - first.B) * ratio)));
        }

        /// <summary>
        /// 색조의 색상(hue)은 지키고 밝기를 표면에 붙여 만든 배경 — 글자가 밝은 테마는 표면보다 조금 밝게, 어두운 글자의 테마는
        /// 조금 어둡게 시작해, <paramref name="text"/>가 <paramref name="minimumContrast"/>에 닿을 때까지 글자에서 먼 쪽으로 밝기를
        /// 옮긴다. 카드처럼 "테마 표면 위의 색 면"이 팔레트 칩 밝기 그대로라 글자가 묻히는 자리(중간톤 테마의 KPI 카드, 2026-10-20)에
        /// 쓴다. 채도는 색조와 0.40 중 큰 쪽이라 표면 옆에서 색이 읽힌다.
        /// </summary>
        internal static Color AnchorToSurface(Color tone, Color surface, Color text, double minimumContrast)
        {
            double hue;
            double saturation;
            double toneLightness;
            RgbToHsl(tone, out hue, out saturation, out toneLightness);

            double surfaceHue;
            double surfaceSaturation;
            double surfaceLightness;
            RgbToHsl(surface, out surfaceHue, out surfaceSaturation, out surfaceLightness);

            bool lightText = WcagLuminance(text) > WcagLuminance(surface);
            double step = lightText ? -0.02 : 0.02;
            double lightness = surfaceLightness + (lightText ? 0.07 : -0.07);
            double chroma = Math.Max(saturation, 0.40);
            Color candidate = HslToRgb(hue, chroma, Clamp01(lightness));

            for (int attempt = 0; attempt < 50 && WcagContrast(text, candidate) < minimumContrast; attempt++)
            {
                lightness = Clamp01(lightness + step);
                candidate = HslToRgb(hue, chroma, lightness);

                if (lightness <= 0.0 || lightness >= 1.0)
                {
                    break;
                }
            }

            return candidate;
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0.0, Math.Min(1.0, value));
        }

        // 유도된 전경이 4.5:1(WCAG 일반 텍스트 기준)에 못 미치면 **같은 색상의 톤을 대비가 닿을 때까지 민다** —
        // 밝은 배경이면 더 진하게(명도 0.30 → 0), 어두운 배경이면 더 밝게(0.95 → 1). 그래도 안 되면 검정/흰색 중
        // 대비가 큰 쪽이다. 전에는 한 번에 검정/흰색으로 뛰어, 중간톤 파랑 칩(TrackIn)만 옆 칩과 달리 새까만 글자가
        // 되고 액센트 바탕에서는 흰색(4.53)과 검정(4.64)이 박빙이라 검정이 뽑혀 거꾸로 보였다(2026-10-20 현장 보고).
        private static Color EnsureMinimumContrast(
                Color derived, Color background, double hue, double saturation, bool darkBackground)
        {
            if (WcagContrast(derived, background) >= 4.5)
            {
                return derived;
            }

            double tone = Math.Max(saturation, 0.55);

            for (int step = 1; step <= 6; step++)
            {
                double lightness = darkBackground
                        ? Math.Min(1.0, 0.95 + (step * 0.01))
                        : Math.Max(0.0, 0.30 - (step * 0.05));
                Color candidate = HslToRgb(hue, tone, lightness);

                if (WcagContrast(candidate, background) >= 4.5)
                {
                    return candidate;
                }
            }

            Color black = Color.FromRgb(0, 0, 0);
            Color white = Color.FromRgb(255, 255, 255);

            return WcagContrast(black, background) >= WcagContrast(white, background)
                    ? black
                    : white;
        }

        // WCAG 대비율(1~21). 상대 휘도는 감마 역보정(선형화)한 채널로 계산한다 —
        // 위의 단순 가중 평균은 밝/어두움 분기용이고, 대비 판정은 이 정식 값으로 한다.
        internal static double WcagContrast(Color first, Color second)
        {
            double firstLuminance = WcagLuminance(first);
            double secondLuminance = WcagLuminance(second);
            double lighter = Math.Max(firstLuminance, secondLuminance);
            double darker = Math.Min(firstLuminance, secondLuminance);

            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double WcagLuminance(Color color)
        {
            return (0.2126 * LinearChannel(color.R))
                    + (0.7152 * LinearChannel(color.G))
                    + (0.0722 * LinearChannel(color.B));
        }

        private static double LinearChannel(byte value)
        {
            double channel = value / 255.0;

            return channel <= 0.03928
                    ? channel / 12.92
                    : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }

        // RGB → HSL 변환. hue/saturation/lightness 모두 0~1 범위.
        private static void RgbToHsl(Color color, out double hue, out double saturation, out double lightness)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            lightness = (max + min) / 2.0;

            if (delta == 0.0)
            {
                hue = 0.0;
                saturation = 0.0;
                return;
            }

            if (lightness > 0.5)
            {
                saturation = delta / (2.0 - max - min);
            }
            else
            {
                saturation = delta / (max + min);
            }

            if (max == r)
            {
                hue = (((g - b) / delta) + (g < b ? 6.0 : 0.0)) / 6.0;
            }
            else if (max == g)
            {
                hue = (((b - r) / delta) + 2.0) / 6.0;
            }
            else
            {
                hue = (((r - g) / delta) + 4.0) / 6.0;
            }
        }

        // HSL → RGB 변환. hue/saturation/lightness 모두 0~1 범위.
        private static Color HslToRgb(double hue, double saturation, double lightness)
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

            return Color.FromRgb(
                (byte)Math.Round(r * 255.0),
                (byte)Math.Round(g * 255.0),
                (byte)Math.Round(b * 255.0));
        }

        // HSL 보조: 색상 위치 t에 해당하는 채널 값(0~1)을 구한다.
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
