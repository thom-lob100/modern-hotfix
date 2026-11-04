using System;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>큐 목록(<see cref="ModernQueueListControl"/>)의 줄 클릭 · 더블클릭 이벤트 인자 — 그 줄의 원본 항목과 행 키를 알린다.</summary>
    public sealed class QueueRowEventArgs : EventArgs
    {
        /// <summary>그 줄이 가리키는 원본 항목(DataRowView 등). 소스가 비어 있으면 null.</summary>
        public object Item { get; }

        /// <summary>KeyMember 로 읽은 행 키. 키 멤버가 없으면 빈 문자열.</summary>
        public string Key { get; }

        public QueueRowEventArgs(object item, string key)
        {
            this.Item = item;
            this.Key = key ?? string.Empty;
        }
    }
}
