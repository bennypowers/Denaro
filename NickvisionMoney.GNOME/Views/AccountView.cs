﻿using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using System;
using System.IO;
using System.Collections.Generic;

namespace NickvisionMoney.GNOME.Views;

public class AccountView
{
    private readonly AccountViewController _controller;
    private bool _accountLoading;

    public Adw.TabPage Page;
    private readonly Adw.Flap _flap;
    private readonly Gtk.ScrolledWindow _scrollPane;
    private readonly Gtk.Box _paneBox;
    private readonly Gtk.Label _lblTotal;
    private readonly Adw.ActionRow _rowTotal;
    private readonly Gtk.Label _lblIncome;
    private readonly Gtk.CheckButton _chkIncome;
    private readonly Adw.ActionRow _rowIncome;
    private readonly Gtk.Label _lblExpense;
    private readonly Gtk.CheckButton _chkExpense;
    private readonly Adw.ActionRow _rowExpense;
    private readonly Gtk.Box _boxButtonsOverview;
    private readonly Gtk.MenuButton _btnMenuAccountActions;
    private readonly Adw.ButtonContent _btnMenuAccountActionsContent;
    private readonly Gtk.Button _btnResetOverviewFilter;
    private readonly Adw.PreferencesGroup _grpOverview;
    private readonly Gtk.Box _boxButtonsGroups;
    private readonly Gtk.Button _btnNewGroup;
    private readonly Adw.ButtonContent _btnNewGroupContent;
    private readonly Gtk.Button _btnResetGroupsFilter;
    private readonly Adw.PreferencesGroup _grpGroups;
    private readonly Gtk.Calendar _calendar;
    private readonly Gtk.Button _btnResetCalendarFilter;
    private readonly Gtk.DropDown _ddStartYear;
    private readonly Gtk.DropDown _ddStartMonth;
    private readonly Gtk.DropDown _ddStartDay;
    private readonly Gtk.DropDown _ddEndYear;
    private readonly Gtk.DropDown _ddEndMonth;
    private readonly Gtk.DropDown _ddEndDay;
    private readonly Gtk.Box _boxStartRange;
    private readonly Gtk.Box _boxEndRange;
    private readonly Adw.ActionRow _rowStartRange;
    private readonly Adw.ActionRow _rowEndRange;
    private readonly Adw.PreferencesGroup _grpRange;
    private readonly Adw.ExpanderRow _expRange;
    private readonly Adw.PreferencesGroup _grpCalendar;

