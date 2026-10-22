using System;
using System.IO;
using System.Text;
using Modern.Lab.Theming;

namespace Modern.Lab.Hosting.Theming
{
    /// <summary>
    /// 사용자별 테마 설정 — 회사 프레임워크 스킨에 기대지 않고 모던 테마를 고르는 길(2026-10-23).
    /// <c>%AppData%\Modern.Lab\theme.txt</c> 한 줄(테마 이름)을 읽고 쓴다. 회사는 <see cref="FilePath"/> 를 자기 폴더로 바꾼다.
    /// 앱이 뜰 때 <see cref="Apply"/> 를 한 번 부르면 파일의 테마가 <see cref="ModernTheme.Mode"/> 가 된다 — 첫 모던 폼이 만들어지기 전이어야 하므로
    /// 회사는 <c>ModernFormBase.Host.cs</c> 의 <c>AdaptToHost()</c> 첫 호출에서, 홈 샘플은 <c>Program.cs</c> 에서 부른다.
    /// 파일이 없거나 이름이 틀리면 아무것도 바꾸지 않는다.
    /// </summary>
    public static class ModernThemeSetting
    {
        private static string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Modern.Lab", "theme.txt");

        /// <summary>설정 파일 경로 — 기본 <c>%AppData%\Modern.Lab\theme.txt</c>. 회사는 자기 앱 폴더로 바꾼다.</summary>
        public static string FilePath
        {
            get { return filePath; }
            set { filePath = string.IsNullOrWhiteSpace(value) ? filePath : value; }
        }

        /// <summary>파일의 테마 이름을 읽는다 — 없거나 틀리면 false.</summary>
        public static bool TryLoad(out ModernTheme.ThemeMode mode)
        {
            mode = ModernTheme.ThemeMode.Light;

            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                string name = File.ReadAllText(filePath, Encoding.UTF8).Trim();
                return name.Length > 0 && Enum.TryParse(name, true, out mode) && Enum.IsDefined(typeof(ModernTheme.ThemeMode), mode);
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>파일의 테마를 <see cref="ModernTheme.Mode"/> 에 넣는다 — 적용했으면 true.</summary>
        public static bool Apply()
        {
            ModernTheme.ThemeMode mode;

            if (!TryLoad(out mode))
            {
                return false;
            }

            ModernTheme.Mode = mode;
            return true;
        }

        /// <summary>테마 이름을 파일에 쓴다 — 폴더가 없으면 만든다. 다음 실행부터 적용된다.</summary>
        public static void Save(ModernTheme.ThemeMode mode)
        {
            string folder = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(filePath, mode.ToString(), new UTF8Encoding(false));
        }
    }
}
