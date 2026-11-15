using Modern.Lab.Captions;

namespace Modern.Lab.Hosting
{
    /// <summary>디자이너 글자 번역 연결 — 이 폼과 그 아래 컨트롤의 디자이너 글자를 <see cref="ModernLanguage"/>로 번역해 다시 넣는다(2026-11-15).
    /// 두 걸음이다: 폼이 만들어질 때(<c>EnsureFormInitialized</c>, 컬럼 선언 앞) 디자이너가 적은 글자를 적어 두고,
    /// <c>OnLoad</c> 끝(화면의 Load 처리기가 다 돈 뒤, 창이 보이기 전)에 번역해 넣는다. 화면의 Load 처리기가 버튼의 영어 글자로
    /// 아이콘·메뉴 묶음을 찾으므로(<c>MenuGlyphs.For(button.Text)</c>) 그 전에 글자를 바꾸면 아이콘이 사라진다.
    /// 번역이 꺼져 있으면(진입점이 <c>ModernLanguage.Use</c>를 부르지 않았으면) 아무 일도 없고, 디자인 타임에는 불리지 않는다.
    /// 그리드·정보 카드의 캡션은 여기가 아니라 각 컨트롤이 컬럼·필드를 받아 그리는 자리에서 번역한다.
    /// 코드가 넣는 글자(메시지·다이얼로그 제목·코드로 만든 메뉴)는 이 자리가 보지 못한다.</summary>
    public partial class ModernFormBase
    {
        partial void LanguageRemember()
        {
            ModernLanguageApplier.Remember(this);
        }

        partial void LanguageApply()
        {
            ModernLanguageApplier.ApplyRemembered(this);
        }
    }
}
