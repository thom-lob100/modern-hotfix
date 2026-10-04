# Hold · NotOnHold · Skip — 공정 사전 검사 걷어내기

- 날짜: 2026-10-04
- 기준: v0.71.0 에 `Entities` → `Schemas` 이름 변경을 적용한 상태
- DLL 교체 없음. 업무 화면 소스 5개만 바꾼다.

## 바뀌는 것

Hold · NotOnHold · Skip 이 Lot 의 공정 값을 전문에 싣는 것은 그대로다.
화면이 그 값을 두고 하던 판정만 없앴다. 공정이 필수인지, 창을 연 사이 공정이 바뀐 요청을 어떻게 할지는 서버가 판정한다.

- Hold / NotOnHold
  - Lot 의 `OPER_ID` 가 비면 창을 열지 않던 것을 없앴다.
  - `OPER_ID` 가 비면 확인 버튼이 꺼지던 것을 없앴다.
  - 확인 뒤 `OPER_ID` 를 다시 비교해 다르면 보내지 않던 것을 없앴다.
- Skip
  - `OPER_ID` 가 비면 실행 버튼이 꺼지고 안내가 뜨던 것을 없앴다.
  - 확인 뒤 `OPER_ID` 를 다시 비교해 다르면 보내지 않던 것을 없앴다.
  - `OPER_ID` 가 비면 전문 조립에서 오류를 내던 것을 없앴다.

사유 코드 선택 · 엔지니어 선택 · Rules 파일의 버튼 조건 · 조회 실패 처리는 바꾸지 않았다.

## 파일

이 폴더 아래 경로가 프로젝트 안의 경로와 같다.

| 파일 | 고친 곳 |
|---|---|
| `Modern.Lab.Samples/Screens/Lots/LotManagementForm.cs` | `OpenHoldDialog` 의 빈 공정 차단 삭제. `HoldInputCurrent` 를 `HoldInputComplete` 로 바꾸고 `ProcessHold` · `ProcessNotOnHold` 의 호출을 맞춤 |
| `Modern.Lab.Samples/Dialogs/Lots/LotHoldDialogForm.cs` | `RefreshSelection` 의 `CurrentOperId` 조건 한 줄 삭제 |
| `Modern.Lab.Samples/Screens/Equipment/EquipmentLotForm.cs` | `ProcessSkip` 의 공정 재비교와 빈 공정 차단 삭제 |
| `Modern.Lab.Samples/Screens/Equipment/EquipmentLotForm.Server.cs` | `JobSkip` 의 `operId` 빈 값 오류 삭제 |
| `Modern.Lab.Samples/Dialogs/Equipment/JobSkipDialogForm.cs` | `OperId` 버튼 조건과 안내 문구 삭제 |

`LotManagementForm.cs` 에는 같은 날의 ChangeSpec 변경(`2026-10-04-changespec-description` 폴더)도 들어 있다.
전체 교체하면 두 변경이 함께 들어간다. `EquipmentLotForm.Server.cs` 와 `LotManagementForm.Server.cs` 처럼
회사에서 전문명·키를 고친 파일은 전체 교체하지 말고 아래 부분만 고친다.

## 손으로 고칠 때

### LotManagementForm.cs

`OpenHoldDialog` 에서 아래 블록을 지운다.

```csharp
if (string.IsNullOrWhiteSpace(TableHelper.CellText(lot, Schemas.Lot.OperId.Name)))
{
    this.ShowErrorMessage("Lot unavailable", "Refresh the lot information before continuing.", "OPER_ID is missing or empty.");
    return;
}
```

`HoldInputCurrent` 메서드를 아래로 바꾼다.

```csharp
private static bool HoldInputComplete(string lotId, string code, string engrUserId)
{
    return !string.IsNullOrWhiteSpace(lotId) && !string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(engrUserId);
}
```

`ProcessHold` · `ProcessNotOnHold` 의 첫 줄을 맞춘다.

```csharp
if (!HoldInputComplete(lotId, holdCode, engrUserId)) { return; }
```

```csharp
if (!HoldInputComplete(lotId, releaseCode, engrUserId)) { return; }
```

### LotHoldDialogForm.cs

`RefreshSelection` 에서 이 줄을 지운다.

```csharp
&& !string.IsNullOrWhiteSpace(this.CurrentOperId)
```

### EquipmentLotForm.cs

`ProcessSkip` 의 앞부분을 아래로 바꾼다(`operNow` 변수와 `skip.OperId == operNow` 비교를 뺀다).

```csharp
string lotId = TableHelper.CellText(job, Schemas.LotPriority.LotId.Name).Trim();
string reason = skip != null && skip.LotId == lotId
                && SameJobTarget(job, EquipmentLotPresenter.JobSummary(this.decision))
        ? JobActionReason(key, this.decision)
        : JobTargetChanged;
```

```csharp
if (skip.ReasonCode.Length == 0)
{
    return;
}
```

### EquipmentLotForm.Server.cs

`JobSkip` 에서 아래 블록을 지운다.

```csharp
if (string.IsNullOrWhiteSpace(operId))
{
    throw new ArgumentException("OPER_ID is required to skip this lot.");
}
```

### JobSkipDialogForm.cs

상태 문구를 한 줄로 바꾼다.

```csharp
this.lblStatus.Text = codes.Rows.Count == 0 ? "No skip codes are registered." : string.Empty;
```

`RefreshSelection` 에서 이 줄을 지운다.

```csharp
&& this.OperId.Length > 0
```

## 확인한 것

- 빌드 통과.
- 화면 검사는 돌리지 않았다. 실제 화면과 회사 서버의 판정은 확인하지 않았다.
