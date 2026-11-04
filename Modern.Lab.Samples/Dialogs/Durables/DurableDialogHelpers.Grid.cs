using Modern.Lab.Hosting.Schema;
using Modern.Lab.Samples.Services;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples
{
    internal static class DurableDialogHelpers
    {
        // Durable Info · RfId · Hold/Release · Adjust/Scrap · Transport · Carrier Move 창이 함께 쓰는 Durable 카드의 항목과 순서.
        // 캡션·형식·배지는 엔티티(Schemas.Durable)에서 온다. 회사에서 항목을 바꾸면 이 목록만 고친다.
        internal static ModernFieldDefinition[] SourceFields()
        {
            return new ModernFieldDefinition[]
            {
                CardFields.Of(Schemas.Durable.DurableId),
                CardFields.Of(Schemas.Durable.DurableTyp),
                CardFields.Of(Schemas.Durable.DurableStatCd),
                CardFields.Of(Schemas.Durable.WfLoadStatCd),
                CardFields.Of(Schemas.Durable.LotQty),
                CardFields.Of(Schemas.Durable.FromFac),
                CardFields.Of(Schemas.Durable.LocNm)
            };
        }

        // Manual Out · Manual In Cancel · Manual Out Cancel 창의 카드 — 공용 항목 뒤에 장비 · 포트.
        internal static ModernFieldDefinition[] TransportFields()
        {
            ModernFieldDefinition[] source = SourceFields();
            ModernFieldDefinition[] fields = new ModernFieldDefinition[source.Length + 2];
            source.CopyTo(fields, 0);
            fields[source.Length] = CardFields.Of(Schemas.Durable.EqpId);
            fields[source.Length + 1] = CardFields.Of(Schemas.Durable.PortNm);
            return fields;
        }
    }
}
