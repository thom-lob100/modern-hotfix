using System;

namespace Modern.Lab.Hosting.Messaging
{
    /// <summary>
    /// 전문에 실리는 이름/값 한 쌍. <see cref="RequestParameters.Take(string)"/>가 돌려주고,
    /// <c>ModernFormBase.RequestFields</c>의 인자 목록에 섞어 넣으면 <c>이름=값</c> 하나로 적힌다
    /// (<see cref="ServerMessageFormat.ExpandParameters"/>).
    /// </summary>
    public sealed class RequestParameter
    {
        public RequestParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Parameter name must be a non-empty string.", "name");
            }

            this.Name = name;
            this.Value = value ?? string.Empty;
        }

        /// <summary>전문에 적히는 키 — 글자 그대로 나간다.</summary>
        public string Name { get; private set; }

        /// <summary>값. 글자로 바꾸는 규칙은 <see cref="ServerMessageFormat.FormatValue"/>.</summary>
        public object Value { get; private set; }
    }
}
