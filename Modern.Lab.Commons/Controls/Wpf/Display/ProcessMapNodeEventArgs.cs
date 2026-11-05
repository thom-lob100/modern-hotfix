using System;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>공정 맵 노드 클릭 이벤트 인자.</summary>
    public sealed class ProcessMapNodeEventArgs : EventArgs
    {
        /// <summary>클릭된 노드의 키.</summary>
        public string Key { get; }

        /// <summary>클릭된 노드의 키로 인자를 만든다.</summary>
        /// <param name="key">노드 키. null 은 빈 문자열로 바꾼다.</param>
        public ProcessMapNodeEventArgs(string key)
        {
            this.Key = key ?? string.Empty;
        }
    }
}
