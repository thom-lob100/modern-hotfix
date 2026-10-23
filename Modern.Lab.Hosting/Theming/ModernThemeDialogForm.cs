using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Modern.Lab.Theming;
using Modern.Lab.WinForms.Controls.Layout;

namespace Modern.Lab.Hosting.Theming
{
    /// <summary>
    /// 테마 설정 창 — 테마를 Light / Dark 두 묶음의 타일 격자(<see cref="ThemeTile"/>)로 한눈에 보이고, 고른 하나를
    /// <see cref="ModernThemeSetting"/> 파일에 저장한다(2026-10-23, 콤보 → 타일 격자). 타일은 그 테마의 바탕색 · 카드색 · 액센트 ·
    /// 글자색으로 그려져 고르기 전에 느낌이 보인다. 정적 테마라 지금 창은 바뀌지 않고 다음 실행부터 적용된다.
    /// 모든 모던 폼에서 <see cref="ModernFormBase.ThemeDialogShortcut"/>(기본 Ctrl+Shift+T)로 열리고, 회사 셸 메뉴에서는 <see cref="ShowFor"/> 를 부른다.
    ///
    /// 조작: 클릭 · Space 로 고르고 Save(Enter) 로 저장, 타일 더블클릭은 고르기 + 저장. ←→↑↓ 는 격자 안 이동(↑↓ 는 묶음 경계를 넘는다),
    /// Home · End 는 첫 · 끝 타일. 상태 낱말은 Current(지금 켜진 테마) 와 Saved(파일의 테마) 하나만 보인다.
    /// </summary>
    public partial class ModernThemeDialogForm : ModernFormBase
    {
        private const string RestartNote = "Applies the next time the program starts.";
        private const string SavedNote = "Saved. Restart the program to apply.";
        private const string ThemesSuffix = " themes";
        private const int TileRowHeight = 88;

        private readonly List<ThemeTile> tiles = new List<ThemeTile>();
        private bool syncingTiles;

        public ModernThemeDialogForm()
        {
            this.InitializeComponent();
            this.InitializeModernForm(false);
        }

        /// <summary>창을 모달로 연다 — 셸 메뉴나 단축키에서 부르는 하나의 입구.</summary>
        public static void ShowFor(IWin32Window owner)
        {
            using (ModernThemeDialogForm dialog = new ModernThemeDialogForm())
            {
                dialog.ShowDialog(owner);
            }
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (IsDesignTime() || this.DesignMode)
            {
                return;
            }

            ModernTheme.ThemeMode saved;
            bool hasSaved = ModernThemeSetting.TryLoad(out saved);

            this.BuildTiles(this.grpLight, this.lightTable, false, hasSaved, saved);
            this.BuildTiles(this.grpDark, this.darkTable, true, hasSaved, saved);

            ModernTheme.ThemeMode initial = hasSaved ? saved : ModernTheme.Mode;
            ThemeTile initialTile = this.FindTile(initial) ?? (this.tiles.Count > 0 ? this.tiles[0] : null);

            if (initialTile != null)
            {
                initialTile.Checked = true;
            }

            this.lblPath.Text = ModernThemeSetting.FilePath;
            this.lblNote.Text = RestartNote;
        }

        // 묶음 하나를 채운다 — 열 4개 고정, 행은 테마 수에 맞춰 늘리고 묶음 높이도 그에 맞춘다(스크롤 호스트가 넘침을 받는다).
        private void BuildTiles(ModernGroupBox group, TableLayoutPanel table, bool dark, bool hasSaved, ModernTheme.ThemeMode saved)
        {
            int count = 0;

            foreach (ModernTheme.ThemeMode mode in Enum.GetValues(typeof(ModernTheme.ThemeMode)))
            {
                if (ModernTheme.IsDarkBasedOf(mode) != dark)
                {
                    continue;
                }

                ThemeTile tile = new ThemeTile();
                tile.Mode = mode;
                tile.IsCurrent = mode == ModernTheme.Mode;
                tile.IsSaved = hasSaved && mode == saved;
                tile.Dock = DockStyle.Fill;
                tile.Margin = new Padding(4);
                tile.TabIndex = this.tiles.Count;
                tile.CheckedChanged += this.OnTileCheckedChanged;
                tile.DoubleClick += this.OnTileDoubleClick;
                tile.KeyDown += this.OnTileKeyDown;

                table.Controls.Add(tile, count % table.ColumnCount, count / table.ColumnCount);
                this.tiles.Add(tile);
                count++;
            }

            int rows = Math.Max(1, (count + table.ColumnCount - 1) / table.ColumnCount);
            table.RowCount = rows;
            table.RowStyles.Clear();

            for (int row = 0; row < rows; row++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, TileRowHeight));
            }

