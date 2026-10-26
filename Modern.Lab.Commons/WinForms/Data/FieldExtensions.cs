using System;
using Modern.Lab.Controls.Wpf.Data;

namespace Modern.Lab.WinForms.Controls.Data
{
    /// <summary>
    /// 필드 선언 체인 — <c>Field("LOT_QTY").Caption("Qty").Format("#,##0")</c>.
    /// 제네릭 확장이라 파생 타입(<c>Field&lt;WfLoadStatField&gt;("…").Badge()</c>)이 체인 뒤에도 그 타입으로 남는다.
    /// C# 7.3에는 공변 반환이 없어 인스턴스 메서드로는 이것이 되지 않는다.
    /// </summary>
    public static class FieldExtensions
    {
        /// <summary>표시 캡션 — 생략하면 캡션 사전과 자동 변환을 쓴다.</summary>
        public static T Caption<T>(this T field, string caption) where T : Field
        {
            Require(field);
            field.SetCaption(caption);
            return field;
        }

        /// <summary>그리드 헤더 본 캡션 위의 작은 글씨 — <c>Field("SOURCE_SLOT").Caption("Slot").TopLabel("Source")</c>.</summary>
        public static T TopLabel<T>(this T field, string text) where T : Field
        {
            Require(field);
            field.SetTopLabel(text);
            return field;
        }

        /// <summary>그리드 헤더 본 캡션 아래의 작은 글씨 — 위 글씨와 함께 쓸 수 있다.</summary>
        public static T BottomLabel<T>(this T field, string text) where T : Field
        {
            Require(field);
            field.SetBottomLabel(text);
            return field;
        }

        /// <summary>표시 형식 — 값 검증이 아니다.</summary>
        public static T Format<T>(this T field, string format) where T : Field
        {
            Require(field);
            field.SetFormat(format);
            return field;
        }

        /// <summary>그리드 셀 정렬 — <c>Field("LOT_QTY").Format("#,##0").TextAlignment(GridTextAlignment.Right)</c>. 캡션 사전·이름 관례보다 우선한다.</summary>
        public static T TextAlignment<T>(this T field, GridTextAlignment alignment) where T : Field
        {
            Require(field);
            field.SetTextAlignment(alignment);
            return field;
        }

        /// <summary>그리드 헤더 정렬 — 셀 정렬과 따로 정한다. 위·아래 작은 글씨 줄은 <see cref="TopLabelAlignment"/>·<see cref="BottomLabelAlignment"/>로 따로 준다.</summary>
        public static T HeaderAlignment<T>(this T field, GridTextAlignment alignment) where T : Field
        {
            Require(field);
            field.SetHeaderAlignment(alignment);
            return field;
        }

        /// <summary>그리드 헤더 위 작은 글씨 줄의 정렬 — <c>Field("SOURCE_SLOT").Caption("Slot").TopLabel("Source").HeaderAlignment(Center).TopLabelAlignment(Left)</c>.</summary>
        public static T TopLabelAlignment<T>(this T field, GridTextAlignment alignment) where T : Field
        {
            Require(field);
            field.SetTopLabelAlignment(alignment);
            return field;
        }

        /// <summary>그리드 헤더 아래 작은 글씨 줄의 정렬.</summary>
        public static T BottomLabelAlignment<T>(this T field, GridTextAlignment alignment) where T : Field
        {
            Require(field);
            field.SetBottomLabelAlignment(alignment);
            return field;
        }

        /// <summary>배지로 보인다 — 색은 기존 자동색 규칙을 쓴다.</summary>
        public static T Badge<T>(this T field) where T : Field
        {
            Require(field);
            field.SetBadge(true);
            return field;
        }

        /// <summary>
        /// 그 값일 때 배지 테두리가 돈다(콤마로 여러 개) — 배지도 함께 켠다. 이 필드를 보이는 모든 표·카드가 따라오고,
        /// 화면은 그리드 <c>.Spin()</c> · 카드 <c>.BadgeSpin()</c> 으로 덮어쓸 수 있다(2026-10-26).
        /// </summary>
        public static T Spin<T>(this T field, string values) where T : Field
        {
            Require(field);
            field.SetBadge(true);
            field.SetSpin(values);
            return field;
        }

        /// <summary>상태값으로 적는 판 — <c>this.LastEventCd.Spin(this.LastEventCd.JobStart)</c>.</summary>
        public static T Spin<T>(this T field, params FieldValue[] values) where T : Field
        {
            Require(field);
            string[] codes = new string[values == null ? 0 : values.Length];

            for (int index = 0; index < codes.Length; index++)
            {
                codes[index] = values[index] == null ? string.Empty : values[index].Value;
            }

            return field.Spin(string.Join(",", codes));
        }

        private static void Require(Field field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
        }
    }
}
