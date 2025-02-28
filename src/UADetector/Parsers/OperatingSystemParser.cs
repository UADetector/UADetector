using System.Collections.Frozen;

using UADetector.Models;
using UADetector.Regexes.Models;

namespace UADetector.Parsers;

public class OperatingSystemParser
{
    private const string ResourceName = "Regexes.Resources.operating_systems.yml";

    private static readonly IEnumerable<OperatingSystemRegex> Regexes =
        ParserExtensions.LoadRegexes<OperatingSystemRegex>(ResourceName);

    private static readonly FrozenDictionary<OsShortName, string> OsShortNameToFullNameMapping =
        new Dictionary<OsShortName, string>
        {
            { OsShortName.AIX, OsName.Aix },
            { OsShortName.AND, OsName.Android },
            { OsShortName.ADR, OsName.AndroidTv },
            { OsShortName.ALP, OsName.AlpineLinux },
            { OsShortName.AMZ, OsName.AmazonLinux },
            { OsShortName.AMG, OsName.AmigaOs },
            { OsShortName.ARM, OsName.ArmadilloOs },
            { OsShortName.ARO, OsName.Aros },
            { OsShortName.ATV, OsName.TvOs },
            { OsShortName.ARL, OsName.ArchLinux },
            { OsShortName.AOS, OsName.AosCos },
            { OsShortName.ASP, OsName.AspLinux },
            { OsShortName.AZU, OsName.AzureLinux },
            { OsShortName.BTR, OsName.BackTrack },
            { OsShortName.SBA, OsName.Bada },
            { OsShortName.BYI, OsName.BaiduYi },
            { OsShortName.BEO, OsName.BeOs },
            { OsShortName.BLB, OsName.BlackBerryOs },
            { OsShortName.QNX, OsName.BlackBerryTabletOs },
            { OsShortName.PAN, OsName.BlackPantherOs },
            { OsShortName.BOS, OsName.BlissOs },
            { OsShortName.BMP, OsName.Brew },
            { OsShortName.BSN, OsName.BrightSignOs },
            { OsShortName.CAI, OsName.CaixaMagica },
            { OsShortName.CES, OsName.CentOs },
            { OsShortName.CST, OsName.CentOsStream },
            { OsShortName.CLO, OsName.ClearLinuxOs },
            { OsShortName.CLR, OsName.ClearOsMobile },
            { OsShortName.COS, OsName.ChromeOs },
            { OsShortName.CRS, OsName.ChromiumOs },
            { OsShortName.CHN, OsName.ChinaOs },
            { OsShortName.COL, OsName.CoolitaOs },
            { OsShortName.CYN, OsName.CyanogenMod },
            { OsShortName.DEB, OsName.Debian },
            { OsShortName.DEE, OsName.Deepin },
            { OsShortName.DFB, OsName.DragonFly },
            { OsShortName.DVK, OsName.DvkBuntu },
            { OsShortName.ELE, OsName.ElectroBsd },
            { OsShortName.EUL, OsName.EulerOs },
            { OsShortName.FED, OsName.Fedora },
            { OsShortName.FEN, OsName.Fenix },
            { OsShortName.FOS, OsName.FirefoxOs },
            { OsShortName.FIR, OsName.FireOs },
            { OsShortName.FOR, OsName.ForesightLinux },
            { OsShortName.FRE, OsName.Freebox },
            { OsShortName.BSD, OsName.FreeBsd },
            { OsShortName.FRI, OsName.FritzOs },
            { OsShortName.FYD, OsName.FydeOs },
            { OsShortName.FUC, OsName.Fuchsia },
            { OsShortName.GNT, OsName.Gentoo },
            { OsShortName.GNX, OsName.Genix },
            { OsShortName.GEO, OsName.Geos },
            { OsShortName.GNS, OsName.GNewSense },
            { OsShortName.GRI, OsName.GridOs },
            { OsShortName.GTV, OsName.GoogleTv },
            { OsShortName.HPX, OsName.HpUx },
            { OsShortName.HAI, OsName.HaikuOs },
            { OsShortName.IPA, OsName.IPadOs },
            { OsShortName.HAR, OsName.HarmonyOs },
            { OsShortName.HAS, OsName.HasCodingOs },
            { OsShortName.HEL, OsName.HelixOs },
            { OsShortName.IRI, OsName.Irix },
            { OsShortName.INF, OsName.Inferno },
            { OsShortName.JME, OsName.JavaMe },
            { OsShortName.JOL, OsName.JoliOs },
            { OsShortName.KOS, OsName.KaiOs },
            { OsShortName.KAL, OsName.Kali },
            { OsShortName.KAN, OsName.Kanotix },
            { OsShortName.KIN, OsName.Kinos },
            { OsShortName.KNO, OsName.Knoppix },
            { OsShortName.KTV, OsName.KreaTv },
            { OsShortName.KBT, OsName.Kubuntu },
            { OsShortName.LIN, OsName.GnuLinux },
            { OsShortName.LEA, OsName.LeafOs },
            { OsShortName.LND, OsName.LindowsOs },
            { OsShortName.LNS, OsName.Linspire },
            { OsShortName.LEN, OsName.LineageOs },
            { OsShortName.LIR, OsName.LiriOs },
            { OsShortName.LOO, OsName.Loongnix },
            { OsShortName.LBT, OsName.Lubuntu },
            { OsShortName.LOS, OsName.LuminOs },
            { OsShortName.LUN, OsName.LuneOs },
            { OsShortName.VLN, OsName.VectorLinux },
            { OsShortName.MAC, OsName.Mac },
            { OsShortName.MAE, OsName.Maemo },
            { OsShortName.MAG, OsName.Mageia },
            { OsShortName.MDR, OsName.Mandriva },
            { OsShortName.SMG, OsName.MeeGo },
            { OsShortName.MET, OsName.MetaHorizon },
            { OsShortName.MCD, OsName.MocorDroid },
            { OsShortName.MON, OsName.MoonOs },
            { OsShortName.EZX, OsName.MotorolaEzx },
            { OsShortName.MIN, OsName.Mint },
            { OsShortName.MLD, OsName.MildWild },
            { OsShortName.MOR, OsName.MorphOs },
            { OsShortName.NBS, OsName.NetBsd },
            { OsShortName.MTK, OsName.MtkNucleus },
            { OsShortName.MRE, OsName.Mre },
            { OsShortName.NXT, OsName.NeXtStep },
            { OsShortName.NWS, OsName.NewsOs },
            { OsShortName.WII, OsName.Nintendo },
            { OsShortName.NDS, OsName.NintendoMobile },
            { OsShortName.NOV, OsName.Nova },
            { OsShortName.OS2, OsName.Os2 },
            { OsShortName.T64, OsName.Osf1 },
            { OsShortName.OBS, OsName.OpenBsd },
            { OsShortName.OVS, OsName.OpenVms },
            { OsShortName.OVZ, OsName.OpenVz },
            { OsShortName.OWR, OsName.OpenWrt },
            { OsShortName.OTV, OsName.OperaTv },
            { OsShortName.ORA, OsName.OracleLinux },
            { OsShortName.ORD, OsName.Ordissimo },
            { OsShortName.PAR, OsName.Pardus },
            { OsShortName.PCL, OsName.PcLinuxOs },
            { OsShortName.PIC, OsName.PicoOs },
            { OsShortName.PLA, OsName.PlasmaMobile },
            { OsShortName.PSP, OsName.PlayStationPortable },
            { OsShortName.PS3, OsName.PlayStation },
            { OsShortName.PVE, OsName.ProxmoxVe },
            { OsShortName.PUF, OsName.PuffinOs },
            { OsShortName.PUR, OsName.PureOs },
            { OsShortName.QTP, OsName.Qtopia },
            { OsShortName.PIO, OsName.RaspberryPiOs },
            { OsShortName.RAS, OsName.Raspbian },
            { OsShortName.RHT, OsName.RedHat },
            { OsShortName.RST, OsName.RedStar },
            { OsShortName.RED, OsName.RedOs },
            { OsShortName.REV, OsName.RevengeOs },
            { OsShortName.RIS, OsName.RisingOs },
            { OsShortName.ROS, OsName.RiscOs },
            { OsShortName.ROC, OsName.RockyLinux },
            { OsShortName.ROK, OsName.RokuOs },
            { OsShortName.RSO, OsName.Rosa },
            { OsShortName.ROU, OsName.RouterOs },
            { OsShortName.REM, OsName.RemixOs },
            { OsShortName.RRS, OsName.ResurrectionRemixOs },
            { OsShortName.REX, OsName.Rex },
            { OsShortName.RZD, OsName.RazoDroid },
            { OsShortName.RXT, OsName.RtosNext },
            { OsShortName.SAB, OsName.Sabayon },
            { OsShortName.SSE, OsName.Suse },
            { OsShortName.SAF, OsName.SailfishOs },
            { OsShortName.SCI, OsName.ScientificLinux },
            { OsShortName.SEE, OsName.SeewoOs },
            { OsShortName.SER, OsName.SerenityOs },
            { OsShortName.SIR, OsName.SirinOs },
            { OsShortName.SLW, OsName.Slackware },
            { OsShortName.SOS, OsName.Solaris },
            { OsShortName.SBL, OsName.StarBladeOs },
            { OsShortName.SYL, OsName.Syllable },
            { OsShortName.SYM, OsName.Symbian },
            { OsShortName.SYS, OsName.SymbianOs },
            { OsShortName.S40, OsName.SymbianOsSeries40 },
            { OsShortName.S60, OsName.SymbianOsSeries60 },
            { OsShortName.SY3, OsName.Symbian3 },
            { OsShortName.TEN, OsName.TencentOs },
            { OsShortName.TDX, OsName.ThreadX },
            { OsShortName.TIZ, OsName.Tizen },
            { OsShortName.TIV, OsName.TiVoOs },
            { OsShortName.TOS, OsName.TmaxOs },
            { OsShortName.TUR, OsName.Turbolinux },
            { OsShortName.UBT, OsName.Ubuntu },
            { OsShortName.ULT, OsName.Ultrix },
            { OsShortName.UOS, OsName.Uos },
            { OsShortName.VID, OsName.Vidae },
            { OsShortName.VIZ, OsName.ViziOs },
            { OsShortName.WAS, OsName.WatchOs },
            { OsShortName.WER, OsName.WearOs },
            { OsShortName.WTV, OsName.WebTv },
            { OsShortName.WHS, OsName.WhaleOs },
            { OsShortName.WIN, OsName.Windows },
            { OsShortName.WCE, OsName.WindowsCe },
            { OsShortName.WIO, OsName.WindowsIoT },
            { OsShortName.WMO, OsName.WindowsMobile },
            { OsShortName.WPH, OsName.WindowsPhone },
            { OsShortName.WRT, OsName.WindowsRt },
            { OsShortName.WPO, OsName.WoPhone },
            { OsShortName.XBX, OsName.Xbox },
            { OsShortName.XBT, OsName.Xubuntu },
            { OsShortName.YNS, OsName.YunOs },
            { OsShortName.ZEN, OsName.Zenwalk },
            { OsShortName.ZOR, OsName.ZorinOs },
            { OsShortName.IOS, OsName.IOs },
            { OsShortName.POS, OsName.PalmOs },
            { OsShortName.WEB, OsName.Webian },
            { OsShortName.WOS, OsName.WebOs },
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, OsShortName> OsFullNameToShortNameMapping =
        OsShortNameToFullNameMapping.ToDictionary(e => e.Value, e => e.Key)
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

}
