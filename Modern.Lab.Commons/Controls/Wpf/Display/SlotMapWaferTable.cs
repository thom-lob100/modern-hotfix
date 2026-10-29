using System;
using System.Collections.Generic;
using System.Data;
using Modern.Lab.Data;

namespace Modern.Lab.Controls.Wpf.Display
{
    /// <summary>
    /// 수납 현황(캐리어 웨이퍼) 테이블의 **풀 맵 정규화** 공용 헬퍼 — 슬롯 맵
    /// 계열(ModernSlotMap / ModernSlotMapMini)이 쓰는 자리당 1행 데이터 규칙
    /// (SUB_PROD_TYP/SLOT_NO/FINGER_ID/FINGER_INDEX/WF_ID/LOT_ID)의 단일 원천.
    ///
    /// 조회는 보통 **채워진 자리 행만** 주므로, 수납 구조 스켈레톤(빈
    /// 자리 포함)을 먼저 만들고 조회 행을 자리(SUB_PROD_TYP+SLOT_NO+FINGER_ID)로 얹는다 —
    /// 빈 자리까지 포함한 풀 응답이 와도 같은 결과가 나온다(중복 무해).
    /// 자리 수는 서버가 관리하는 CAPA 값이 원천이고(수치 하드코딩 금지),
    /// CAPA가 없으면 조회 데이터의 최대 자리와 호출측 폴백(물리 구조 기본값) 중 큰 값이다 —
    /// 채워진 자리 행만 오는 조회에서 빈 자리가 사라지지 않게(2026-10-29).
    ///
    /// 폼은 이 결과(풀 맵)를 ModernSlotMap 구획 변환/채움 집계/용량 판정의
    /// 전제로 쓴다. ModernSlotMapMini는 같은 규칙을 컨트롤 내부에서 수행하므로
    /// 원본 조회 테이블을 그대로 주면 된다.
    /// </summary>
    public static class SlotMapWaferTable
    {
        /// <summary>LCC 핑거 이름의 물리 구조 기본값 — 응답에 어떤 핑거가 왔든 항상 그리고,
        /// 그 밖의 이름이 오면 더한다(2026-10-29, 미니 맵과 같은 규칙).</summary>
        public static readonly string[] DefaultFingerNames =
                new string[] { "A", "B", "C", "D", "E" };

        /// <summary>FOUP 슬롯 자리 수의 물리 구조 기본값 — 서버 CAPA 가 없을 때 호출측이 넘기는 폴백(carrier-edit 2026-10-03: FOUP 25).</summary>
        public const int DefaultSlotCount = 25;

        /// <summary>TRAY 스텁(Chip) 자리 수의 물리 구조 기본값 — 서버 CAPA 가 없을 때의 폴백(TRAY = 칩 6).</summary>
        public const int DefaultStubCount = 6;

        /// <summary>TRAY LCC 슬롯 자리 수의 물리 구조 기본값 — 서버 CAPA 가 없을 때의 폴백(TRAY = 라멜라 슬롯 25).</summary>
        public const int DefaultLccCount = 25;

        /// <summary>빈 수납 현황 테이블 — 조회 실패 폴백이자 풀 맵 스켈레톤의
        /// 스키마 (SUB_PROD_TYP/SLOT_NO/FINGER_ID/FINGER_INDEX/WF_ID/LOT_ID).</summary>
        public static DataTable CreateEmpty()
        {
            DataTable table = new DataTable();
            table.Columns.Add("SUB_PROD_TYP", typeof(string));
            table.Columns.Add("SLOT_NO", typeof(string));
            table.Columns.Add("FINGER_ID", typeof(string));
            table.Columns.Add("FINGER_INDEX", typeof(string));
            table.Columns.Add("WF_ID", typeof(string));
            table.Columns.Add("LOT_ID", typeof(string));
            return table;
        }

