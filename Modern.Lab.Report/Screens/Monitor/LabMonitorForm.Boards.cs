using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Report.Services;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Input;

namespace Modern.Lab.Report
{
    public partial class LabMonitorForm
    {
        private const int tileHeight = 64;
        private const int sideWidth = 300;
        private const int lineSideWidth = 340;
        private const int branchButtonHeight = 28;
        private const int branchRowHeight = 28;
        private const int handlerHeight = 236;

        private sealed class StageColumn
        {
            public Panel Panel;
            public ModernLabel RunTitle;
            public ModernLabel RunSub;
            public ModernStatusBadge Lamp;
            public ModernLabel QueueCount;
            public ModernHandlerLayout Handler;
            public ModernQueueList Queue;
        }

        private readonly Dictionary<string, ModernStageTile> flowTiles = new Dictionary<string, ModernStageTile>(StringComparer.Ordinal);
        private readonly Dictionary<string, ModernStageTile> lineTiles = new Dictionary<string, ModernStageTile>(StringComparer.Ordinal);
        private readonly Dictionary<string, StageColumn> flowColumns = new Dictionary<string, StageColumn>(StringComparer.Ordinal);
        private readonly Dictionary<string, StageColumn> lineColumns = new Dictionary<string, StageColumn>(StringComparer.Ordinal);
        private readonly Dictionary<string, ModernQueueList> branchLists = new Dictionary<string, ModernQueueList>(StringComparer.Ordinal);
        private readonly List<KeyValuePair<string, ModernButton>> branchButtons = new List<KeyValuePair<string, ModernButton>>();

        private ModernQueueList listFlowAttention;
        private ModernQueueList listLineAttention;
        private ModernQueueList listIncoming;
        private ModernLabel lblFlowStorageFoups;
        private ModernLabel lblFlowStorageStored;
        private ModernLabel lblFlowStorageTrays;
        private ModernLabel lblLineStorage;

        private void InitializeBoards()
        {
            this.BuildFlowBoard();
            this.BuildLineBoard();
        }

        private static readonly float[] FlowWeights = new float[] { 10f, 10f, 8f, 10f, 13f, 16f, 13f, 10f };

        private void BuildFlowBoard()
        {
            string[] stages = this.StageKeys();
            this.flowPanel.SuspendLayout();
            this.flowPanel.ColumnCount = stages.Length + 1;
            this.flowPanel.RowCount = 3;
            this.flowPanel.ColumnStyles.Clear();
            this.flowPanel.RowStyles.Clear();

            for (int index = 0; index < stages.Length; index++)
            {
                this.flowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, FlowWeights[index]));
            }

