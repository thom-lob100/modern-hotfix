using System;
using System.Data;

namespace Modern.Lab.Hosting.MasterData
{
    /// <summary>편집기 콤보 한 컬럼에 항목 표가 실렸다 — <see cref="ModernPropertyGrid.ComboSourceApplied"/>. 폼 베이스의
    /// <c>ShareLookup</c> 이 받아 조회조건 콤보에 같은 표를 싣는다(2026-11-09).</summary>
    public sealed class ComboSourceEventArgs : EventArgs
    {
        public ComboSourceEventArgs(string column, DataTable items)
        {
            this.Column = column ?? string.Empty;
            this.Items = items;
        }

        /// <summary>편집기 컬럼 이름.</summary>
        public string Column { get; private set; }

        /// <summary>실린 항목 표 — null 이면 비웠다.</summary>
        public DataTable Items { get; private set; }
    }
}
