using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// 조회 결과의 컬럼을 그대로 그리드에 싣는다 — 컬럼 정의를 손으로 적지 않는다.
    /// 캡션은 비워 두면 <see cref="GridCaptionCatalog"/>가 "LOT_ID" → "Lot Id"로 푼다.
    ///
    /// ★ 컬럼을 늘리거나 줄이거나 순서를 바꾸는 일은 **쿼리**에서 한다. 화면에 남는 것은
    ///   "쿼리 결과만으로는 알 수 없는 것"뿐이다 — 숨길 컬럼, 텍스트가 아닌 셀(배지·링크·
    ///   체크), 문맥에 따라 달라지는 캡션.
    ///
    /// <code>
    /// GridColumns.Of(table)
    ///         .Hide("SUB_TYPE")
    ///         .Column("WF_ID").Caption("Chip Id")
    ///         .Column("SLOT_NO").Center()
    ///         .Bind(this.gridDurableChips);   // 컬럼 + DataSource 한 번에
    /// </code>
    ///
    /// **적지 않아도 되는 것** — 캡션(용어사전), 너비(AutoFitColumns), 이름이
    /// <c>_COLOR</c>로 끝나는 보조 컬럼(자동 제외), 이름이 <c>_TM</c>으로 끝나는
    /// 시각 컬럼(표준 표기 + 가운데 정렬 자동).
    ///
    /// **체크 칸은 두 종류다** — 서버가 내려준 Y/N 값을 보여 주는
    /// <see cref="GridColumnSelection.YesNo"/>와, 사람이 고르라고 화면이 만들어 붙이는
    /// <see cref="GridColumnSelection.SelectBox"/>.
    ///
    /// 컬럼 하나의 설정은 <see cref="Column"/>으로 그 컬럼을 고른 뒤 점으로 잇는다 — 캡션 사전·엔티티와 같은 문법이다(2026-10-04).
    ///
    /// 없는 컬럼 이름은 조용히 무시한다 — 쿼리가 바뀌어도 화면이 깨지지 않는다.
    /// </summary>
    public sealed class GridColumns
    {
        // 선택 체크 칸이 짝으로 쓰는 "토글 가능" 컬럼의 접미사.
        private const string enabledSuffix = "_ENABLED";

        private readonly List<ModernDataGridColumn> columns;
        private DataTable table;

        private GridColumns(DataTable source)
        {
            // 기본 규칙(캡션·_COLOR 제외·_TM 시각)은 Commons의 AutoColumns 한 곳에 있다 —
            // 그리드가 자동으로 만들 때와 같은 결과여야 하므로 여기서 다시 정하지 않는다.
            this.columns = new List<ModernDataGridColumn>(AutoColumns.Of(source));
            this.table = source;
        }

        private GridColumns(DataTable source, ModernDataGridColumn[] declared)
        {
            this.columns = new List<ModernDataGridColumn>(declared);
            this.table = source;
        }

        public static GridColumns Of(DataTable table)
        {
            return new GridColumns(table);
        }

        /// <summary>
        /// 표에 없는 컬럼도 이름으로 만든다 — 엔티티가 정상 0건 응답의 헤더를 <c>DisplayFields</c>로 구성할 때.
        /// 컬럼 규칙은 <see cref="AutoColumns.OfMembers"/>와 같고 표는 그대로 바인딩한다(그리드가 선언 컬럼을 빈 표에 보장한다).
        /// </summary>
        internal static GridColumns Declared(DataTable table, string[] names)
        {
            return new GridColumns(table, AutoColumns.OfMembers(names));
        }

        /// <summary>적은 컬럼만 적은 순서대로 남긴다 — 한 조회가 표와 카드를 함께 채울 때.</summary>
        public GridColumns Only(params string[] names)
        {
            List<ModernDataGridColumn> kept = new List<ModernDataGridColumn>();

            foreach (string name in Names(names))
            {
                ModernDataGridColumn column = this.Find(name);

                if (column != null)
                {
                    kept.Add(column);
                }
            }

            if (kept.Count > 0)
            {
                this.columns.Clear();
                this.columns.AddRange(kept);
            }

            return this;
        }

        public GridColumns Hide(params string[] names)
        {
            foreach (string name in Names(names))
            {
                ModernDataGridColumn column = this.Find(name);

                if (column != null)
                {
                    this.columns.Remove(column);
                }
            }

            return this;
        }

        /// <summary>
        /// 컬럼을 고른다 — 설정은 돌려받은 <see cref="GridColumnSelection"/>에 점으로 잇는다(캡션 사전·엔티티와 같은 문법, 2026-10-04 통일).
        /// <c>.Column("SOURCE_SLOT").Caption("Slot").TopLabel("Source").Width(60d)</c>. 이름을 여러 개 주면 같은 설정이 모두에 걸리고
        /// (<c>.Column(a, b).Link()</c>), 표에 없는 이름은 조용히 무시한다.
        /// </summary>
        public GridColumnSelection Column(params string[] names)
        {
            return new GridColumnSelection(this, Names(names));
        }

        // 사람이 고르는 선택 칸 — 조회 결과에 없는 칸이므로 표에 만들어 붙인다(원본은 건드리지 않고 사본에).
        // 맨 앞에 놓이고 캡션은 없다. locked 면 전부 체크된 채 토글이 막히고 헤더 전체 선택도 없다.
        // 화면은 Column(name).SelectBox(locked) 로 부른다.
        internal void AddSelectBox(string name, bool locked)
        {
            if (this.table == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            string enabledName = name + enabledSuffix;
            this.table = this.table.Copy();

            if (!this.table.Columns.Contains(name))
            {
                this.table.Columns.Add(name, typeof(bool));
            }

            if (!this.table.Columns.Contains(enabledName))
            {
                this.table.Columns.Add(enabledName, typeof(bool));
            }

            foreach (DataRow row in this.table.Rows)
            {
                row[name] = locked;
                row[enabledName] = !locked;
            }

            ModernDataGridColumn column = this.Find(name);

            if (column == null)
            {
                column = new ModernDataGridColumn(name);
                this.columns.Insert(0, column);
            }

            column.Kind = GridColumnKind.CheckBox;
            column.HeaderText = string.Empty;
            column.Width = GridWidths.Check;
            column.HeaderCheckBox = !locked;

            if (locked)
            {
                column.CheckEnabledMember = enabledName;
            }

            this.Hide(enabledName);
            this.First(name);
        }

        /// <summary>적은 순서대로 맨 앞으로 — 쿼리 순서가 화면 순서와 다를 때만 쓴다.</summary>
        public GridColumns First(params string[] names)
        {
            int position = 0;

            foreach (string name in Names(names))
            {
                ModernDataGridColumn column = this.Find(name);

                if (column == null)
                {
                    continue;
                }

                this.columns.Remove(column);
                this.columns.Insert(position, column);
                position++;
            }

            return this;
        }

        public ModernDataGridColumn[] ToArray()
        {
            return this.columns.ToArray();
        }

        /// <summary>
        /// 그리드에 싣는다. 컬럼 구성이 지금과 같으면 다시 만들지 않는다 —
        /// 재구성은 값 필터·찾기 상태를 지우므로, 재조회마다 그것이 풀리면 안 된다.
        /// 결과가 비어 있으면(조회 실패·null) 지금 컬럼을 그대로 둔다.
        /// </summary>
        public void Apply(ModernDataGrid grid)
        {
            if (grid == null || this.columns.Count == 0 || this.SameAs(grid.ColumnDefinitions))
            {
                return;
            }

            grid.ConfigureColumns(this.columns.ToArray());
        }

        /// <summary>컬럼을 싣고 데이터까지 붙인다 — 둘의 순서를 틀릴 일이 없다.</summary>
        public void Bind(ModernDataGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            this.Apply(grid);
            grid.DataSource = this.table;
        }

        /// <summary>
        /// 트리의 값 컬럼에 싣는다 — 트리 컬럼(Id·이름)은 그 자리가 따로 있으므로
        /// 여기서는 <see cref="Only"/>로 고른 몇 칸만 넣는다.
        /// </summary>
        public void Apply(ModernTreeList tree)
        {
            if (tree == null || this.columns.Count == 0)
            {
                return;
            }

            tree.ConfigureColumns(this.columns.ToArray());
        }

        private bool SameAs(ModernDataGridColumn[] current)
        {
            if (current == null || current.Length != this.columns.Count)
            {
                return false;
            }

            for (int index = 0; index < current.Length; index++)
            {
                if (!SameDefinition(current[index], this.columns[index]))
                {
                    return false;
                }
            }

            return true;
        }

        // 이름·캡션만 비교하면 배지·형식·정렬·체크 설정만 바뀐 재적용이 조용히 무시된다(엔티티 표시 정의를 바꿔도
        // 그리드가 그대로인 것). 셀 표시에 관여하는 설정을 함께 비교하되, 사용자가 끌어 바꾸는 폭은 비교하지 않는다 —
        // 재조회마다 폭이 되돌아가면 값 필터·찾기 상태까지 잃는다.
        private static bool SameDefinition(ModernDataGridColumn current, ModernDataGridColumn next)
        {
            return current.DataPropertyName == next.DataPropertyName
                    && current.HeaderText == next.HeaderText
                    && current.Kind == next.Kind
                    && current.AllowFilter == next.AllowFilter
                    && current.Format == next.Format
                    && current.TextAlignment == next.TextAlignment
                    && current.HeaderAlignment == next.HeaderAlignment
                    && current.TopLabelAlignment == next.TopLabelAlignment
                    && current.BottomLabelAlignment == next.BottomLabelAlignment
                    && current.TopLabel == next.TopLabel
                    && current.BottomLabel == next.BottomLabel
                    && current.HeaderGroup == next.HeaderGroup
                    && current.TextColor == next.TextColor
                    && current.TextSemiBold == next.TextSemiBold
                    && current.BadgeAutoColor == next.BadgeAutoColor
                    && current.BadgeColorMember == next.BadgeColorMember
                    && current.BadgeSemanticMember == next.BadgeSemanticMember
                    && current.BadgeSpinValues == next.BadgeSpinValues
                    && current.BadgeSpinBorderMember == next.BadgeSpinBorderMember
                    && current.BadgeWidthValues == next.BadgeWidthValues
                    && current.BadgeWidthByValue == next.BadgeWidthByValue
                    && current.CheckTrueValue == next.CheckTrueValue
                    && current.CheckFalseValue == next.CheckFalseValue
                    && current.CheckEnabledMember == next.CheckEnabledMember
                    && current.HeaderCheckBox == next.HeaderCheckBox
                    && current.ButtonTextMember == next.ButtonTextMember
                    && current.ButtonAutoColor == next.ButtonAutoColor;
        }

        internal ModernDataGridColumn Find(string name)
        {
            foreach (ModernDataGridColumn column in this.columns)
            {
                if (string.Equals(column.DataPropertyName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            return null;
        }

        private static string[] Names(string[] names)
        {
            return names == null ? new string[0] : names;
        }
    }
}
