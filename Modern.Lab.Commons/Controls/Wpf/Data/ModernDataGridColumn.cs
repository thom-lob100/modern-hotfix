using System;
using Modern.Lab.Controls.Wpf.Common;
using Modern.Lab.Theming;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernDataGrid / ModernDataGridControl이 사용하는 컬럼 정의.
    /// WinForms 폼이 WPF DataGrid 타입을 직접 다루지 않고도 컬럼을 정의할 수 있도록
    /// 하는 단순 데이터 홀더이다.
    /// </summary>
    public class ModernDataGridColumn
    {
        /// <summary>
        /// 캡션 해석기 재정의 훅 — HeaderText를 생략한 생성자가 캡션을 물어볼
        /// 전역 제공자. 비워 두면(기본) GridCaptionCatalog 사전을 쓰므로 보통은
        /// 앱 시작 시 호스트 앱의 용어집(GridCaptionDictionary — 시드 +
        /// DB(UI_TERM_DIC) 조회 결과)을 GridCaptionCatalog에 등록하는 것으로
        /// 충분하다. 사전 조회로 부족한 해석 로직(리소스/다국어 등)이 필요할
        /// 때만 등록한다. 제공자가 null/빈 값을 돌려주면 DB 컬럼 표기를 읽기
        /// 좋게 바꾼 폴백("EQP_ID" → "Eqp Id")이 캡션이 된다. 회사 공통 용어는
        /// 용어집 하드코딩, 화면 전용 용어는 앱 등록(GridCaptionCatalog.Register)을
        /// 사용한다.
        /// </summary>
        public static Func<string, string> CaptionResolver { get; set; }

        // 용어사전에서 캡션을 해석한다 — 사전에 없으면 DB 컬럼 표기를 읽기 좋게
        // 바꾼 폴백을 쓴다 ("EQP_ID" → "Eqp Id", GridCaptionCatalog.Humanize).
        // CaptionResolver가 등록돼 있으면 그것이 GridCaptionCatalog보다 우선한다.
        // 그리드 컬럼뿐 아니라 ModernFieldDefinition(멤버만 준 필드)·ModernLabel
        // (CaptionMember)도 같은 해석기를 쓴다 — 캡션 어휘의 단일 원천.
        internal static string ResolveCaption(string dataPropertyName)
        {
            Func<string, string> resolver = CaptionResolver;
            string caption = resolver != null
                    ? resolver(dataPropertyName)
                    : GridCaptionCatalog.Resolve(dataPropertyName);

            return string.IsNullOrEmpty(caption)
                    ? GridCaptionCatalog.Humanize(dataPropertyName)
                    : caption;
        }

        /// <summary>빈 정의를 만든다(스타 너비, 가운데 정렬, 형식 없음, 텍스트 셀).</summary>
        public ModernDataGridColumn()
        {
            this.DataPropertyName = string.Empty;
            this.HeaderText = string.Empty;
            this.Width = -1d;
            this.TextAlignment = GridTextAlignment.Center;
            this.HeaderAlignment = GridTextAlignment.Center;
            this.AllowFilter = true;
            this.TopLabel = string.Empty;
            this.BottomLabel = string.Empty;
            this.Format = string.Empty;
            this.Kind = GridColumnKind.Text;
            this.TextColor = string.Empty;
            this.TextSemiBold = false;
            this.BadgeColorMember = string.Empty;
            this.BadgeAutoColor = false;
            this.BadgeGroupMember = string.Empty;
            this.BadgeSemanticMember = string.Empty;
            this.BadgeSpinValues = string.Empty;
            this.BadgeWidthValues = string.Empty;
            this.BadgeWidthByValue = false;
            this.BadgeAccentValues = string.Empty;
            this.BadgeShape = ChipShape.Rounded;
            this.ButtonText = string.Empty;
            this.ButtonEnabledMember = string.Empty;
            this.ButtonTextMember = string.Empty;
            this.ButtonAutoColor = false;
            this.HeaderCheckBox = false;
            this.CheckStyle = GridCheckStyle.CheckBox;
            this.CheckEnabledMember = string.Empty;
            this.CheckTrueValue = string.Empty;
            this.CheckFalseValue = string.Empty;
            this.ComboItems = null;
            this.ComboItemColors = null;
            this.ComboEnabledMember = string.Empty;
            this.ComboItemsMember = string.Empty;
            this.SpinnerActiveMember = string.Empty;
            this.BadgeSpinBorderMember = string.Empty;
            this.ReadOnly = false;
            this.Frozen = false;
            this.HeaderGroup = string.Empty;
            this.Aggregate = GridAggregateKind.None;
        }

        /// <summary>
        /// 캡션을 용어사전(GridCaptionCatalog, 또는 CaptionResolver 재정의)에서
        /// 찾는 스타 너비 컬럼을 만든다. 사전에 없으면 DB 컬럼 표기를 읽기 좋게
        /// 바꾼 폴백이 캡션이 된다 ("EQP_ID" → "Eqp Id" —
        /// <see cref="GridCaptionCatalog.Humanize"/>). 화면 문맥상 다른 캡션이
        /// 필요하면 headerText를 받는 생성자로 명시해 재정의한다.
        /// </summary>
        public ModernDataGridColumn(string dataPropertyName)
            : this(dataPropertyName, ResolveCaption(dataPropertyName))
        {
            this.TopLabel = GridCaptionCatalog.ResolveTopLabel(dataPropertyName) ?? string.Empty;
            this.BottomLabel = GridCaptionCatalog.ResolveBottomLabel(dataPropertyName) ?? string.Empty;
        }

        /// <summary>지정한 컬럼/속성에 바인딩되는 스타 너비 컬럼을 만든다.</summary>
        public ModernDataGridColumn(string dataPropertyName, string headerText)
            : this()
        {
            this.DataPropertyName = dataPropertyName;
            this.HeaderText = headerText;
            GridCaptionCatalog.ApplyDisplay(this);
        }

        /// <summary>지정한 컬럼/속성에 바인딩되는 고정 너비 컬럼을 만든다.</summary>
        public ModernDataGridColumn(string dataPropertyName, string headerText, double width)
            : this(dataPropertyName, headerText)
        {
            this.Width = width;
        }

        /// <summary>
        /// 표시 형식까지 한 줄로 선언하는 고정 너비 컬럼을 만든다 — 날짜/숫자
        /// 컬럼용 (예: <c>new ModernDataGridColumn("SENT_DT", "Sent", 140,
        /// "yyyy-MM-dd HH:mm")</c>). <see cref="Format"/> 속성 대입과 완전히
        /// 같다 — 문자열 컬럼이라도 값이 날짜/숫자로 해석되면 적용되고,
        /// 화면·복사·찾기·엑셀 내보내기가 같은 형식을 쓴다.
        /// </summary>
        public ModernDataGridColumn(
                string dataPropertyName, string headerText, double width, string format)
            : this(dataPropertyName, headerText, width)
        {
            this.Format = format ?? string.Empty;
        }

        /// <summary>원본 컬럼/속성 이름(DataTable 컬럼 또는 객체 속성).</summary>
        public string DataPropertyName { get; set; }

        /// <summary>헤더 본 캡션(기본 헤더 글자). 작은 위·아래 글씨는 <see cref="TopLabel"/>·<see cref="BottomLabel"/>에 따로 둔다.</summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// 본 캡션 위의 작은 글씨(기본 빈 값 = 없음) — 예: 본 캡션 "Slot" 위의 "Source". 아래 글씨와 함께 쓸 수 있다.
        /// 크기·색은 토큰(Font.Size.SubLabel · Brush.TextSecondary)이다. 그리드 헤더에만 두 줄 이상으로 그리고, 찾기·엑셀·콤보 목록은
        /// <see cref="PlainHeaderText"/>의 한 줄 글자를 쓴다. 캡션 사전에 함께 등록한 컬럼은 캡션 인자 없는 생성자가 받는다.
        /// </summary>
        public string TopLabel { get; set; }

        /// <summary>본 캡션 아래의 작은 글씨(기본 빈 값 = 없음) — 위 글씨와 같은 규칙이다.</summary>
        public string BottomLabel { get; set; }

        /// <summary>한 줄 헤더 글자 — 위·본·아래를 공백 하나로 잇는다("Source Slot"). 찾기·엑셀·콤보 목록 헤더가 쓴다.</summary>
        public string PlainHeaderText
        {
            get { return GridHeaderCaption.PlainText(this.TopLabel, this.HeaderText, this.BottomLabel); }
        }

        /// <summary>픽셀 너비. 0 이하이면 스타 크기 조정(남은 공간 채우기)을 의미한다.</summary>
        public double Width { get; set; }

        /// <summary>셀 텍스트의 가로 정렬 — 기본 Center(2026-09-30, 그 전 Left). 이름 관례로 숫자(QTY·CNT)는 Right, 글(NM·DESC·DESCRIPTION)은 Left.</summary>
        public GridTextAlignment TextAlignment { get; set; }

        /// <summary>
        /// 헤더 캡션의 가로 정렬(기본 Center — 2026-09-30 사용자 결정, 그 전에는 Left). 셀 정렬(TextAlignment)과 따로 정한다 — 셀 정렬을 따라가게 하면
        /// 가운데·오른쪽 셀 컬럼의 헤더가 한꺼번에 바뀐다. 위·아래 작은 글씨 줄은 <see cref="TopLabelAlignment"/>·<see cref="BottomLabelAlignment"/>가 정하지 않으면 이 정렬을 따르고, "\n" 헤더는 줄마다 같은 정렬이다.
        /// Center는 오른쪽 정렬 표시·필터 깔때기 자리를 빼지 않은 헤더 전체 폭의 가운데다. 그리드 헤더에만 적용된다.
        /// </summary>
        public GridTextAlignment HeaderAlignment { get; set; }

        /// <summary>
        /// 위 작은 글씨(<see cref="TopLabel"/>) 줄의 정렬 — null(기본)이면 <see cref="HeaderAlignment"/>를 따른다. 버튼의 TopLabelAlignment 와 같은 꼴이다.
        /// 본 캡션과 다르게 주면 헤더 내용을 글리프 자리까지 펴서 줄마다 정렬한다 — Right 줄은 정렬·필터 글리프 바로 앞에서 끝난다.
        /// </summary>
        public GridTextAlignment? TopLabelAlignment { get; set; }

        /// <summary>아래 작은 글씨(<see cref="BottomLabel"/>) 줄의 정렬 — null(기본)이면 <see cref="HeaderAlignment"/>를 따른다.</summary>
        public GridTextAlignment? BottomLabelAlignment { get; set; }

        /// <summary>
        /// 표시 형식 문자열 (예: 숫자 "N0"/"N2", 날짜 "yyyy-MM-dd").
        /// 원본이 숫자/날짜 타입 컬럼이면 그대로 적용되고, **문자열 컬럼이라도
        /// 값이 날짜/숫자로 해석되면 적용된다** — 서버가 java Timestamp 문자열
        /// ("2026-07-29 12:00:00.0")로 내려보내는 경우도 Format만 지정하면 된다
        /// (해석 실패 시 원본 그대로 표시, 예외 없음). 비어 있으면 값의 기본
        /// 문자열 표현을 쓴다. 정렬(sort)은 형식과 무관하게 원본 값 기준으로
        /// 동작한다.
        /// </summary>
        public string Format { get; set; }

        /// <summary>셀 표시 종류 (기본 Text). CheckBox/Badge/Button 셀로 전환한다.</summary>
        public GridColumnKind Kind { get; set; }

        /// <summary>
        /// Text 전용: 그리드가 편집 모드(<c>ReadOnly = false</c>)일 때 이 컬럼만
        /// 편집을 잠글지 여부 (기본 false = 편집 가능). 키 컬럼(ID 등)처럼 값이
        /// 바뀌면 안 되는 컬럼에 켠다. 그리드 자체가 읽기 전용이면 이 값과
        /// 무관하게 모든 셀이 잠긴다.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 가로 스크롤 시 왼쪽에 고정할 컬럼 여부 (기본 false) —
        /// <c>DataGridViewColumn.Frozen</c> 대응. 고정은 왼쪽부터 연속이어야
        /// 하므로, 중간 컬럼에만 켜면 그 왼쪽 컬럼들까지 함께 고정된다
        /// (DataGridView와 같은 규칙). 키 컬럼(ID 등)을 넓은 그리드에서
        /// 항상 보이게 할 때 쓴다.
        /// </summary>
        public bool Frozen { get; set; }

        /// <summary>
        /// 이 컬럼에 값 필터 깔때기를 붙일지(기본 true, 2026-09-30). 그리드의 <c>AllowColumnFilters</c>가 켜져 있어도 false 면 이 컬럼만 빠진다 —
        /// 순위·번호처럼 거를 일이 없는 좁은 컬럼에서 깔때기 자리(AutoFit 폭)를 없앤다. 체크박스·버튼 컬럼은 원래 대상이 아니다.
        /// </summary>
        public bool AllowFilter { get; set; }

        /// <summary>
        /// 집계 푸터의 집계 종류 (기본 None = 없음). 하나라도 지정되면 그리드
        /// 하단에 집계 행이 생기고, 이 컬럼 위치에 집계 값이 표시된다 —
        /// Sum/Average/Min/Max는 숫자로 해석되는 값만 대상으로 하고(문자열
        /// 숫자도 Format 규칙으로 해석), Count는 비어 있지 않은 값의 수다.
        /// 집계 대상은 **현재 뷰**(컬럼 필터 반영)이고 셀 편집을 커밋하면
        /// 즉시 다시 계산된다. 표시 서식은 컬럼 Format을 따른다.
        /// </summary>
        public GridAggregateKind Aggregate { get; set; }

        /// <summary>
        /// 같은 값 세로 셀 병합 여부 (기본 false). 켜면 **현재 뷰 순서에서
        /// 연속으로 같은 값**인 셀들이 하나로 합쳐져 보인다 — 구간의 첫 행에만
        /// 값이 표시되고 이어지는 행은 값과 행 구분선을 숨긴다 (FarPoint
        /// Spread의 자동 병합 대응). **표시 전용**이다: 행 선택/복사/엑셀
        /// 내보내기는 병합과 무관하게 행 단위 그대로이며, 정렬·필터로 뷰
        /// 순서가 바뀌면 병합 구간도 그에 맞춰 다시 계산된다. 병합 컬럼은
        /// 대개 키/그룹 컬럼이므로 정렬된 데이터와 함께 쓴다.
        /// </summary>
        public bool MergeCells { get; set; }

        /// <summary>
        /// 2단 그룹 헤더의 상단 그룹 캡션 (기본 빈 문자열 = 그룹 없음).
        /// **이웃한** 컬럼들이 같은 캡션을 쓰면 그 구간 위에 하나로 합쳐진
        /// 그룹 셀이 표시된다 (예: Qty/Purity/Result에 "Inspection").
        /// 하나라도 지정되면 그리드 위에 그룹 줄이 생기고, 그룹이 없는
        /// 컬럼 위는 빈 헤더 배경으로 남는다. 그룹은 표시 전용이다 —
        /// 엑셀 내보내기/복사에는 하위 컬럼 캡션만 나간다. 고정(Frozen)
        /// 컬럼과 함께 쓸 때 그룹이 고정 경계를 걸치지 않게 구성한다.
        /// </summary>
        public string HeaderGroup { get; set; }

        /// <summary>
        /// Text 전용: 컬럼 전체 텍스트에 적용할 글자색 — 파생 지표(Duration 등)를
        /// 강조할 때 쓴다. 값은 "#0078D4" 같은 색 문자열이며 비어 있거나 해석
        /// 불가하면 기본 글자색을 유지한다. 선택 행에서도 이 색이 유지되므로
        /// 선택 배경과 대비가 좋은 색을 고른다.
        /// </summary>
        public string TextColor { get; set; }

        /// <summary>
        /// Text 전용: 컬럼 전체 텍스트를 SemiBold로 강조한다 (기본 false).
        /// TextColor와 조합해 파생 지표를 색+굵기로 강조할 때 쓴다.
        /// AutoFitColumns 측정도 SemiBold 폭 기준으로 계산된다.
        /// </summary>
        public bool TextSemiBold { get; set; }

        /// <summary>
        /// Badge 전용: 배지 배경색으로 쓸 컬럼/속성 이름. 값은 "#FEE2E2" 같은
        /// 색 문자열. 비었거나 해석 불가한 행은 배경 없는 일반 텍스트로 표시된다.
        /// </summary>
        public string BadgeColorMember { get; set; }

        /// <summary>
        /// Badge 전용: 배지 배경색을 **셀 값 자체에서 자동 유도**할지 여부
        /// (기본 false). 켜면 색 컬럼 없이 <see cref="Palette.FromValue(string)"/>
        /// 규칙으로 색이 정해진다 — 같은 값은 어떤 화면·어떤 조회에서든 항상
        /// 같은 색(값 정규화: 공백/대소문자 무시). 색 우선순위:
        /// BadgeColorMember &gt; BadgeSemanticMember &gt; BadgeGroupMember &gt; BadgeAutoColor.
        /// </summary>
        public bool BadgeAutoColor { get; set; }

        /// <summary>
        /// Badge 전용: 배지 색 그룹 키로 쓸 컬럼/속성 이름 (선택 사항) —
        /// 쿼리가 CASE WHEN으로 그룹을 판정해 내려주는 표준 경로. 값이 정수면
        /// Palette 인덱스에 직결되고(0부터, 완전 결정적·상호 구분 보장),
        /// 문자열이면 그 키의 <see cref="Palette.FromValue(string)"/> 색이 된다.
        /// 색만 그룹 키에서 오고 배지에 표시되는 텍스트는 여전히 이 컬럼
        /// (<see cref="DataPropertyName"/>)의 값이다.
        /// </summary>
        public string BadgeGroupMember { get; set; }

        /// <summary>
        /// Badge 전용: 의미 어휘로 쓸 컬럼/속성 이름 (선택 사항) — 쿼리가 판정
        /// 결과("Y"/"N", "success"/"warning"/"error"/"info"/"neutral")를 내려주면
        /// <see cref="SemanticColors"/> 표준색으로 그린다. 모르는 어휘/빈 값은
        /// Neutral (예외 없음). 어휘 전체는 <see cref="SemanticColors.FromName(string)"/> 참조.
        /// </summary>
        public string BadgeSemanticMember { get; set; }

        /// <summary>
        /// Badge 전용: 배지 **값 자체**로 회전 하이라이트를 판정하는 값 목록
        /// (선택 사항) — 세미콜론/쉼표 구분 ("SENDING;PROC"). 셀 값이 목록에
        /// 있으면(공백/대소문자 무시 — FromValue와 같은 정규화) 배지 테두리에
        /// 도는 하이라이트가 생긴다. 더미 판정 컬럼 없이 쓰는 선언형 경로이며,
        /// <see cref="BadgeSpinBorderMember"/>가 지정돼 있으면 그쪽이 우선한다.
        /// </summary>
        public string BadgeSpinValues { get; set; }

        /// <summary>
        /// Badge 전용: **폭을 재는 기준이 되는 값 목록** (세미콜론/쉼표 구분, 기본 빈
        /// 문자열). 비어 있으면 지금처럼 **들어온 데이터**의 가장 긴 값으로 통일 폭을
        /// 잡고, 값이 있으면 **그 목록**의 가장 긴 값으로 잡는다.
        ///
        /// 데이터 기준은 조회마다 폭이 흔들린다 — 이번 조회에 긴 값이 있으면 넓어지고
        /// 다음 조회에 없으면 좁아진다. 상태 컬럼처럼 **올 수 있는 값이 선언돼 있는**
        /// 자리는 그 어휘로 재면 데이터와 무관하게 폭이 고정된다.
        /// </summary>
        public string BadgeWidthValues { get; set; }

        /// <summary>
        /// Badge 전용: **배지마다 제 값 폭으로 그린다** (기본 false). 기본은 그 컬럼의
        /// 가장 긴 값에 맞춘 **통일 고정 폭**이라 배지가 가지런하다 — 같은 컬럼의 배지는
        /// 같은 물건으로 읽혀야 하기 때문이다.
        ///
        /// true 로 두면 통일을 끄고 값 길이대로 들쑥날쑥 그린다. 값 자체의 길이가 뜻을
        /// 갖는 자리(길이로 양을 가늠하는 표)에만 쓴다.
        /// </summary>
        public bool BadgeWidthByValue { get; set; }

        /// <summary>
        /// Badge 전용: **빨간 글자로 강조할 값 목록** (세미콜론/쉼표 구분,
        /// 기본 빈 문자열). 셀 값이 이 중 하나면 배경과 글자를 오류 색 짝으로
        /// 바꾼다 — 색 우선순위 넷(BadgeColorMember/Semantic/Group/AutoColor)보다
        /// 앞선다. 값 비교는 FromValue와 같은 정규화(공백/대소문자 무시).
        ///
        /// 글자만 빨갛게 하지 않는 이유: 값 유도 자동색은 배경이 값마다 달라서
        /// 그 위에 빨간 글자를 얹으면 테마 10종 중 9종에서 대비 4.5:1을 못 넘고
        /// 일부는 아예 안 보인다. 배경까지 바꾸면 전 테마에서 읽힌다.
        /// </summary>
        public string BadgeAccentValues { get; set; }

        /// <summary>
        /// Badge/ComboBadge 전용: 배지 모양 (기본 Rounded=둥근 사각, Pill=알약).
        /// </summary>
        public ChipShape BadgeShape { get; set; }

        /// <summary>Button 전용: 버튼 캡션.</summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// Button 전용: 행별 버튼 활성화 여부로 쓸 컬럼/속성 이름 (선택 사항).
        /// bool 컬럼 또는 "Y"/"true"/"1" 계열 문자열을 참으로 해석한다.
        /// 비워 두면 모든 행에서 버튼이 활성화된다.
        /// </summary>
        public string ButtonEnabledMember { get; set; }

        /// <summary>
        /// 버튼 캡션을 <b>행마다</b> 정하는 컬럼 이름. 비어 있으면
        /// <see cref="ButtonText"/> 하나를 모든 행에 쓴다.
        ///
        /// 한 행이 단계 하나만 가질 때 액션 컬럼을 여러 개 두지 않기 위한 것이다 —
        /// 값이 <b>빈 문자열이면 그 행에는 버튼을 그리지 않는다</b>(비활성 버튼이
        /// 아니라 아예 없다). 더 할 일이 없는 행에 죽은 버튼을 남기지 않는다.
        /// </summary>
        public string ButtonTextMember { get; set; }

        /// <summary>
        /// Button 전용: 버튼 색을 <b>캡션 값 자체에서 자동 유도</b>할지 여부
        /// (기본 false). 켜면 배지 자동색과 같은 <see cref="Palette.FromValue(string)"/>
        /// 규칙으로 행별 캡션(<see cref="ButtonTextMember"/> 값, 없으면
        /// <see cref="ButtonText"/>)이 배경색이 되고 글자색은 배경에서 유도된다 —
        /// 같은 캡션(Accept/Receive/Create 등)은 어떤 화면·어떤 조회에서든 항상
        /// 같은 색이다 (값 정규화: 공백/대소문자 무시). hover/pressed는 같은 색을
        /// 한 단계씩 진하게 하고 글자색은 그 배경에서 다시 유도해 대비를 지킨다.
        /// 비활성 행은 중립 버튼과 같은 회색 처리, 캡션이 비면 중립 버튼 색이다.
        /// </summary>
        public bool ButtonAutoColor { get; set; }

        /// <summary>
        /// Combo 전용: 셀 콤보의 고정 선택지 (예: {"SUCC", "FAIL"}). 선택하면
        /// 원본 행 컬럼 값이 즉시 그 문자열로 갱신된다. 비워 두면 빈 콤보가 된다.
        /// </summary>
        public string[] ComboItems { get; set; }

        /// <summary>
        /// Combo 전용: 선택지별 배지 색 (선택 사항) — ComboItems와 같은 순서의
        /// "#DCFCE7" 같은 색 문자열 배열. 지정하면 선택 값과 드롭다운 항목이
        /// 알약 배지로 표시된다 (글자색은 배경에서 자동 유도 — 배지 컬럼과
        /// 같은 규칙). 비워 두면 일반 텍스트 콤보다.
        /// </summary>
        public string[] ComboItemColors { get; set; }

        /// <summary>
        /// Combo 전용: 행별 입력 가능 여부로 쓸 컬럼/속성 이름 (선택 사항).
        /// bool 컬럼 또는 "Y"/"true"/"1" 계열 문자열을 참으로 해석한다.
        /// 비워 두면 모든 행에서 입력할 수 있다.
        /// </summary>
        public string ComboEnabledMember { get; set; }

        /// <summary>
        /// Combo 전용: **행별** 선택지 목록으로 쓸 컬럼/속성 이름 (선택 사항).
        /// 그 컬럼 셀 값이 <c>string[]</c> / <c>IEnumerable</c>이면 그 행의 콤보
        /// 선택지가 된다 — 행마다 범위가 다를 때 쓴다(예: STUB 슬롯 1~6 / LCC
        /// 슬롯 1~25). 지정하면 <see cref="ComboItems"/>(컬럼 공통 목록)보다
        /// 우선한다. 비워 두면 공통 목록을 쓴다.
        /// </summary>
        public string ComboItemsMember { get; set; }

        /// <summary>
        /// Combo 전용: **입력 가능 콤보** 여부 (기본 false — 고르기 전용, 2026-11-15). true 면 셀에 글자를 쳐서
        /// 선택지를 좁혀 고른다 — 친 글자로 시작하는 선택지만 남고(대소문자 무시), 칸을 떠날 때(Tab · Enter ·
        /// 포커스 이동) 친 글자가 선택지와 같거나 후보가 하나뿐이면 그 값이 들어간다. 선택지에 없는 글자는
        /// 들어가지 않고 칸이 비워진다(자유 입력이 아니다). 치는 동안에는 원본 행 값이 바뀌지 않고 Esc 는
        /// 원래 값으로 되돌린다. 셀에 포커스만 둔 채 글자를 쳐도 입력이 시작된다(F2 는 입력란으로 들어간다).
        /// 그리드의 <c>ReadOnly</c> 와 무관하게 동작한다. 배지형(<see cref="ComboItemColors"/>)에는 적용되지 않는다 —
        /// 색이 지정된 콤보는 고르기 전용으로 남는다.
        /// </summary>
        public bool ComboEditable { get; set; }

        /// <summary>
        /// Badge 전용: 행별로 배지 테두리에 **도는 하이라이트**(밝은 띠가 배지
        /// 둘레를 회전)를 줄지 여부로 쓸 컬럼/속성 이름 (선택 사항). 참인 행만
        /// 효과가 생긴다(작업중 등 활성 강조). 발광(글로우) 없이 테두리를 도는
        /// 모션만. bool/"Y"/"true"/"1" 참. 비우면 효과 없음.
        /// </summary>
        public string BadgeSpinBorderMember { get; set; }

        /// <summary>
        /// Spinner 전용: 행별로 스피너를 돌릴지 여부로 쓸 컬럼/속성 이름.
        /// 참인 행만 회전 스피너가 보이고(작업중 표시), 거짓인 행은 빈 셀이다.
        /// bool 컬럼 또는 "Y"/"true"/"1" 계열 문자열을 참으로 해석한다.
        /// 비워 두면 모든 행에서 스피너가 돈다.
        /// </summary>
        public string SpinnerActiveMember { get; set; }

        /// <summary>
        /// CheckBox 전용: 헤더에 전체 선택/해제 체크박스를 표시할지 여부
        /// (기본 false). 켜면 헤더 캡션 대신 체크박스가 올라가고, 클릭하면
        /// 현재 그리드에 표시 중인 모든 행의 값이 일괄 설정된다. 헤더 상태는
        /// 행 값에 따라 체크(전체)/해제(없음)/중간(일부)으로 자동 갱신된다.
        /// </summary>
        public bool HeaderCheckBox { get; set; }

        /// <summary>
        /// CheckBox 전용: 셀 모양 (기본 CheckBox). Switch면 온/오프 토글
        /// 스위치로 그린다 — 행 선택이 아니라 **행마다의 설정을 켜고 끄는**
        /// 컬럼용이다. HeaderCheckBox(전체 선택)는 Switch와 함께 쓰지 않는다.
        /// </summary>
        public GridCheckStyle CheckStyle { get; set; }

        /// <summary>
        /// CheckBox 전용: 행별 토글 가능 여부로 쓸 컬럼/속성 이름 (선택 사항).
        /// bool 컬럼 또는 "Y"/"true"/"1" 계열 문자열을 참으로 해석한다.
        /// 비워 두면 모든 행에서 토글할 수 있다. 거짓인 행은 잠긴 모양으로
        /// 표시되고 클릭해도 값이 바뀌지 않는다 (조건이 맞을 때만 바꿀 수 있는
        /// 설정 — 예: 포트가 전부 비었을 때만 자동 운전 전환).
        /// </summary>
        public string CheckEnabledMember { get; set; }

        /// <summary>
        /// CheckBox 전용: 값 컬럼이 bool이 아니라 <b>"Y"/"N" 같은 문자열 어휘</b>일 때
        /// 참에 해당하는 값 (선택 사항, 기본 빈 문자열 = bool 컬럼 그대로).
        /// <see cref="CheckFalseValue"/>와 <b>둘 다 채워야</b> 어휘 모드가 켜진다 —
        /// 한쪽만 채우면 끈 자리에 무엇을 써야 할지 정해지지 않기 때문이다.
        ///
        /// 켜면 조회가 이미 내려주는 Y/N 컬럼에 체크 상태를 바로 물릴 수 있다.
        /// 화면용 bool 컬럼(<c>..._CAN</c> 등)을 따로 만들 필요가 없다. 표시는
        /// 어휘 또는 일반 참 규칙("Y"/"YES"/"TRUE"/"1")으로 읽고, 토글은 <b>원본
        /// 어휘 그대로</b> 되쓴다(체크 → CheckTrueValue). 헤더 전체 선택도 같다.
        ///
        /// WinForms <c>DataGridViewCheckBoxColumn.TrueValue</c>/<c>FalseValue</c>와
        /// 같은 자리다.
        /// </summary>
        public string CheckTrueValue { get; set; }

        /// <summary>
        /// CheckBox 전용: 거짓에 해당하는 값 (기본 빈 문자열).
        /// <see cref="CheckTrueValue"/> 참고 — 둘 다 채워야 어휘 모드가 켜진다.
        /// </summary>
        public string CheckFalseValue { get; set; }
    }
}
