using System;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>단계 타일(<see cref="ModernStageTileControl"/>) 클릭 이벤트 인자 — 타일의 키를 알린다.</summary>
    public sealed class StageTileEventArgs : EventArgs
    {
        /// <summary>타일의 <c>Key</c>. 없으면 빈 문자열.</summary>
        public string Key { get; }

        public StageTileEventArgs(string key)
        {
            this.Key = key ?? string.Empty;
        }
    }
}
