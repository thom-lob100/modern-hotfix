using System;

namespace Modern.Lab.Hosting.Messaging
{
    /// <summary>
    /// 응답을 읽을 수 없을 때 <see cref="DataActionResult"/> 가 던지는 예외. 두 갈래다 —
    /// 서버가 코드와 문구로 거절했거나(<see cref="Rejected"/> = true, <see cref="Exception.Message"/> 가 서버 문구),
    /// 응답이 오지 않았다(false, 끊은 예외가 <see cref="Exception.InnerException"/>).
    /// 폼 베이스는 거절 문구는 본문에 그대로 보이고, 통신 실패는 고정 문구로 보이며 원문은 상세로 내린다.
    /// <c>Data["Request"]</c> 에 보낸 전문 글자, <c>Data["ServerReply"]</c> 에 갈래("rejected" / "lost"),
    /// <c>Data["Code"]</c> 에 반환 코드가 실린다 — 폼 베이스(<c>ModernFormBase.cs</c>)는 갤러리 exe 가 이 파일 없이도
    /// 링크해 쓰므로 타입이 아니라 이 키로 읽는다.
    /// </summary>
    public sealed class ServerReplyException : InvalidOperationException
    {
        /// <summary>예외를 만든다.</summary>
        /// <param name="message">본문 — 거절이면 서버 문구, 통신 실패면 끊긴 사정.</param>
        /// <param name="code">서버 반환 코드(없으면 빈 값).</param>
        /// <param name="requestText">보낸 전문 글자.</param>
        /// <param name="rejected">서버가 코드로 거절했으면 true, 응답이 없었으면 false.</param>
        /// <param name="cause">교환을 끊은 예외(통신 실패일 때).</param>
        public ServerReplyException(
                string message, string code, string requestText, bool rejected, Exception cause)
            : base(message, cause)
        {
            this.Code = code ?? string.Empty;
            this.RequestText = requestText ?? string.Empty;
            this.Rejected = rejected;
            this.Data["Request"] = this.RequestText;
            this.Data["ServerReply"] = rejected ? "rejected" : "lost";

            if (this.Code.Length > 0)
            {
                this.Data["Code"] = this.Code;
            }
        }

        /// <summary>서버 반환 코드 — 거절일 때만 값이 있다.</summary>
        public string Code { get; private set; }

        /// <summary>보낸 전문 글자.</summary>
        public string RequestText { get; private set; }

        /// <summary>서버가 코드로 거절했으면 true, 응답이 오지 않았으면 false.</summary>
        public bool Rejected { get; private set; }
    }
}
