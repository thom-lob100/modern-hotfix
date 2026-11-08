using System;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// <see cref="GridColumns.Column"/>이 돌려주는 "지금 고른 컬럼" — 컬럼을 먼저 잡고 설정을 점으로 잇는다.
    /// 캡션 사전·엔티티와 같은 어휘다(2026-10-04 통일).
    ///
    /// <code>
    /// GridColumns.Of(table)
    ///         .Column("SOURCE_SLOT").Caption("Slot").TopLabel("Source").TopLabelAlignment(GridTextAlignment.Left).Width(60d)
    ///         .Column("REQ_SERIAL_NO", "DURABLE_ID").Link()
    ///         .Hide("SUB_TYPE")
    ///         .Bind(this.gridPlan);
    /// </code>
    ///
    /// 이름을 여러 개 주면 같은 설정이 모두에 걸린다. 표에 없는 이름은 조용히 무시한다. 각 설정의 뜻은
    /// <see cref="ModernDataGridColumnExtensions"/> 한 곳에 있다. <see cref="GridColumns"/>를 받는 자리에 그대로 넘길 수 있다.
    /// </summary>
    public sealed class GridColumnSelection
    {
        private readonly GridColumns owner;
        private readonly string[] names;

        internal GridColumnSelection(GridColumns owner, string[] names)
        {
            this.owner = owner;
            this.names = names ?? new string[0];
        }

        /// <summary>컬럼 고르기를 끝내고 목록으로 돌아간다 — <see cref="GridColumns"/>를 받는 자리에 그대로 쓸 수 있다.</summary>
        public static implicit operator GridColumns(GridColumnSelection selection)
        {
            return selection == null ? null : selection.owner;
        }

        /// <summary>본 캡션 — 캡션 사전·엔티티에서 받은 위·아래 작은 글씨는 함께 지운다. 필요하면 뒤에 <see cref="TopLabel"/>을 잇는다.</summary>
        public GridColumnSelection Caption(string caption)
        {
            return this.Each(column => column.Caption(caption));
        }

        /// <summary>본 캡션 위의 작은 글씨.</summary>
        public GridColumnSelection TopLabel(string text)
        {
            return this.Each(column => column.TopLabel(text));
        }

        /// <summary>본 캡션 아래의 작은 글씨.</summary>
        public GridColumnSelection BottomLabel(string text)
        {
            return this.Each(column => column.BottomLabel(text));
        }

        /// <summary>머리글 묶음 — 같은 이름의 이웃 컬럼이 한 묶음 머리글 아래 선다. null 이면 묶음 없음.</summary>
        public GridColumnSelection HeaderGroup(string name)
        {
            return this.Each(column => column.HeaderGroup = name);
        }

        public GridColumnSelection Width(double width)
        {
            return this.Each(column => column.Width(width));
        }

        public GridColumnSelection Format(string format)
        {
            return this.Each(column => column.Format(format));
        }

        /// <summary>값 필터 깔때기를 끈다.</summary>
        public GridColumnSelection NoFilter()
        {
            return this.Each(column => column.NoFilter());
        }

        public GridColumnSelection Center()
        {
            return this.Each(column => column.Center());
        }

        public GridColumnSelection Right()
        {
            return this.Each(column => column.Right());
        }

        /// <summary>셀 정렬.</summary>
        public GridColumnSelection TextAlignment(GridTextAlignment alignment)
        {
            return this.Each(column => column.TextAlignment(alignment));
        }

        /// <summary>헤더 캡션 정렬 — 셀 정렬과 따로 정한다.</summary>
        public GridColumnSelection HeaderAlignment(GridTextAlignment alignment)
        {
            return this.Each(column => column.HeaderAlignment(alignment));
        }

        /// <summary>위 작은 글씨 줄의 정렬 — 주지 않으면 헤더 정렬을 따른다.</summary>
        public GridColumnSelection TopLabelAlignment(GridTextAlignment alignment)
        {
            return this.Each(column => column.TopLabelAlignment(alignment));
        }

        /// <summary>아래 작은 글씨 줄의 정렬 — 주지 않으면 헤더 정렬을 따른다.</summary>
        public GridColumnSelection BottomLabelAlignment(GridTextAlignment alignment)
        {
            return this.Each(column => column.BottomLabelAlignment(alignment));
        }

        /// <summary>값에서 색이 나오는 배지 셀.</summary>
        public GridColumnSelection Badge()
        {
            return this.Each(column => column.Badge());
        }

        /// <summary>배지 폭 통일을 끈다.</summary>
        public GridColumnSelection BadgeWidthByValue()
        {
            return this.Each(column => column.BadgeWidthByValue());
        }

        /// <summary>배지 폭을 올 수 있는 값들 기준으로 고정한다.</summary>
        public GridColumnSelection BadgeWidth(params string[] values)
        {
            return this.Each(column => column.BadgeWidth(values));
        }

        /// <summary>그 값일 때 배지가 돌아간다(콤마로 여러 개).</summary>
        public GridColumnSelection Spin(string values)
        {
            return this.Each(column => column.Spin(values));
        }

        /// <summary>색이 다른 컬럼에서 오는 배지.</summary>
        public GridColumnSelection BadgeColor(string colorMember)
        {
            return this.Each(column => column.BadgeColor(colorMember));
        }

        /// <summary>도는지 여부가 다른 컬럼에서 오는 배지.</summary>
        public GridColumnSelection BadgeSpin(string activeMember)
        {
            return this.Each(column => column.BadgeSpin(activeMember));
        }

        public GridColumnSelection Link()
        {
            return this.Each(column => column.Link());
        }

        /// <summary>날짜시각 칸 — <c>_TM</c> 관례를 벗어난 이름에만 쓴다.</summary>
        public GridColumnSelection Time()
        {
            return this.Each(column => column.Time());
        }

        /// <summary>서버가 내려주는 Y/N 플래그를 체크 칸으로. 행마다 토글 가능 여부가 다르면 그 bool 컬럼 이름을 준다(없으면 빈 값).</summary>
        public GridColumnSelection YesNo(string enabledMember)
        {
            return this.Each(column => column.YesNo(enabledMember));
        }

        /// <summary>
        /// 사람이 고르는 선택 칸 — 조회 결과에 없는 칸이므로 표에 만들어 붙인다(원본은 건드리지 않고 사본에). 맨 앞에 놓이고 캡션은 없다.
        /// <paramref name="locked"/>면 전부 체크된 채 토글이 막히고 헤더 전체 선택도 없다.
        /// </summary>
        public GridColumnSelection SelectBox(bool locked)
        {
            foreach (string name in this.names)
            {
                this.owner.AddSelectBox(name, locked);
            }

            return this;
        }

        /// <summary>다른 컬럼을 고른다.</summary>
        public GridColumnSelection Column(params string[] columnNames)
        {
            return this.owner.Column(columnNames);
        }

        /// <summary>적은 컬럼만 적은 순서대로 남긴다.</summary>
        public GridColumns Only(params string[] columnNames)
        {
            return this.owner.Only(columnNames);
        }

        public GridColumns Hide(params string[] columnNames)
        {
            return this.owner.Hide(columnNames);
        }

        /// <summary>적은 순서대로 맨 앞으로.</summary>
        public GridColumns First(params string[] columnNames)
        {
            return this.owner.First(columnNames);
        }

        public ModernDataGridColumn[] ToArray()
        {
            return this.owner.ToArray();
        }

        public void Apply(ModernDataGrid grid)
        {
            this.owner.Apply(grid);
        }

        public void Bind(ModernDataGrid grid)
        {
            this.owner.Bind(grid);
        }

        public void Apply(ModernTreeList tree)
        {
            this.owner.Apply(tree);
        }

        private GridColumnSelection Each(Action<ModernDataGridColumn> apply)
        {
            foreach (string name in this.names)
            {
                ModernDataGridColumn column = this.owner.Find(name);

                if (column != null)
                {
                    apply(column);
                }
            }

            return this;
        }
    }
}
