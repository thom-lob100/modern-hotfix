using System.Collections.Generic;
using System.ComponentModel;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// ModernTreeListControl의 노드 아이템 모델. 평면 자기참조 행 하나가 노드
    /// 하나가 되며, 그리드에 평탄화되어 바인딩된다 — 트리 컬럼 템플릿이
    /// Depth(들여쓰기)/HasChildren(셰브런 표시)/IsExpanded(셰브런 방향)/Text를,
    /// 값 컬럼이 "Row.컬럼명" 경로로 원본 행을 읽는다.
    /// </summary>
    public sealed class TreeListNode : INotifyPropertyChanged
    {
        private bool isExpanded;

        /// <summary>WPF 바인딩 알림 (IsExpanded 셰브런 갱신용).</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal TreeListNode(object id, string text, object row)
        {
            this.Id = id;
            this.Text = text;
            this.Row = row;
            this.Children = new List<TreeListNode>();
            this.isExpanded = true;
        }

        /// <summary>노드 키 (IdMember 값).</summary>
        public object Id { get; private set; }

        /// <summary>트리 컬럼 표시 텍스트 (DisplayMember 값).</summary>
        public string Text { get; private set; }

        /// <summary>원본 행 (DataRowView 등) — 값 컬럼이 "Row.컬럼명"으로 읽는다.</summary>
        public object Row { get; private set; }

        /// <summary>부모 노드 (루트는 null).</summary>
        public TreeListNode Parent { get; internal set; }

        /// <summary>자식 노드 목록 (데이터 순서 유지).</summary>
        public List<TreeListNode> Children { get; private set; }

        /// <summary>루트 0부터의 깊이 — 트리 컬럼 들여쓰기 계산용.</summary>
        public int Depth { get; internal set; }

        /// <summary>항상 false — 트리 리스트의 Y/N 체크 칸은 읽기 전용이라 체크박스의 IsEnabled 가 이 값에 묶인다(2026-10-26).</summary>
        public bool IsCheckEditable
        {
            get { return false; }
        }

        /// <summary>트리 컬럼 들여쓰기 폭(px) — 셀 템플릿이 바인딩한다.</summary>
        public double Indent
        {
            get { return this.Depth * 16d; }
        }

        /// <summary>자식이 있는지 — 셰브런 표시 여부.</summary>
        public bool HasChildren
        {
            get { return this.Children.Count > 0; }
        }

        /// <summary>펼침 상태 (기본 true). 셰브런 방향이 이 값을 따라간다.</summary>
        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            internal set
            {
                if (this.isExpanded == value)
                {
                    return;
                }

                this.isExpanded = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded"));
                }
            }
        }
    }
}
