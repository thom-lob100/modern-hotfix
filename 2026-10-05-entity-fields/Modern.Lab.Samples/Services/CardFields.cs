using Modern.Lab.Controls.Wpf.Data;
using Modern.Lab.WinForms.Controls.Data;
using Modern.Lab.WinForms.Controls.Display;

namespace Modern.Lab.Samples.Services
{
    internal static class CardFields
    {
        internal static ModernFieldDefinition Of(Field field)
        {
            ModernFieldDefinition item = new ModernFieldDefinition(field.Name);

            if (field.CaptionSetting != null)
            {
                item.Caption(GridCaptionCatalog.PlainCaption(
                        field.TopLabelSetting, field.CaptionSetting, field.BottomLabelSetting));
            }

            if (field.FormatSetting != null)
            {
                item.Format = field.FormatSetting;
            }

            if (field.BadgeSetting)
            {
                item.IsBadge = true;
            }

            return item;
        }
    }
}
