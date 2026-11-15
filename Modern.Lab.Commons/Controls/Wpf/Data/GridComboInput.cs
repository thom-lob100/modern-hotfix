using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Modern.Lab.Controls.Wpf.Data
{
    /// <summary>
    /// 그리드 셀의 입력 가능 콤보(<see cref="ModernDataGridColumn.ComboEditable"/>) 동작 — 글자를 쳐서 선택지를 좁혀 고른다.
    ///
    /// - 좁히기: 친 글자로 **시작하는** 선택지만 남는다(대소문자 무시). 목록은 셀마다 따로 갖는다 —
    ///   같은 선택지 배열을 쓰는 다른 행의 콤보는 좁혀지지 않는다.
    /// - 확정은 <see cref="Commit"/> 한 곳이다. Tab · Enter · 포커스 이동 · 목록에서 고르기 · 그리드의
    ///   <c>CommitPendingEdit</c>(래퍼 <c>EndEdit</c>)가 모두 이 메서드를 부른다. 친 글자가 선택지와 같으면 그 값,
    ///   아니면 그 글자로 시작하는 선택지가 하나뿐일 때 그 값이 들어가고, 그 밖에는 칸을 비운다(선택지 밖의 값은 넣지 않는다).
    /// - 치는 동안에는 원본 행 값을 건드리지 않는다. 값은 <see cref="ValueProperty"/>(행 컬럼과 양방향)로만 드나들고
    ///   확정할 때만 쓴다. Esc · <c>CancelPendingEdit</c> 는 친 글자를 버리고 원래 값을 다시 보인다.
    ///
    /// WPF 의 편집 가능 ComboBox 는 선택이 풀리면 입력 글자를 지우고, 드롭다운을 열면 글자 전체를 선택한다.
    /// 그래서 목록을 다시 채우거나 드롭다운을 여닫는 동안에는 <c>Suppress</c> 로 우리 쪽 변경을 표시하고
    /// 친 글자와 캐럿을 되돌려 놓는다(화면 콤보 <c>ModernComboBoxControl</c> 과 같은 방식).
    ///
    /// 키는 콤보가 아니라 **그리드의 터널 단계**에서 받는다(<see cref="HandlePreviewKey"/>). 편집 가능 ComboBox 는
    /// 입력란의 키를 자기 PreviewKeyDown 에서 먼저 처리하기 때문에(닫힌 채 ↑↓ = 값 바꿈, 열린 채 Enter = 강조 항목 선택)
    /// 콤보에 건 핸들러로는 그보다 앞설 수 없다.
    /// </summary>
    internal static class GridComboInput
    {
        /// <summary>선택지 전체(글자 목록). 지정하면 그 콤보가 입력 가능 콤보로 배선된다.</summary>
        public static readonly DependencyProperty ChoicesProperty =
            DependencyProperty.RegisterAttached(
                "Choices",
                typeof(IEnumerable),
                typeof(GridComboInput),
                new PropertyMetadata(null, OnChoicesChanged));

        /// <summary>확정된 값 — 행 컬럼과 양방향으로 묶는다. 치는 동안에는 바뀌지 않는다.</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(object),
                typeof(GridComboInput),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        private static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached(
                "State",
                typeof(State),
                typeof(GridComboInput),
                new PropertyMetadata(null));

        // 확정하지 않은 입력이 남아 있는 콤보 — 그리드의 CommitPendingEdit/CancelPendingEdit 가 찾아 쓴다.
        // 입력은 한 번에 한 칸에서만 일어나므로 하나면 충분하다.
        private static WeakReference pending;

        public static IEnumerable GetChoices(DependencyObject element)
        {
            return (IEnumerable)element.GetValue(ChoicesProperty);
        }

        public static void SetChoices(DependencyObject element, IEnumerable value)
        {
            element.SetValue(ChoicesProperty, value);
        }

        public static object GetValue(DependencyObject element)
        {
            return element.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject element, object value)
        {
            element.SetValue(ValueProperty, value);
        }

        /// <summary>셀 안의 입력 가능 콤보를 찾는다 — 없으면 null.</summary>
        internal static ComboBox FindIn(DependencyObject root)
        {
            if (root == null)
            {
                return null;
            }

            ComboBox combo = root as ComboBox;

            if (combo != null && StateOf(combo) != null)
            {
                return combo;
            }

            int count = VisualTreeHelper.GetChildrenCount(root);

            for (int index = 0; index < count; index++)
            {
                ComboBox found = FindIn(VisualTreeHelper.GetChild(root, index));

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>요소가 입력 가능 콤보의 입력란(또는 그 안쪽)인지.</summary>
        internal static bool IsEditorElement(object element)
        {
            return EditorState(element) != null;
        }

        // 입력란(또는 그 안쪽 요소)에서 그 콤보의 상태를 찾는다 — 입력 가능 콤보의 입력란이 아니면 null.
        private static State EditorState(object element)
        {
            DependencyObject current = element as DependencyObject;

            while (current != null)
            {
                TextBox editor = current as TextBox;

                if (editor != null)
                {
                    return StateOf(editor.TemplatedParent);
                }

                current = current is Visual ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// 입력 가능 콤보의 입력란에서 온 키를 처리한다. 그리드가 터널 단계에서 부른다 — 입력란의 키면 true 를 돌려주고
        /// (그리드는 그 키로 행 복사 같은 자기 처리를 하지 않는다), 이 클래스가 끝낸 키는 <c>e.Handled</c> 를 세운다.
        /// - Esc: 치던 글자를 버리고 원래 값으로(목록이 열려 있으면 닫는다).
        /// - Enter: 확정하고 그 칸에 머문다.
        /// - Tab: 확정만 하고 키는 넘긴다 — 그리드가 다음 칸으로 옮긴다.
        /// - ↑↓(목록이 닫혀 있을 때): 값을 바꾸지 않고 목록을 연다. 열려 있으면 WPF 가 목록 안에서 옮긴다.
        /// </summary>
        internal static bool HandlePreviewKey(KeyEventArgs e)
        {
            State state = EditorState(e.OriginalSource);

            if (state == null)
            {
                return false;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    if (state.Dirty || state.Combo.IsDropDownOpen)
                    {
                        Cancel(state);
                        e.Handled = true;
                    }

                    break;

                case Key.Enter:
                    if (state.Dirty)
                    {
                        Commit(state);
                        e.Handled = true;
                    }
                    else if (state.Combo.IsDropDownOpen)
                    {
                        state.Combo.IsDropDownOpen = false;
                        e.Handled = true;
                    }

                    break;

                case Key.Tab:
                    Commit(state);
                    break;

                case Key.Up:
                case Key.Down:
                    if (!state.Combo.IsDropDownOpen && (Keyboard.Modifiers & ModifierKeys.Alt) == 0)
                    {
                        state.Combo.IsDropDownOpen = state.View.Count > 0;
                        e.Handled = true;
                    }

                    break;
            }

            return true;
        }

        /// <summary>
        /// 셀에 포커스가 있을 때 친 첫 글자로 입력을 시작한다 — 입력란에 포커스를 주고 그 글자를 넣은 뒤 목록을 좁힌다.
        /// 콤보가 꺼져 있거나 입력란을 못 찾으면 false.
        /// </summary>
        internal static bool BeginTyping(ComboBox combo, string text)
        {
            State state = StateOf(combo);

            if (state == null || !combo.IsEnabled || string.IsNullOrEmpty(text))
            {
                return false;
            }

            TextBox editor = EditorOf(state);

            if (editor == null)
            {
                return false;
            }

            editor.Focus();
            state.Suppress = true;

            try
            {
                editor.Text = text;
                editor.CaretIndex = text.Length;
            }
            finally
            {
                state.Suppress = false;
            }

            Narrow(state);
            return true;
        }

        /// <summary>입력란에 포커스를 주고 글자 전체를 선택한다(F2).</summary>
        internal static bool FocusEditor(ComboBox combo)
        {
            State state = StateOf(combo);

            if (state == null || !combo.IsEnabled)
            {
                return false;
            }

            TextBox editor = EditorOf(state);

            if (editor == null)
            {
                return false;
            }

            editor.Focus();
            editor.SelectAll();
            return true;
        }

        /// <summary>root 안에 확정하지 않은 콤보 입력이 있으면 확정한다.</summary>
        internal static void CommitPending(DependencyObject root)
        {
            State state = PendingWithin(root);

            if (state != null)
            {
                Commit(state);
            }
        }

        /// <summary>root 안에 확정하지 않은 콤보 입력이 있으면 버리고 원래 값을 다시 보인다.</summary>
        internal static void CancelPending(DependencyObject root)
        {
            State state = PendingWithin(root);

            if (state != null)
            {
                Cancel(state);
            }
        }

        private static State PendingWithin(DependencyObject root)
        {
            State state = pending == null ? null : pending.Target as State;

            if (state == null || !state.Dirty || root == null)
            {
                return null;
            }

            Visual ancestor = root as Visual;
            return ancestor != null && state.Combo.IsDescendantOf(ancestor) ? state : null;
        }

        private static State StateOf(DependencyObject element)
        {
            return element == null ? null : (State)element.GetValue(StateProperty);
        }

        private static void OnChoicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComboBox combo = d as ComboBox;

            if (combo == null)
            {
                return;
            }

            State state = StateOf(combo);

            if (state == null)
            {
                state = new State(combo);
                combo.SetValue(StateProperty, state);
                combo.ItemsSource = state.View;
                combo.Loaded += OnComboLoaded;
                combo.DropDownClosed += OnDropDownClosed;
                combo.IsKeyboardFocusWithinChanged += OnFocusWithinChanged;
                combo.DataContextChanged += OnDataContextChanged;
            }

            state.All.Clear();
            IEnumerable choices = e.NewValue as IEnumerable;

            if (choices != null && !(choices is string))
            {
                foreach (object choice in choices)
                {
                    string text = TextOf(choice);

                    if (text.Length > 0)
                    {
                        state.All.Add(text);
                    }
                }
            }

            // 표시 목록은 바뀐 선택지로 다시 채운다. 치던 중이면 친 글자로 새 목록을 좁힌다 — 예전 목록의 항목이
            // 남으면 골라도 새 선택지에 없어 칸이 비워진다.
            if (state.Dirty)
            {
                Narrow(state);
            }
            else
            {
                ShowValue(state);
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            State state = StateOf(d);

            if (state != null && !state.Dirty && !state.Suppress)
            {
                ShowValue(state);
            }
        }

        // 셀 컨테이너가 재사용돼 다른 행을 받으면 치던 글자는 그 행의 것이 아니다 — 버린다.
        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            State state = StateOf(sender as DependencyObject);

            if (state == null)
            {
                return;
            }

            if (state.Dirty)
            {
                Cancel(state);
            }
            else
            {
                ShowValue(state);
            }
        }

        private static void OnComboLoaded(object sender, RoutedEventArgs e)
        {
            State state = StateOf(sender as DependencyObject);

            if (state == null)
            {
                return;
            }

            EditorOf(state);

            if (!state.Dirty)
            {
                ShowValue(state);
            }
        }

        // 입력란은 템플릿이 적용된 뒤에야 잡힌다 — 처음 쓸 때 찾아 배선하고, 템플릿이 다시 적용되면 다시 잡는다.
        private static TextBox EditorOf(State state)
        {
            state.Combo.ApplyTemplate();
            TextBox editor = state.Combo.Template == null
                    ? null
                    : state.Combo.Template.FindName("PART_EditableTextBox", state.Combo) as TextBox;

            if (!object.ReferenceEquals(editor, state.Editor))
            {
                if (state.Editor != null)
                {
                    state.Editor.TextChanged -= OnEditorTextChanged;
                    state.Editor.PreviewMouseLeftButtonDown -= OnEditorPreviewMouseDown;
                }

                state.Editor = editor;

                if (editor != null)
                {
                    editor.TextChanged += OnEditorTextChanged;
                    editor.PreviewMouseLeftButtonDown += OnEditorPreviewMouseDown;
                }
            }

            return editor;
        }

        // 포커스가 없던 입력란을 누르면 글자 전체를 선택한다 — 바로 치면 기존 값을 갈아 쓴다.
        private static void OnEditorPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox editor = sender as TextBox;

            if (editor == null || editor.IsKeyboardFocusWithin)
            {
                return;
            }

            editor.Focus();
            editor.SelectAll();
            e.Handled = true;
        }

        private static void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox editor = sender as TextBox;
            State state = editor == null ? null : StateOf(editor.TemplatedParent);

            if (state == null || state.Suppress || !state.Combo.IsKeyboardFocusWithin)
            {
                return;
            }

            object selected = state.Combo.SelectedItem;

            // 목록에서 ↑↓ · 클릭으로 옮기면 WPF 가 그 항목 글자를 입력란에 쓴다 — 친 것이 아니므로 좁히지 않는다.
            if (selected != null && string.Equals(editor.Text, TextOf(selected), StringComparison.Ordinal))
            {
                MarkDirty(state);
                return;
            }

            Narrow(state);
        }

        // 친 글자로 목록을 좁히고 후보가 있으면 드롭다운을 연다. 친 글자와 캐럿은 그대로 둔다.
        private static void Narrow(State state)
        {
            TextBox editor = state.Editor;

            if (editor == null)
            {
                return;
            }

            MarkDirty(state);
            string typed = editor.Text ?? string.Empty;
            int caret = editor.CaretIndex;
            state.Suppress = true;

            try
            {
                if (state.Combo.SelectedItem != null)
                {
                    state.Combo.SelectedItem = null;
                }

                Fill(state, typed.Trim());

                if (!string.Equals(editor.Text, typed, StringComparison.Ordinal))
                {
                    editor.Text = typed;
                }

                state.Combo.IsDropDownOpen = typed.Trim().Length > 0 && state.View.Count > 0;

                if (!string.Equals(editor.Text, typed, StringComparison.Ordinal))
                {
                    editor.Text = typed;
                }

                editor.Select(Math.Min(caret, typed.Length), 0);
            }
            finally
            {
                state.Suppress = false;
            }
        }

        // 목록에서 항목을 눌러 닫힌 경우 — 그 항목으로 확정한다(치지 않고 펼쳐서 고른 경우도 여기로 온다).
        // 고르지 않고 닫은 것(셰브런 다시 누름)은 치던 글자를 그대로 둔다. WPF 가 닫으면서 강조 항목을 선택으로
        // 옮기는 도중일 수 있어, 목록을 다시 채우는 확정은 그 처리가 끝난 뒤로 미룬다.
        private static void OnDropDownClosed(object sender, EventArgs e)
        {
            State state = StateOf(sender as DependencyObject);

            if (state == null || state.Suppress)
            {
                return;
            }

            state.Combo.Dispatcher.BeginInvoke(
                new Action(delegate { CommitPicked(state); }),
                DispatcherPriority.Input);
        }

        private static void CommitPicked(State state)
        {
            if (state.Suppress || state.Combo.IsDropDownOpen)
            {
                return;
            }

            object selected = state.Combo.SelectedItem;
            string shown = state.Editor != null ? state.Editor.Text : state.Combo.Text;

            if (selected != null && string.Equals(shown, TextOf(selected), StringComparison.Ordinal))
            {
                MarkDirty(state);
                Commit(state);
            }
        }

        private static void OnFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            State state = StateOf(sender as DependencyObject);

            if (state != null && !(bool)e.NewValue)
            {
                Commit(state);
            }
        }

        private static void MarkDirty(State state)
        {
            state.Dirty = true;
            pending = new WeakReference(state);
        }

        // 확정의 단일 경로 — 친 글자를 선택지로 풀어 값에 쓰고 입력 상태를 끝낸다.
        private static void Commit(State state)
        {
            if (!state.Dirty)
            {
                return;
            }

            string typed = state.Editor != null ? state.Editor.Text : state.Combo.Text;
            string resolved = Resolve(state.All, (typed ?? string.Empty).Trim());
            Finish(state);

            if (!string.Equals(TextOf(resolved), TextOf(GetValue(state.Combo)), StringComparison.Ordinal))
            {
                state.Combo.SetCurrentValue(ValueProperty, resolved);
            }

            ShowValue(state);
        }

        private static void Cancel(State state)
        {
            Finish(state);
            ShowValue(state);
        }

        // 입력 상태를 끝낸다 — 드롭다운을 닫고 목록을 전체로 되돌린다.
        private static void Finish(State state)
        {
            state.Suppress = true;

            try
            {
                state.Dirty = false;

                if (pending != null && object.ReferenceEquals(pending.Target, state))
                {
                    pending = null;
                }

                if (state.Combo.IsDropDownOpen)
                {
                    state.Combo.IsDropDownOpen = false;
                }

                Fill(state, string.Empty);
            }
            finally
            {
                state.Suppress = false;
            }
        }

        // 확정된 값을 입력란과 목록 선택에 다시 보인다. 선택지에 없는 값(서버가 준 값)도 글자로는 보인다.
        private static void ShowValue(State state)
        {
            string value = TextOf(GetValue(state.Combo));
            state.Suppress = true;

            try
            {
                if (!ShowsAll(state))
                {
                    Fill(state, string.Empty);
                }

                string match = null;

                foreach (string choice in state.All)
                {
                    if (string.Equals(choice, value, StringComparison.OrdinalIgnoreCase))
                    {
                        match = choice;
                        break;
                    }
                }

                if (!object.Equals(state.Combo.SelectedItem, match))
                {
                    state.Combo.SelectedItem = match;
                }

                if (!string.Equals(state.Combo.Text, value, StringComparison.Ordinal))
                {
                    state.Combo.SetCurrentValue(ComboBox.TextProperty, value);
                }

                if (state.Editor != null && !string.Equals(state.Editor.Text, value, StringComparison.Ordinal))
                {
                    state.Editor.Text = value;
                }
            }
            finally
            {
                state.Suppress = false;
            }
        }

        // 표시 목록이 선택지 전체와 같은지 — 개수만 보면 같은 개수의 다른 목록으로 바뀐 것을 놓친다.
        private static bool ShowsAll(State state)
        {
            if (state.View.Count != state.All.Count)
            {
                return false;
            }

            for (int index = 0; index < state.All.Count; index++)
            {
                if (!string.Equals(state.View[index], state.All[index], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static void Fill(State state, string prefix)
        {
            state.View.Clear();

            foreach (string choice in state.All)
            {
                if (prefix.Length == 0 || choice.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    state.View.Add(choice);
                }
            }
        }

        // 친 글자 → 선택지. 같은 선택지가 있으면 그것, 없으면 그 글자로 시작하는 선택지가 하나뿐일 때 그것, 그 밖에는 null(비움).
        private static string Resolve(List<string> choices, string typed)
        {
            if (typed.Length == 0)
            {
                return null;
            }

            string only = null;
            int count = 0;

            foreach (string choice in choices)
            {
                if (string.Equals(choice, typed, StringComparison.OrdinalIgnoreCase))
                {
                    return choice;
                }

                if (choice.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                {
                    only = choice;
                    count++;
                }
            }

            return count == 1 ? only : null;
        }

        private static string TextOf(object value)
        {
            return value == null || value == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private sealed class State
        {
            internal State(ComboBox combo)
            {
                this.Combo = combo;
                this.All = new List<string>();
                this.View = new ObservableCollection<string>();
            }

            internal ComboBox Combo { get; private set; }

            internal TextBox Editor { get; set; }

            internal List<string> All { get; private set; }

            internal ObservableCollection<string> View { get; private set; }

            // 우리 코드가 글자 · 목록 · 선택을 바꾸는 중 — 그 변경을 사용자 입력으로 읽지 않는다.
            internal bool Suppress { get; set; }

            // 사용자가 치거나 목록에서 옮긴 뒤 아직 확정하지 않았다.
            internal bool Dirty { get; set; }
        }
    }
}
