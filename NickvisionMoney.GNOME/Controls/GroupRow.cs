using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Controls;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.GNOME.Controls;

/// <summary>
/// A row for displaying a group
/// </summary>
public partial class GroupRow : Adw.ActionRow, IGroupRowControl
{
    private CultureInfo _cultureAmount;

    [Gtk.Connect] private readonly Gtk.CheckButton _filterCheckButton;
    [Gtk.Connect] private readonly Gtk.Label _amountLabel;
    [Gtk.Connect] private readonly Gtk.Button _editButton;
    [Gtk.Connect] private readonly Gtk.Button _deleteButton;
    [Gtk.Connect] private readonly Gtk.FlowBox _flowBox;

    /// <summary>
    /// The Id of the Group the row represents
    /// </summary>
    public uint Id { get; private set; }

    /// <summary>
    /// Occurs when the filter checkbox is changed on the row
    /// </summary>
    public event EventHandler<(uint Id, bool Filter)>? FilterChanged;
    /// <summary>
    /// Occurs when the edit button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? EditTriggered;
    /// <summary>
    /// Occurs when the delete button on the row is clicked
    /// </summary>
    public event EventHandler<uint>? DeleteTriggered;

    private GroupRow(Gtk.Builder builder, Group group, CultureInfo cultureAmount, Localizer localizer, bool filterActive) : base(builder.GetPointer("_root"), false)
    {
        _cultureAmount = cultureAmount;
        //Build UI
        builder.Connect(this);
        //Filter Checkbox
        _filterCheckButton.OnToggled += FilterToggled;
        //Edit Button
        _editButton.OnClicked += Edit;
        //Delete Button
        _deleteButton.OnClicked += Delete;
        UpdateRow(group, cultureAmount, filterActive);
    }

    /// <summary>
    /// Constructs a group row 
    /// </summary>
    /// <param name="group">The Group to display</param>
    /// <param name="cultureAmount">The CultureInfo to use for the amount string</param>
    /// <param name="localizer">The Localizer for the app</param>
    /// <param name="filterActive">Whether or not the filter checkbutton should be active</param>
    public GroupRow(Group group, CultureInfo cultureAmount, Localizer localizer, bool filterActive) : this(Builder.FromFile("group_row.ui", localizer), group, cultureAmount, localizer, filterActive)
    {
    }

    /// <summary>
    /// Whether or not the filter checkbox is checked
    /// </summary>
    public bool FilterChecked
    {
        get => _filterCheckButton.GetActive();

        set => _filterCheckButton.SetActive(value);
    }

    /// <summary>
    /// Updates the row with the new model
    /// </summary>
    /// <param name="group">The new Group model</param>
    /// <param name="cultureAmount">The culture to use for displaying amount strings</param>
    /// <param name="filterActive">Whether or not the filter checkbox is active</param>
    public void UpdateRow(Group group, CultureInfo cultureAmount, bool filterActive)
    {
        Id = group.Id;
        _cultureAmount = cultureAmount;
        //Row Settings
        SetTitle(group.Name);
        SetSubtitle(group.Description);
        //Filter Checkbox
        _filterCheckButton.SetActive(filterActive);
        //Amount Label
        _amountLabel.SetLabel($"{(group.Balance >= 0 ? "+  " : "-  ")}{Math.Abs(group.Balance).ToAmountString(_cultureAmount)}");
        _amountLabel.AddCssClass(group.Balance >= 0 ? "denaro-income" : "denaro-expense");
        if (group.Id == 0)
        {
            _editButton.SetVisible(false);
            _deleteButton.SetVisible(false);
            _flowBox.SetValign(Gtk.Align.Center);
        }
    }

    /// <summary>
    /// Occurs when the filter checkbutton is toggled
    /// </summary>
    /// <param name="sender">Gtk.CheckButton</param>
    /// <param name="e">EventArgs</param>
    private void FilterToggled(Gtk.CheckButton sender, EventArgs e) => FilterChanged?.Invoke(this, (Id, _filterCheckButton.GetActive()));

    /// <summary>
    /// Occurs when the edit button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Edit(Gtk.Button sender, EventArgs e) => EditTriggered?.Invoke(this, Id);

    /// <summary>
    /// Occurs when the delete button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void Delete(Gtk.Button sender, EventArgs e) => DeleteTriggered?.Invoke(this, Id);
}