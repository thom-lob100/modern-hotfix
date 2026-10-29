using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.Hosting
{
    public static class ModernLoadCover
    {
        public const string DefaultMessage = "Loading…";

        private sealed class CoverEntry
        {
            public Panel Panel;
            public Control Host;
            public EventHandler SyncColor;
        }

        private static readonly Dictionary<Form, CoverEntry> attached =
                new Dictionary<Form, CoverEntry>();

        public static void Attach(Form form)
        {
            Attach(form, null, DefaultMessage);
        }

        public static void Attach(Form form, string message)
        {
            Attach(form, null, message);
        }

        public static void Attach(Form form, Control target)
        {
            Attach(form, target, DefaultMessage);
        }

        public static void Attach(Form form, Control target, string message)
        {
            Attach(form, target, message, true);
        }

        public static void Attach(Form form, Control target, string message, bool autoReleaseOnIdle)
        {
            if (form == null)
            {
                return;
            }

            if (attached.ContainsKey(form))
            {
                return;
            }

            Control host = target ?? (Control)form;

            Panel loadCover = new Panel();
            loadCover.BackColor = host.BackColor;
            loadCover.Bounds = host.ClientRectangle;
            loadCover.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                    | AnchorStyles.Left | AnchorStyles.Right;

            LoadCoverSurface surface = new LoadCoverSurface(message);
            surface.Dock = DockStyle.Fill;
            surface.BackColor = host.BackColor;
            loadCover.Controls.Add(surface);

            EventHandler syncColor = delegate
            {
                loadCover.BackColor = host.BackColor;
                surface.BackColor = host.BackColor;
            };
            host.BackColorChanged += syncColor;

            host.Controls.Add(loadCover);
            loadCover.BringToFront();

            CoverEntry entry = new CoverEntry();
            entry.Panel = loadCover;
            entry.Host = host;
            entry.SyncColor = syncColor;
            attached[form] = entry;

            form.Disposed += delegate
            {
                CoverEntry leftover;

                if (attached.TryGetValue(form, out leftover))
                {
                    leftover.Host.BackColorChanged -= leftover.SyncColor;
                    attached.Remove(form);
                }
            };

            // 처음 보이는 순간 커버를 강제로 그린다 — 자식 창의 첫 WM_PAINT 는 큐에 쌓인 메시지(첫 조회 응답의 BeginInvoke 등)를 다 처리한
            // 뒤에 오므로, 응답이 빨리 오면 "Loading…" 이 그려지기도 전에 응답 처리가 돌아 그동안 빈 화면으로 보였다(2026-10-29 회사 보고).
            form.Shown += delegate
            {
                if (!loadCover.IsDisposed && loadCover.Parent != null && loadCover.Visible)
                {
                    loadCover.Update();
                    surface.Refresh();
                }
            };

            if (!autoReleaseOnIdle)
            {
                return;
            }

            form.Load += delegate
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new Action(delegate { Release(form); }));
            };
        }

        public static void Release(Form form)
        {
            if (form == null)
            {
                return;
            }

            CoverEntry entry;
            if (!attached.TryGetValue(form, out entry))
            {
                return;
            }

            attached.Remove(form);
            entry.Host.BackColorChanged -= entry.SyncColor;

            Panel loadCover = entry.Panel;

            if (loadCover.IsDisposed)
            {
                return;
            }

            if (loadCover.Parent != null)
            {
                loadCover.Parent.Controls.Remove(loadCover);
            }

            loadCover.Dispose();
        }
    }
}
