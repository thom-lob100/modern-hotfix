using System;
using System.Collections.Generic;
using Modern.Lab.WinForms.Rendering;

namespace Modern.Lab.Samples.Services
{
    public static class MenuGlyphs
    {
        public const string CancelWord = "Cancel";

        private static readonly Dictionary<string, string> byLabel = Create();

        public static string For(string label)
        {
            string glyph;
            return label != null && byLabel.TryGetValue(label, out glyph) ? glyph : string.Empty;
        }

        public static string AccentOf(string label)
        {
            return label != null && label.Length > CancelWord.Length + 1
                    && label.EndsWith(" " + CancelWord, StringComparison.Ordinal) ? CancelWord : string.Empty;
        }

        public static ModernMenuItem CreateItem(string label)
        {
            ModernMenuItem item = new ModernMenuItem(label);
            item.Glyph = For(label);
            item.AccentText = AccentOf(label);
            return item;
        }

        private static Dictionary<string, string> Create()
        {
            Dictionary<string, string> glyphs = new Dictionary<string, string>(StringComparer.Ordinal);

            glyphs["Create Wafer"] = ModernMenuGlyphs.Add;
            glyphs["Create Chip"] = ModernMenuGlyphs.Add;
            glyphs["Create Lamella"] = ModernMenuGlyphs.Add;
            glyphs["Hold"] = ModernMenuGlyphs.Pause;
            glyphs["NotOnHold"] = ModernMenuGlyphs.Play;
            glyphs["ChangeSpec"] = ModernMenuGlyphs.Edit;
            glyphs["ChangeSpec Cancel"] = ModernMenuGlyphs.Cancel;
            glyphs["Scrap"] = ModernMenuGlyphs.Delete;
            glyphs["Scrap Cancel"] = ModernMenuGlyphs.Cancel;

            glyphs["JobPrep"] = ModernMenuGlyphs.Flag;
            glyphs["JobPrep Cancel"] = ModernMenuGlyphs.Cancel;
            glyphs["JobStart"] = ModernMenuGlyphs.Play;
            glyphs["JobStart Cancel"] = ModernMenuGlyphs.Cancel;
            glyphs["JobEnd"] = ModernMenuGlyphs.Stop;
            glyphs["Skip Oper"] = ModernMenuGlyphs.Next;
            glyphs["Back to Prev Oper"] = ModernMenuGlyphs.Undo;

            glyphs["OnLineRemote"] = ModernMenuGlyphs.Globe;
            glyphs["OnLineLocal"] = ModernMenuGlyphs.Monitor;
            glyphs["OffLine"] = ModernMenuGlyphs.Power;
            glyphs["Auto"] = ModernMenuGlyphs.Setting;
            glyphs["Load"] = ModernMenuGlyphs.Download;
            glyphs["UnLoad"] = ModernMenuGlyphs.Upload;
            glyphs["Reserve"] = ModernMenuGlyphs.Calendar;

            glyphs["Create"] = ModernMenuGlyphs.Add;
            glyphs["Register"] = ModernMenuGlyphs.Add;
            glyphs["Move Wafer"] = ModernMenuGlyphs.Switch;
            glyphs["RFID"] = ModernMenuGlyphs.Tag;
            glyphs["Exchange"] = ModernMenuGlyphs.Switch;
            glyphs["Edit"] = ModernMenuGlyphs.Edit;
            glyphs["Release"] = ModernMenuGlyphs.Play;
            glyphs["Adjust"] = ModernMenuGlyphs.Setting;
            glyphs["Transport"] = ModernMenuGlyphs.Send;

            return glyphs;
        }
    }
}
