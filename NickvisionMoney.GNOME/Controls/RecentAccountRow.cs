using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying recent accounts
/// </summary>
public partial class RecentAccountRow : Adw.ActionRow
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(ref Color rgba, string spec);

    [Gtk.Connect] private readonly Gtk.Image _prefixColor;
    [Gtk.Connect] private readonly Gtk.Button _prefixButton;
    [Gtk.Connect] private readonly Gtk.DrawingArea _tagArea;
    [Gtk.Connect] private readonly Gtk.Label _tagLabel;

    private readonly Color _color;

    public event EventHandler? OnOpenAccount;

    /// <summary>
    /// Constructs a RecentAccountRow
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="account">The RecentAccount model</param>
    /// <param name="colorString">The color string of the recent account</param>
    /// <param name="onStartScreen">Whether or not the row is being shown on the start screen</param>
    /// <param name="localizer">The Localizer for the app</param>
    private RecentAccountRow(Gtk.Builder builder, RecentAccount account, string colorString, bool onStartScreen, Localizer localizer) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        SetTitle(account.Name);
        SetSubtitle(account.Path);
        _prefixButton.OnClicked += (sender, e) => OnOpenAccount?.Invoke(this, EventArgs.Empty);
        _color = new Color();
        gdk_rgba_parse(ref _color, colorString);
        var luma = _color.Red * 0.2126 + _color.Green * 0.7152 + _color.Blue * 0.0722;
        if (onStartScreen)
        {
            var sizeGroup = Gtk.SizeGroup.New(Gtk.SizeGroupMode.Horizontal);
            sizeGroup.AddWidget(_tagArea);
            sizeGroup.AddWidget(_tagLabel);
            _tagLabel.SetLabel($"<span weight=\"bold\" size=\"9pt\">{localizer["AccountType", account.Type.ToString()]}</span>");
            _tagLabel.AddCssClass(luma > 0.5 ? "tag-dark" : "tag-light");
            _tagArea.SetDrawFunc(DrawTag);
        }
        else
        {
            var pixbuf = GdkPixbuf.Pixbuf.New(GdkPixbuf.Colorspace.Rgb, false, 8, 1, 1);
            var red = (int)(_color.Red * 255);
            var green = (int)(_color.Green * 255);
            var blue = (int)(_color.Blue * 255);
            uint colorPixbuf;
            if (uint.TryParse(red.ToString("X2") + green.ToString("X2") + blue.ToString("X2") + "FF", System.Globalization.NumberStyles.HexNumber, null, out colorPixbuf))
            {
                pixbuf.Fill(colorPixbuf);
                _prefixColor.SetFromPixbuf(pixbuf);
                _prefixButton.AddCssClass(luma > 0.5 ? "tag-dark" : "tag-light");
            }
        }
    }

    /// <summary>
    /// Constructs a RecentAccountRow
    /// </summary>
    /// <param name="account">The RecentAccount model</param>
    /// <param name="colorString">The color string of the recent account</param>
    /// <param name="onStartScreen">Whether or not the row is being shown on the start screen</param>
    /// <param name="localizer">The Localizer for the app</param>
    public RecentAccountRow(RecentAccount account, string colorString, bool onStartScreen, Localizer localizer) : this(Builder.FromFile("recent_account_row.ui", localizer), account, colorString, onStartScreen, localizer)
    {
    }

    /// <summary>
    /// Draws the tag icon
    /// </summary>
    /// <param name="area">Gtk.DrawingArea</param>
    /// <param name="cr">Cairo.Context</param>
    /// <param name="width">The width of the tag</param>
    /// <param name="height">The height of the tag</param>
    private void DrawTag(Gtk.DrawingArea area, Cairo.Context cr, int width, int height)
    {
        cr.SetSourceRgba(_color.Red, _color.Green, _color.Blue, 1);
        cr.Arc(height / 2, height / 2, height / 2, 0.5 * Math.PI, 1.5 * Math.PI);
        cr.Arc(width - height / 2, height / 2, height / 2, -0.5 * Math.PI, 0.5 * Math.PI);
        cr.ClosePath();
        cr.Fill();
    }
}