            group.Height = rows * TileRowHeight + group.Padding.Vertical;
            group.TitleRightText = count + ThemesSuffix;
        }

        private ThemeTile FindTile(ModernTheme.ThemeMode mode)
        {
            foreach (ThemeTile tile in this.tiles)
            {
                if (tile.Mode == mode)
                {
                    return tile;
                }
            }

            return null;
        }

        private ThemeTile SelectedTile()
        {
            foreach (ThemeTile tile in this.tiles)
            {
                if (tile.Checked)
                {
                    return tile;
                }
            }

            return null;
        }

        // 배타 선택 — 하나가 켜지면 나머지를 끈다. 끄는 동안 되돌아오는 이벤트는 무시.
        private void OnTileCheckedChanged(object sender, EventArgs e)
        {
            ThemeTile changed = sender as ThemeTile;

            if (this.syncingTiles || changed == null || !changed.Checked)
            {
                return;
            }

            this.syncingTiles = true;

            try
            {
                foreach (ThemeTile tile in this.tiles)
                {
                    if (tile != changed)
                    {
                        tile.Checked = false;
                    }
                }
            }
            finally
            {
                this.syncingTiles = false;
            }
        }

        private void OnTileDoubleClick(object sender, EventArgs e)
        {
            ThemeTile tile = sender as ThemeTile;

            if (tile == null)
            {
                return;
            }

            tile.Checked = true;
            this.SaveSelected();
        }

        // ←→ 는 전체 순서로 한 칸, ↑↓ 는 같은 열로 한 행(묶음 끝에서는 다음 묶음으로), Home · End 는 첫 · 끝.
        private void OnTileKeyDown(object sender, KeyEventArgs e)
        {
            ThemeTile tile = sender as ThemeTile;

            if (tile == null)
            {
                return;
            }

            ThemeTile target = null;
            int index = this.tiles.IndexOf(tile);

            switch (e.KeyCode)
            {
                case Keys.Left:
                    target = index > 0 ? this.tiles[index - 1] : tile;
                    break;
                case Keys.Right:
                    target = index < this.tiles.Count - 1 ? this.tiles[index + 1] : tile;
                    break;
                case Keys.Up:
                    target = this.TileInNextRow(tile, -1) ?? tile;
                    break;
                case Keys.Down:
                    target = this.TileInNextRow(tile, 1) ?? tile;
                    break;
                case Keys.Home:
                    target = this.tiles.Count > 0 ? this.tiles[0] : tile;
                    break;
                case Keys.End:
                    target = this.tiles.Count > 0 ? this.tiles[this.tiles.Count - 1] : tile;
                    break;
                default:
                    return;
            }

            e.Handled = true;
            target.Focus();
            target.Checked = true;
        }

        private ThemeTile TileInNextRow(ThemeTile tile, int direction)
        {
            TableLayoutPanel table = tile.Parent as TableLayoutPanel;

            if (table == null)
            {
                return null;
            }

            TableLayoutPanelCellPosition position = table.GetPositionFromControl(tile);
            int row = position.Row + direction;

            if (row >= 0 && row < table.RowCount)
            {
                return TileAtOrBefore(table, position.Column, row);
            }

            TableLayoutPanel other = table == this.lightTable
                    ? (direction > 0 ? this.darkTable : null)
                    : (direction < 0 ? this.lightTable : null);

            if (other == null)
            {
                return null;
            }

            return TileAtOrBefore(other, position.Column, direction > 0 ? 0 : other.RowCount - 1);
        }

        // 그 칸이 비어 있으면(마지막 행의 꼬리) 같은 행에서 왼쪽으로 가장 가까운 타일.
        private static ThemeTile TileAtOrBefore(TableLayoutPanel table, int column, int row)
        {
            for (int col = column; col >= 0; col--)
            {
                ThemeTile tile = table.GetControlFromPosition(col, row) as ThemeTile;

                if (tile != null)
                {
                    return tile;
                }
            }

            return null;
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            this.SaveSelected();
        }

        private void SaveSelected()
        {
            ThemeTile selected = this.SelectedTile();

            if (selected == null)
            {
                this.ShowWarningMessage("Select a theme.");
                return;
            }

            try
            {
                ModernThemeSetting.Save(selected.Mode);
            }
            catch (Exception ex)
            {
                this.ShowErrorMessage("Theme not saved", "The theme file could not be written.", ex.ToString());
                return;
            }

            foreach (ThemeTile tile in this.tiles)
            {
                tile.IsSaved = tile == selected;
            }

            this.lblNote.Text = SavedNote;
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
