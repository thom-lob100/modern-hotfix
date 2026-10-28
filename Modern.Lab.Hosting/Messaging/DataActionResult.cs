using System;
using System.Collections.Generic;
using System.Data;

namespace Modern.Lab.Hosting.Messaging
{

    public sealed class DataActionResult
    {
        private readonly Dictionary<string, object> fields;

        private string data;
        private DataTable table;
        private string[] values;
        private bool queryFault;
        private string requestText = string.Empty;

        private Func<string, DataTable> tableParser;

        public DataActionResult(Dictionary<string, object> responseFields)
        {
            this.fields = responseFields ?? new Dictionary<string, object>();
            this.Message = string.Empty;
            this.Code = string.Empty;
        }

        public static DataActionResult CommunicationLost(string message)
        {
            DataActionResult result = new DataActionResult(null);
            result.CommunicationFailure = true;
            result.Message = message ?? string.Empty;
            return result;
        }

        /// <summary>교환을 끊은 예외를 함께 갖는 통신 실패 — 폼 베이스가 원인(시간 초과 · 연결 끊김 · 화면 오류)을 갈라 알린다.</summary>
        public static DataActionResult CommunicationLost(string message, Exception cause)
        {
            DataActionResult result = CommunicationLost(message);
            result.Cause = cause;
            return result;
        }

        public static DataActionResult FromReply(string reply)
        {
            Dictionary<string, object> map = ReplyMessageParser.Parse(reply);
            DataActionResult result = new DataActionResult(map);

            if (!map.ContainsKey(ServerMessageFormat.FieldReturnCode))
            {
                throw new InvalidOperationException("Malformed reply message: " + reply);
            }

            result.Success = string.Equals(
                    result.Text(ServerMessageFormat.FieldReturnCode).Trim(),
                    ServerMessageFormat.SuccessReturnCode,
                    StringComparison.Ordinal);

            result.Message = result.Success
                    ? result.Text(ServerMessageFormat.FieldSendMessage)
                    : First(result.Text(ServerMessageFormat.FieldErrorMessage),
                            result.Text(ServerMessageFormat.FieldResultMessage));

            result.Code = result.Success
                    ? result.Text(ServerMessageFormat.FieldResultCode)
                    : First(result.Text(ServerMessageFormat.FieldErrorCode),
                            result.Text(ServerMessageFormat.FieldResultCode));

            result.data = First(result.Text(ServerMessageFormat.FieldResultMessage),
                    result.Text(ServerMessageFormat.FieldSendMessage));

            return result;
        }

        private static string First(string primary, string fallback)
        {
            return string.IsNullOrEmpty(primary) ? (fallback ?? string.Empty) : primary;
        }

        internal void SetTable(DataTable queryTable)
        {
            this.table = queryTable;
        }

        internal void SetValues(string[] queryValues)
        {
            this.values = queryValues;
        }

        internal void SetTableParser(Func<string, DataTable> parser)
        {
            this.tableParser = parser;
        }

        internal void MarkQueryFault()
        {
            this.queryFault = true;
        }

        /// <summary>보낸 전문 글자 — 읽기 실패 예외의 <c>Data["Request"]</c>에 실어 어느 전문이 실패했는지 알린다.</summary>
        internal void SetRequestText(string text)
        {
            this.requestText = text ?? string.Empty;
        }

        public bool Success { get; internal set; }

        public bool CommunicationFailure { get; internal set; }

        /// <summary>교환을 끊은 예외 — 통신 실패일 때만 있고, 그 밖에는 null 이다.</summary>
        public Exception Cause { get; private set; }

        /// <summary>보낸 전문 글자 — 실패 창의 상세에 실린다.</summary>
        public string RequestText
        {
            get { return this.requestText; }
        }

        public string Message { get; internal set; }

        public string Code { get; internal set; }

        public int Count { get; internal set; }