    public AccountView(Gtk.Window parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle, AccountViewController controller)
    {
        _controller = controller;
        //Flap
        _flap = Adw.Flap.New();
        btnFlapToggle.BindProperty("active", _flap, "reveal-flap", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate));
        //Left Pane
        _scrollPane = Gtk.ScrolledWindow.New();
        _scrollPane.AddCssClass("background");
        _scrollPane.SetSizeRequest(350, -1);
        _flap.SetFlap(_scrollPane);
        //Pane Box
        _paneBox = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        _paneBox.SetHexpand(false);
        _paneBox.SetVexpand(true);
        _paneBox.SetMarginTop(10);
        _paneBox.SetMarginStart(10);
        _paneBox.SetMarginEnd(10);
        _paneBox.SetMarginBottom(10);
        _scrollPane.SetChild(_paneBox);
        //Account Total
        _lblTotal = Gtk.Label.New("");
        _lblTotal.SetValign(Gtk.Align.Center);
        _lblTotal.AddCssClass("accent");
        _lblTotal.AddCssClass("money-total");
        _rowTotal = Adw.ActionRow.New();
        _rowTotal.SetTitle(_controller.Localizer["Total"]);
        _rowTotal.AddSuffix(_lblTotal);
        //Account Income
        _lblIncome = Gtk.Label.New("");
        _lblIncome.SetValign(Gtk.Align.Center);
        _lblIncome.AddCssClass("success");
        _lblTotal.AddCssClass("money-income");
        _chkIncome = Gtk.CheckButton.New();
        _chkIncome.SetActive(true);
        _chkIncome.AddCssClass("selection-mode");
        //_chkIncome.OnToggled += () => _controller.UpdateFilterValue(-3, _chkIncome.GetActive());
        _rowIncome = Adw.ActionRow.New();
        _rowIncome.SetTitle(_controller.Localizer["Income"]);
        _rowIncome.AddPrefix(_chkIncome);
        _rowIncome.AddSuffix(_lblIncome);
        //Account Expense
        _lblExpense = Gtk.Label.New("");
        _lblExpense.SetValign(Gtk.Align.Center);
        _lblExpense.AddCssClass("error");
        _lblExpense.AddCssClass("money-expense");
        _chkExpense = Gtk.CheckButton.New();
        _chkExpense.SetActive(true);
        _chkExpense.AddCssClass("selection-mode");
        //_chkExpense.OnToggled += () => _controller.UpdateFilterValue(-3, _chkExpense.GetActive());
        _rowExpense = Adw.ActionRow.New();
        _rowExpense.SetTitle(_controller.Localizer["Expense"]);
        _rowExpense.AddPrefix(_chkExpense);
        _rowExpense.AddSuffix(_lblExpense);
        //Overview Buttons Box
        _boxButtonsOverview = Gtk.Box.New(Gtk.Orientation.Horizontal, 0);
        //Button Menu Account Actions
        _btnMenuAccountActions = Gtk.MenuButton.New();
        _btnMenuAccountActions.AddCssClass("flat");
        _btnMenuAccountActionsContent = Adw.ButtonContent.New();
        _btnMenuAccountActionsContent.SetIconName("document-properties-symbolic");
        _btnMenuAccountActionsContent.SetLabel(_controller.Localizer["AccountActions", "GTK"]);
        _btnMenuAccountActions.SetChild(_btnMenuAccountActionsContent);
        var menuActionsCsv = Gio.Menu.New();
        menuActionsCsv.Append(_controller.Localizer["ExportToFile"], "account.exportToFile");
        menuActionsCsv.Append(_controller.Localizer["ImportFromFile"], "account.importFromFile");
        var menuActions = Gio.Menu.New();
        menuActions.Append(_controller.Localizer["TransferMoney"], "account.transferMoney");
        menuActions.AppendSection(null, menuActionsCsv);
        _btnMenuAccountActions.SetMenuModel(menuActions);
        _boxButtonsOverview.Append(_btnMenuAccountActions);
        //Button Reset Overview Filter
        _btnResetOverviewFilter = Gtk.Button.NewFromIconName("larger-brush-symbolic");
        _btnResetOverviewFilter.AddCssClass("flat");
        _btnResetOverviewFilter.SetTooltipText(_controller.Localizer["ResetFilters", "Overview"]);
        _btnResetOverviewFilter.OnClicked += OnResetOverviewFilter;
        _boxButtonsOverview.Append(_btnResetOverviewFilter);
        //Overview Group
        _grpOverview = Adw.PreferencesGroup.New();
        _grpOverview.SetTitle(_controller.Localizer["Overview"]);
        _grpOverview.Add(_rowTotal);
        _grpOverview.Add(_rowIncome);
        _grpOverview.Add(_rowExpense);
        _grpOverview.SetHeaderSuffix(_boxButtonsOverview);
        _paneBox.Append(_grpOverview);
        //Group Buttons Box
        _boxButtonsGroups = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        //Button New Group
        _btnNewGroup = Gtk.Button.New();
        _btnNewGroup.AddCssClass("flat");
        _btnNewGroupContent = Adw.ButtonContent.New();
        _btnNewGroupContent.SetIconName("list-add-symbolic");
        _btnNewGroupContent.SetLabel(_controller.Localizer["NewGroup"]);
        _btnNewGroup.SetChild(_btnNewGroupContent);
        _btnNewGroup.SetTooltipText(_controller.Localizer["NewGroup", "Tooltip"]);
        _btnNewGroup.SetDetailedActionName("account.newGroup");
        _boxButtonsGroups.Append(_btnNewGroup);
        //Button Reset Groups Filter
        _btnResetGroupsFilter = Gtk.Button.NewFromIconName("larger-brush-symbolic");
        _btnResetGroupsFilter.AddCssClass("flat");
        _btnResetGroupsFilter.SetTooltipText(_controller.Localizer["ResetFilters", "Groups"]);
        _btnResetGroupsFilter.OnClicked += OnResetGroupsFilter;
        _boxButtonsGroups.Append(_btnResetGroupsFilter);
        //Groups Group
        _grpGroups = Adw.PreferencesGroup.New();
        _grpGroups.SetTitle(_controller.Localizer["Groups"]);
        _grpGroups.SetHeaderSuffix(_boxButtonsGroups);
        _paneBox.Append(_grpGroups);
        //Calendar Widget
        _calendar = Gtk.Calendar.New();
        _calendar.SetName("calendarAccount");
        _calendar.AddCssClass("card");
        _calendar.OnPrevMonth += OnCalendarMonthYearChanged;
        _calendar.OnPrevYear += OnCalendarMonthYearChanged;
        _calendar.OnNextMonth += OnCalendarMonthYearChanged;
        _calendar.OnNextYear += OnCalendarMonthYearChanged;
        _calendar.OnDaySelected += OnCalendarSelectedDateChanged;
        //Button Reset Calendar Filter
        _btnResetCalendarFilter = Gtk.Button.NewFromIconName("larger-brush-symbolic");
        _btnResetCalendarFilter.AddCssClass("flat");
        _btnResetCalendarFilter.SetTooltipText(_controller.Localizer["ResetFilters", "Calendar"]);
        _btnResetCalendarFilter.OnClicked += OnResetCalendarFilter;
        //Start Range DropDowns
        _ddStartYear = Gtk.DropDown.NewFromStrings(new string[1] { "2022" });
        _ddStartYear.SetValign(Gtk.Align.Center);
        _ddStartYear.SetShowArrow(false);
        //g_signal_connect(_ddStartYear, "notify::selected", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeStartYearChanged(); }), this);
        _ddStartMonth = Gtk.DropDown.NewFromStrings(new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        _ddStartMonth.SetValign(Gtk.Align.Center);
        _ddStartMonth.SetShowArrow(false);
        //g_signal_connect(m_ddStartMonth, "notify::selected", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeStartMonthChanged(); }), this);
        _ddStartDay = Gtk.DropDown.NewFromStrings(new string[31]{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" });
        _ddStartDay.SetValign(Gtk.Align.Center);
        _ddStartDay.SetShowArrow(false);
        //g_signal_connect(m_ddStartDay, "notify::selected", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeStartDayChanged(); }), this);
        //End Range DropDowns
        _ddEndYear = Gtk.DropDown.NewFromStrings(new string[1] { "2022" });
        _ddEndYear.SetValign(Gtk.Align.Center);
        _ddEndYear.SetShowArrow(false);
        //g_signal_connect(m_ddEndYear, "notify::selected", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeEndYearChanged(); }), this);
        _ddEndMonth = Gtk.DropDown.NewFromStrings(new string[12] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        _ddEndMonth.SetValign(Gtk.Align.Center);
        _ddEndMonth.SetShowArrow(false);
        //g_signal_connect(m_ddEndMonth, "notify::selected", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeEndMonthChanged(); }), this);
        _ddEndDay = Gtk.DropDown.NewFromStrings(new string[31]{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" });
        _ddEndDay.SetValign(Gtk.Align.Center);
        _ddEndDay.SetShowArrow(false);
        //g_signal_connect(m_ddEndDay, "notify::selected", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeEndDayChanged(); }), this);
        //Start Range Boxes
        _boxStartRange = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxStartRange.Append(_ddStartYear);
        _boxStartRange.Append(_ddStartMonth);
        _boxStartRange.Append(_ddStartDay);
        //End Range Boxes
        _boxEndRange = Gtk.Box.New(Gtk.Orientation.Horizontal, 6);
        _boxEndRange.Append(_ddEndYear);
        _boxEndRange.Append(_ddEndMonth);
        _boxEndRange.Append(_ddEndDay);
        //Start Range Row
        _rowStartRange = Adw.ActionRow.New();
        _rowStartRange.SetTitle(controller.Localizer["Start", "DateRange"]);
        _rowStartRange.AddSuffix(_boxStartRange);
        //End Range Row
        _rowEndRange = Adw.ActionRow.New();
        _rowEndRange.SetTitle(_controller.Localizer["End", "DateRange"]);
        _rowEndRange.AddSuffix(_boxEndRange);
        //Select Range Group
        _grpRange = Adw.PreferencesGroup.New();
        //Expander Row Select Range
        _expRange = Adw.ExpanderRow.New();
        _expRange.SetTitle(_controller.Localizer["SelectRange"]);
        _expRange.SetEnableExpansion(false);
        _expRange.SetShowEnableSwitch(true);
        _expRange.AddRow(_rowStartRange);
        _expRange.AddRow(_rowEndRange);
        //g_signal_connect(m_expRange, "notify::enable-expansion", G_CALLBACK((void (*)(GObject*, GParamSpec*, gpointer))[](GObject*, GParamSpec*, gpointer data) { reinterpret_cast<AccountView*>(data)->onDateRangeToggled(); }), this);
        _grpRange.Add(_expRange);
        //Calendar Group
        _grpCalendar = Adw.PreferencesGroup.New();
        _grpCalendar.SetTitle(_controller.Localizer["Calendar"]);
        _grpCalendar.SetHeaderSuffix(_btnResetCalendarFilter);
        _grpCalendar.Add(_calendar);
        _paneBox.Append(_grpCalendar);
        _paneBox.Append(_grpRange);
        //Separator
        _flap.SetSeparator(Gtk.Separator.New(Gtk.Orientation.Vertical));

        //Tab Page
        Page = parentTabView.Append(_flap);
        Page.SetTitle(_controller.AccountTitle);

        OnAccountInfoChanged();
    }

    private void OnAccountInfoChanged()
    {
        _accountLoading = true;
        //Overview
        _lblTotal.SetLabel(_controller.AccountTotalString);
        _lblIncome.SetLabel(_controller.AccountIncomeString);
        _lblExpense.SetLabel(_controller.AccountExpenseString);

        _accountLoading = false;
    }

    private void OnResetOverviewFilter(Gtk.Button sender, EventArgs e)
    {
        //TODO
    }

    private void OnResetGroupsFilter(Gtk.Button sender, EventArgs e)
    {
        //TODO
    }

    private void OnCalendarMonthYearChanged(Gtk.Calendar sender, EventArgs e)
    {
        //TODO
    }

    private void OnCalendarSelectedDateChanged(Gtk.Calendar sender, EventArgs e)
    {
        //TODO
    }

    private void OnResetCalendarFilter(Gtk.Button sender, EventArgs e)
    {
        //TODO
    }
}