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

        // 편집기 콤보의 선택지. Chip 은 STUB 1~6, Lamella 는 LCC 1~25 · 핑거 A~E · 위치 Top/Left/Right.
        // 회사 수납 구성이 다르면 이 숫자와 목록만 고친다.
        private const int ChipSlotCount = 6;
        private const int LamellaSlotCount = 25;
        private static readonly string[] FingerIds = { "A", "B", "C", "D", "E" };
        private const string ComboValueColumn = "VALUE";

        // 왼쪽 표의 컬럼 구성 — 읽기 전용 목록이다. 입력 칸을 앞에 두고 나머지는 서버가 준 순서대로 뒤에 온다.
        // Chip 은 FINGER_POSITION 을 숨긴다. 폭은 그리드 자동 맞춤(AutoFitColumns)이 정하고 캡션은 엔티티·캡션 사전이 정한다.
        private static ModernDataGridColumn[] JudgeColumns(DataTable table, JudgeKind kind)
        {
            GridColumns columns = Schemas.JudgeResult.Columns(table)
                    .First(Schemas.JudgeResult.DurableId.Name, Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name,
                            Schemas.JudgeResult.FingerPosition.Name, Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name);

            if (kind != JudgeKind.Lamella)
            {
                columns.Hide(Schemas.JudgeResult.FingerPosition.Name);
            }

            return columns.ToArray();
        }

        // 오른쪽 편집기가 나열할 컬럼 — 표의 컬럼 그대로이고, Chip 은 FINGER_POSITION 을 빼서 편집기에도 보이지 않게 한다.
        private static DataTable EditorSchema(DataTable table, JudgeKind kind)
        {
            DataTable schema = table.Clone();

            if (kind != JudgeKind.Lamella && schema.Columns.Contains(Schemas.JudgeResult.FingerPosition.Name))
            {
                schema.Columns.Remove(Schemas.JudgeResult.FingerPosition.Name);
            }

            return schema;
        }

        // 사용자가 채우는 칸. 이 밖의 컬럼은 서버가 준 값이라 편집기에서 잠근다.
        private static string[] InputColumns(JudgeKind kind)
        {
            return kind == JudgeKind.Lamella
                    ? new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.FingerId.Name, Schemas.JudgeResult.FingerPosition.Name,
                        Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    }
                    : new string[]
                    {
                        Schemas.JudgeResult.SlotNo.Name, Schemas.JudgeResult.JudgeRslt.Name, Schemas.JudgeResult.GridIndex.Name
                    };
        }

        // 편집기 설정 — 기준정보 화면의 DefineEditors 에 해당한다. 엔티티 연결(키·필수·읽기 전용·캡션)은 UseEntity 한 번이고,
        // 그 뒤에 이 창만의 콤보와 Lamella 전용 필수를 잇는다. Chip 의 FINGER_ID 는 '-' 고정이라 잠근다.
        private void DefineJudgeEditors()
        {
            this.editorJudge.UseEntity(Schemas.JudgeResult);
            this.editorJudge.Field(Schemas.JudgeResult.SlotNo.Name)
                    .Combo(ValueTable(SlotItems(this.Kind == JudgeKind.Chip ? ChipSlotCount : LamellaSlotCount)), ComboValueColumn, ComboValueColumn);
            this.editorJudge.Field(Schemas.JudgeResult.JudgeRslt.Name)
                    .Combo(ValueTable(Schemas.JudgeResult.JudgeRslt.ValueCodes()), ComboValueColumn, ComboValueColumn);

            if (this.Kind == JudgeKind.Lamella)
            {
                this.editorJudge.Field(Schemas.JudgeResult.FingerId.Name)
                        .Combo(ValueTable(FingerIds), ComboValueColumn, ComboValueColumn);
                this.editorJudge.Field(Schemas.JudgeResult.FingerPosition.Name)
                        .Combo(ValueTable(Schemas.JudgeResult.FingerPosition.ValueCodes()), ComboValueColumn, ComboValueColumn)
                        .Required();
            }
            else
            {
                this.editorJudge.Field(Schemas.JudgeResult.FingerId.Name).ReadOnly();
            }
        }

        // 표를 받은 뒤 서버가 준 컬럼을 잠근다 — 입력 칸이 아닌 모든 컬럼이 대상이다.
        private void LockServerColumns()
        {
            HashSet<string> inputs = new HashSet<string>(InputColumns(this.Kind), System.StringComparer.OrdinalIgnoreCase);
            List<string> locked = new List<string>();

            foreach (string column in this.editorJudge.Columns)
            {
                if (!inputs.Contains(column))
                {
                    locked.Add(column);
                }
            }

            if (locked.Count > 0)
            {
                this.editorJudge.Field(locked.ToArray()).ReadOnly();
            }
        }

        private static DataTable ValueTable(string[] items)
        {
            DataTable table = new DataTable();
            table.Columns.Add(ComboValueColumn, typeof(string));

            foreach (string item in items)
            {
                table.Rows.Add(item);
            }

            return table;
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
