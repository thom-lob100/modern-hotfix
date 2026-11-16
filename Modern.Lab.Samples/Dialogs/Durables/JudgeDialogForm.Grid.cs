using System.Collections.Generic;
using System.Data;
using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Samples
{
    public partial class JudgeDialogForm
    {
        // Chip 은 핑거가 없다 — FINGER_ID 에 반드시 '-' 를 실어 보낸다(회사 적재 규칙).
        internal const string ChipFingerId = "-";

        // 콤보의 선택지. Chip 은 STUB 1~6, Lamella · 분석은 LCC 1~25 · 핑거 A~E · 위치 Top/Left/Right.
        // 회사 수납 구성이 다르면 이 숫자와 목록만 고친다.
        private const int ChipSlotCount = 6;
        private const int LamellaSlotCount = 25;
        private static readonly string[] FingerIds = { "A", "B", "C", "D", "E" };

        // 핑거가 있는 판정 — Lamella 생성과 분석이다. 분석은 Lamella 와 같은 칸 · 같은 값 규칙에 ANALYZE_RSLT 만 더한다.
        private static bool HasFinger(JudgeKind kind)
        {
            return kind == JudgeKind.Lamella || kind == JudgeKind.Analysis;
        }

        // 표의 컬럼 구성 — 표 안에서 바로 입력한다(2026-11-15). 입력 칸을 앞에 두고 나머지는 서버가 준 순서대로 뒤에 온다.
        // 입력 칸(InputColumns)만 열리고 서버가 준 컬럼은 잠긴다. 입력 칸 중 선택지(ComboChoices)가 있는 칸은 콤보, 없는 칸은 글자 입력이다.
        // 콤보는 글자를 쳐서도 고른다(ComboEditable) — 친 글자로 시작하는 선택지로 좁혀지고 선택지 밖의 글자는 들어가지 않는다.
        // Chip 은 FINGER_ID('-' 고정)가 잠기고 FINGER_POSITION 이 없다. 폭은 그리드 자동 맞춤(AutoFitColumns)이 정하고 캡션은 엔티티·캡션 사전이 정한다.
        // 순서를 바꾸려면 First(...) 목록만 고친다.
        private static ModernDataGridColumn[] JudgeColumns(DataTable table, JudgeKind kind)
        {
            GridColumns columns = Schemas.JudgeResult.Columns(table)
                    .First(Schemas.JudgeResult.DurableId.Name, Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name,
                            Schemas.JudgeResult.FingerPosition.Name, Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.AnalyzeRslt.Name,
                            Schemas.JudgeResult.GridIndex.Name);

            if (!HasFinger(kind))
            {
                columns.Hide(Schemas.JudgeResult.FingerPosition.Name);
            }

            HashSet<string> inputs = new HashSet<string>(InputColumns(kind), System.StringComparer.OrdinalIgnoreCase);
            HashSet<string> frozen = new HashSet<string>(FrozenColumns(), System.StringComparer.OrdinalIgnoreCase);
            ModernDataGridColumn[] shown = columns.ToArray();

            foreach (ModernDataGridColumn column in shown)
            {
                string name = column.DataPropertyName;
                bool input = inputs.Contains(name);
                string[] choices = input ? ComboChoices(kind, name) : null;

                column.ReadOnly = !input;
                column.Frozen = frozen.Contains(name);

                if (choices != null)
                {
                    column.Kind = GridColumnKind.Combo;
                    column.ComboItems = choices;
                    column.ComboEditable = true;
                }
            }

            return shown;
        }

        // 사용자가 채우는 칸. 이 밖의 컬럼은 서버가 준 값이라 표에서 잠긴다.
        // 회사에서 입력 칸이 늘면 여기에 더하고, 콤보로 고르는 칸이면 ComboChoices 에 선택지를, 비우면 안 되는 칸이면 RequiredColumns 에도 더한다.
        private static string[] InputColumns(JudgeKind kind)
        {
            switch (kind)
            {
                case JudgeKind.Lamella:
                    return new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name, Schemas.JudgeResult.FingerPosition.Name,
                        Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    };

                case JudgeKind.Analysis:
                    return new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name, Schemas.JudgeResult.FingerPosition.Name,
                        Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.AnalyzeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    };

                default:
                    return new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    };
            }
        }

        // 비우면 OK 가 열리지 않는 칸 — 지금은 입력 칸 전부다. 선택 입력 칸이 생기면 InputColumns 에만 더하고 여기서는 뺀다.
        private static string[] RequiredColumns(JudgeKind kind)
        {
            return InputColumns(kind);
        }

        // 콤보로 고르는 입력 칸의 선택지. 여기에 없는 입력 칸(GRID_INDEX)은 셀에 글자로 입력한다.
        private static string[] ComboChoices(JudgeKind kind, string column)
        {
            if (column == Schemas.JudgeResult.SlotNo.Name)
            {
                return SlotItems(kind == JudgeKind.Chip ? ChipSlotCount : LamellaSlotCount);
            }

            if (column == Schemas.JudgeResult.FingerId.Name)
            {
                return FingerIds;
            }

            if (column == Schemas.JudgeResult.FingerPosition.Name)
            {
                return Schemas.JudgeResult.FingerPosition.ValueCodes();
            }

            if (column == Schemas.JudgeResult.JudgeRslt.Name)
            {
                return Schemas.JudgeResult.JudgeRslt.ValueCodes();
            }

            if (column == Schemas.JudgeResult.AnalyzeRslt.Name)
            {
                return Schemas.JudgeResult.AnalyzeRslt.ValueCodes();
            }

            return null;
        }

        // 가로로 스크롤해도 왼쪽에 남는 칸 — 어느 행을 입력 중인지 보이게 한다. 고정은 왼쪽부터 이어진 칸만 되므로
        // JudgeColumns 의 First(...) 앞머리와 같은 순서로 적는다.
        private static string[] FrozenColumns()
        {
            return new string[]
            {
                Schemas.JudgeResult.DurableId.Name, Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name
            };
        }

        private static string[] SlotItems(int count)
        {
            string[] items = new string[count];

            for (int index = 0; index < count; index++)
            {
                items[index] = (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return items;
        }
    }
}
