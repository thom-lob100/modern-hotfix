using Modern.Lab.WinForms.Controls.Data;

namespace Modern.Lab.Hosting.Schema
{
    /// <summary>Y · N 두 값만 갖는 칸 — 뜻이 칸 이름에 있는 플래그(REWORK_YN · RETURN_YN · SWAP_YN · SCRAP_DUE_YN)에 쓴다(2026-11-04).
    /// 뜻이 하나로 정해진 칸(<see cref="AutoYnField"/> · <see cref="DurableMultiLotYnField"/>)은 자기 타입을 그대로 둔다.</summary>
    public sealed class YnField : Field
    {
        public readonly FieldValue Y = Value("Y");
        public readonly FieldValue N = Value("N");
    }
}
