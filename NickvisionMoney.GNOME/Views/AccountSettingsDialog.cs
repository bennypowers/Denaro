using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// A dialog to configure account
/// </summary>
public partial class AccountSettingsDialog
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_css_provider_load_from_data(nint provider, string data, int length);

    private bool _constructing;
    private readonly AccountSettingsDialogController _controller;
    private readonly Adw.MessageDialog _dialog;
    private readonly Gtk.Box _boxMain;
    private readonly Gtk.Box _boxHeader;
    private readonly Gtk.Label _lblTitle;
    private readonly Adw.PreferencesGroup _grpAccount;
    private readonly Adw.EntryRow _rowName;
    private readonly Adw.ComboRow _rowAccountType;
    private readonly Gtk.ToggleButton _btnIncome;
    private readonly Gtk.ToggleButton _btnExpense;
    private readonly Gtk.Box _boxTypeButtons;
    private readonly Adw.ActionRow _rowTransactionType;
    private readonly Gtk.Label _lblReportedCurrency;
    private readonly Adw.PreferencesGroup _grpCurrency;
    private readonly Adw.ExpanderRow _rowCustomCurrency;
    private readonly Gtk.Entry _txtCustomSymbol;
    private readonly Adw.ActionRow _rowCustomSymbol;
    private readonly Gtk.Entry _txtCustomCode;
    private readonly Adw.ActionRow _rowCustomCode;
    private readonly Adw.PreferencesGroup _grpPassword;
    private readonly Adw.ExpanderRow _rowPassword;
    private readonly Adw.PasswordEntryRow _rowNewPassword;
    private readonly Adw.PasswordEntryRow _rowNewPasswordConfirm;
    private readonly Adw.ActionRow _rowRemovePassword;
    private readonly Gtk.Button _btnRemovePassword;

    /// <summary>
    /// Constructs an AccountSettingsDialog
    /// </summary>
    /// <param name="controller">AccountSettingsDialogController</param>
    /// <param name="parentWindow">Gtk.Window</param>
    public AccountSettingsDialog(AccountSettingsDialogController controller, Gtk.Window parentWindow)
    {
        _constructing = true;
        _controller = controller;
        //Dialog Settings
        _dialog = Adw.MessageDialog.New(parentWindow, "", "");
        _dialog.SetDefaultSize(450, -1);
        _dialog.SetHideOnClose(true);
        _dialog.SetModal(true);
        if (!_controller.NeedsSetup)
        {
            _dialog.AddResponse("cancel", _controller.Localizer["Cancel"]);
            _dialog.SetCloseResponse("cancel");
        }
        _dialog.AddResponse("ok", _controller.Localizer["Apply"]);
        _dialog.SetDefaultResponse("ok");
        _dialog.SetResponseAppearance("ok", Adw.ResponseAppearance.Suggested);
        _dialog.OnResponse += (sender, e) => _controller.Accepted = e.Response == "ok";
        //Main Box
        _boxMain = Gtk.Box.New(Gtk.Orientation.Vertical, 16);
        //Header Box
        _boxHeader = Gtk.Box.New(Gtk.Orientation.Horizontal, 4);
        _boxMain.Append(_boxHeader);
        //Title
        _lblTitle = Gtk.Label.New(_controller.Localizer["AccountSettings"]);
        _lblTitle.SetHexpand(true);
        _lblTitle.SetMarginStart(4);
        _lblTitle.SetHalign(Gtk.Align.Start);
        _lblTitle.SetValign(Gtk.Align.Center);
        _lblTitle.AddCssClass("title-1");
        _boxHeader.Append(_lblTitle);
        //Preferences Group
        _grpAccount = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpAccount);
        //Account Name
        _rowName = Adw.EntryRow.New();
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _rowName.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _grpAccount.Add(_rowName);
        //Account Type
        _rowAccountType = Adw.ComboRow.New();
        _rowAccountType.SetModel(Gtk.StringList.New(new string[3] { _controller.Localizer["AccountType", "Checking"], _controller.Localizer["AccountType", "Savings"], _controller.Localizer["AccountType", "Business"] }));
        _rowAccountType.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                Validate();
            }
        };
        _rowAccountType.SetTitle(_controller.Localizer["AccountType", "Field"]);
        _rowAccountType.SetSubtitle(_controller.Localizer["AccountType", "Description"]);
        _rowAccountType.SetSubtitleLines(4);
        _grpAccount.Add(_rowAccountType);
        //Default Transaction Type
        _btnIncome = Gtk.ToggleButton.NewWithLabel(_controller.Localizer["Income"]);
        _btnIncome.OnToggled += OnTransactionTypeChanged;
        _btnExpense = Gtk.ToggleButton.NewWithLabel(_controller.Localizer["Expense"]);
        _btnExpense.OnToggled += OnTransactionTypeChanged;
        _btnExpense.BindProperty("active", _btnIncome, "active", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate | GObject.BindingFlags.InvertBoolean));
        _boxTypeButtons = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        _boxTypeButtons.SetValign(Gtk.Align.Center);
        _boxTypeButtons.AddCssClass("linked");
        _boxTypeButtons.Append(_btnIncome);
        _boxTypeButtons.Append(_btnExpense);
        _rowTransactionType = Adw.ActionRow.New();
        _rowTransactionType.SetTitle(_controller.Localizer["DefaultTransactionType", "Field"]);
        _rowTransactionType.AddSuffix(_boxTypeButtons);
        _grpAccount.Add(_rowTransactionType);
        //Reported Currency
        _lblReportedCurrency = Gtk.Label.New($"{_controller.Localizer["ReportedCurrency"]}\n<b>{_controller.ReportedCurrencyString}</b>");
        _lblReportedCurrency.SetUseMarkup(true);
        _lblReportedCurrency.SetJustify(Gtk.Justification.Center);
        _boxMain.Append(_lblReportedCurrency);
        //Custom Currency
        _grpCurrency = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpCurrency);
        _rowCustomCurrency = Adw.ExpanderRow.New();
        _rowCustomCurrency.SetTitle(_controller.Localizer["UseCustomCurrency", "Field"]);
        _rowCustomCurrency.SetShowEnableSwitch(true);
        _rowCustomCurrency.SetEnableExpansion(false);
        _rowCustomCurrency.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "enable-expansion")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _grpCurrency.Add(_rowCustomCurrency);
        _txtCustomSymbol = Gtk.Entry.New();
        _txtCustomSymbol.SetValign(Gtk.Align.Center);
        _txtCustomSymbol.SetMaxLength(3);
        _txtCustomSymbol.SetPlaceholderText(_controller.Localizer["CustomCurrencySymbol", "Placeholder"]);
        _txtCustomSymbol.SetActivatesDefault(true);
        _txtCustomSymbol.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _rowCustomSymbol = Adw.ActionRow.New();
        _rowCustomSymbol.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Field"]);
        _rowCustomSymbol.AddSuffix(_txtCustomSymbol);
        _rowCustomCurrency.AddRow(_rowCustomSymbol);
        _txtCustomCode = Gtk.Entry.New();
        _txtCustomCode.SetValign(Gtk.Align.Center);
        _txtCustomCode.SetMaxLength(3);
        _txtCustomCode.SetPlaceholderText(_controller.Localizer["CustomCurrencyCode", "Placeholder"]);
        _txtCustomCode.SetActivatesDefault(true);
        _txtCustomCode.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _rowCustomCode = Adw.ActionRow.New();
        _rowCustomCode.SetTitle(_controller.Localizer["CustomCurrencyCode", "Field"]);
        _rowCustomCode.AddSuffix(_txtCustomCode);
        _rowCustomCurrency.AddRow(_rowCustomCode);
        //Password Row
        _grpPassword = Adw.PreferencesGroup.New();
        _boxMain.Append(_grpPassword);
        _rowPassword = Adw.ExpanderRow.New();
        _rowPassword.SetTitle(_controller.Localizer["ManagePassword"]);
        _rowPassword.SetSubtitle(_controller.Localizer["ManagePassword", "Description"]);
        _rowPassword.SetIconName("dialog-password-symbolic");
        _rowPassword.SetShowEnableSwitch(true);
        _rowPassword.SetEnableExpansion(false);
        _rowPassword.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "enable-expansion")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _grpPassword.Add(_rowPassword);
        _rowNewPassword = Adw.PasswordEntryRow.New();
        _rowNewPassword.SetTitle(_controller.Localizer["NewPassword", "Field"]);
        _rowNewPassword.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _rowPassword.AddRow(_rowNewPassword);
        _rowNewPasswordConfirm = Adw.PasswordEntryRow.New();
        _rowNewPasswordConfirm.SetTitle(_controller.Localizer["ConfirmPassword", "Field"]);
        _rowNewPasswordConfirm.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                if (!_constructing)
                {
                    Validate();
                }
            }
        };
        _rowPassword.AddRow(_rowNewPasswordConfirm);
        _rowRemovePassword = Adw.ActionRow.New();
        _rowRemovePassword.SetSubtitle(_controller.Localizer["ManagePassword", "Warning"]);
        _rowRemovePassword.AddCssClass("warning");
        _rowPassword.AddRow(_rowRemovePassword);
        _btnRemovePassword = Gtk.Button.NewWithLabel(_controller.Localizer["Remove"]);
        _btnRemovePassword.AddCssClass("destructive-action");
        _btnRemovePassword.SetValign(Gtk.Align.Center);
        _btnRemovePassword.SetVisible(_controller.IsEncrypted);
        _btnRemovePassword.OnClicked += OnRemovePassword;
        _rowRemovePassword.AddSuffix(_btnRemovePassword);
        //Layout
        _dialog.SetExtraChild(_boxMain);
        //Load
        _rowName.SetText(_controller.Metadata.Name);
        _rowAccountType.SetSelected((uint)_controller.Metadata.AccountType);
        _btnIncome.SetActive(_controller.Metadata.DefaultTransactionType == TransactionType.Income);
        _rowCustomCurrency.SetEnableExpansion(_controller.Metadata.UseCustomCurrency);
        _txtCustomSymbol.SetText(_controller.Metadata.CustomCurrencySymbol ?? "");
        _txtCustomCode.SetText(_controller.Metadata.CustomCurrencyCode ?? "");
        Validate();
        _constructing = false;
    }

    public event GObject.SignalHandler<Adw.MessageDialog, Adw.MessageDialog.ResponseSignalArgs> OnResponse
    {
        add
        {
            _dialog.OnResponse += value;
        }
        remove
        {
            _dialog.OnResponse -= value;
        }
    }

    /// <summary>
    /// Shows the dialog
    /// </summary>
    public void Show() => _dialog.Show();

    /// <summary>
    /// Destroys the dialog
    /// </summary>
    public void Destroy() => _dialog.Destroy();

    /// <summary>
    /// Validates the dialog's input
    /// </summary>
    private void Validate()
    {
        var transactionType = _btnIncome.GetActive() ? TransactionType.Income : TransactionType.Expense;
        var newPassword = "";
        var newPasswordConfirm = "";
        if (_rowPassword.GetEnableExpansion())
        {
            newPassword = _rowNewPassword.GetText();
            newPasswordConfirm = _rowNewPasswordConfirm.GetText();
        }
        var checkStatus = _controller.UpdateMetadata(_rowName.GetText(), (AccountType)_rowAccountType.GetSelected(), _rowCustomCurrency.GetEnableExpansion(), _txtCustomSymbol.GetText(), _txtCustomCode.GetText(), transactionType, newPassword, newPasswordConfirm);
        _rowName.RemoveCssClass("error");
        _rowName.SetTitle(_controller.Localizer["Name", "Field"]);
        _rowCustomSymbol.RemoveCssClass("error");
        _rowCustomSymbol.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Field"]);
        _rowCustomCode.RemoveCssClass("error");
        _rowCustomCode.SetTitle(_controller.Localizer["CustomCurrencyCode", "Field"]);
        if (checkStatus == AccountMetadataCheckStatus.Valid)
        {
            _dialog.SetResponseEnabled("ok", true);
        }
        else
        {
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyName))
            {
                _rowName.AddCssClass("error");
                _rowName.SetTitle(_controller.Localizer["Name", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencySymbol))
            {
                _rowCustomSymbol.AddCssClass("error");
                _rowCustomSymbol.SetTitle(_controller.Localizer["CustomCurrencySymbol", "Empty"]);
            }
            if (checkStatus.HasFlag(AccountMetadataCheckStatus.EmptyCurrencyCode))
            {
                _rowCustomCode.AddCssClass("error");
                _rowCustomCode.SetTitle(_controller.Localizer["CustomCurrencyCode", "Empty"]);
            }
            _dialog.SetResponseEnabled("ok", false);
        }
    }

    /// <summary>
    /// Occurs when either Income or Expense button is toggled
    /// </summary>
    /// <param name="sender">Gtk.ToggleButton</param>
    /// <param name="e">EventArgs</param>
    private void OnTransactionTypeChanged(Gtk.ToggleButton sender, EventArgs e)
    {
        if (_btnIncome.GetActive())
        {
            _btnIncome.AddCssClass("denaro-income");
            _btnExpense.RemoveCssClass("denaro-expense");
        }
        else
        {
            _btnIncome.RemoveCssClass("denaro-income");
            _btnExpense.AddCssClass("denaro-expense");
        }
        if (!_constructing)
        {
            Validate();
        }
    }

    /// <summary>
    /// Occurs when the remove password button is clicked
    /// </summary>
    /// <param name="sender">Gtk.Button</param>
    /// <param name="e">EventArgs</param>
    private void OnRemovePassword(Gtk.Button sender, EventArgs e)
    {
        _controller.SetRemovePassword();
        _rowPassword.SetEnableExpansion(false);
        _rowPassword.SetSensitive(false);
        _rowPassword.SetTitle(_controller.Localizer["PasswordRemoveRequest.GTK"]);
        _rowPassword.SetSubtitle("");
    }
}
