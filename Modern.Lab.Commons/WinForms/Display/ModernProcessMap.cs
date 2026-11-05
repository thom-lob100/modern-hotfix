using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// ModernProcessMapControl(WPF)의 WinForms 래퍼 — 단계 노드를 격자에 놓고 흐름 · 가지 · 되돌림 선으로 잇는 공정 맵.
    /// (직접 대응하는 WinForms 컨트롤 없음.)
    ///
    /// 사용 흐름: 폼이 노드 · 연결 · 내부 칸을 행 목록(DataTable / DataView / IList / IEnumerable)으로 만들어
    /// <see cref="SetMapSource"/> 로 통째로 주고(재조회 = 재구성), 노드 클릭을 <see cref="NodeClick"/> 으로 받는다.
    /// 어느 컬럼을 읽을지는 <c>Node*Member</c> · <c>Link*Member</c> · <c>Cell*Member</c> 가 정한다. 컨트롤은 판정하지 않고 그리기만 한다.
    /// 선 · 자물쇠 · 곡선이 노드 위로 지나가야 해서 노드마다 따로 호스트하지 않고 한 장(호스트 하나)으로 그린다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernProcessMap : WpfElementHostBase<ModernProcessMapControl>
    {
        private readonly Dictionary<string, string> fallbackMembers = new Dictionary<string, string>(StringComparer.Ordinal);
        private string fallbackSelectedKey = string.Empty;
        private string fallbackColumnWidths = string.Empty;
        private string fallbackRowHeights = string.Empty;

        /// <summary>사용자가 노드를 누르면 발생한다 — 이미 선택된 노드를 다시 눌러도 난다. 소스 교체 · <see cref="SelectedKey"/> 대입은 내지 않는다.</summary>
        public event EventHandler<ProcessMapNodeEventArgs> NodeClick;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernProcessMap()
        {
            this.Size = new Size(720, 400);

            if (this.Wpf != null)
            {
                this.Wpf.NodeClick += this.OnInnerNodeClick;
            }
        }

        /// <summary>선택된 노드 키(빈 값 = 선택 없음). 대입은 이벤트를 내지 않는다.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedKey
        {
            get { return this.Wpf != null ? this.Wpf.SelectedKey : this.fallbackSelectedKey; }
            set
            {
                this.fallbackSelectedKey = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.SelectedKey = this.fallbackSelectedKey;
                }
            }
        }

        /// <summary>열 폭 목록 — 공백이나 쉼표로 나눈 GridLength 표기(예: "1.7* 2.8* 4.4* 2.8* 1*").</summary>
        [Category("Modern")]
        [Description("Column widths as GridLength tokens separated by spaces (e.g. \"1* 2* 1*\")")]
        [DefaultValue("")]
        public string ColumnWidths
        {
            get { return this.Wpf != null ? this.Wpf.ColumnWidths : this.fallbackColumnWidths; }
            set
            {
                this.fallbackColumnWidths = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.ColumnWidths = this.fallbackColumnWidths;
                }
            }
        }

        /// <summary>행 높이 목록 — 공백이나 쉼표로 나눈 GridLength 표기(예: "56 56 56 56 *").</summary>
        [Category("Modern")]
        [Description("Row heights as GridLength tokens separated by spaces (e.g. \"56 56 *\")")]
        [DefaultValue("")]
        public string RowHeights
        {
            get { return this.Wpf != null ? this.Wpf.RowHeights : this.fallbackRowHeights; }
            set
            {
                this.fallbackRowHeights = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.RowHeights = this.fallbackRowHeights;
                }
            }
        }

        /// <summary>노드 행에서 키를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the node key")]
        [DefaultValue("")]
        public string NodeKeyMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeKeyMember : this.Fallback("NodeKeyMember"); }
            set
            {
                this.fallbackMembers["NodeKeyMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeKeyMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 모양(Flow · Equipment · Branch)을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the node kind (Flow / Equipment / Branch)")]
        [DefaultValue("")]
        public string NodeKindMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeKindMember : this.Fallback("NodeKindMember"); }
            set
            {
                this.fallbackMembers["NodeKindMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeKindMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 격자 열 번호(0부터)를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the grid column (0-based)")]
        [DefaultValue("")]
        public string NodeColumnMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeColumnMember : this.Fallback("NodeColumnMember"); }
            set
            {
                this.fallbackMembers["NodeColumnMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeColumnMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 격자 행 번호(0부터)를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the grid row (0-based)")]
        [DefaultValue("")]
        public string NodeRowMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeRowMember : this.Fallback("NodeRowMember"); }
            set
            {
                this.fallbackMembers["NodeRowMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeRowMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 걸치는 열 수를 읽을 컬럼/속성 이름. 비면 1.</summary>
        [Category("Modern")]
        [Description("Column/property name of the column span (empty = 1)")]
        [DefaultValue("")]
        public string NodeColumnSpanMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeColumnSpanMember : this.Fallback("NodeColumnSpanMember"); }
            set
            {
                this.fallbackMembers["NodeColumnSpanMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeColumnSpanMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 걸치는 행 수를 읽을 컬럼/속성 이름. 비면 1.</summary>
        [Category("Modern")]
        [Description("Column/property name of the row span (empty = 1)")]
        [DefaultValue("")]
        public string NodeRowSpanMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeRowSpanMember : this.Fallback("NodeRowSpanMember"); }
            set
            {
                this.fallbackMembers["NodeRowSpanMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeRowSpanMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 이름을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the node title")]
        [DefaultValue("")]
        public string NodeTitleMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeTitleMember : this.Fallback("NodeTitleMember"); }
            set
            {
                this.fallbackMembers["NodeTitleMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeTitleMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 이름 옆 작은 글자(장비 Id 등)를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the small id next to the title")]
        [DefaultValue("")]
        public string NodeIdMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeIdMember : this.Fallback("NodeIdMember"); }
            set
            {
                this.fallbackMembers["NodeIdMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeIdMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 상태 줄을 읽을 컬럼/속성 이름. 톤 색으로 그린다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the state line (drawn in the tone color)")]
        [DefaultValue("")]
        public string NodeStateMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeStateMember : this.Fallback("NodeStateMember"); }
            set
            {
                this.fallbackMembers["NodeStateMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeStateMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 톤(SemanticKind 이름)을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the node tone (SemanticKind name)")]
        [DefaultValue("")]
        public string NodeToneMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeToneMember : this.Fallback("NodeToneMember"); }
            set
            {
                this.fallbackMembers["NodeToneMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeToneMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 큰 숫자(건수)를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the large count")]
        [DefaultValue("")]
        public string NodeCountMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeCountMember : this.Fallback("NodeCountMember"); }
            set
            {
                this.fallbackMembers["NodeCountMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeCountMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 건수 단위를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the count unit word")]
        [DefaultValue("")]
        public string NodeCountWordMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeCountWordMember : this.Fallback("NodeCountWordMember"); }
            set
            {
                this.fallbackMembers["NodeCountWordMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeCountWordMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 짧은 요약을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the short summary")]
        [DefaultValue("")]
        public string NodeSummaryMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeSummaryMember : this.Fallback("NodeSummaryMember"); }
            set
            {
                this.fallbackMembers["NodeSummaryMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeSummaryMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 Hold 글자를 읽을 컬럼/속성 이름. 값이 있으면 자물쇠와 함께 붙는다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the hold text (shown with a lock when not empty)")]
        [DefaultValue("")]
        public string NodeHoldMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeHoldMember : this.Fallback("NodeHoldMember"); }
            set
            {
                this.fallbackMembers["NodeHoldMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeHoldMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 상세 줄을 읽을 컬럼/속성 이름 — 빈 줄로 묶음을 나누고 묶음 첫 줄은 소제목.</summary>
        [Category("Modern")]
        [Description("Column/property name of the detail lines (blank line separates sections; first line is the caption)")]
        [DefaultValue("")]
        public string NodeDetailMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeDetailMember : this.Fallback("NodeDetailMember"); }
            set
            {
                this.fallbackMembers["NodeDetailMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeDetailMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>노드 행에서 툴팁 글자를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the node tooltip")]
        [DefaultValue("")]
        public string NodeToolTipMember
        {
            get { return this.Wpf != null ? this.Wpf.NodeToolTipMember : this.Fallback("NodeToolTipMember"); }
            set
            {
                this.fallbackMembers["NodeToolTipMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.NodeToolTipMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>내부 칸 행에서 소속 노드 키를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the owning node key of a cell")]
        [DefaultValue("")]
        public string CellNodeMember
        {
            get { return this.Wpf != null ? this.Wpf.CellNodeMember : this.Fallback("CellNodeMember"); }
            set
            {
                this.fallbackMembers["CellNodeMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.CellNodeMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>내부 칸 행에서 칸 이름을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the cell name")]
        [DefaultValue("")]
        public string CellIdMember
        {
            get { return this.Wpf != null ? this.Wpf.CellIdMember : this.Fallback("CellIdMember"); }
            set
            {
                this.fallbackMembers["CellIdMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.CellIdMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>내부 칸 행에서 상태 글자를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the cell state")]
        [DefaultValue("")]
        public string CellStateMember
        {
            get { return this.Wpf != null ? this.Wpf.CellStateMember : this.Fallback("CellStateMember"); }
            set
            {
                this.fallbackMembers["CellStateMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.CellStateMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>내부 칸 행에서 랏 글자를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the cell lot text")]
        [DefaultValue("")]
        public string CellLotMember
        {
            get { return this.Wpf != null ? this.Wpf.CellLotMember : this.Fallback("CellLotMember"); }
            set
            {
                this.fallbackMembers["CellLotMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.CellLotMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>내부 칸 행에서 톤(SemanticKind 이름)을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the cell tone (SemanticKind name)")]
        [DefaultValue("")]
        public string CellToneMember
        {
            get { return this.Wpf != null ? this.Wpf.CellToneMember : this.Fallback("CellToneMember"); }
            set
            {
                this.fallbackMembers["CellToneMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.CellToneMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>연결 행에서 시작 노드 키를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the link start node key")]
        [DefaultValue("")]
        public string LinkFromMember
        {
            get { return this.Wpf != null ? this.Wpf.LinkFromMember : this.Fallback("LinkFromMember"); }
            set
            {
                this.fallbackMembers["LinkFromMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.LinkFromMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>연결 행에서 끝 노드 키를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the link end node key")]
        [DefaultValue("")]
        public string LinkToMember
        {
            get { return this.Wpf != null ? this.Wpf.LinkToMember : this.Fallback("LinkToMember"); }
            set
            {
                this.fallbackMembers["LinkToMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.LinkToMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>연결 행에서 모양(Main · Branch · Rework)을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the link style (Main / Branch / Rework)")]
        [DefaultValue("")]
        public string LinkStyleMember
        {
            get { return this.Wpf != null ? this.Wpf.LinkStyleMember : this.Fallback("LinkStyleMember"); }
            set
            {
                this.fallbackMembers["LinkStyleMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.LinkStyleMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>연결 행에서 이름표 글자를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the link label")]
        [DefaultValue("")]
        public string LinkLabelMember
        {
            get { return this.Wpf != null ? this.Wpf.LinkLabelMember : this.Fallback("LinkLabelMember"); }
            set
            {
                this.fallbackMembers["LinkLabelMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.LinkLabelMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>연결 행에서 톤(SemanticKind 이름)을 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the link tone (SemanticKind name)")]
        [DefaultValue("")]
        public string LinkToneMember
        {
            get { return this.Wpf != null ? this.Wpf.LinkToneMember : this.Fallback("LinkToneMember"); }
            set
            {
                this.fallbackMembers["LinkToneMember"] = value ?? string.Empty;

                if (this.Wpf != null)
                {
                    this.Wpf.LinkToneMember = value ?? string.Empty;
                }
            }
        }

        /// <summary>그리고 있는 노드 수.</summary>
        [Browsable(false)]
        public int NodeCount
        {
            get { return this.Wpf != null ? this.Wpf.NodeCount : 0; }
        }

        /// <summary>
        /// 노드 · 연결 · 내부 칸을 행 목록(DataTable / DataView / IList / IEnumerable)으로 받아 맵을 통째로 바꾼다.
        /// null · 빈 소스 · 없는 컬럼은 빈 값으로 처리하며 예외를 내지 않는다. 이벤트를 내지 않고, 같은 키의 노드가 남으면 선택도 남는다.
        /// </summary>
        /// <param name="nodes">노드 행 목록.</param>
        /// <param name="links">연결 행 목록.</param>
        /// <param name="cells">내부 칸 행 목록.</param>
        public void SetMapSource(object nodes, object links, object cells)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetMapSource(
                        DataSourceConverter.ToItemsSource(nodes),
                        DataSourceConverter.ToItemsSource(links),
                        DataSourceConverter.ToItemsSource(cells));
            }
        }

        /// <summary>맵을 비운다.</summary>
        public void ClearMap()
        {
            this.SetMapSource(null, null, null);
        }

        private string Fallback(string name)
        {
            string value;
            return this.fallbackMembers.TryGetValue(name, out value) ? value : string.Empty;
        }

        private void OnInnerNodeClick(object sender, ProcessMapNodeEventArgs e)
        {
            EventHandler<ProcessMapNodeEventArgs> handler = this.NodeClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
