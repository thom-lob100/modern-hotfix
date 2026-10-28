using System;
using System.Collections.Generic;
using System.Data;

using Modern.Lab.Hosting.Messaging;

namespace Modern.Lab.Hosting
{

    public partial class ModernFormBase
    {

        protected virtual bool UseServerMessaging
        {
            get { return false; }
        }

        protected virtual string SendRequestText(string request)
        {

            throw new NotSupportedException(
                    "Server messaging is not wired yet — see ModernFormBase.SendRequestText.");
        }

        protected virtual DataTable ParseQueryData(string sendMessage)
        {
            return ServerMessageFormat.ParseQueryTable(sendMessage);
        }

        protected DataActionResult Request(string requestText)
        {
            DataActionResult reply;

            try
            {
                reply = this.Exchange(requestText);
            }
            catch (Exception failure)
            {
                reply = DataActionResult.CommunicationLost(this.ServerFailureText(failure), failure);
            }

            reply.SetTableParser(this.ParseQueryData);
            reply.SetRequestText(requestText);

            if (!reply.Success && !reply.CommunicationFailure)
            {
                if (this.IsNoDataReply(reply))
                {
                    reply.SetTable(new DataTable());
                    reply.SetValues(new string[0]);
                }
                else
                {
                    reply.MarkQueryFault();
                }
            }

            return reply;
        }

