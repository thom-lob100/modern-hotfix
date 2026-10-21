using System;
using System.Collections.Generic;
using System.Drawing;

using Modern.Lab.Controls.Wpf.Common;

namespace Modern.Lab.Theming
{
    /// <summary>
    /// `Themes/Tokens.xaml`(+ 현재 테마 오버라이드)의 브러시 토큰을 GDI+가 쓸
    /// <see cref="System.Drawing.Color"/>로 읽어 준다.
    ///
    /// 왜 미러가 아니라 리더인가: GDI+로 그리는 컨트롤이 늘면서 필요한 색이
    /// 27종(버튼 하나만으로도)에 이르렀고, 테마 오버라이드들이 그 토큰을 사실상 전부
    /// 오버라이드한다 — 손으로 미러하면 189개 값을 복제해야 하고 토큰을 고칠 때마다
    /// 두 곳을 맞춰야 한다. 게다가 같은 색을 쓰는 WPF 컨트롤(그리드 필터 팝업의
    /// 버튼 등)이 함께 화면에 있으므로 **드리프트가 눈에 바로 보인다**.
    /// 여기서는 WPF 컨트롤이 쓰는 것과 **같은 사전**을 읽으므로 드리프트가 불가능하다.
    ///
    /// 비용: 토큰 사전 첫 로드가 약 20ms이고 이후 <see cref="SharedResourceDictionary"/>
    /// 프로세스 캐시에서 재사용된다(실측). 버튼이 있는 화면은 어차피 다른 WPF 섬을
    /// 갖고 있고, 앱 시작 시 워밍업(<c>ModernWpfWarmup.Run</c>)을 하면 그 시점에
    /// 이미 지불되므로 실질 추가 비용은 없다.
    ///
    /// <see cref="ModernTheme"/>의 색 속성들과의 관계: 그쪽은 예전에 손으로 미러한
    /// 16색이며 그대로 둔다(순수 C#이라 WPF 로드가 필요 없다). 새로 필요한 색은
    /// 이 리더로 읽는다.
    /// </summary>
    public static class ModernTokenColors
    {
        private static readonly object syncRoot = new object();

        // 키 → 색 캐시. 테마가 바뀌면 버린다(테마는 앱 시작 시 한 번 설정되지만
        // 데모/테스트에서 바뀔 수 있으므로 방어한다).
        private static readonly Dictionary<string, Color> cache = new Dictionary<string, Color>();
        private static ModernTheme.ThemeMode cachedMode;
        private static bool cacheInitialized;

        // 토큰 사전 — 현재 테마의 오버라이드까지 병합된 것.
        private static System.Windows.ResourceDictionary tokens;

        /// <summary>
        /// 브러시 토큰의 색을 읽는다. 토큰이 없거나 색 브러시가 아니면
        /// <paramref name="fallback"/>을 돌려준다(폼이 죽지 않게 한다).
        /// </summary>
        /// <param name="brushKey">토큰 키 (예: "Brush.Accent").</param>
        /// <param name="fallback">읽지 못했을 때 쓸 색.</param>
        public static Color Get(string brushKey, Color fallback)
        {
            if (string.IsNullOrEmpty(brushKey))
            {
                return fallback;
            }

            lock (syncRoot)
            {
                EnsureCacheForCurrentTheme();

                Color cached;
                if (cache.TryGetValue(brushKey, out cached))
                {
                    return cached;
                }

                Color resolved = Resolve(brushKey, fallback);
                cache[brushKey] = resolved;
                return resolved;
            }
        }

        // 테마가 바뀌었으면 캐시와 사전을 버린다.
        private static void EnsureCacheForCurrentTheme()
        {
            if (cacheInitialized && cachedMode == ModernTheme.Mode)
            {
                return;
            }

            cache.Clear();
            tokens = null;
            cachedMode = ModernTheme.Mode;
            cacheInitialized = true;
        }

        // 토큰 사전에서 브러시를 찾아 색으로 변환한다.
        private static Color Resolve(string brushKey, Color fallback)
        {
            try
            {
                if (tokens == null)
                {
                    // 컨트롤 XAML과 같은 경로로 로드한다 — SharedResourceDictionary가
                    // 현재 테마 오버라이드를 함께 병합하고 프로세스 캐시를 쓴다.
                    SharedResourceDictionary shared = new SharedResourceDictionary();
                    shared.Source = new Uri(
                            "/Modern.Lab.Commons;component/Themes/Tokens.xaml", UriKind.Relative);
                    tokens = shared;
                }

                System.Windows.Media.SolidColorBrush brush =
                        tokens[brushKey] as System.Windows.Media.SolidColorBrush;

                if (brush == null)
                {
                    return fallback;
                }

                System.Windows.Media.Color value = brush.Color;
                return Color.FromArgb(value.A, value.R, value.G, value.B);
            }
            catch (Exception)
            {
                // 토큰을 읽지 못해도 화면은 떠야 한다.
                return fallback;
            }
        }

        /// <summary>
        /// 두 색을 비율로 섞는다 — WPF의 <c>Opacity</c>를 GDI+에서 흉내낼 때 쓴다
        /// (GDI 텍스트 렌더러는 알파를 무시하므로 배경과 미리 섞어야 한다).
        /// </summary>
        /// <param name="foreground">앞 색.</param>
        /// <param name="background">뒤 색(불투명이라고 가정).</param>
        /// <param name="foregroundRatio">앞 색의 비율 0~1.</param>
        public static Color Blend(Color foreground, Color background, double foregroundRatio)
        {
            double ratio = foregroundRatio;

            if (ratio < 0d)
            {
                ratio = 0d;
            }

            if (ratio > 1d)
            {
                ratio = 1d;
            }

            double inverse = 1d - ratio;

            return Color.FromArgb(
                    255,
                    (int)Math.Round((foreground.R * ratio) + (background.R * inverse)),
                    (int)Math.Round((foreground.G * ratio) + (background.G * inverse)),
                    (int)Math.Round((foreground.B * ratio) + (background.B * inverse)));
        }
    }
}
