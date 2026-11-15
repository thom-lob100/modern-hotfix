using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    internal static class TransportDialogHelpers
    {
        // Transport 창 넷(Manual In · Manual Out · Manual Cancel · Change Destination)이 함께 쓰는 Transport 카드의 항목과 순서 —
        // Transport Management 에서 고른 반송 행을 그대로 보인다. 캡션 · 형식 · 배지는 엔티티에서 온다. 회사에서 항목을 바꾸면 이 목록만 고친다.
        internal static ModernFieldDefinition[] SourceFields()
        {
            return new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.Transport.DurableId),
                CardFields.Of(Schemas.Transport.DurableTyp),
                CardFields.Of(Schemas.Transport.LotId),
                CardFields.Of(Schemas.Transport.DurableStatCd),
                CardFields.Of(Schemas.Transport.LocNm),
                CardFields.Of(Schemas.Transport.MoveStatCd),
                CardFields.Of(Schemas.Transport.LastEventDesc)
            };
        }

        // Manual Out · Manual In Cancel · Manual Out Cancel 창의 카드 — 공용 항목 뒤에 지금 실려 있는 장비 · 포트(Durable 을 조인해 받는 칸).
        internal static ModernFieldDefinition[] LocationFields()
        {
            return Join(SourceFields(), new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.Durable.EqpId),
                CardFields.Of(Schemas.Durable.PortNm)
            });
        }

        // Change Destination 창의 카드 — 공용 항목 뒤에 출발 자리와 지금 목적지.
        internal static ModernFieldDefinition[] RouteFields()
        {
            return Join(SourceFields(), new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.Transport.StartGoalVal),
                CardFields.Of(Schemas.Transport.StartLocCd),
                CardFields.Of(Schemas.Transport.EndGoalVal),
                CardFields.Of(Schemas.Transport.EndLocCd)
            });
        }

        private static ModernFieldDefinition[] Join(ModernFieldDefinition[] first, ModernFieldDefinition[] second)
        {
            ModernFieldDefinition[] joined = new ModernFieldDefinition[first.Length + second.Length];
            first.CopyTo(joined, 0);
            second.CopyTo(joined, first.Length);
            return joined;
        }
    }
}
