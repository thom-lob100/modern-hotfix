using System;
using System.Data;
using Modern.Lab.Hosting;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class DurableChangeDestDialogForm
    {
        // Change Destination 버튼이 열리는 조건(사용자 2026-11-11 · 2026-11-12 · 2026-11-15) — 반송 표 · Eqp Id · Port 콤보는 서버가 거른 목록 그대로이고,
        // 이 조건은 목록을 거르지 않고 마지막 버튼만 켜고 끈다. 고른 반송 행과 고른 포트 행을 합친 판정 행(RuleRow)을 위에서부터 보고,
        // 처음 막은 줄의 문구가 버튼 툴팁에 뜬다. 꺼진 버튼을 누르면 규칙 안내 창이 필드 · 요구 값 · 지금 값을 보인다.
        // 한 줄에 하나, 회사 조건이 바뀌면 여기만 고친다. 응답에 컬럼이 없으면 막는다.
        // 장비는 보지 않는다 — 고른 반송의 출발 · 목적 장비(START_GOAL_VAL · END_GOAL_VAL)와 Destination 을 실어 보내 서버가 거른다.
        private const string TransportEntityName = "Transport";
        private const string PortEntityName = "Port";

        private static readonly ActionRule[] ChangeDestinationRules =
        {
            // 반송 — 이동 상태(MOVE_STAT_CD)가 Request · Requested · Scheduled · Moving · Changed 이고 Durable 이 폐기(Scrapped)되지 않은 반송만 목적지를 바꾼다.
            ActionRule.Value(TransportEntityName, Schemas.Transport.MoveStatCd,
                    Schemas.Transport.MoveStatCd.Request, Schemas.Transport.MoveStatCd.Requested, Schemas.Transport.MoveStatCd.Scheduled,
                    Schemas.Transport.MoveStatCd.Moving, Schemas.Transport.MoveStatCd.Changed),
            ActionRule.NotValue(TransportEntityName, Schemas.Transport.DurableStatCd, Schemas.Transport.DurableStatCd.Scrapped),

            // 포트 — 장비 · STB · Stocker 모두 같은 인포트 조건이다(사용자 2026-11-09 에 받은 포트 조건 7개 — 홈 서버도 같은 조건으로 목록을 거른다).
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortTyp, Schemas.PortDet.PortTyp.Input, Schemas.PortDet.PortTyp.InputOutput),
            ActionRule.Value(PortEntityName, Schemas.PortDet.AcssTyp, Schemas.PortDet.AcssTyp.Auto),
            ActionRule.Value(PortEntityName, Schemas.PortDet.ResrcStatTyp, Schemas.PortDet.ResrcStatTyp.InService),
            ActionRule.Value(PortEntityName, Schemas.PortDet.PortStatNm, Schemas.PortDet.PortStatNm.Up),
            ActionRule.Value(PortEntityName, Schemas.PortDet.VirtPortYn, Schemas.PortDet.VirtPortYn.N),
            ActionRule.Value(PortEntityName, Schemas.PortDet.TransferStatCd, Schemas.PortDet.TransferStatCd.ReadyToLoad),
            ActionRule.Empty(PortEntityName, Schemas.PortDet.CurrDurableId)
        };

        // Destination 콤보의 두 번째 항목 — Type 이 FOUP 이면 STB, 그 밖(TRAY)은 Stocker 다.
        private static bool UsesStb(string durableTyp)
        {
            return string.Equals(durableTyp, Schemas.Transport.DurableTyp.Foup.Value, StringComparison.OrdinalIgnoreCase);
        }

        // 판정 행 — 고른 반송 행의 칸에 고른 포트 행의 칸을 더한 한 행이다. 같은 이름의 칸은 포트 값을 쓰고(포트 조건은 고른 포트를 본다),
        // DURABLE_ID 만 반송 값이다 — 규칙 안내 창의 머리글이다. 응답에 없던 칸은 만들지 않으므로 그 칸을 읽는 조건은 막는다.
        private static DataRow RuleRow(DataRow transport, DataRow port)
        {
            if (transport == null || port == null)
            {
                return null;
            }

            string keyName = Schemas.Transport.DurableId.Name;
            DataTable table = new DataTable("ChangeDestinationRule");
            table.Locale = System.Globalization.CultureInfo.InvariantCulture;
            AddCells(table, transport, null);
            AddCells(table, port, keyName);
            table.Rows.Add(table.NewRow());
            DataRow row = table.Rows[0];
            CopyCells(row, transport, null);
            CopyCells(row, port, keyName);

            if (table.Columns.Contains(keyName) && !row.IsNull(keyName))
            {
                table.PrimaryKey = new DataColumn[] { table.Columns[keyName] };
            }

            return row;
        }

        private static void AddCells(DataTable table, DataRow source, string skippedName)
        {
            foreach (DataColumn column in source.Table.Columns)
            {
                if (!table.Columns.Contains(column.ColumnName) && !IsSkipped(column, skippedName))
                {
                    table.Columns.Add(column.ColumnName, typeof(object));
                }
            }
        }

        private static void CopyCells(DataRow target, DataRow source, string skippedName)
        {
            foreach (DataColumn column in source.Table.Columns)
            {
                if (!IsSkipped(column, skippedName))
                {
                    target[column.ColumnName] = source[column];
                }
            }
        }

        private static bool IsSkipped(DataColumn column, string skippedName)
        {
            return skippedName != null && string.Equals(column.ColumnName, skippedName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