        /// <summary>
        /// 조회 결과를 풀 맵(자리당 1행, 빈 자리 포함)으로 정규화한다.
        /// 구획은 자리 수가 0보다 큰 것만 만든다 — FOUP 캐리어면 슬롯 CAPA만,
        /// TRAY 캐리어면 STUB/LCC CAPA만 넘기면 된다.
        /// </summary>
        /// <param name="raw">수납 현황 조회 결과 (채워진 자리 행만 있어도 됨; null 허용).</param>
        /// <param name="slotCapa">FOUP 슬롯 자리 수 — 서버 관리 CAPA (0 = 데이터/폴백에서 유도).</param>
        /// <param name="stubCapa">STUB 자리 수 — 서버 관리 CAPA (0 = 유도).</param>
        /// <param name="lccCapa">LCC 슬롯 자리 수 — 서버 관리 CAPA, 예: 25 (0 = 유도).</param>
        /// <param name="fallbackSlotCount">슬롯 최후 폴백 자리 수 — CAPA도 데이터도 없을 때만 쓴다 (0 = 구획 없음).</param>
        /// <param name="fallbackStubCount">STUB 최후 폴백 자리 수.</param>
        /// <param name="fallbackLccCount">LCC 최후 폴백 **자리(위치) 수** (핑거 단위 총량이 아님).</param>
        /// <param name="fingerNames">LCC 핑거 구성 (null = A~E 항상 + 데이터에 나타난 그 밖의 이름).</param>
        public static DataTable Normalize(
                DataTable raw,
                int slotCapa, int stubCapa, int lccCapa,
                int fallbackSlotCount, int fallbackStubCount, int fallbackLccCount,
                string[] fingerNames = null)
        {
            Dictionary<string, DataRow> occupied =
                    new Dictionary<string, DataRow>(StringComparer.Ordinal);
            List<string> observedFingers = new List<string>();
            int maxSlot = 0;
            int maxStub = 0;
            int maxLcc = 0;

            if (raw != null)
            {
                foreach (DataRow row in raw.Rows)
                {
                    string kind = TableHelper.CellText(row, "SUB_PROD_TYP").Trim();
                    int pos = TableHelper.ParseInt(TableHelper.CellText(row, "SLOT_NO"));

                    if (pos <= 0)
                    {
                        continue;
                    }

                    // 풀 응답이 오는 환경이면 조회 자체가 자리 수를 알려 준다 —
                    // CAPA 컬럼이 없는 환경의 폴백 근거로 최대 자리를 기억한다.
                    // Chip/Lamella 외의 정상 값은 FOUP Wafer 슬롯으로 본다.
                    if (string.Equals(kind, "Chip", StringComparison.OrdinalIgnoreCase))
                    {
                        maxStub = Math.Max(maxStub, pos);
                    }
                    else if (string.Equals(kind, "Lamella", StringComparison.OrdinalIgnoreCase))
                    {
                        maxLcc = Math.Max(maxLcc, pos);
                        string finger = NormalizeFinger(TableHelper.CellText(row, "FINGER_ID"));

                        if (finger.Length > 0 && !observedFingers.Contains(finger))
                        {
                            observedFingers.Add(finger);
                        }
                    }
                    else
                    {
                        maxSlot = Math.Max(maxSlot, pos);
                    }

                    // 웨이퍼이 있는 행만 얹는다 — 빈 자리 행은 스켈레톤이 대신한다.
                    if (TableHelper.CellText(row, "WF_ID").Trim().Length == 0)
                    {
                        continue;
                    }

                    occupied[PositionKey(
                            kind,
                            TableHelper.CellText(row, "SLOT_NO"),
                            TableHelper.CellText(row, "FINGER_ID"))] = row;
                }
            }

            // 핑거 구성 — 지정이 있으면 지정, 없으면 물리 구조 기본값(A~E)을 항상 그리고 데이터에 그 밖의 이름이 나타나면 더한다(정렬).
            // 채워진 핑거 행만 온 응답(E 하나뿐인 트레이)이 핑거 집합을 줄이지 않는다 — 미니 맵과 같은 규칙(2026-10-29 사용자).
            string[] fingers = fingerNames;

            if (fingers == null || fingers.Length == 0)
            {
                foreach (string name in DefaultFingerNames)
                {
                    if (!observedFingers.Contains(name))
                    {
                        observedFingers.Add(name);
                    }
                }

                observedFingers.Sort(StringComparer.Ordinal);
                fingers = observedFingers.ToArray();
            }

            // 자리 수 — 서버 CAPA → 데이터 최대 자리 → 호출측 폴백 순.
            int slotCount = ResolveCount(slotCapa, maxSlot, fallbackSlotCount);
            int stubCount = ResolveCount(stubCapa, maxStub, fallbackStubCount);
            int lccCount = ResolveCount(lccCapa, maxLcc, fallbackLccCount);

            DataTable full = CreateEmpty();

            for (int pos = 1; pos <= slotCount; pos++)
            {
                AddSkeletonRow(full, occupied, "Wafer", pos, string.Empty);
            }

            for (int pos = 1; pos <= stubCount; pos++)
            {
                AddSkeletonRow(full, occupied, "Chip", pos, string.Empty);
            }

            for (int pos = 1; pos <= lccCount; pos++)
            {
                foreach (string finger in fingers)
                {
                    AddSkeletonRow(full, occupied, "Lamella", pos, finger);
                }
            }

            return full;
        }

