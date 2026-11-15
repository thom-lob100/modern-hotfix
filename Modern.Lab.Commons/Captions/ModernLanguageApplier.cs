using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Modern.Lab.Captions
{
    /// <summary>
    /// 화면에 놓인 컨트롤의 <b>디자이너 글자</b>를 <see cref="ModernLanguage"/>로 번역해 다시 넣는다(2026-11-15) — 버튼·라벨·탭·카드 제목·
    /// 빈 표 문구·윗/아랫글씨. 번역이 꺼져 있으면 아무 일도 없다.
    ///
    /// 두 걸음으로 나뉜다. <see cref="Remember"/>가 폼이 만들어진 직후의 글자(디자이너가 적은 값)를 적어 두고,
    /// <see cref="ApplyRemembered"/>가 화면의 Load 처리기가 끝난 뒤에 번역해 넣는다 — 화면 코드가 Load 에서 버튼의 영어 글자로
    /// 아이콘·메뉴 묶음을 찾기 때문에(<c>MenuGlyphs.For(button.Text)</c>) 그 전에 글자를 바꾸면 아이콘이 사라진다.
    /// 그 사이에 코드가 바꾼 글자는 디자이너 글자가 아니므로 건드리지 않는다. 폼 베이스(<c>ModernFormBase</c>)가 두 걸음을 부르고,
    /// 폼 베이스를 쓰지 않는 폼은 Load 처리기 끝에서 <see cref="Apply"/> 한 번을 부른다.
    ///
    /// 번역하는 글자:
    /// <list type="bullet">
    /// <item>모든 컨트롤의 <c>Text</c> — 입력값·데이터를 <c>Text</c>로 갖는 컨트롤(텍스트 상자·콤보·바코드 등)은 뺀다.</item>
    /// <item>이 라이브러리의 래퍼가 <c>[Localizable(true)]</c>로 표시한 문자열 속성(<c>TopLabel</c>·<c>PlaceholderText</c>·<c>EmptyText</c> …).</item>
    /// <item>디자이너에 놓인 메뉴(<c>ToolStrip</c>·컨트롤의 <c>ContextMenuStrip</c>) 항목의 <c>Text</c>.</item>
    /// </list>
    /// <b>화면이 지정한 글자만</b> 번역한다 — 속성 값이 그 속성의 기본값 그대로(<c>"No data"</c>·<c>"{0:N0} rows"</c>)이면 건드리지 않는다.
    /// 코드가 나중에 넣는 글자(메시지·코드로 만든 메뉴 항목)는 이 적용기가 보지 못한다.
    /// </summary>
    public static class ModernLanguageApplier
    {
        private static readonly Assembly ownAssembly = typeof(ModernLanguageApplier).Assembly;

        // 형식마다 번역할 속성 목록 — 리플렉션은 형식당 한 번만 한다.
        private static readonly Dictionary<Type, PropertyDescriptor[]> textProperties = new Dictionary<Type, PropertyDescriptor[]>();

        // Remember 가 적어 둔 글자 — 폼이 사라지면 함께 사라진다.
        private static readonly ConditionalWeakTable<Control, RememberedTexts> remembered =
            new ConditionalWeakTable<Control, RememberedTexts>();

        private static readonly object syncRoot = new object();

        /// <summary>
        /// 컨트롤이 놓인 화면 이름 — 가장 가까운 폼의 클래스 이름("DurableManagementForm"). 폼에 아직 올라가지 않았으면 빈 문자열이다.
        /// 번역 표의 화면 줄과 누락 목록의 화면 칸이 이 이름을 쓴다.
        /// </summary>
        public static string ScreenOf(Control control)
        {
            if (control == null)
            {
                return string.Empty;
            }

            Form form = control as Form ?? control.FindForm();
            return form == null ? string.Empty : form.GetType().Name;
        }

        /// <summary>
        /// <paramref name="root"/>와 그 아래 모든 컨트롤의 디자이너 글자를 지금 번역한다 — <see cref="Remember"/> 뒤 곧바로
        /// <see cref="ApplyRemembered"/>를 부른 것과 같다. 화면의 초기화 코드가 끝난 자리(Load 처리기의 끝)에서 부른다.
        /// </summary>
        public static void Apply(Control root)
        {
            Remember(root);
            ApplyRemembered(root);
        }

        /// <summary>
        /// 지금의 디자이너 글자를 적어 둔다(번역은 하지 않는다) — 폼이 만들어진 직후, 화면의 Load 처리기가 돌기 전에 부른다.
        /// 번역이 꺼져 있거나 root 가 null 이면 아무 일도 없다. 다시 부르면 새로 적는다.
        /// </summary>
        public static void Remember(Control root)
        {
            if (root == null || !ModernLanguage.IsActive)
            {
                return;
            }

            RememberedTexts texts = new RememberedTexts(ScreenOf(root));
            HashSet<ToolStrip> visitedStrips = new HashSet<ToolStrip>();
            Collect(root, texts, visitedStrips);

            lock (syncRoot)
            {
                remembered.Remove(root);
                remembered.Add(root, texts);
            }
        }

        /// <summary>
        /// <see cref="Remember"/>가 적어 둔 글자를 번역해 넣는다. 그 뒤로 코드가 바꾼 글자는 건드리지 않고(디자이너 글자가 아니다),
        /// 누락 목록에도 넣지 않는다. 적어 둔 것이 없으면 아무 일도 없다. 속성 하나에서 난 예외는 그 속성만 건너뛴다 —
        /// 글자 하나 때문에 화면이 못 열리면 안 된다.
        /// </summary>
        public static void ApplyRemembered(Control root)
        {
            if (root == null)
            {
                return;
            }

            RememberedTexts texts;

            lock (syncRoot)
            {
                if (!remembered.TryGetValue(root, out texts))
                {
                    return;
                }

                remembered.Remove(root);
            }

            foreach (RememberedText entry in texts.Entries)
            {
                try
                {
                    entry.Apply(texts.Screen);
                }
                catch (Exception)
                {
                    // 속성 하나가 읽기·쓰기에 실패해도 나머지 화면은 그대로 번역한다.
                }
            }
        }

        private static void Collect(Control control, RememberedTexts texts, HashSet<ToolStrip> visitedStrips)
        {
            ToolStrip strip = control as ToolStrip;

            if (strip != null)
            {
                CollectStrip(strip, texts, visitedStrips);
            }
            else
            {
                CollectProperties(control, texts);
            }

            ContextMenuStrip menu = control.ContextMenuStrip;

            if (menu != null)
            {
                CollectStrip(menu, texts, visitedStrips);
            }

            foreach (Control child in control.Controls)
            {
                Collect(child, texts, visitedStrips);
            }
        }

        private static void CollectProperties(Control control, RememberedTexts texts)
        {
            foreach (PropertyDescriptor property in TextPropertiesOf(control.GetType()))
            {
                try
                {
                    // 기본값 그대로인 속성은 화면이 지정한 글자가 아니다 — 디자이너가 .Designer.cs 에 적지 않는 값과 같은 기준이다.
                    if (!property.ShouldSerializeValue(control))
                    {
                        continue;
                    }

                    string source = property.GetValue(control) as string;

                    if (!string.IsNullOrEmpty(source))
                    {
                        texts.Entries.Add(new RememberedText(control, property, source, control.Name));
                    }
                }
                catch (Exception)
                {
                    // 읽지 못한 속성은 번역 대상에서 빠질 뿐이다.
                }
            }
        }

        private static void CollectStrip(ToolStrip strip, RememberedTexts texts, HashSet<ToolStrip> visitedStrips)
        {
            if (!visitedStrips.Add(strip))
            {
                return;
            }

            CollectItems(strip.Items, texts, visitedStrips);
        }

        private static void CollectItems(ToolStripItemCollection items, RememberedTexts texts, HashSet<ToolStrip> visitedStrips)
        {
            foreach (ToolStripItem item in items)
            {
                // 메뉴 띠 안의 입력 칸(ToolStripTextBox · ToolStripComboBox 등 컨트롤을 품은 항목)의 Text 는 사용자의 값이다 —
                // 일반 컨트롤의 TextIsValue 와 같은 이유로 번역하지 않는다.
                if (item is ToolStripControlHost)
                {
                    continue;
                }

                string source = item.Text;

                if (!string.IsNullOrEmpty(source))
                {
                    texts.Entries.Add(new RememberedText(item, null, source, item.Name));
                }

                ToolStripDropDownItem parent = item as ToolStripDropDownItem;

                if (parent != null && parent.HasDropDownItems && visitedStrips.Add(parent.DropDown))
                {
                    CollectItems(parent.DropDownItems, texts, visitedStrips);
                }
            }
        }

        private static PropertyDescriptor[] TextPropertiesOf(Type type)
        {
            lock (syncRoot)
            {
                PropertyDescriptor[] cached;

                if (textProperties.TryGetValue(type, out cached))
                {
                    return cached;
                }

                List<PropertyDescriptor> found = new List<PropertyDescriptor>();
                bool textIsValue = TextIsValue(type);

                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(type))
                {
                    if (property.PropertyType != typeof(string) || property.IsReadOnly)
                    {
                        continue;
                    }

                    if (property.Name == "Text")
                    {
                        if (!textIsValue)
                        {
                            found.Add(property);
                        }

                        continue;
                    }

                    if (property.IsLocalizable && DeclaredInOwnAssembly(type, property.Name))
                    {
                        found.Add(property);
                    }
                }

                cached = found.ToArray();
                textProperties[type] = cached;
                return cached;
            }
        }

        // Text 가 캡션이 아니라 입력값·데이터인 컨트롤 — 번역하면 사용자의 값이 바뀐다.
        // 자리표시 글자(PlaceholderText · Placeholder)를 가진 래퍼는 입력 컨트롤이다.
        private static bool TextIsValue(Type type)
        {
            if (typeof(TextBoxBase).IsAssignableFrom(type)
                    || typeof(ListControl).IsAssignableFrom(type)
                    || typeof(UpDownBase).IsAssignableFrom(type)
                    || typeof(DateTimePicker).IsAssignableFrom(type)
                    || typeof(Modern.Lab.WinForms.Controls.Input.ModernRichTextBox).IsAssignableFrom(type)
                    || typeof(Modern.Lab.WinForms.Controls.Display.ModernBarcode).IsAssignableFrom(type))
            {
                return true;
            }

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
            return properties["PlaceholderText"] != null || properties["Placeholder"] != null;
        }

        // 이 라이브러리의 래퍼가 선언한 속성인지 — 표준 WinForms 컨트롤의 다른 Localizable 속성(AccessibleName · ImageKey …)은 화면 글자가 아니다.
        private static bool DeclaredInOwnAssembly(Type type, string propertyName)
        {
            for (Type current = type; current != null && current != typeof(Control); current = current.BaseType)
            {
                PropertyInfo declared = current.GetProperty(
                        propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (declared != null)
                {
                    return current.Assembly == ownAssembly;
                }
            }

            return false;
        }

        // 한 폼에서 적어 둔 글자 묶음.
        private sealed class RememberedTexts
        {
            internal RememberedTexts(string screen)
            {
                this.Screen = screen;
                this.Entries = new List<RememberedText>();
            }

            internal string Screen { get; private set; }

            internal List<RememberedText> Entries { get; private set; }
        }

        // 적어 둔 글자 한 건 — 컨트롤의 속성이거나(property 있음) 메뉴 항목의 Text(property 없음)다.
        private sealed class RememberedText
        {
            private readonly object target;
            private readonly PropertyDescriptor property;
            private readonly string source;
            private readonly string hint;

            internal RememberedText(object target, PropertyDescriptor property, string source, string hint)
            {
                this.target = target;
                this.property = property;
                this.source = source;
                this.hint = hint;
            }

            // 적어 둔 뒤로 값이 그대로일 때만 번역한다 — 그 사이에 코드가 바꾼 글자는 디자이너 글자가 아니다.
            internal void Apply(string screen)
            {
                ToolStripItem item = this.target as ToolStripItem;

                if (item != null)
                {
                    if (!item.IsDisposed && string.Equals(item.Text, this.source, StringComparison.Ordinal))
                    {
                        string itemText = ModernLanguage.Translate(screen, this.source, this.hint);

                        if (!string.Equals(itemText, this.source, StringComparison.Ordinal))
                        {
                            item.Text = itemText;
                        }
                    }

                    return;
                }

                Control control = this.target as Control;

                if (control == null || control.IsDisposed
                        || !string.Equals(this.property.GetValue(control) as string, this.source, StringComparison.Ordinal))
                {
                    return;
                }

                string text = ModernLanguage.Translate(screen, this.source, this.hint);

                if (!string.Equals(text, this.source, StringComparison.Ordinal))
                {
                    this.property.SetValue(control, text);
                }
            }
        }
    }
}
