using System.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Report.Services;

namespace Modern.Lab.Report
{
    public partial class LabOverviewForm
    {
        private const string mapColumnWidths = "84 84 2.5* 3.8* 2.5* 96";
        private const string mapRowHeights = "68 68 68 68 *";

        private void ConfigureMap()
        {
            this.mapProcess.ColumnWidths = mapColumnWidths;
            this.mapProcess.RowHeights = mapRowHeights;
            this.mapProcess.NodeKeyMember = LabOverviewPresenter.KeyColumn;
            this.mapProcess.NodeKindMember = LabOverviewPresenter.KindColumn;
            this.mapProcess.NodeColumnMember = LabOverviewPresenter.ColumnColumn;
            this.mapProcess.NodeRowMember = LabOverviewPresenter.RowColumn;
            this.mapProcess.NodeColumnSpanMember = LabOverviewPresenter.ColumnSpanColumn;
            this.mapProcess.NodeRowSpanMember = LabOverviewPresenter.RowSpanColumn;
            this.mapProcess.NodeTitleMember = LabOverviewPresenter.TitleColumn;
            this.mapProcess.NodeIdMember = LabOverviewPresenter.EquipmentColumn;
            this.mapProcess.NodeStateMember = LabOverviewPresenter.StateColumn;
            this.mapProcess.NodeToneMember = LabOverviewPresenter.ToneColumn;
            this.mapProcess.NodeCountMember = LabOverviewPresenter.CountColumn;
            this.mapProcess.NodeCountWordMember = LabOverviewPresenter.CountWordColumn;
            this.mapProcess.NodeSummaryMember = LabOverviewPresenter.SummaryColumn;
            this.mapProcess.NodeHoldMember = LabOverviewPresenter.HoldColumn;
            this.mapProcess.NodeDetailMember = LabOverviewPresenter.DetailColumn;
            this.mapProcess.NodeToolTipMember = LabOverviewPresenter.ToolTipColumn;
            this.mapProcess.CellNodeMember = LabOverviewPresenter.CellNodeColumn;
            this.mapProcess.CellIdMember = LabOverviewPresenter.CellIdColumn;
            this.mapProcess.CellStateMember = LabOverviewPresenter.CellStateColumn;
            this.mapProcess.CellLotMember = LabOverviewPresenter.CellLotColumn;
            this.mapProcess.CellToneMember = LabOverviewPresenter.CellToneColumn;
            this.mapProcess.LinkFromMember = LabOverviewPresenter.LinkFromColumn;
            this.mapProcess.LinkToMember = LabOverviewPresenter.LinkToColumn;
            this.mapProcess.LinkStyleMember = LabOverviewPresenter.LinkStyleColumn;
            this.mapProcess.LinkLabelMember = LabOverviewPresenter.LinkLabelColumn;
            this.mapProcess.LinkToneMember = LabOverviewPresenter.LinkToneColumn;
            this.mapProcess.NodeClick += this.OnMapNodeClick;
        }

        private static DataTable NodeLayout()
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            DataTable nodes = LabOverviewPresenter.NewNodeTable();
            AddNode(nodes, monStage.Request.Value, "Request", ProcessMapNodeKind.Flow, 0, 0, 2, 1);
            AddNode(nodes, monStage.Send.Value, "Send", ProcessMapNodeKind.Flow, 0, 1, 2, 1);
            AddNode(nodes, monStage.Arrival.Value, "Arrival", ProcessMapNodeKind.Flow, 0, 2, 2, 1);
            AddNode(nodes, monStage.Exchange.Value, "Exchange", ProcessMapNodeKind.Flow, 0, 3, 2, 1);
            AddNode(nodes, LabOverviewPresenter.KeyReturn, "Return", ProcessMapNodeKind.Branch, 0, 4, 1, 1);
            AddNode(nodes, LabOverviewPresenter.KeySwap, "Swap", ProcessMapNodeKind.Branch, 1, 4, 1, 1);
            AddNode(nodes, monStage.Prep.Value, "PREP", ProcessMapNodeKind.Equipment, 2, 1, 1, 4);
            AddNode(nodes, monStage.Fib.Value, "FIB", ProcessMapNodeKind.Equipment, 3, 1, 1, 4);
            AddNode(nodes, monStage.Tem.Value, "TEM", ProcessMapNodeKind.Equipment, 4, 1, 1, 4);
            AddNode(nodes, monStage.Done.Value, "Done", ProcessMapNodeKind.Flow, 5, 2, 1, 1);
            AddNode(nodes, LabOverviewPresenter.KeyScrap, "Scrap due", ProcessMapNodeKind.Branch, 5, 3, 1, 2);
            return nodes;
        }

        private static DataTable LinkLayout()
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            DataTable links = LabOverviewPresenter.NewLinkTable();
            AddLink(links, monStage.Request.Value, monStage.Send.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Send.Value, monStage.Arrival.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Arrival.Value, monStage.Exchange.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Exchange.Value, monStage.Prep.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Prep.Value, monStage.Fib.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Fib.Value, monStage.Tem.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Tem.Value, monStage.Done.Value, ProcessMapLinkStyle.Main);
            AddLink(links, monStage.Exchange.Value, LabOverviewPresenter.KeyReturn, ProcessMapLinkStyle.Branch);
            AddLink(links, monStage.Exchange.Value, LabOverviewPresenter.KeySwap, ProcessMapLinkStyle.Branch);
            AddLink(links, monStage.Done.Value, LabOverviewPresenter.KeyScrap, ProcessMapLinkStyle.Branch);
            AddLink(links, monStage.Tem.Value, monStage.Fib.Value, ProcessMapLinkStyle.Rework);
            return links;
        }

        private static void AddNode(DataTable nodes, string key, string title, ProcessMapNodeKind kind, int column, int row, int columnSpan, int rowSpan)
        {
            DataRow node = nodes.NewRow();
            node[LabOverviewPresenter.KeyColumn] = key;
            node[LabOverviewPresenter.KindColumn] = kind.ToString();
            node[LabOverviewPresenter.TitleColumn] = title;
            node[LabOverviewPresenter.ColumnColumn] = column;
            node[LabOverviewPresenter.RowColumn] = row;
            node[LabOverviewPresenter.ColumnSpanColumn] = columnSpan;
            node[LabOverviewPresenter.RowSpanColumn] = rowSpan;
            nodes.Rows.Add(node);
        }

        private static void AddLink(DataTable links, string from, string to, ProcessMapLinkStyle style)
        {
            DataRow link = links.NewRow();
            link[LabOverviewPresenter.LinkFromColumn] = from;
            link[LabOverviewPresenter.LinkToColumn] = to;
            link[LabOverviewPresenter.LinkStyleColumn] = style.ToString();
            link[LabOverviewPresenter.LinkLabelColumn] = string.Empty;
            link[LabOverviewPresenter.LinkToneColumn] = string.Empty;
            links.Rows.Add(link);
        }
    }
}