        // 자리 수 결정 — 서버 CAPA 가 있으면 그 값, 없으면 조회 데이터의 최대 자리와 호출측 폴백 중 큰 값.
        // 폴백은 물리 구조(FOUP 25 · 스텁 6 · LCC 25)라 채워진 자리 행만 와도 빈 자리가 다 그려지고,
        // 데이터가 폴백보다 큰 자리를 쓰면 그 자리까지 그린다(2026-10-29).
        private static int ResolveCount(int capa, int observedMax, int fallback)
        {
            if (capa > 0)
            {
                return capa;
            }

            return Math.Max(observedMax, fallback);
        }

        // 핑거 토큰 정규화 — 트림 + 대문자. 핑거가 없는 자리(STUB/슬롯)는
        // 데이터 소스에 따라 '-'(하이픈)나 'N/A' 같은 자리표시자로 오고,
        // 스켈레톤은 빈 문자열로 만든다. 둘을 같은 "핑거 없음"으로 취급하지
        // 않으면 키가 어긋나(예: "STUB|3|-" vs "STUB|3|") 채운 자리를 못 찾는다.
        // 실제 핑거 이름(A~E)이 아닌 자리표시자는 전부 빈 문자열로 흡수한다.
        private static string NormalizeFinger(string finger)
        {
            string trimmed = (finger ?? string.Empty).Trim().ToUpperInvariant();

            if (trimmed.Length == 0 || trimmed == "-" || trimmed == "--"
                    || trimmed == "N/A" || trimmed == "NA" || trimmed == "NULL")
            {
                return string.Empty;
            }

            return trimmed;
        }

        // 자리 식별 키 — SLOT_NO는 숫자로 정규화해 "1"/"01" 표기 차이를 흡수하고,
        // SUB_PROD_TYP은 대소문자를 흡수하며 FINGER_ID 자리표시자는 빈 값으로 흡수한다.
        private static string PositionKey(string kind, string pos, string finger)
        {
            return (kind ?? string.Empty).Trim().ToUpperInvariant()
                    + "|" + TableHelper.ParseInt(pos)
                    + "|" + NormalizeFinger(finger);
        }

        // 스켈레톤 자리 한 행을 추가하고, 조회에 그 자리의 웨이퍼이 있으면 얹는다.
        private static void AddSkeletonRow(
                DataTable full, Dictionary<string, DataRow> occupied,
                string kind, int pos, string finger)
        {
            DataRow row = full.NewRow();
            row["SUB_PROD_TYP"] = kind;
            row["SLOT_NO"] = pos.ToString();
            row["FINGER_ID"] = finger;
            row["FINGER_INDEX"] = string.Empty;
            row["WF_ID"] = string.Empty;
            row["LOT_ID"] = string.Empty;

            DataRow source;

            // 조회측 키와 같은 정규화(PositionKey)로 맞춰 찾는다 — 호출측이
            // 소문자 핑거 이름을 넘겨도 어긋나지 않는다.
            if (occupied.TryGetValue(PositionKey(kind, pos.ToString(), finger), out source))
            {
                row["FINGER_INDEX"] = TableHelper.CellText(source, "FINGER_INDEX").Trim();
                row["WF_ID"] = TableHelper.CellText(source, "WF_ID").Trim();
                row["LOT_ID"] = TableHelper.CellText(source, "LOT_ID").Trim();
            }

            full.Rows.Add(row);
        }
    }
}
