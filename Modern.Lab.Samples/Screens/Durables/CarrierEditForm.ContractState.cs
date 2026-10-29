using System;
using System.Data;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.Data;
using Modern.Lab.Hosting.Contracts;
using Modern.Lab.Hosting.ResponseContracts;
using Modern.Lab.Samples.Contracts;
using Modern.Lab.Samples.Services;
using Modern.Lab.Hosting.Schema;

namespace Modern.Lab.Samples
{
    public partial class CarrierEditForm
    {
        private static readonly ResponseContractSet FoupContracts =
                CarrierEditContracts.Build(Schemas.Durable.DurableTyp.Foup.Value);

        private static readonly ResponseContractSet TrayContracts =
                CarrierEditContracts.Build(Schemas.Durable.DurableTyp.Tray.Value);

        private static ResponseContractSet ContractsFor(string durableTyp)
        {
            return string.Equals(durableTyp, Schemas.Durable.DurableTyp.Tray.Value, StringComparison.Ordinal)
                    ? TrayContracts
                    : FoupContracts;
        }

        private TableResponse carrierCurrent;
        private TableResponse sourceMapCurrent;
        private TableResponse targetMapCurrent;

        private void ResetActionContractState()
        {
            this.carrierCurrent = null;
            this.ResetSourceMapContractState();
            this.ResetTargetMapContractState();
        }

        private void ResetSourceMapContractState()
        {
            this.sourceMapCurrent = null;
        }

        private void ResetTargetMapContractState()
        {
            this.targetMapCurrent = null;
        }

        private void BeginCarrierListContractLoad(string durableTyp)
        {
            this.carrierCurrent = null;
            this.ResetSourceMapContractState();
            this.ResetTargetMapContractState();
        }

        private void BeginSourceMapContractLoad(string durableTyp, string carrierId)
        {
            this.sourceMapSettled = false;
            this.sourceMapCurrent = null;
        }

        private void BeginTargetMapContractLoad(string durableTyp, string carrierId)
        {
            this.targetMapSettled = false;
            this.targetMapCurrent = null;
        }

        private DataTable BindCarrierResponse(DataTable incoming, string durableTyp)
        {
            ResponseContractSet contracts = ContractsFor(durableTyp);
            this.carrierCurrent = TableResponse.Read(
                    ResponseKind.Data,
                    incoming ?? new DataTable(),
                    contracts.Aliases,
                    contracts.TableFor(CarrierEditContracts.CarrierTable));

            if (this.carrierCurrent.State == TableResponseState.MissingRequired)
            {
                this.ShowMissingColumns(this.carrierCurrent, false);
                return DeclaredEmpty(CarrierEditContracts.CarrierTable, durableTyp);
            }

            return this.carrierCurrent.Table;
        }

        private DataTable BindMapResponse(
                string tableId,
                DataTable incoming,
                string durableTyp,
                string carrierId,
                Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            bool schemaWasAbsent = incoming == null || incoming.Columns.Count == 0;
            DataTable prepared = CarrierEditPresenter.PrepareMap(incoming);

            if (prepared.Rows.Count == 0)
            {
                prepared = CarrierEditPresenter.PrepareMap(DeclaredEmpty(tableId, durableTyp));
            }

            TableResponse received = TableResponse.Read(
                    ResponseKind.Data,
                    prepared,
                    ContractsFor(durableTyp).Aliases,
                    ContractsFor(durableTyp).TableFor(tableId));

            if (received.State == TableResponseState.MissingRequired)
            {
                this.SetMapCurrent(tableId, received, durableTyp, carrierId);
                this.ShowMissingColumns(received, false);
                return SlotMapWaferTable.CreateEmpty();
            }

            if (schemaWasAbsent && !this.SelectedCarrierIsEmpty(combo))
            {
                DataTable inconsistent = new DataTable();
                inconsistent.Columns.Add("SparseMap", typeof(string));
                TableResponse blocked = TableResponse.Read(
                        ResponseKind.Data,
                        inconsistent,
                        ContractsFor(durableTyp).Aliases,
                        RequiredMapSchemaContract(ContractsFor(durableTyp).TableFor(tableId)));
                this.SetMapCurrent(tableId, blocked, durableTyp, carrierId);
                this.ShowEmptyMapForLoadedCarrier(tableId, carrierId, combo);
                return SlotMapWaferTable.CreateEmpty();
            }

            DataTable normalized = this.NormalizeWafers(prepared, durableTyp, combo);
            TableResponse full = TableResponse.Read(
                    ResponseKind.Data,
                    CarrierEditPresenter.PrepareMap(normalized),
                    ContractsFor(durableTyp).Aliases,
                    ContractsFor(durableTyp).TableFor(tableId));
            TableResponse current = received == null || received.State == TableResponseState.Empty
                    ? full
                    : received;

            this.SetMapCurrent(tableId, current, durableTyp, carrierId);
            return full.Table;
        }