        public IDictionary<string, string> Fields
        {
            get
            {
                Dictionary<string, string> map = new Dictionary<string, string>();

                map[ServerMessageFormat.FieldReturnCode] = this.HasField(
                        ServerMessageFormat.FieldReturnCode)
                        ? this.Text(ServerMessageFormat.FieldReturnCode)
                        : (this.Success ? ServerMessageFormat.SuccessReturnCode : "1");
                map[ServerMessageFormat.FieldResultCode] =
                        this.Text(ServerMessageFormat.FieldResultCode);
                map[ServerMessageFormat.FieldResultMessage] = this.HasField(
                        ServerMessageFormat.FieldResultMessage)
                        ? this.Text(ServerMessageFormat.FieldResultMessage)
                        : (this.Success ? (this.data ?? string.Empty) : string.Empty);
                map[ServerMessageFormat.FieldErrorCode] = this.HasField(
                        ServerMessageFormat.FieldErrorCode)
                        ? this.Text(ServerMessageFormat.FieldErrorCode)
                        : (this.Success ? string.Empty : this.Code);
                map[ServerMessageFormat.FieldErrorMessage] = this.HasField(
                        ServerMessageFormat.FieldErrorMessage)
                        ? this.Text(ServerMessageFormat.FieldErrorMessage)
                        : (this.Success ? string.Empty : this.Message);
                map[ServerMessageFormat.FieldSendMessage] = this.HasField(
                        ServerMessageFormat.FieldSendMessage)
                        ? this.Text(ServerMessageFormat.FieldSendMessage)
                        : (this.Success ? this.Message : string.Empty);

                return map;
            }
        }

        public string Data
        {
            get { return this.data ?? string.Empty; }
        }

        public DataTable Table
        {
            get
            {
                this.EnsureReadable();

                if (this.table == null)
                {
                    if (this.values != null)
                    {
                        this.table = SingleColumnTable(this.values);
                    }
                    else if (this.tableParser != null)
                    {
                        this.table = this.tableParser(this.data);
                    }
                    else
                    {
                        this.table = new DataTable();
                    }
                }

                return this.table;
            }
        }

        public string[] Values
        {
            get
            {
                this.EnsureReadable();

                if (this.values == null)
                {
                    this.values = ServerMessageFormat.ColumnValues(this.Table);
                }

                return this.values;
            }
        }

        public string[] ValuesOf(string columnName)
        {
            this.EnsureReadable();

            return ServerMessageFormat.ColumnValues(this.Table, columnName);
        }

        public string Text(string fieldName)
        {
            object value;

            if (fieldName == null || !this.fields.TryGetValue(fieldName, out value) || value == null)
            {
                return string.Empty;
            }

            return Convert.ToString(value);
        }

        // 읽을 수 없는 응답은 ServerReplyException 으로 — 거절(서버 코드·문구)과 통신 실패(원인 예외)를 갈라 던진다.
        // 반환 코드는 본문에 섞지 않고 예외의 Code 로 넘긴다(폼 베이스가 상세에 적는다).
        private void EnsureReadable()
        {
            if (this.CommunicationFailure)
            {
                throw new ServerReplyException(
                        this.Message.Length > 0
                                ? this.Message
                                : "Server request failed before a reply arrived.",
                        string.Empty,
                        this.requestText,
                        false,
                        this.Cause);
            }

            if (this.queryFault)
            {
                throw new ServerReplyException(
                        this.Message.Length > 0
                                ? this.Message
                                : "Server returned an error reply.",
                        this.Code,
                        this.requestText,
                        true,
                        null);
            }
        }

        private bool HasField(string fieldName)
        {
            return fieldName != null && this.fields.ContainsKey(fieldName);
        }

        private static DataTable SingleColumnTable(string[] source)
        {
            DataTable built = new DataTable();
            built.Locale = System.Globalization.CultureInfo.InvariantCulture;
            built.Columns.Add("VALUE", typeof(string));

            for (int i = 0; i < source.Length; i = i + 1)
            {
                built.Rows.Add(source[i]);
            }

            return built;
        }
    }
}
