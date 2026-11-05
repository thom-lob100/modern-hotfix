using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Modern.Lab.Controls.Wpf.Display;
using Modern.Lab.WinForms.Controls.Hosting;

namespace Modern.Lab.WinForms.Controls.Display
{
    /// <summary>
    /// ModernHandlerLayoutControl(WPF)의 WinForms 래퍼 — 메인 핸들러의 물리
    /// 구조(인포트 → 내부 장비 F1~F4 → 아웃포트)를 물류 흐름 순서로 그리는
    /// 구조도. (직접 대응하는 WinForms 컨트롤 없음.)
    ///
    /// 사용 흐름: 폼이 서버 조회 결과를 HandlerLayoutPort/HandlerLayoutUnit
    /// 목록으로 변환해 SetLayout으로 통째로 주고(재조회 = 재구성), 내부 장비
    /// 타일 클릭을 UnitClicked로 받는다. 내부 장비는 모니터링 대상이라 선택
    /// 상태를 만들지 않는다 — 처리는 장비(핸들러) 단위 진입점이 담당한다.
    /// </summary>
    [ToolboxItem(true)]
    public class ModernHandlerLayout : WpfElementHostBase<ModernHandlerLayoutControl>
    {
        /// <summary>내부 장비 타일을 클릭하면 발생한다 — 클릭된 장비 ID를 알린다.</summary>
        public event EventHandler<HandlerUnitEventArgs> UnitClicked;

        /// <summary>포트 칩을 클릭하면 발생한다 — 칩이 선택 표시되고, 폼이
        /// 포트 상세 행 선택과 동기화해 포트 처리(Port Actions)의 대상으로
        /// 삼는다.</summary>
        public event EventHandler<HandlerPortEventArgs> PortClicked;

        /// <summary>포트 칩을 우클릭하면 발생한다 — 클릭과 같은 선택 동작 후
        /// 폼이 포트 컨텍스트 메뉴를 띄우는 데 쓴다.</summary>
        public event EventHandler<HandlerPortEventArgs> PortRightClick;

        /// <summary>적절한 기본 크기로 컨트롤을 생성한다.</summary>
        public ModernHandlerLayout()
        {
            this.Size = new Size(360, 300);

            if (this.Wpf != null)
            {
                this.Wpf.UnitClicked += this.OnInnerUnitClicked;
                this.Wpf.PortClicked += this.OnInnerPortClicked;
                this.Wpf.PortRightClick += this.OnInnerPortRightClick;
            }
        }

