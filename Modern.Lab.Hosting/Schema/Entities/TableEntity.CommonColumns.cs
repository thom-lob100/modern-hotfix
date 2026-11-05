using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    public abstract partial class TableEntity
    {
        /// <summary>여러 업무가 함께 쓰는 칸 사전 — 회사 칸 이름 · 기본 캡션이 다르면 여기 한 줄만 고친다. 값 목록은 <see cref="CommonFields"/>.</summary>
        protected static class CommonColumns
        {
            public static CommonFields.AcssTyp AcssTyp()
            {
                return Field<CommonFields.AcssTyp>("ACSS_TYP").Caption("Access");
            }

            public static Field BoxId()
            {
                return Field("BOX_ID").Caption("Carrier");
            }

            public static Field CommonTyp()
            {
                return Field("COMMON_TYP");
            }

            public static Field CtnDesc()
            {
                return Field("CTN_DESC");
            }

            public static Field Description()
            {
                return Field("DESCRIPTION").Caption("Description");
            }

            public static Field EventCd()
            {
                return Field("EVENT_CD").Caption("Event");
            }

            public static Field EventTm()
            {
                return Field("EVENT_TM").Caption("Event Time");
            }

            public static Field FacId()
            {
                return Field("FAC_ID");
            }

            public static Field FlowId()
            {
                return Field("FLOW_ID").Caption("Flow");
            }

            public static Field GoalPortNm()
            {
                return Field("GOAL_PORT_NM").Caption("Goal Port");
            }

            public static CommonFields.JudgeRslt JudgeRslt()
            {
                return Field<CommonFields.JudgeRslt>("JUDGE_RSLT").Caption("Judge Result");
            }

            public static CommonFields.LastEventCd LastEventCd()
            {
                return Field<CommonFields.LastEventCd>("LAST_EVENT_CD").Caption("Last Event");
            }

            public static Field Location()
            {
                return Field("LOCATION").Caption("Location");
            }

            public static CommonFields.LocNm LocNm()
            {
                return Field<CommonFields.LocNm>("LOC_NM").Caption("Location");
            }

            public static CommonFields.McsItgYn McsItgYn()
            {
                return Field<CommonFields.McsItgYn>("MCS_ITG_YN").Caption("MCS");
            }

            public static Field OperId()
            {
                return Field("OPER_ID").Caption("Operation");
            }

            public static Field Priority()
            {
                return Field("PRIORITY").Caption("Prio");
            }

            public static Field ProdId()
            {
                return Field("PROD_ID").Caption("Prod");
            }

            public static Field Remark()
            {
                return Field("REMARK").Caption("Remark");
            }

            public static Field SendFac()
            {
                return Field("SEND_FAC").Caption("Send Fac");
            }

            public static Field SlotNo()
            {
                return Field("SLOT_NO").Caption("Slot");
            }

            public static Field SortNo()
            {
                return Field("SORT_NO");
            }

            public static CommonFields.SubProdTyp SubProdTyp()
            {
                return Field<CommonFields.SubProdTyp>("SUB_PROD_TYP").Caption("Type");
            }

            public static Field TimeKey()
            {
                return Field("TIMEKEY").Caption("Timekey");
            }

            public static Field UpdatedAt()
            {
                return Field("UPDATED_AT");
            }

            public static Field UpdatedBy()
            {
                return Field("UPDATED_BY");
            }

            public static Field UseYn()
            {
                return Field("USE_YN");
            }

            public static Field UserId()
            {
                return Field("USER_ID").Caption("User");
            }
        }
    }
}
