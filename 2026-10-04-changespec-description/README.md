# ChangeSpec — Description 추가, 전문에 currentOperId · description

- 날짜: 2026-10-04
- 기준: v0.71.0 에 `Entities` → `Schemas` 이름 변경을 적용한 상태
- DLL 교체 없음. 업무 화면 소스 4개만 바꾼다.

## 바뀌는 것

- ChangeSpec 창 라벨이 `Prod Id` · `Flow Id` · `Oper Id` 이다.
- `Flow Id` 와 `Oper Id` 가 한 줄에 있다.
- 그 아래에 `Description` 입력칸이 있다(선택 입력).
- `ChangeLotSpec` 전문에 두 값을 더 싣는다.
  - `currentOperId` — Lot 정보에 있는 지금 `OPER_ID`
  - `description` — 입력한 설명

```text
ChangeLotSpec LOT_ID=… PROD_ID=… FLOW_ID=… OPER_ID=… currentOperId=… description=… USER_ID=…
```

`PROD_ID` · `FLOW_ID` · `OPER_ID` 는 전과 같이 바꿀 새 스펙이다. 공백이 있는 값은 공통이 대괄호로 감싼다.
화면은 `currentOperId` 가 비어도 막지 않는다. 필수 여부는 서버가 판정한다.

## 파일

이 폴더 아래 경로가 프로젝트 안의 경로와 같다.

| 파일 | 반영 방법 |
|---|---|
| `Modern.Lab.Samples/Dialogs/Lots/ChangeSpecDialogForm.Designer.cs` | 전체 교체 |
| `Modern.Lab.Samples/Dialogs/Lots/ChangeSpecDialogForm.cs` | 전체 교체, 또는 아래 세 곳만 수정 |
| `Modern.Lab.Samples/Screens/Lots/LotManagementForm.cs` | 아래 세 곳만 수정(회사에서 고친 부분이 있으면 전체 교체하지 않는다) |
| `Modern.Lab.Samples/Screens/Lots/LotManagementForm.Server.cs` | `ChangeLotSpec` 메서드만 수정(회사 전문명·키는 회사 것 유지) |

## 손으로 고칠 때

### ChangeSpecDialogForm.cs

```csharp
public string CurrentOperId { get; private set; }

public string Description { get; private set; }
```

```csharp
public ChangeSpecDialogForm(DataRow lot) : this()
{
    this.lot = lot;
    this.CurrentOperId = lot == null ? string.Empty : TableHelper.CellText(lot, Schemas.Lot.OperId.Name).Trim();
}
```

확인 버튼(`OnOkClick`)에서 `this.OperId = operId;` 다음 줄:

```csharp
this.Description = this.txtDescription.Text ?? string.Empty;
```

### LotManagementForm.cs

```csharp
this.ProcessChangeSpec(lotId, dialog.ProdId, dialog.FlowId, dialog.OperId, dialog.CurrentOperId, dialog.Description);
```

```csharp
private void ProcessChangeSpec(
        string lotId, string prodId, string flowId, string operId, string currentOperId, string description)
```

```csharp
() => this.ChangeLotSpec(lotId, prodId, flowId, operId, currentOperId, description),
```

### LotManagementForm.Server.cs

```csharp
private DataActionResult ChangeLotSpec(
        string lotId, string prodId, string flowId, string operId, string currentOperId, string description)
{
    return this.RequestFields("ChangeLotSpec", Schemas.Lot.LotId.Name, lotId,
            Schemas.Lot.ProdId.Name, prodId, Schemas.Lot.FlowId.Name, flowId, Schemas.Lot.OperId.Name, operId,
            "currentOperId", currentOperId, "description", description, ServerFields.UserId, this.CurrentUserId);
}
```

## 확인한 것

- 빌드 통과.
- 홈 환경에서 위 전문이 실제로 나갔다. 홈 서버는 두 값을 저장하지 않는다.
- 회사 서버와의 전문 왕복, 실제 화면 모양은 확인하지 않았다.