        /// <summary>구조도 데이터를 통째로 교체하고 다시 그린다 — 재조회
        /// 시마다 호출한다. null은 빈 목록으로 취급한다.</summary>
        public void SetLayout(IList<HandlerLayoutPort> ports, IList<HandlerLayoutUnit> units)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetLayout(ports, units);
            }
        }

        /// <summary>구조도를 비운다 (안내 문구만 남는다).</summary>
        public void ClearLayout()
        {
            this.SetLayout(null, null);
        }

        /// <summary>포트 칩 선택 표시를 지정한다 (null/미존재 라벨 = 해제) —
        /// 폼이 포트 상세 행 선택과 칩 표시를 맞추는 데 쓴다.</summary>
        public void SetSelectedPort(string portLabel)
        {
            if (this.Wpf != null)
            {
                this.Wpf.SetSelectedPort(portLabel);
            }
        }

        private string portLabelMember = string.Empty;
        private string portDirectionMember = string.Empty;
        private string portStateMember = string.Empty;
        private string portLotMember = string.Empty;
        private string siteIdMember = string.Empty;
        private string siteStateMember = string.Empty;
        private string siteLotMember = string.Empty;
        private string siteReservedLotMember = string.Empty;

        /// <summary>포트 행에서 칩 글자("In 1")를 읽을 컬럼/속성 이름 — <see cref="SetLayoutSource"/> 가 쓴다(2026-11-04).</summary>
        [Category("Modern")]
        [Description("Column/property name of the port chip label (used by SetLayoutSource)")]
        [DefaultValue("")]
        public string PortLabelMember
        {
            get { return this.portLabelMember; }
            set { this.portLabelMember = value ?? string.Empty; }
        }

        /// <summary>포트 행에서 방향을 읽을 컬럼/속성 이름 — 값이 Out · Output 이면 아웃포트, 그 밖은 인포트.</summary>
        [Category("Modern")]
        [Description("Column/property name of the port direction (Out / Output = out port)")]
        [DefaultValue("")]
        public string PortDirectionMember
        {
            get { return this.portDirectionMember; }
            set { this.portDirectionMember = value ?? string.Empty; }
        }

        /// <summary>포트 행에서 상태(Empty · Loaded · Running · Done · Reserved)를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the port state (Empty, Loaded, Running, Done, Reserved)")]
        [DefaultValue("")]
        public string PortStateMember
        {
            get { return this.portStateMember; }
            set { this.portStateMember = value ?? string.Empty; }
        }

        /// <summary>포트 행에서 실린 Lot Id 를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the lot on the port")]
        [DefaultValue("")]
        public string PortLotMember
        {
            get { return this.portLotMember; }
            set { this.portLotMember = value ?? string.Empty; }
        }

        /// <summary>내부 장비(사이트) 행에서 타일 Id("F1")를 읽을 컬럼/속성 이름.</summary>
        [Category("Modern")]
        [Description("Column/property name of the site tile id (e.g. F1)")]
        [DefaultValue("")]
        public string SiteIdMember
        {
            get { return this.siteIdMember; }
            set { this.siteIdMember = value ?? string.Empty; }
        }

        /// <summary>내부 장비 행에서 상태(Down · Run · Idle)를 읽을 컬럼/속성 이름. 비면 Lot 유무로 유도한다.</summary>
        [Category("Modern")]
        [Description("Column/property name of the site state (Down, Run, Idle; empty = derived from lots)")]
        [DefaultValue("")]
        public string SiteStateMember
        {
            get { return this.siteStateMember; }
            set { this.siteStateMember = value ?? string.Empty; }
        }

        /// <summary>내부 장비 행에서 지금 앉은 Lot Id 를 읽을 컬럼/속성 이름(게이지의 채운 칸).</summary>
        [Category("Modern")]
        [Description("Column/property name of the lot sitting on the site (filled gauge cell)")]
        [DefaultValue("")]
        public string SiteLotMember
        {
            get { return this.siteLotMember; }
            set { this.siteLotMember = value ?? string.Empty; }
        }

        /// <summary>내부 장비 행에서 예약 Lot Id 를 읽을 컬럼/속성 이름(게이지의 테두리 칸).</summary>
        [Category("Modern")]
        [Description("Column/property name of the lot reserved for the site (outlined gauge cell)")]
        [DefaultValue("")]
        public string SiteReservedLotMember
        {
            get { return this.siteReservedLotMember; }
            set { this.siteReservedLotMember = value ?? string.Empty; }
        }

        /// <summary>
        /// 포트 · 내부 장비를 행 목록(DataTable / DataView / IList / IEnumerable)으로 받아 구조도를 통째로 교체한다 — 모델 타입을 모르는 폼을 위한
        /// 데이터 계약 경로(2026-11-04). 어느 컬럼을 읽을지는 <c>Port*Member</c> · <c>Site*Member</c> 가 정하고, null · 빈 소스 · 없는 컬럼은 빈 값으로
        /// 처리하며 예외를 내지 않는다. 사이트의 게이지 칸 수는 1 이다(한 자리에 Lot 하나).
        /// </summary>
        public void SetLayoutSource(object ports, object sites)
        {
            List<HandlerLayoutPort> portModels = new List<HandlerLayoutPort>();
            List<HandlerLayoutUnit> siteModels = new List<HandlerLayoutUnit>();

            System.Collections.IEnumerable portRows = DataSourceConverter.ToItemsSource(ports);

            if (portRows != null)
            {
                foreach (object row in portRows)
                {
                    HandlerLayoutPort port = new HandlerLayoutPort();
                    port.Label = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.portLabelMember);
                    string direction = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.portDirectionMember).Trim();
                    port.IsOut = direction.Equals("Out", StringComparison.OrdinalIgnoreCase)
                            || direction.Equals("Output", StringComparison.OrdinalIgnoreCase);
                    port.State = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.portStateMember);
                    port.LotId = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.portLotMember);
                    portModels.Add(port);
                }
            }

            System.Collections.IEnumerable siteRows = DataSourceConverter.ToItemsSource(sites);

            if (siteRows != null)
            {
                foreach (object row in siteRows)
                {
                    HandlerLayoutUnit site = new HandlerLayoutUnit();
                    site.Id = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.siteIdMember);
                    site.State = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.siteStateMember);
                    site.Capacity = 1;

                    string lotId = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.siteLotMember).Trim();
                    string reserved = Modern.Lab.Controls.Wpf.Common.MemberPathReader.ReadDisplayText(row, this.siteReservedLotMember).Trim();

                    if (lotId.Length > 0)
                    {
                        site.LotIds.Add(lotId);
                    }

                    if (reserved.Length > 0)
                    {
                        site.ReservedLotIds.Add(reserved);
                    }

                    siteModels.Add(site);
                }
            }

            this.SetLayout(portModels, siteModels);
        }

        private void OnInnerUnitClicked(object sender, HandlerUnitEventArgs e)
        {
            EventHandler<HandlerUnitEventArgs> handler = this.UnitClicked;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnInnerPortClicked(object sender, HandlerPortEventArgs e)
        {
            EventHandler<HandlerPortEventArgs> handler = this.PortClicked;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnInnerPortRightClick(object sender, HandlerPortEventArgs e)
        {
            EventHandler<HandlerPortEventArgs> handler = this.PortRightClick;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
