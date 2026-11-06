using System.Windows.Forms;

using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Input;
using Modern.Lab.WinForms.Controls.Layout;

namespace Modern.Lab.Hosting.MasterData
{
    public sealed class MasterDataCrudView
    {
        public ModernTextBox Keyword { get; set; }

        /// <summary>조회 버튼(선택). 있으면 쓰기 중에 잠긴다.</summary>
        public ModernButton RefreshButton { get; set; }

        /// <summary>조회 조건 되돌리기 버튼(선택). 있으면 쓰기 중에 잠긴다.</summary>
        public ModernButton ResetButton { get; set; }

        public ModernDataGrid Grid { get; set; }

        public ModernGroupBox EditorCard { get; set; }

        public ModernPropertyGrid Editor { get; set; }

        public ModernButton NewButton { get; set; }

        public ModernButton CancelButton { get; set; }

        public ModernButton SaveButton { get; set; }

        public ModernButton DeleteButton { get; set; }

        /// <summary>Duplicate 버튼(선택, <c>btnDuplicate</c>). 있으면 목록 메뉴의 Duplicate 항목과 같은 판정·핸들러로 묶인다.</summary>
        public ModernButton DuplicateButton { get; set; }

        public ContextMenuStrip ActionMenu { get; set; }

        public ToolStripMenuItem NewMenuItem { get; set; }

        public ToolStripMenuItem CancelMenuItem { get; set; }

        public ToolStripMenuItem SaveMenuItem { get; set; }

        public ToolStripMenuItem DeleteMenuItem { get; set; }
    }
}