            this.flowPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, sideWidth));
            this.flowPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, tileHeight));
            this.flowPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            this.flowPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            for (int index = 0; index < stages.Length; index++)
            {
                ModernStageTile tile = this.NewTile(stages[index], index == stages.Length - 1);
                this.flowTiles[stages[index]] = tile;
                this.flowPanel.Controls.Add(tile, index, 0);

                StageColumn column = this.NewFlowColumn(stages[index]);
                this.flowColumns[stages[index]] = column;
                this.flowPanel.Controls.Add(column.Panel, index, 2);
            }

            Panel side = new Panel();
            side.Dock = DockStyle.Fill;
            side.Margin = new Padding(12, 0, 0, 0);
            side.BackColor = Modern.Lab.Theming.ModernTheme.Surface;
            side.Padding = new Padding(10, 6, 10, 8);

            this.listFlowAttention = this.NewQueueList("Nothing needs attention");
            side.Controls.Add(this.listFlowAttention);
            side.Controls.Add(this.NewSectionLabel("Attention", 26));

            Panel storage = new Panel();
            storage.Dock = DockStyle.Top;
            storage.Height = 100;
            this.lblFlowStorageTrays = this.NewLineLabel("Trays -");
            this.lblFlowStorageStored = this.NewLineLabel("Stored FOUP -");
            this.lblFlowStorageFoups = this.NewLineLabel("STB FOUP -");
            storage.Controls.Add(this.lblFlowStorageTrays);
            storage.Controls.Add(this.lblFlowStorageStored);
            storage.Controls.Add(this.lblFlowStorageFoups);
            storage.Controls.Add(this.NewSectionLabel("Storage", 26));
            side.Controls.Add(storage);

            this.flowPanel.Controls.Add(side, stages.Length, 0);
            this.flowPanel.SetRowSpan(side, 3);
            this.flowPanel.ResumeLayout();
        }

        private StageColumn NewFlowColumn(string stageKey)
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            bool equipment = stageKey == monStage.Prep.Value || stageKey == monStage.Fib.Value || stageKey == monStage.Tem.Value;
            StageColumn column = new StageColumn();
            column.Panel = new Panel();
            column.Panel.Dock = DockStyle.Fill;
            column.Panel.Margin = new Padding(0, 0, 1, 0);
            column.Panel.BackColor = Modern.Lab.Theming.ModernTheme.Surface;
            column.Panel.Padding = new Padding(8, 4, 8, 6);

            column.Queue = this.NewQueueList(equipment ? "Queue is empty" : "None");
            column.Panel.Controls.Add(column.Queue);

            if (stageKey == monStage.Exchange.Value)
            {
                column.Panel.Controls.Add(this.NewBranchBlock(focusReturn, "Return", 2));
                column.Panel.Controls.Add(this.NewBranchBlock(focusSwap, "Swap", 1));
            }
            else if (stageKey == monStage.Tem.Value)
            {
                column.Panel.Controls.Add(this.NewBranchBlock(focusRework, "Rework", 2));
            }
            else if (stageKey == monStage.Done.Value)
            {
                column.Panel.Controls.Add(this.NewBranchBlock(focusScrap, "Scrap", 2));
            }

            if (stageKey == monStage.Fib.Value)
            {
                column.Handler = new ModernHandlerLayout();
                column.Handler.Dock = DockStyle.Top;
                column.Handler.Height = handlerHeight;
                column.Panel.Controls.Add(column.Handler);
            }

            if (equipment)
            {
                column.Panel.Controls.Add(this.NewRunningBlock(column, false));
            }

            return column;
        }

        private void BuildLineBoard()
        {
            string[] stages = this.StageKeys();
            this.linePanel.SuspendLayout();

            TableLayoutPanel body = new TableLayoutPanel();
            body.Dock = DockStyle.Fill;
            body.Margin = new Padding(0);
            body.ColumnCount = 4;
            body.RowCount = 1;
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, lineSideWidth));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            this.lineColumns[monStage.Prep.Value] = this.NewLinePanel("PREP", false);
            this.lineColumns[monStage.Fib.Value] = this.NewLinePanel("FIB", true);
            this.lineColumns[monStage.Tem.Value] = this.NewLinePanel("TEM", false);
            body.Controls.Add(this.lineColumns[monStage.Prep.Value].Panel, 0, 0);
            body.Controls.Add(this.lineColumns[monStage.Fib.Value].Panel, 1, 0);
            body.Controls.Add(this.lineColumns[monStage.Tem.Value].Panel, 2, 0);

            TableLayoutPanel side = new TableLayoutPanel();
            side.Dock = DockStyle.Fill;
            side.Margin = new Padding(12, 0, 0, 0);
            side.ColumnCount = 1;
            side.RowCount = 3;
            side.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
            side.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
            side.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

            Panel attention = this.NewSidePanel("Attention");
            this.listLineAttention = this.NewQueueList("Nothing needs attention");
            attention.Controls.Add(this.listLineAttention);
            attention.Controls.SetChildIndex(this.listLineAttention, 0);
            side.Controls.Add(attention, 0, 0);

            Panel incoming = this.NewSidePanel("Incoming · Exchange");
            this.listIncoming = this.NewQueueList("Nothing incoming");
            this.listIncoming.TagMember = LabMonitorPresenter.TagColumn;
            incoming.Controls.Add(this.listIncoming);
            incoming.Controls.SetChildIndex(this.listIncoming, 0);
            side.Controls.Add(incoming, 0, 1);

            Panel storage = this.NewSidePanel("Storage");
            this.lblLineStorage = this.NewLineLabel("-");
            this.lblLineStorage.Dock = DockStyle.Fill;
            storage.Controls.Add(this.lblLineStorage);
            storage.Controls.SetChildIndex(this.lblLineStorage, 0);
            side.Controls.Add(storage, 0, 2);
            body.Controls.Add(side, 3, 0);

            TableLayoutPanel branches = new TableLayoutPanel();
            branches.Dock = DockStyle.Top;
            branches.Height = branchRowHeight + 8;
            branches.Margin = new Padding(0);
            branches.ColumnCount = stages.Length;
            branches.RowCount = 1;
            branches.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            TableLayoutPanel tiles = new TableLayoutPanel();
            tiles.Dock = DockStyle.Top;
            tiles.Height = tileHeight;
            tiles.Margin = new Padding(0);
            tiles.ColumnCount = stages.Length;
            tiles.RowCount = 1;
            tiles.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            for (int index = 0; index < stages.Length; index++)
            {
                tiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / stages.Length));
                branches.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / stages.Length));
                ModernStageTile tile = this.NewTile(stages[index], index == stages.Length - 1);
                this.lineTiles[stages[index]] = tile;
                tiles.Controls.Add(tile, index, 0);
            }

            FlowLayoutPanel exchangeBranches = new FlowLayoutPanel();
            exchangeBranches.Dock = DockStyle.Fill;
            exchangeBranches.Margin = new Padding(0, 4, 0, 4);
            exchangeBranches.WrapContents = false;
            exchangeBranches.Controls.Add(this.NewBranchButton(focusReturn, "Return"));
            exchangeBranches.Controls.Add(this.NewBranchButton(focusSwap, "Swap"));
            branches.Controls.Add(exchangeBranches, 3, 0);
            branches.Controls.Add(this.WrapBranchButton(this.NewBranchButton(focusRework, "Rework")), 6, 0);
            branches.Controls.Add(this.WrapBranchButton(this.NewBranchButton(focusScrap, "Scrap")), 7, 0);

            this.linePanel.Controls.Add(body);
            this.linePanel.Controls.Add(branches);
            this.linePanel.Controls.Add(tiles);
            this.linePanel.ResumeLayout();
        }

        private StageColumn NewLinePanel(string name, bool handler)
        {
            StageColumn column = new StageColumn();
            column.Panel = new Panel();
            column.Panel.Dock = DockStyle.Fill;
            column.Panel.Margin = new Padding(0, 0, 12, 0);
            column.Panel.BackColor = Modern.Lab.Theming.ModernTheme.Surface;
            column.Panel.Padding = new Padding(12, 6, 12, 8);

            column.Queue = this.NewQueueList("Queue is empty");
            column.Panel.Controls.Add(column.Queue);

            if (handler)
            {
                column.Handler = new ModernHandlerLayout();
                column.Handler.Dock = DockStyle.Top;
                column.Handler.Height = handlerHeight + 20;
                column.Panel.Controls.Add(column.Handler);
            }

            column.Panel.Controls.Add(this.NewRunningBlock(column, true));

            Panel head = new Panel();
            head.Dock = DockStyle.Top;
            head.Height = 36;

            column.QueueCount = new ModernLabel();
            column.QueueCount.Kind = LabelKind.Helper;
            column.QueueCount.Dock = DockStyle.Right;
            column.QueueCount.Width = 90;
            column.QueueCount.Text = "queue -";
            head.Controls.Add(column.QueueCount);

            ModernLabel title = new ModernLabel();
            title.Kind = LabelKind.Title;
            title.Dock = DockStyle.Fill;
            title.Padding = new Padding(8, 0, 0, 0);
            title.Text = name;
            head.Controls.Add(title);

            column.Lamp = new ModernStatusBadge();
            column.Lamp.Dock = DockStyle.Left;
            column.Lamp.Width = 64;
            column.Lamp.Semantic = Modern.Lab.Theming.SemanticKind.Neutral;
            column.Lamp.Shape = BadgeShape.Rounded;
            column.Lamp.Text = "-";
            head.Controls.Add(column.Lamp);
            column.Panel.Controls.Add(head);

            return column;
        }

        private Panel NewRunningBlock(StageColumn column, bool large)
        {
            Panel block = new Panel();
            block.Dock = DockStyle.Top;
            block.Height = large ? 58 : 44;
            block.Padding = new Padding(0, 2, 0, 4);

            column.RunSub = new ModernLabel();
            column.RunSub.Kind = LabelKind.Helper;
            column.RunSub.Dock = DockStyle.Fill;
            column.RunSub.Text = "-";
            block.Controls.Add(column.RunSub);

            column.RunTitle = new ModernLabel();
            column.RunTitle.Kind = large ? LabelKind.Title : LabelKind.Body;
            column.RunTitle.Dock = DockStyle.Top;
            column.RunTitle.Height = large ? 30 : 22;
            column.RunTitle.Text = "-";
            block.Controls.Add(column.RunTitle);

            if (!large)
            {
                column.Lamp = new ModernStatusBadge();
                column.Lamp.Dock = DockStyle.Left;
                column.Lamp.Width = 52;
                column.Lamp.Semantic = Modern.Lab.Theming.SemanticKind.Neutral;
                column.Lamp.Shape = BadgeShape.Rounded;
                column.Lamp.Text = "-";
                block.Controls.Add(column.Lamp);
            }

            return block;
        }

        private Panel NewBranchBlock(string branchKey, string label, int rows)
        {
            Panel block = new Panel();
            block.Dock = DockStyle.Bottom;
            block.Height = branchButtonHeight + rows * 48 + 6;
            block.Padding = new Padding(0, 6, 0, 0);

            ModernQueueList list = this.NewQueueList("None");
            this.branchLists[branchKey] = list;
            block.Controls.Add(list);
            block.Controls.Add(this.NewBranchButton(branchKey, label));
            return block;
        }

        private ModernButton NewBranchButton(string branchKey, string label)
        {
            ModernButton button = new ModernButton();
            button.Kind = ButtonKind.Subtle;
            button.Dock = DockStyle.Top;
            button.Height = branchButtonHeight;
            button.Width = 88;
            button.Margin = new Padding(0, 0, 4, 0);
            button.Tag = branchKey;
            button.Text = label;
            button.Click += this.OnRailClick;
            this.branchButtons.Add(new KeyValuePair<string, ModernButton>(branchKey, button));
            return button;
        }

        private Panel WrapBranchButton(ModernButton button)
        {
            Panel wrap = new Panel();
            wrap.Dock = DockStyle.Fill;
            wrap.Margin = new Padding(0, 4, 0, 4);
            button.Dock = DockStyle.Left;
            wrap.Controls.Add(button);
            return wrap;
        }

        private ModernStageTile NewTile(string stageKey, bool last)
        {
            ModernStageTile tile = new ModernStageTile();
            tile.Dock = DockStyle.Fill;
            tile.Margin = new Padding(0);
            tile.Key = stageKey;
            tile.Label = this.StageLabel(stageKey);
            tile.Count = "-";
            tile.ShowChevron = !last;
            tile.Click += this.OnTileClick;
            return tile;
        }

        private ModernQueueList NewQueueList(string emptyText)
        {
            ModernQueueList list = new ModernQueueList();
            list.Dock = DockStyle.Fill;
            list.EmptyText = emptyText;
            this.ConfigureList(list);
            return list;
        }

        private Panel NewSidePanel(string title)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = new Padding(0, 0, 0, 12);
            panel.BackColor = Modern.Lab.Theming.ModernTheme.Surface;
            panel.Padding = new Padding(10, 6, 10, 8);
            panel.Controls.Add(this.NewSectionLabel(title, 26));
            return panel;
        }

        private ModernLabel NewSectionLabel(string text, int height)
        {
            ModernLabel label = new ModernLabel();
            label.Kind = LabelKind.Label;
            label.Dock = DockStyle.Top;
            label.Height = height;
            label.Text = text;
            return label;
        }

        private ModernLabel NewLineLabel(string text)
        {
            ModernLabel label = new ModernLabel();
            label.Kind = LabelKind.Body;
            label.Dock = DockStyle.Top;
            label.Height = 24;
            label.Text = text;
            return label;
        }

        private string[] StageKeys()
        {
            MonitorFields.MonStage monStage = Schemas.MonitorLot.MonStage;
            return new string[]
            {
                monStage.Request.Value, monStage.Send.Value, monStage.Arrival.Value, monStage.Exchange.Value,
                monStage.Prep.Value, monStage.Fib.Value, monStage.Tem.Value, monStage.Done.Value
            };
        }

        private void OnTileClick(object sender, StageTileEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Key))
            {
                this.ShowFocus(e.Key);
            }
        }
    }
}
