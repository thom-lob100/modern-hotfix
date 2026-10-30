using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Modern.Lab.Controls.Wpf.Input;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Input
{
    /// <summary>
    /// 속성 편집기의 WPF 섬(<see cref="ModernPropertyGridControl"/>)을 담는 WinForms 그릇 — 상위 Common 의 기준정보
    /// 편집기(<c>Modern.Lab.Hosting.MasterData.ModernPropertyGrid</c>)가 이 클래스를 상속한다.
    ///
    /// WPF 쪽 호출은 모두 이 클래스의 일반 멤버로만 드러낸다. 그래서 상속하는 프로젝트는 PresentationCore ·
    /// PresentationFramework · WindowsBase 를 참조하지 않아도 컴파일된다(2026-10-30 — 편집기가 WPF 컨트롤을 직접
    /// 상속하자 회사 상위 Common 프로젝트에 WPF 참조가 들어가 기존 소스의 <c>OpenFileDialog</c> 가 WPF 의 같은 이름과
    /// 겹쳐 빌드가 깨졌다). 디자인 타임에는 WPF 섬이 없으므로 모든 멤버가 아무 일도 하지 않는다.
    /// </summary>
    [ToolboxItem(false)]
    public class ModernPropertyGridHost : WpfElementHostBase<ModernPropertyGridControl>
    {
        public ModernPropertyGridHost()
        {
            if (this.Wpf != null)
            {
                this.Wpf.ValueChanged += this.OnEditorValueChanged;
                this.Wpf.DropDownRefresh = this.RefreshEditorDropDown;
            }
        }

        /// <summary>WPF 섬이 있는지 — 디자인 타임에는 없다.</summary>
        protected bool HasEditor
        {
            get { return this.Wpf != null; }
        }

        /// <summary>편집 값이 바뀌면 불린다(어느 컬럼이든).</summary>
        protected virtual void OnEditorValueChanged(object sender, EventArgs e)
        {
        }

        /// <summary>콤보를 펼칠 때 불린다 — 새 목록을 돌려주고, 이미 실었으면 null 을 돌려준다.</summary>
        protected virtual IEnumerable RefreshEditorDropDown(string column)
        {
            return null;
        }

        protected void SetEditorLabelWidth(int value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.LabelWidth = value;
            }
        }

        protected void SetEditorMinColumnWidth(int value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.MinColumnWidth = value;
            }
        }

        protected void SetEditorHeight(int value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.EditorHeight = value;
            }
        }

        protected void SetEditorMultilineHeight(int value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.MultilineHeight = value;
            }
        }

        protected void SetEditorRowGap(int value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.RowGap = value;
            }
        }

        protected void SetEditorFields(IList<PropertyGridFieldSpec> specs)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetFields(specs);
            }
        }

        protected string GetEditorValue(string column)
        {
            return this.Wpf == null ? null : this.Wpf.GetValue(column);
        }

        protected void SetEditorValue(string column, string value)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetValue(column, value);
            }
        }

        protected void SetEditorReadOnly(string column, bool readOnly)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetReadOnly(column, readOnly);
            }
        }

        protected void SetEditorRequired(string column, bool required)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetRequired(column, required);
            }
        }

        protected void BindEditorComboItems(string column, IEnumerable items)
        {
            if (this.Wpf != null)
            {
                this.Wpf.BindComboItems(column, items);
            }
        }

        protected bool FocusEditorField(string column)
        {
            return this.Wpf != null && this.Wpf.FocusField(column);
        }
    }
}