        private bool SelectedCarrierIsEmpty(
                Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            DataRowView selected = combo == null ? null : combo.SelectedItem as DataRowView;
            return selected != null
                    && CarrierEditPresenter.IsValidCarrier(selected.Row)
                    && SelectedCarrierSlotCount(selected.Row) == 0;
        }

        private static int SelectedCarrierSlotCount(DataRow row)
        {
            string durableTyp = TableHelper.CellText(row, Schemas.Durable.DurableTyp.Name).Trim();

            if (durableTyp == Schemas.Durable.DurableTyp.Foup.Value)
            {
                return TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.FoupLotQty.Name));
            }

            return TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.StubLotQty.Name))
                    + TableHelper.ParseInt(TableHelper.CellText(row, Schemas.Durable.LccLotQty.Name));
        }

        private static TableContract RequiredMapSchemaContract(TableContract contract)
        {
            string[] columns = new string[contract.DeclaredNames.Count];

            for (int index = 0; index < contract.DeclaredNames.Count; index++)
            {
                columns[index] = contract.DeclaredNames[index];
            }

            return new TableContract(contract.TableId).Require(columns);
        }

        private void SetMapCurrent(
                string tableId,
                TableResponse current,
                string durableTyp,
                string carrierId)
        {
            if (tableId == CarrierEditContracts.SourceMapTable)
            {
                this.sourceMapCurrent = current;
            }
            else
            {
                this.targetMapCurrent = current;
            }
        }

        private void SetCarrierFailure(string durableTyp, Exception failure)
        {
            this.carrierCurrent = Failure(CarrierEditContracts.CarrierTable, durableTyp, failure);
            this.ResetSourceMapContractState();
            this.ResetTargetMapContractState();
        }

        private void SetMapFailure(string tableId, string durableTyp, string carrierId, Exception failure)
        {
            TableResponse current = Failure(tableId, durableTyp, failure);

            if (tableId == CarrierEditContracts.SourceMapTable)
            {
                this.sourceMapCurrent = current;
            }
            else
            {
                this.targetMapCurrent = current;
            }

        }

        private bool CanExecuteCarrierAction(string action)
        {
            return this.CanExecuteCarrierAction(action, this.stagedWafers);
        }

        private bool CanExecuteCarrierAction(string action, DataTable staged)
        {
            return this.CanStartAction() && this.CarrierActionReasonFor(action, staged).Length == 0;
        }

        private string CarrierActionReason(string action)
        {
            return this.CarrierActionReasonFor(action, this.stagedWafers);
        }

        private string CarrierActionReasonFor(string action, DataTable staged)
        {
            if (action == CarrierEditPresenter.ActionMove && this.IsExchange
                    && CarrierEditPresenter.CountFilled(staged) == 0 && this.stageCapacityReason.Length > 0)
            {
                return this.stageCapacityReason;
            }

            DataRowView source = this.cboSource.SelectedItem as DataRowView;
            DataRowView target = this.cboTarget.SelectedItem as DataRowView;
            return CarrierEditForm.ActionReason(
                    action, this.entryMode, source == null ? null : source.Row, target == null ? null : target.Row,
                    this.SelectedDurableTyp(), this.SourceId(), this.TargetId(),
                    this.sourceData, this.targetData, staged);
        }

        private void ShowEmptyMapForLoadedCarrier(
                string tableId, string carrierId, Modern.Lab.WinForms.Controls.Selection.ModernComboBox combo)
        {
            DataRowView selected = combo == null ? null : combo.SelectedItem as DataRowView;
            int reported = selected == null ? 0 : SelectedCarrierSlotCount(selected.Row);

            this.ShowErrorMessage(
                    this.QueryFailedCaption,
                    "The slot map for this carrier came back empty although the carrier reports loaded units. Try again, or contact support.",
                    ShortTableName(tableId) + " — no columns received for carrier " + (carrierId ?? string.Empty)
                            + " (carrier reports " + reported.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + " loaded units)");
        }

        private static string ShortTableName(string tableId)
        {
            int dot = (tableId ?? string.Empty).LastIndexOf('.');
            return dot < 0 ? (tableId ?? string.Empty) : tableId.Substring(dot + 1);
        }

        private static TableResponse Failure(string tableId, string durableTyp, Exception failure)
        {
            return TableResponse.Read(
                    ResponseKind.Failed,
                    null,
                    ContractsFor(durableTyp).Aliases,
                    ContractsFor(durableTyp).TableFor(tableId),
                    string.Empty,
                    failure == null ? "Carrier query failed." : failure.Message);
        }

        private static DataTable DeclaredEmpty(string tableId, string durableTyp)
        {
            return TableResponse.Read(
                    ResponseKind.Data,
                    new DataTable(),
                    ContractsFor(durableTyp).Aliases,
                    ContractsFor(durableTyp).TableFor(tableId)).Table;
        }

    }
}