        protected virtual bool IsNoDataReply(DataActionResult reply)
        {
            if (reply == null)
            {
                return false;
            }

            return string.Equals(reply.Code, "NOT_FOUND", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(reply.Code, "DATA_NOT_FOUND", StringComparison.OrdinalIgnoreCase);
        }

        private DataActionResult Exchange(string requestText)
        {
            return DataActionResult.FromReply(this.SendRequestText(requestText));
        }

        protected void RunAction(Func<DataActionResult> call, Action<DataActionResult> onSuccess)
        {
            this.RunAction(call, onSuccess, null, this.ActionBusyText);
        }

        protected void RunAction(
                Func<DataActionResult> call, Action<DataActionResult> onSuccess, string busyText)
        {
            this.RunAction(call, onSuccess, null, busyText);
        }

        protected void RunAction(
                Func<DataActionResult> call,
                Action<DataActionResult> onSuccess,
                Action<DataActionResult> onRejected,
                string busyText)
        {
            if (call == null)
            {
                return;
            }

            this.RunActionCoreAsync(call, onSuccess, onRejected, busyText);
        }

        protected void RunActionBatch<T>(
                IList<T> items,
                Func<T, DataActionResult> call,
                Action<BatchProgress> onProgress,
                Action<BatchOutcome> onDone,
                string busyText)
        {
            if (call == null || items == null || items.Count == 0)
            {
                return;
            }

            this.RunActionBatchCoreAsync(items, call, onProgress, onDone, busyText);
        }

        protected virtual string ActionBusyText
        {
            get { return "Working…"; }
        }

        private async void RunActionCoreAsync(
                Func<DataActionResult> call,
                Action<DataActionResult> onSuccess,
                Action<DataActionResult> onRejected,
                string busyText)
        {
            if (!this.BeginAction())
            {
                return;
            }

            DataActionResult reply = null;
            Exception callerMistake = null;

            try
            {
                using (this.Busy(busyText ?? this.ActionBusyText))
                {
                    reply = await System.Threading.Tasks.Task.Run(call);
                }
            }
            catch (Exception failure)
            {
                reply = this.ClassifyActionFailure(failure, out callerMistake);
            }
            finally
            {
                this.EndAction();
            }

            if (this.IsDisposed || this.Disposing)
            {
                return;
            }

            if (callerMistake != null)
            {
                this.ShowCallerMistake(callerMistake);
                return;
            }

            try
            {
                this.SettleAction(reply, onSuccess, onRejected);
            }
            catch (Exception callbackFailure)
            {
                this.ShowServerFailure(callbackFailure);
            }
        }

        private async void RunActionBatchCoreAsync<T>(
                IList<T> items,
                Func<T, DataActionResult> call,
                Action<BatchProgress> onProgress,
                Action<BatchOutcome> onDone,
                string busyText)
        {
            if (!this.BeginAction())
            {
                return;
            }

            int total = items.Count;
            int succeeded = 0;
            int failed = 0;
            DataActionResult firstFailure = null;
            Exception callerMistake = null;

            bool communicationFailureSeen = false;

            try
            {
                using (this.Busy(busyText ?? this.ActionBusyText))
                {
                    for (int index = 0; index < total; index = index + 1)
                    {
                        T item = items[index];
                        DataActionResult one;

                        try
                        {
                            one = await System.Threading.Tasks.Task.Run(
                                    delegate { return call(item); });
                        }
                        catch (Exception failure)
                        {
                            one = this.ClassifyActionFailure(failure, out callerMistake);
                        }

                        if (callerMistake != null)
                        {
                            break;
                        }

                        if (one.Success)
                        {
                            succeeded = succeeded + 1;
                        }
                        else
                        {
                            failed = failed + 1;

                            if (firstFailure == null)
                            {
                                firstFailure = one;
                            }

                            if (one.CommunicationFailure)
                            {
                                communicationFailureSeen = true;
                            }
                        }

                        if (this.IsDisposed || this.Disposing)
                        {
                            return;
                        }

                        if (onProgress != null)
                        {
                            try
                            {
                                onProgress(new BatchProgress(index + 1, total, one));
                            }
                            catch (Exception progressFailure)
                            {
                                this.ShowServerFailure(progressFailure);
                            }
                        }
                    }
                }
            }
            finally
            {
                this.EndAction();
            }

            if (this.IsDisposed || this.Disposing)
            {
                return;
            }

            if (callerMistake != null)
            {
                this.ShowCallerMistake(callerMistake);
                return;
            }

            BatchOutcome outcome = new BatchOutcome(total, succeeded, failed, firstFailure);

            if (communicationFailureSeen)
            {
                this.NotifyActionCommunicationFailure();
            }
            else
            {
                this.NotifyActionCommunicationSuccess();
            }

            try
            {
                if (onDone != null)
                {
                    onDone(outcome);
                }
                else if (failed > 0 && firstFailure != null)
                {
                    this.ShowActionFailure(firstFailure);
                }
            }
            catch (Exception callbackFailure)
            {
                this.ShowServerFailure(callbackFailure);
            }
        }

        private DataActionResult ClassifyActionFailure(
                Exception failure, out Exception callerMistake)
        {
            ArgumentException contractFailure = failure as ArgumentException;

            if (contractFailure != null)
            {
                callerMistake = contractFailure;
                return null;
            }

            callerMistake = null;

            return DataActionResult.CommunicationLost(this.ServerFailureText(failure), failure);
        }

        private bool BeginAction()
        {
            if (this.IsDisposed || this.Disposing)
            {
                return false;
            }

            if (!this.CanStartAction())
            {
                this.ShowToast(
                        this.ActionRunningText,
                        Modern.Lab.Controls.Wpf.Display.ToastKind.Warning);
                return false;
            }

            this.MarkActionInProgress(true);
            return true;
        }

        private void EndAction()
        {
            this.MarkActionInProgress(false);
        }

        private void SettleAction(
                DataActionResult reply,
                Action<DataActionResult> onSuccess,
                Action<DataActionResult> onRejected)
        {
            if (reply.CommunicationFailure)
            {
                this.NotifyActionCommunicationFailure();
            }
            else
            {
                this.NotifyActionCommunicationSuccess();
            }

            if (reply.Success)
            {
                if (onSuccess != null)
                {
                    onSuccess(reply);
                }

                return;
            }

            if (onRejected != null)
            {
                onRejected(reply);
                return;
            }

            this.ShowActionFailure(reply);
        }

        /// <summary>
        /// 조회 응답에 화면이 읽는 컬럼이 없을 때의 알림 — 여섯 화면이 같은 코드를 갖고 있던 것을
        /// 2026-09-18 에 여기로 모았다. 조용한 재조회에서는 알리지 않는다.
        /// </summary>
        protected void ShowMissingColumns(ResponseContracts.TableResponse response, bool silent)
        {
            if (silent)
            {
                return;
            }

            this.ShowErrorMessage(
                    this.QueryFailedCaption,
                    ResponseContracts.ResponseColumns.MissingMessage,
                    ResponseContracts.ResponseColumns.MissingDetails(response));
        }

        /// <summary>
        /// 실행 실패 창 — 서버가 거절했으면 그 문구가 본문이고 반환 코드 · 전문은 상세로, 응답이 오지 않았으면
        /// 고정 문구가 본문이고 예외 원문은 상세로 간다. 제목은 짧은 명사형(<see cref="ActionFailedCaption"/>)이다.
        /// </summary>
        /// <param name="reply">실패 응답.</param>
        protected virtual void ShowActionFailure(DataActionResult reply)
        {
            if (reply == null)
            {
                return;
            }

            if (reply.CommunicationFailure)
            {
                Exception cause = reply.Cause ?? new ServerReplyException(
                        reply.Message, string.Empty, reply.RequestText, false, null);

                this.ShowErrorMessage(
                        this.ActionFailedCaption, this.FailureSummary(cause), FailureDetails(cause));
                return;
            }

            string reason = string.IsNullOrEmpty(reply.Message)
                    ? this.ActionFailedText
                    : reply.Message;

            this.ShowErrorMessage(this.ActionFailedCaption, reason, RejectionDetails(reply));
        }

        // 거절 응답의 상세 — 반환 코드와 보낸 전문.
        private static string RejectionDetails(DataActionResult reply)
        {
            List<string> lines = new List<string>();

            if (reply.Code.Length > 0)
            {
                lines.Add("code: " + reply.Code);
            }

            if (reply.RequestText.Length > 0)
            {
                lines.Add("request: " + reply.RequestText);
            }

            return string.Join(Environment.NewLine, lines.ToArray());
        }

        /// <summary>화면이 전문을 잘못 조립한 경우(인자 예외) — 제목은 화면 오류, 본문은 고정 문구, 예외 원문은 상세.</summary>
        /// <param name="mistake">인자 예외.</param>
        protected virtual void ShowCallerMistake(Exception mistake)
        {
            if (mistake == null)
            {
                return;
            }

            this.ShowErrorMessage(this.ScreenErrorCaption, this.CallerMistakeText, FailureDetails(mistake));
        }

        protected virtual string CallerMistakeText
        {
            get { return "This screen built the request incorrectly. Copy the details and send them to support."; }
        }

        protected virtual string ActionFailedCaption
        {
            get { return "Action failed"; }
        }

        protected virtual string ActionFailedText
        {
            get { return "The request could not be completed."; }
        }

        public sealed class BatchProgress
        {
            internal BatchProgress(int completed, int total, DataActionResult last)
            {
                this.Completed = completed;
                this.Total = total;
                this.Last = last;
            }

            public int Completed { get; private set; }

            public int Total { get; private set; }

            public DataActionResult Last { get; private set; }

            public string Text
            {
                get
                {
                    return this.Completed.ToString(
                                    System.Globalization.CultureInfo.InvariantCulture)
                            + " / "
                            + this.Total.ToString(
                                    System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }

        public sealed class BatchOutcome
        {
            internal BatchOutcome(
                    int total, int succeeded, int failed, DataActionResult firstFailure)
            {
                this.Total = total;
                this.Succeeded = succeeded;
                this.Failed = failed;
                this.FirstFailure = firstFailure;
            }

            public int Total { get; private set; }

            public int Succeeded { get; private set; }

            public int Failed { get; private set; }

            public DataActionResult FirstFailure { get; private set; }

            public bool AllSucceeded
            {
                get { return this.Failed == 0; }
            }

            public string Text
            {
                get
                {
                    System.Globalization.CultureInfo culture =
                            System.Globalization.CultureInfo.InvariantCulture;

                    if (this.Failed == 0)
                    {
                        return this.Succeeded.ToString(culture) + " succeeded";
                    }

                    return this.Succeeded.ToString(culture) + " succeeded, "
                            + this.Failed.ToString(culture) + " failed";
                }
            }
        }
    }
}
