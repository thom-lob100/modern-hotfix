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

        private void EnsureReadable()
        {
            if (this.CommunicationFailure)
            {
                throw this.ReadFailure(
                        this.Message.Length > 0
                                ? this.Message
                                : "Server request failed before a reply arrived.");
            }

            if (this.queryFault)
            {
                string text = this.Message.Length > 0
                        ? this.Message
                        : "Server returned an error reply.";

                if (this.Code.Length > 0)
                {
                    text = "[" + this.Code + "] " + text;
                }

                throw this.ReadFailure(text);
            }
        }

        private InvalidOperationException ReadFailure(string text)
        {
            InvalidOperationException failure = new InvalidOperationException(text);
            failure.Data["Request"] = this.requestText;
            return failure;
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
