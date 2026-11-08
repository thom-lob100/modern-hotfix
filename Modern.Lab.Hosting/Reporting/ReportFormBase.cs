using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Messaging;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.WinForms.Controls.Layout;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.Hosting.Reporting
{
    public class ReportFormBase : ModernFormBase
    {
        private const string ChannelReport = "report";
        private const string ChannelCodes = "codes";
        private const string LoadingText = "Loading report...";
        private const string ResetGlyph = "";
        private const string RefreshGlyph = "";

        internal const int LabelWidth = 88;
        internal const int LabelGap = 8;
        internal const int RowHeight = 40;
        internal const int FieldHeight = 32;

        private const int ConditionGap = 24;
        private const int ChipHeight = 24;
        private const int ChipGap = 8;
        private const int ChipClearWidth = 24;
        private const int ButtonWidth = 120;
        private const int ButtonGap = 8;
        private const int FlowLabelWidth = 72;
        private const int KeyPanelWidth = 280;
        private const int KeyBadgeWidth = 64;
        private const int KeyBadgeGap = 4;
        private const int CollapseWidth = 32;
        private const string CollapseGlyph = "\uE70E";
        private const string ExpandGlyph = "\uE70D";
        private const int KpiBandHeight = 72;
        private const int KpiGap = 8;
        private const string NoValueText = "-";

        private readonly List<ModernStepIndicator> indicators = new List<ModernStepIndicator>();
        private readonly List<ColumnStyle> flowStyles = new List<ColumnStyle>();
        private readonly List<ModernKpiCard> kpiCards = new List<ModernKpiCard>();
        private readonly List<ReportDefinition.KpiCard> kpiDefinitions = new List<ReportDefinition.KpiCard>();
        private Panel keyPanel;
        private ColumnStyle keyStyle;
        private Panel kpiZone;
        private TableLayoutPanel kpiBand;
        private readonly List<ReportCondition> primaryConditions = new List<ReportCondition>();
        private readonly List<ReportCondition> drawerConditions = new List<ReportCondition>();
        private readonly List<ModernStatusBadge> chips = new List<ModernStatusBadge>();
        private readonly List<ModernButton> chipClears = new List<ModernButton>();
        private ReportDefinition definition;
        private ReportConditionDrawer drawer;
        private DataTable reportData;
        private ModernCardPanel searchCard;
        private Panel conditionArea;
        private Panel buttonArea;
        private ModernButton btnRefresh;
        private ModernButton btnConditions;
        private ModernButton btnCollapse;
        private ModernLabel lblSummary;
        private bool collapsed;
        private readonly List<ModernStatusBadge> keyBadges = new List<ModernStatusBadge>();
        private ModernLabel lblKey;
        private ModernStatusBadge badgeState;
        private Panel mainZone;
        private ModernDataGrid grid;

        public ReportFormBase()
        {
        }

        protected ModernDataGrid ReportGrid
        {
            get { return this.grid; }
        }

        protected DataTable ReportData
        {
            get { return this.reportData; }
        }

        protected void InitializeReport()
        {
            if (IsDesignTime() || this.definition != null)
            {
                return;
            }

            ReportDefinition report = new ReportDefinition();
            this.DefineReport(report);
            this.definition = report;

            this.BuildLayout();

            this.InitializeModernForm(this.mainZone);
            this.RegisterRefreshShortcut(this.btnRefresh);
            this.RegisterFindShortcut(this.grid);
            this.DeferredResize = true;
        }

        protected virtual void DefineReport(ReportDefinition report)
        {
        }

        protected virtual DataTable RequestReport(RequestParameters parameters)
        {
            throw new NotSupportedException("Report query is not connected.");
        }

        /// <summary>
        /// 코드표 조회 자리 — 코드표 흐름(<c>CodeFlow</c>) · 가지(<c>Branches</c>)가 쓰는 공통코드 한 벌을 돌려준다
        /// (<c>TYP_VAL</c> · <c>DESCRIPTION</c> · <c>SORT_NO</c> · <c>USE_YN</c>). 첫 조회 전에 코드표마다 한 번 부르고 성공하면 다시 묻지 않는다.
        /// 리포트의 <c>.Server.cs</c> 가 재정의한다.
        /// </summary>
        protected virtual DataTable RequestCodes(string codeType)
        {
            throw new NotSupportedException("Code table query is not connected.");
        }

        protected virtual void PrepareRows(DataTable rows)
        {
        }

        protected override void DeclareColumns()
        {
            if (this.grid != null && this.definition != null)
            {
                this.grid.ConfigureColumns(this.definition.BuildColumns);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.definition == null || IsDesignTime() || this.DesignMode)
            {
                return;
            }

            foreach (ReportCondition condition in this.definition.Conditions)
            {
                condition.ApplyDefault();
            }

            this.RefreshChips();
            this.ApplyIndicators(null);
            this.UpdateSearchState();
            this.WatchHostForDrawer();

            if (this.definition.SearchOnOpen)
            {
                this.ExecuteSearch();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            this.UnwatchHostForDrawer();
            base.OnFormClosed(e);
        }

        internal void OnConditionChanged()
        {
            this.UpdateSearchState();
            this.RefreshChips();
        }

        internal void OnConditionEnter()
        {
            this.ExecuteSearch();
            this.HideDrawer();
        }

        private int Px(int logical)
        {
            return Modern.Lab.WinForms.Controls.Hosting.ScreenDpi.Scale(this, logical);
        }

        private void BuildLayout()
        {
            this.SuspendLayout();

            Panel titlePanel = this.BuildTitle();
            this.searchCard = this.BuildSearchCard();
            ModernCardPanel indicatorCard = this.definition.Flows.Count > 0 ? this.BuildIndicatorCard() : null;
            this.mainZone = this.BuildMainZone();

            this.Controls.Add(this.mainZone);

            if (indicatorCard != null)
            {
                this.Controls.Add(this.Spacer());
                this.Controls.Add(indicatorCard);
            }

            this.Controls.Add(this.Spacer());
            this.Controls.Add(this.searchCard);
            this.Controls.Add(this.Spacer());
            this.Controls.Add(titlePanel);

            this.ResumeLayout(false);
            this.LayoutConditions();
        }

        private Panel Spacer()
        {
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = this.Px(8);
            return spacer;
        }

        private Panel BuildTitle()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = this.Px(28);

            ModernLabel title = new ModernLabel();
            title.Dock = DockStyle.Fill;
            title.Kind = LabelKind.Title;
            title.TitleBar = true;
            title.Text = this.definition.Title;
            panel.Controls.Add(title);

            if (this.definition.Environment.Length > 0)
            {
                ModernStatusBadge environment = new ModernStatusBadge();
                environment.Dock = DockStyle.Right;
                environment.Width = this.Px(60);
                environment.Semantic = SemanticKind.Neutral;
                environment.Shape = BadgeShape.Rounded;
                environment.Text = this.definition.Environment;
                panel.Controls.Add(environment);
            }

            return panel;
        }

        private ModernCardPanel BuildSearchCard()
        {
            ModernCardPanel card = new ModernCardPanel();
            card.Dock = DockStyle.Top;
            card.BackColor = ModernTheme.Surface;
            card.Padding = new Padding(this.Px(12), this.Px(8), this.Px(12), this.Px(8));

            int collapseSpan = this.definition.Collapsible ? CollapseWidth + ButtonGap : 0;
            this.buttonArea = new Panel();
            this.buttonArea.Size = new System.Drawing.Size(this.Px(collapseSpan + ButtonWidth * 2 + ButtonGap), this.Px(RowHeight));

            if (this.definition.Collapsible)
            {
                this.btnCollapse = this.NewButton(string.Empty, CollapseGlyph, ButtonKind.Subtle);
                this.btnCollapse.SetBounds(0, this.Px(4), this.Px(CollapseWidth), this.Px(FieldHeight));
                this.btnCollapse.Click += this.OnCollapseClick;
                this.buttonArea.Controls.Add(this.btnCollapse);
            }

            ModernButton btnReset = this.NewButton("Reset", ResetGlyph, ButtonKind.Subtle);
            btnReset.SetBounds(this.Px(collapseSpan), this.Px(4), this.Px(ButtonWidth), this.Px(FieldHeight));
            btnReset.Click += this.OnResetClick;

            this.btnRefresh = this.NewButton("Refresh", RefreshGlyph, ButtonKind.Primary);
            this.btnRefresh.SetBounds(this.Px(collapseSpan + ButtonWidth + ButtonGap), this.Px(4), this.Px(ButtonWidth), this.Px(FieldHeight));
            this.btnRefresh.Click += this.OnRefreshClick;

            this.buttonArea.Controls.Add(btnReset);
            this.buttonArea.Controls.Add(this.btnRefresh);

            this.lblSummary = new ModernLabel();
            this.lblSummary.Kind = LabelKind.Helper;
            this.lblSummary.Visible = false;

            this.btnConditions = this.NewButton("Conditions", ModernMenuGlyphs.Filter, ButtonKind.Secondary);
            this.btnConditions.Click += this.OnConditionsClick;

            this.conditionArea = new Panel();
            this.conditionArea.Dock = DockStyle.Fill;

            this.primaryConditions.AddRange(this.definition.PrimaryConditions());
            this.drawerConditions.AddRange(this.definition.DrawerConditions());

            foreach (ReportCondition condition in this.definition.Conditions)
            {
                condition.Build(this);
            }

            foreach (ReportCondition condition in this.primaryConditions)
            {
                this.conditionArea.Controls.Add(condition.Label);
                this.conditionArea.Controls.Add(condition.Editor);
            }

            foreach (ReportCondition condition in this.drawerConditions)
            {
                this.BuildChip(condition);
            }

            this.btnConditions.Visible = this.drawerConditions.Count > 0;
            this.conditionArea.Controls.Add(this.btnConditions);

            if (this.drawerConditions.Count > 0)
            {
                this.drawer = new ReportConditionDrawer(this, this.drawerConditions);
                this.drawer.VisibleChanged += delegate { this.btnConditions.Selected = this.drawer.Visible; };
            }

            card.Resize += delegate { this.LayoutConditions(); };

            card.Controls.Add(this.conditionArea);
            card.Controls.Add(this.lblSummary);
            card.Controls.Add(this.buttonArea);
            this.buttonArea.BringToFront();
            return card;
        }

        private void BuildChip(ReportCondition condition)
        {
            ReportCondition captured = condition;

            ModernStatusBadge chip = new ModernStatusBadge();
            chip.Shape = BadgeShape.Rounded;
            chip.Semantic = SemanticKind.Neutral;
            chip.AutoSize = true;
            chip.Cursor = Cursors.Hand;
            chip.Visible = false;
            chip.Click += delegate { this.OpenDrawer(captured); };

            ModernButton clear = this.NewButton(string.Empty, ModernMenuGlyphs.Cancel, ButtonKind.Subtle);
            clear.Visible = false;
            clear.Click += delegate { captured.Clear(); };

            this.conditionArea.Controls.Add(chip);
            this.conditionArea.Controls.Add(clear);
            this.chips.Add(chip);
            this.chipClears.Add(clear);
        }

        private ModernButton NewButton(string text, string glyph, ButtonKind kind)
        {
            ModernButton button = new ModernButton();
            button.Text = text;
            button.Kind = kind;
            button.IconGlyph = glyph;
            button.Cursor = Cursors.Hand;
            return button;
        }

        private ModernCardPanel BuildIndicatorCard()
        {
            ModernCardPanel card = new ModernCardPanel();
            card.Dock = DockStyle.Top;
            card.BackColor = ModernTheme.Surface;
            card.Height = this.Px(80);
            card.Padding = new Padding(this.Px(12), this.Px(8), this.Px(12), this.Px(8));

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Margin = new Padding(0);
            layout.RowCount = 1;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.ColumnCount = 1 + this.definition.Flows.Count * 2;
            this.keyStyle = new ColumnStyle(SizeType.Absolute, this.Px(KeyPanelWidth));
            layout.ColumnStyles.Add(this.keyStyle);

            this.keyPanel = new Panel();
            this.keyPanel.Dock = DockStyle.Fill;
            this.keyPanel.Margin = new Padding(0);

            this.lblKey = new ModernLabel();
            this.lblKey.Kind = LabelKind.Title;
            this.lblKey.Text = "-";
            this.lblKey.SetBounds(0, this.Px(18), this.Px(150), this.Px(28));

            this.badgeState = new ModernStatusBadge();
            this.badgeState.Semantic = SemanticKind.Neutral;
            this.badgeState.Shape = BadgeShape.Rounded;
            this.badgeState.Visible = false;
            this.badgeState.SetBounds(this.Px(154), this.Px(20), this.Px(112), this.Px(24));

            this.keyPanel.Controls.Add(this.lblKey);
            this.keyPanel.Controls.Add(this.badgeState);
            this.SyncBranchBadges();

            layout.Controls.Add(this.keyPanel, 0, 0);

            int column = 1;

            foreach (ReportFlow flow in this.definition.Flows)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, this.Px(FlowLabelWidth)));
                ColumnStyle flowStyle = new ColumnStyle(SizeType.Percent, flow.StepCount);
                layout.ColumnStyles.Add(flowStyle);
                this.flowStyles.Add(flowStyle);

                ModernLabel flowLabel = new ModernLabel();
                flowLabel.Dock = DockStyle.Fill;
                flowLabel.Margin = new Padding(0);
                flowLabel.Kind = LabelKind.Label;
                flowLabel.Text = flow.Label;

                ModernStepIndicator indicator = new ModernStepIndicator();
                indicator.Dock = DockStyle.Fill;
                indicator.Margin = new Padding(0);
                indicator.DisplayMember = ReportFlow.StepLabelColumn;
                indicator.StateMember = ReportFlow.StepStateColumn;
                indicator.LabelFontSize = 10D;

                layout.Controls.Add(flowLabel, column, 0);
                layout.Controls.Add(indicator, column + 1, 0);
                this.indicators.Add(indicator);
                column += 2;
            }

            card.Controls.Add(layout);
            return card;
        }

        /// <summary>가지 코드표가 오면 띠의 배지를 코드 수만큼 맞춘다(노랑, 글자가 있을 때만 보임) — 키 칸 폭도 함께 늘린다.</summary>
        private void SyncBranchBadges()
        {
            if (this.keyPanel == null)
            {
                return;
            }

            int wanted = this.definition.BranchCount;

            while (this.keyBadges.Count < wanted)
            {
                ModernStatusBadge badge = new ModernStatusBadge();
                badge.Semantic = SemanticKind.Warning;
                badge.Shape = BadgeShape.Rounded;
                badge.Visible = false;
                int badgeX = this.Px(154 + 112 + KeyBadgeGap) + this.keyBadges.Count * this.Px(KeyBadgeWidth + KeyBadgeGap);
                badge.SetBounds(badgeX, this.Px(20), this.Px(KeyBadgeWidth), this.Px(24));
                this.keyPanel.Controls.Add(badge);
                this.keyBadges.Add(badge);
            }

            this.keyStyle.Width = this.Px(KeyPanelWidth + wanted * (KeyBadgeWidth + KeyBadgeGap));
        }

        /// <summary>코드표가 오면 인디케이터 폭을 단계 수에 맞춘다.</summary>
        private void SyncIndicatorWidths()
        {
            for (int index = 0; index < this.flowStyles.Count && index < this.definition.Flows.Count; index++)
            {
                this.flowStyles[index].Width = this.definition.Flows[index].StepCount;
            }
        }

        private Panel BuildMainZone()
        {
            Panel zone = new Panel();
            zone.Dock = DockStyle.Fill;

            ModernCardPanel card = new ModernCardPanel();
            card.Dock = DockStyle.Fill;
            card.BackColor = ModernTheme.Surface;
            card.Padding = new Padding(this.Px(8));

            this.grid = new ModernDataGrid();
            this.grid.Dock = DockStyle.Fill;
            this.grid.AllowColumnFilters = true;
            this.grid.AutoFitColumns = true;
            this.grid.EmptyText = this.definition.EmptyText;
            this.grid.ExportSheetName = this.definition.ExportSheetName;
            this.grid.ExportFileNamePrefix = this.definition.ExportFilePrefix;
            this.grid.ShowStatusBar = true;
            this.grid.StatusCountFormat = "{0:N0} rows";

            if (this.definition.KeyMember.Length > 0)
            {
                this.grid.RowKeyMember = this.definition.KeyMember;
            }

            this.grid.RowColorSelector = this.definition.RowColor;
            this.grid.SelectionChanged += this.OnGridSelectionChanged;
            this.grid.ColumnFiltersChanged += this.OnColumnFiltersChanged;

            card.Controls.Add(this.grid);
            zone.Controls.Add(card);

            this.kpiZone = new Panel();
            this.kpiZone.Dock = DockStyle.Bottom;
            this.kpiZone.Margin = new Padding(0);
            this.kpiZone.Height = 0;
            zone.Controls.Add(this.kpiZone);
            this.SyncKpiBand();

            return zone;
        }

        /// <summary>KPI 띠를 정의의 카드 목록(키 · 실패 · 가지)에 맞춰 다시 짓는다 — 코드표가 오면 카드 수가 바뀐다. 카드가 없으면 띠가 없다.</summary>
        private void SyncKpiBand()
        {
            List<ReportDefinition.KpiCard> cards = this.definition.KpiCards();

            if (cards.Count == this.kpiCards.Count && SameTitles(cards, this.kpiDefinitions))
            {
                return;
            }

            this.kpiZone.SuspendLayout();
            this.kpiZone.Controls.Clear();
            this.kpiCards.Clear();
            this.kpiDefinitions.Clear();
            this.kpiDefinitions.AddRange(cards);

            if (this.kpiBand != null)
            {
                this.kpiBand.Dispose();
                this.kpiBand = null;
            }

            if (cards.Count == 0)
            {
                this.kpiZone.Height = 0;
                this.kpiZone.ResumeLayout();
                return;
            }

            TableLayoutPanel band = new TableLayoutPanel();
            band.Dock = DockStyle.Fill;
            band.Margin = new Padding(0);
            band.RowCount = 1;
            band.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            band.ColumnCount = cards.Count;

            for (int index = 0; index < cards.Count; index++)
            {
                band.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / cards.Count));

                ModernKpiCard kpi = new ModernKpiCard();
                kpi.Dock = DockStyle.Fill;
                kpi.Margin = new Padding(index == 0 ? 0 : this.Px(KpiGap) / 2, 0, index == cards.Count - 1 ? 0 : this.Px(KpiGap) / 2, 0);
                kpi.Title = cards[index].Title;
                kpi.Value = NoValueText;

                band.Controls.Add(kpi, index, 0);
                this.kpiCards.Add(kpi);
            }

            this.kpiBand = band;
            this.kpiZone.Padding = new Padding(0, this.Px(8), 0, 0);
            this.kpiZone.Height = this.Px(KpiBandHeight + 8);
            this.kpiZone.Controls.Add(band);
            this.kpiZone.ResumeLayout();
        }

        private static bool SameTitles(List<ReportDefinition.KpiCard> left, List<ReportDefinition.KpiCard> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (int index = 0; index < left.Count; index++)
            {
                if (!string.Equals(left[index].Title, right[index].Title, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private void LayoutConditions()
        {
            if (this.definition == null || this.conditionArea == null || this.buttonArea == null || this.searchCard == null)
            {
                return;
            }

            this.buttonArea.SetBounds(
                    this.searchCard.ClientSize.Width - this.searchCard.Padding.Right - this.buttonArea.Width,
                    this.searchCard.Padding.Top,
                    this.buttonArea.Width,
                    this.buttonArea.Height);

            if (this.collapsed)
            {
                this.conditionArea.Visible = false;
                this.lblSummary.Visible = true;
                this.lblSummary.SetBounds(
                        this.searchCard.Padding.Left, this.searchCard.Padding.Top + this.Px(4),
                        Math.Max(0, this.searchCard.ClientSize.Width - this.searchCard.Padding.Horizontal - this.buttonArea.Width - this.Px(ConditionGap)),
                        this.Px(FieldHeight));
                int collapsedHeight = this.Px(RowHeight) + this.searchCard.Padding.Vertical;

                if (this.searchCard.Height != collapsedHeight)
                {
                    this.searchCard.Height = collapsedHeight;
                }

                return;
            }

            this.lblSummary.Visible = false;
            this.conditionArea.Visible = true;

            int fullWidth = this.conditionArea.ClientSize.Width;
            int available = fullWidth - this.buttonArea.Width - this.Px(ConditionGap);
            int x = 0;
            int y = this.Px(4);
            int rows = 1;

            foreach (ReportCondition condition in this.primaryConditions)
            {
                int width = this.Px(LabelWidth + LabelGap + condition.EditorWidth);

                if (x > 0 && x + width > available)
                {
                    x = 0;
                    y += this.Px(RowHeight);
                    rows++;
                    available = fullWidth;
                }

                condition.Label.SetBounds(x, y, this.Px(LabelWidth), this.Px(FieldHeight));
                condition.Editor.SetBounds(
                        x + this.Px(LabelWidth + LabelGap), y, this.Px(condition.EditorWidth), this.Px(FieldHeight));
                x += width + this.Px(ConditionGap);
            }

            if (this.btnConditions.Visible)
            {
                int toggleWidth = this.Px(ButtonWidth);

                if (x > 0 && x + toggleWidth > available)
                {
                    x = 0;
                    y += this.Px(RowHeight);
                    rows++;
                    available = fullWidth;
                }

                this.btnConditions.SetBounds(x, y, toggleWidth, this.Px(FieldHeight));
                x += toggleWidth + this.Px(ConditionGap);
            }

            bool chipRowStarted = this.primaryConditions.Count == 0 && !this.btnConditions.Visible;

            for (int index = 0; index < this.chips.Count; index++)
            {
                if (!this.chips[index].Visible)
                {
                    continue;
                }

                int chipWidth = this.chips[index].GetPreferredSize(System.Drawing.Size.Empty).Width;
                int pairWidth = chipWidth + this.Px(ChipClearWidth);

                if (!chipRowStarted || (x > 0 && x + pairWidth > available))
                {
                    x = 0;
                    y += this.Px(RowHeight);
                    rows++;
                    chipRowStarted = true;
                    available = fullWidth;
                }

                int chipY = y + (this.Px(FieldHeight) - this.Px(ChipHeight)) / 2;
                this.chips[index].SetBounds(x, chipY, chipWidth, this.Px(ChipHeight));
                this.chipClears[index].SetBounds(x + chipWidth, chipY, this.Px(ChipClearWidth), this.Px(ChipHeight));
                x += pairWidth + this.Px(ChipGap);
            }

            int height = rows * this.Px(RowHeight) + this.searchCard.Padding.Vertical;

            if (this.searchCard.Height != height)
            {
                this.searchCard.Height = height;
            }
        }

        private void RefreshChips()
        {
            if (this.chips.Count != this.drawerConditions.Count)
            {
                return;
            }

            int valued = 0;

            for (int index = 0; index < this.chips.Count; index++)
            {
                ReportCondition condition = this.drawerConditions[index];
                bool shown = condition.HasValue;

                if (shown)
                {
                    valued++;
                    this.chips[index].Text = (condition.Caption + " " + condition.ValueText).Trim();
                }

                this.chips[index].Visible = shown;
                this.chipClears[index].Visible = shown;
            }

            this.btnConditions.BadgeCount = valued;
            this.lblSummary.Text = this.SummaryText();
            this.LayoutConditions();
        }

        private string SummaryText()
        {
            List<string> parts = new List<string>();

            foreach (ReportCondition condition in this.definition.Conditions)
            {
                if (condition.HasValue)
                {
                    parts.Add((condition.Caption + " " + condition.ValueText).Trim());
                }
            }

            return parts.Count == 0 ? "No conditions" : string.Join("  ·  ", parts.ToArray());
        }

        private void OnCollapseClick(object sender, EventArgs e)
        {
            this.collapsed = !this.collapsed;
            this.btnCollapse.IconGlyph = this.collapsed ? ExpandGlyph : CollapseGlyph;

            if (this.collapsed)
            {
                this.HideDrawer();
                this.lblSummary.Text = this.SummaryText();
            }

            this.LayoutConditions();
        }

        private void OnConditionsClick(object sender, EventArgs e)
        {
            if (this.drawer == null || this.drawer.ConsumeSuppressedToggle())
            {
                return;
            }

            if (this.drawer.Visible)
            {
                this.drawer.HideDrawer(true);
            }
            else
            {
                this.OpenDrawer(null);
            }
        }

        private void OpenDrawer(ReportCondition focus)
        {
            if (this.drawer != null)
            {
                this.drawer.ShowBelow(this.btnConditions, focus);
            }
        }

        private void HideDrawer()
        {
            if (this.drawer != null)
            {
                this.drawer.HideDrawer(true);
            }
        }

        private void OnHostChanged(object sender, EventArgs e)
        {
            if (this.drawer != null)
            {
                this.drawer.HideFromOutside();
            }
        }

        private void WatchHostForDrawer()
        {
            if (this.drawer == null)
            {
                return;
            }

            foreach (Form host in this.DrawerHosts())
            {
                host.Activated += this.OnHostChanged;
                host.LocationChanged += this.OnHostChanged;
                host.Resize += this.OnHostChanged;
            }
        }

        private void UnwatchHostForDrawer()
        {
            if (this.drawer == null)
            {
                return;
            }

            foreach (Form host in this.DrawerHosts())
            {
                host.Activated -= this.OnHostChanged;
                host.LocationChanged -= this.OnHostChanged;
                host.Resize -= this.OnHostChanged;
            }
        }

        private List<Form> DrawerHosts()
        {
            List<Form> hosts = new List<Form>();
            hosts.Add(this);

            Form top = this.TopLevelControl as Form;

            if (top != null && top != this)
            {
                hosts.Add(top);
            }

            return hosts;
        }

        private void UpdateSearchState()
        {
            if (this.btnRefresh != null && this.definition != null)
            {
                this.btnRefresh.Enabled = this.definition.RequiredComplete();
            }
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            this.ExecuteSearch();
        }

        private void OnResetClick(object sender, EventArgs e)
        {
            foreach (ReportCondition condition in this.definition.Conditions)
            {
                condition.ApplyDefault();
            }

            this.grid.ClearColumnFilters();
            this.UpdateSearchState();
            this.RefreshChips();
            this.ExecuteSearch();
        }

        private async void ExecuteSearch()
        {
            if (this.definition == null || !this.definition.RequiredComplete())
            {
                return;
            }

            RequestParameters parameters = this.definition.Parameters();
            LoadOutcome<DataTable> outcome;

            using (this.Busy(LoadingText, this.definition.Title))
            {
                if (!await this.LoadCodesAsync())
                {
                    return;
                }

                outcome = await this.FetchAsync(ChannelReport, () => this.RequestReport(parameters));
            }

            if (!outcome.IsCurrent)
            {
                return;
            }

            this.BindReport(outcome.Failure != null ? null : outcome.Value);
        }

        /// <summary>
        /// 코드표 흐름 · 가지가 아직 받지 못한 코드표를 하나씩 받는다. 하나라도 실패하면(실패 안내는 베이스 다이얼로그) 리포트를 조회하지 않고
        /// 다음 조회에서 다시 받는다. 받고 나면 인디케이터 폭 · 띠 배지 · KPI 띠를 코드 수에 맞춘다.
        /// </summary>
        private async System.Threading.Tasks.Task<bool> LoadCodesAsync()
        {
            List<string> pending = this.definition.PendingCodeTypes();

            if (pending.Count == 0)
            {
                return true;
            }

            foreach (string codeType in pending)
            {
                string wanted = codeType;
                LoadOutcome<DataTable> outcome = await this.FetchAsync(ChannelCodes + ":" + wanted, () => this.RequestCodes(wanted));

                if (!outcome.IsCurrent || outcome.Failure != null)
                {
                    return false;
                }

                this.definition.ApplyCodes(wanted, outcome.Value);
            }

            this.SyncIndicatorWidths();
            this.SyncBranchBadges();
            this.SyncKpiBand();
            return true;
        }

        private void BindReport(DataTable rows)
        {
            if (rows != null)
            {
                this.definition.Prepare(rows);
                this.PrepareRows(rows);
                rows.AcceptChanges();
            }

            this.reportData = rows;
            this.grid.DataSource = rows;
            this.RefreshCounts();

            this.ApplyIndicators(this.grid.SelectedItem as DataRowView);
        }

        private void OnColumnFiltersChanged(object sender, EventArgs e)
        {
            this.RefreshCounts();
        }

        private void RefreshCounts()
        {
            List<DataRow> visible = this.VisibleRows();

            for (int index = 0; index < this.kpiCards.Count && index < this.kpiDefinitions.Count; index++)
            {
                this.kpiCards[index].Value = this.kpiDefinitions[index].ValueText(this.reportData, visible);
            }

            this.grid.StatusText = string.Empty;
        }

        private List<DataRow> VisibleRows()
        {
            List<DataRow> rows = new List<DataRow>();

            if (this.reportData == null)
            {
                return rows;
            }

            foreach (DataRow row in this.reportData.Rows)
            {
                if (row.RowState != DataRowState.Deleted && this.grid.MatchesColumnFilters(row))
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            this.ApplyIndicators(this.grid.SelectedItem as DataRowView);
        }

        private void ApplyIndicators(DataRowView selected)
        {
            if (this.lblKey == null)
            {
                return;
            }

            DataRow row = selected == null ? null : selected.Row;
            this.lblKey.Text = this.definition.KeyText(row);

            for (int index = 0; index < this.indicators.Count; index++)
            {
                this.indicators[index].DataSource = this.definition.Flows[index].Indicator(row);
            }

            for (int index = 0; index < this.keyBadges.Count; index++)
            {
                string badgeText = index < this.definition.BranchCount ? this.definition.BranchBadgeText(row, index) : string.Empty;
                this.keyBadges[index].Text = badgeText;
                this.keyBadges[index].Visible = badgeText.Length > 0;
            }

            ReportFlow.StateFlow state = this.definition.FirstStateFlow();

            if (state == null)
            {
                this.badgeState.Visible = false;
                return;
            }

            string code = row == null ? string.Empty : state.StateField.Text(row);
            string text = state.StateText(code);
            bool cancelled = state.IsCancelled(code);

            this.badgeState.Text = text;
            this.badgeState.Semantic = cancelled ? SemanticKind.Neutral : SemanticKind.None;
            this.badgeState.ColorValue = cancelled ? string.Empty : code;
            this.badgeState.Visible = text.Length > 0;
        }
    }
}
