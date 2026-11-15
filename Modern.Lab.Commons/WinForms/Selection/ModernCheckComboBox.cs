using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Selection
{
    /// <summary>
    /// 체크 항목을 가진 다중 선택 드롭다운 (WinForms에 직접 대응하는 컨트롤 없음;
    /// WPF ModernCheckComboBoxControl을 ElementHost로 호스팅).
    ///
    /// ComboBox 데이터 계약과 동일한 이름 체계를 따른다: DataSource
    /// (DataTable/DataView/IList/IEnumerable), DisplayMember(화면 표시 명칭),
    /// ValueMember(서버 전송 코드). CheckedValues는 SelectedValue를 대체하며
    /// DataSource보다 먼저 설정해도 된다(보류 후 데이터 도착 시 적용 — 계약 룰 3).
    /// 필드에는 체크된 항목이 ", "로 연결되어 표시되고, 아무것도 체크되지 않으면
    /// 플레이스홀더가 보인다(보통 "전체" 의미).
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("CheckedChanged")]
    public class ModernCheckComboBox : WpfElementHostBase<Modern.Lab.Controls.Wpf.Selection.ModernCheckComboBoxControl>
    {
        private object dataSource;
        private object[] pendingCheckedValues;
        private bool hasPendingCheckedValues;
        private bool suppressCheckedChanged;
        private bool isRaisingDropDown;

        // 디자인 타임 WPF 생성 실패(Wpf == null) 시에도 속성창이 동작하도록
        // 하는 폴백 저장소.
        private string fallbackDisplayMember;
        private string fallbackValueMember;
        private string fallbackPlaceholder;
        private Modern.Lab.Controls.Wpf.Selection.CheckItemStyle fallbackItemStyle;
        private bool fallbackRequired;

        /// <summary>어느 항목이든 체크 상태가 바뀔 때 발생.</summary>
        public event EventHandler CheckedChanged;

        /// <summary>
        /// 드롭다운 목록이 열리기 직전에 발생한다(ModernComboBox.DropDown과 같은 이름).
        /// 이 처리 안에서 DataSource를 다시 대입하면 체크가 유지된다 — 새 목록에 남아 있는
        /// 값만 체크되고, 체크 결과가 달라졌을 때만 CheckedChanged가 발생한다.
        /// </summary>
        public event EventHandler DropDown;

        /// <summary>드롭다운 목록이 닫힌 직후에 발생한다(ModernComboBox.DropDownClosed와 같은 이름).</summary>
        public event EventHandler DropDownClosed;

        /// <summary>합리적인 기본 크기로 컨트롤을 만든다.</summary>
        public ModernCheckComboBox()
        {
            this.Size = new Size(200, 32);
            this.fallbackDisplayMember = string.Empty;
            this.fallbackValueMember = string.Empty;
            this.fallbackPlaceholder = string.Empty;
            this.fallbackItemStyle = Modern.Lab.Controls.Wpf.Selection.CheckItemStyle.CheckBox;
            this.fallbackRequired = false;

            if (this.Wpf != null)
            {
                this.Wpf.CheckedChanged += this.OnWpfCheckedChanged;
                this.Wpf.DropDownOpened += this.OnWpfDropDownOpened;
                this.Wpf.DropDownClosedEvent += this.OnWpfDropDownClosed;
            }
        }

        /// <summary>
        /// 데이터 원본: DataTable, DataView, IList 또는 임의의 IEnumerable.
        /// 할당하면 모든 체크가 초기화되고, 보류 중인 CheckedValues가 있으면 적용한 뒤
        /// CheckedChanged를 정확히 한 번 발생시킨다. null이면 목록을 비운다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;

                if (this.Wpf == null)
                {
                    return;
                }

                // 드롭다운을 여는 중의 재대입은 목록 갱신이다 — 체크를 지우지 않고 이어 간다.
                bool keepChecked = this.isRaisingDropDown && !this.hasPendingCheckedValues;
                object[] checkedBefore = keepChecked ? this.CheckedValues : null;

                this.suppressCheckedChanged = true;

                try
                {
                    this.Wpf.ItemsSource = DataSourceConverter.ToItemsSource(value);

                    if (this.hasPendingCheckedValues)
                    {
                        this.Wpf.ApplyCheckedValues(this.pendingCheckedValues);
                        this.pendingCheckedValues = null;
                        this.hasPendingCheckedValues = false;
                    }
                    else if (keepChecked)
                    {
                        this.Wpf.ApplyCheckedValues(checkedBefore);
                    }
                }
                finally
                {
                    this.suppressCheckedChanged = false;
                }

                if (keepChecked && SameValues(checkedBefore, this.CheckedValues))
                {
                    return;
                }

                this.RaiseCheckedChanged();
            }
        }

        /// <summary>필수 입력 필드 표시 — 필드 왼쪽에 빨간 세로 바를 그린다.</summary>
        [Category("Modern")]
        [Description("Required marker (red vertical bar on the left of the field)")]
        [DefaultValue(false)]
        public bool Required
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Required;
                }

                return this.fallbackRequired;
            }
            set
            {
                this.fallbackRequired = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Required = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>화면 표시 명칭으로 사용할 컬럼/속성 이름 (WinForms 호환 이름).</summary>
        [Category("Modern")]
        [Description("Column/property name used as the display text")]
        [DefaultValue("")]
        public string DisplayMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.DisplayMemberPath;
                }

                return this.fallbackDisplayMember;
            }
            set
            {
                this.fallbackDisplayMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.DisplayMemberPath = value;
                }
            }
        }

        /// <summary>서버 전송용 코드 값으로 사용할 컬럼/속성 이름 (WinForms 호환 이름).</summary>
        [Category("Modern")]
        [Description("Column/property name used as CheckedValues")]
        [DefaultValue("")]
        public string ValueMember
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ValueMemberPath;
                }

                return this.fallbackValueMember;
            }
            set
            {
                this.fallbackValueMember = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ValueMemberPath = value;
                }
            }
        }

        /// <summary>체크된 항목이 없을 때 표시할 힌트 (ModernTextBox와 동일한 속성명).</summary>
        [Category("Modern")]
        [Description("Hint text shown when no item is checked")]
        [Localizable(true)]
        [DefaultValue("")]
        public string PlaceholderText
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.Placeholder;
                }

                return this.fallbackPlaceholder;
            }
            set
            {
                this.fallbackPlaceholder = value;

                if (this.Wpf != null)
                {
                    this.Wpf.Placeholder = value;
                }

                this.InvalidateDesignTimePreview();
            }
        }

        /// <summary>항목 표시 스타일: 모던 체크박스(기본) 또는 온오프 스위치.</summary>
        [Category("Modern")]
        [Description("Drop-down item style — CheckBox (default) or Switch")]
        [DefaultValue(Modern.Lab.Controls.Wpf.Selection.CheckItemStyle.CheckBox)]
        public Modern.Lab.Controls.Wpf.Selection.CheckItemStyle ItemStyle
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.ItemStyle;
                }

                return this.fallbackItemStyle;
            }
            set
            {
                this.fallbackItemStyle = value;

                if (this.Wpf != null)
                {
                    this.Wpf.ItemStyle = value;
                }
            }
        }

        /// <summary>
        /// 체크된 항목들의 코드 값 배열(ValueMember 기준). DataSource보다 먼저
        /// 설정해도 된다(보류 후 데이터 도착 시 적용). null/빈 배열 = 전체 해제.
        /// 서버 호출 시 이 값을 그대로 보낸다.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object[] CheckedValues
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.GetCheckedValues().ToArray();
                }

                return new object[0];
            }
            set
            {
                if (this.Wpf == null)
                {
                    return;
                }

                if (this.HasBoundItems())
                {
                    this.Wpf.ApplyCheckedValues(value);
                }
                else
                {
                    this.pendingCheckedValues = value;
                    this.hasPendingCheckedValues = true;
                }
            }
        }

        /// <summary>체크된 항목들의 원본 행 배열 (DataTable 소스일 때는 DataRowView).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object[] CheckedItems
        {
            get
            {
                if (this.Wpf != null)
                {
                    return this.Wpf.GetCheckedItems().ToArray();
                }

                return new object[0];
            }
        }

        /// <summary>체크된 항목들의 표시 텍스트를 ", "로 연결한 값 (읽기 전용).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text
        {
            get
            {
                if (this.Wpf != null)
                {
                    List<string> texts = new List<string>();

                    foreach (object item in this.Wpf.GetCheckedItems())
                    {
                        texts.Add(Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(item, this.DisplayMember));
                    }

                    return string.Join(", ", texts.ToArray());
                }

                return string.Empty;
            }
            set
            {
                // 텍스트로 체크하는 기능은 지원하지 않음 — CheckedValues를 사용.
            }
        }

        /// <summary>모든 항목을 체크한다. CheckedChanged는 한 번만 발생.</summary>
        public void CheckAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.CheckAll();
            }
        }

        /// <summary>모든 항목의 체크를 해제한다. CheckedChanged는 한 번만 발생.</summary>
        public void UncheckAll()
        {
            if (this.Wpf != null)
            {
                this.Wpf.UncheckAll();
            }
        }

        /// <summary>
        /// 드롭다운을 멀티컬럼(코드+명칭 등) 행으로 구성한다 — "체크 그리드 콤보".
        /// 그리드와 동일한 ModernDataGridColumn 정의를 재사용하며, 팝업 상단에
        /// 헤더 행이 표시된다. 필드의 표시 텍스트는 계속 DisplayMember를 따른다.
        /// DataSource 할당 전에 호출한다.
        /// </summary>
        public void ConfigureDropDownColumns(params Modern.Lab.Controls.Wpf.Data.ModernDataGridColumn[] columns)
        {
            if (this.Wpf != null)
            {
                // 목록 헤더는 번역 사전을 지난 사본으로 그린다(번역이 꺼져 있으면 받은 배열 그대로) — 그리드와 같은 규칙.
                this.Wpf.ApplyDropDownColumns(Modern.Lab.Controls.Wpf.Data.ModernDataGridColumn.ForDisplay(
                        columns, Modern.Lab.Captions.ModernLanguageApplier.ScreenOf(this)));
            }
        }

        private bool HasBoundItems()
        {
            return this.Wpf != null && this.Wpf.ItemsSource != null;
        }

        private void OnWpfCheckedChanged(object sender, EventArgs e)
        {
            if (!this.suppressCheckedChanged)
            {
                this.RaiseCheckedChanged();
            }
        }

        private void OnWpfDropDownOpened(object sender, EventArgs e)
        {
            if (this.DropDown == null)
            {
                return;
            }

            this.isRaisingDropDown = true;

            try
            {
                this.DropDown(this, EventArgs.Empty);
            }
            finally
            {
                this.isRaisingDropDown = false;
            }
        }

        private void OnWpfDropDownClosed(object sender, EventArgs e)
        {
            if (this.DropDownClosed != null)
            {
                this.DropDownClosed(this, EventArgs.Empty);
            }
        }

        private static bool SameValues(object[] left, object[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int index = 0; index < left.Length; index++)
            {
                if (!object.Equals(left[index], right[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private void RaiseCheckedChanged()
        {
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this, EventArgs.Empty);
            }
        }
    }
}
