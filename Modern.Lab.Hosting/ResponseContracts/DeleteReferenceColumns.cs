using System;
using System.Data;
using System.Globalization;

namespace Modern.Lab.Hosting.ResponseContracts
{
    /// <summary>
    /// 기준정보 삭제 거절 응답에 실리는 참조 표의 계약(2026-11-07) — 서버가 "이 항목이 어디에 몇 건 쓰이는지"를
    /// 줄마다 적어 주면 <c>MasterDataCrudFormBase</c> 가 규칙 안내 창과 같은 표로 보인다. 표가 없거나 이 컬럼이
    /// 없으면 전처럼 서버 문구 오류 창이다. 서버가 확인한 규칙은 0건 줄도 보내 통과한 조건까지 보이게 한다.
    /// 거절 응답의 <c>ResultMessage</c> 에 조회 응답과 같은 표 형식으로 싣는다.
    /// </summary>
    public static class DeleteReferenceColumns
    {
        /// <summary>어디에 쓰이는가 — 화면에 보이는 이름 그대로("Flow" · "Lot" · "Bay" …).</summary>
        public const string RefEntity = "REF_ENTITY";

        /// <summary>쓰이는 건수 — 0 이면 그 조건은 통과.</summary>
        public const string RefCount = "REF_COUNT";

        /// <summary>쓰이는 곳의 Id 목록(쉼표, 몇 개까지 싣는지는 서버 재량) — 없어도 된다.</summary>
        public const string RefIds = "REF_IDS";

        /// <summary>참조 표인가 — <see cref="RefEntity"/> · <see cref="RefCount"/> 컬럼이 있고 행이 하나 이상이며, 모든 행의 건수가
        /// 0 이상의 정수다. 빈 값 · 숫자 아님 · 음수가 한 줄이라도 있으면 표 전체를 믿지 않는다(코덱스 5194836b 검토 — 거절인데
        /// 전부 OK 로 보이던 것) — 그때 화면은 서버 문구 오류 창으로 돌아간다.</summary>
        public static bool Describes(DataTable table)
        {
            if (table == null || !table.Columns.Contains(RefEntity) || !table.Columns.Contains(RefCount)
                    || table.Rows.Count == 0)
            {
                return false;
            }

            foreach (DataRow row in table.Rows)
            {
                int count;

                if (!TryCount(row, out count))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>행의 건수 — <see cref="Describes"/> 가 통과한 표에서 읽는다. 읽을 수 없는 값은 0 으로 보이지 않고 1 로 본다(막힘).</summary>
        public static int CountOf(DataRow row)
        {
            int count;
            return TryCount(row, out count) ? count : 1;
        }

        private static bool TryCount(DataRow row, out int count)
        {
            string text = Convert.ToString(row[RefCount], CultureInfo.InvariantCulture);
            count = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out count) && count >= 0;
        }

        /// <summary>행의 Id 목록 글자 — 컬럼이 없으면 빈 글자.</summary>
        public static string IdsOf(DataRow row)
        {
            if (!row.Table.Columns.Contains(RefIds))
            {
                return string.Empty;
            }

            return (Convert.ToString(row[RefIds], CultureInfo.InvariantCulture) ?? string.Empty).Trim();
        }
    }
}
