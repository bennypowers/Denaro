﻿using NickvisionMoney.Shared.Events;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController : IDisposable
{
    private bool _disposed;
    private string? _fileToLaunch;
    private List<AccountViewController> _openAccounts;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;
    /// <summary>
    /// The list of recent accounts
    /// </summary>
    public List<RecentAccount> RecentAccounts => Configuration.Current.RecentAccounts;
    /// <summary>
    /// The number of open accounts
    /// </summary>
    public int NumberOfOpenAccounts => _openAccounts.Count;
    /// <summary>
    /// A function for getting a password for an account
    /// </summary>
    public Func<string, Task<string?>>? AccountLoginAsync;

    /// <summary>
    /// Occurs when a notification is sent
    /// </summary>
    public event EventHandler<NotificationSentEventArgs>? NotificationSent;
    /// <summary>
    /// Occurs when an account is added
    /// </summary>
    public event EventHandler? AccountAdded;
    /// <summary>
    /// Occurs when the recent accounts list is changed
    /// </summary>
    public event EventHandler? RecentAccountsChanged;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        _disposed = false;
        _fileToLaunch = null;
        _openAccounts = new List<AccountViewController>();
        Localizer = new Localizer();
    }

    /// <summary>
    /// Whether or not to show a sun icon on the home page
    /// </summary>
    public bool ShowSun
    {
        get
        {
            var timeNowHours = DateTime.Now.Hour;
            return timeNowHours >= 6 && timeNowHours < 18;
        }
    }

    /// <summary>
    /// The string for greeting on the home page
    /// </summary>
    public string Greeting
    {
        get
        {
            var greeting = DateTime.Now.Hour switch
            {
                >= 0 and < 6 => "Night",
                < 12 => "Morning",
                < 18 => "Afternoon",
                < 24 => "Evening",
                _ => "Generic"
            };
            return Localizer["Greeting", greeting];
        }
    }

    /// <summary>
    /// A file to launch when the window is loaded
    /// </summary>
    public string? FileToLaunch
    {
        get => _fileToLaunch;

        set => _fileToLaunch = (Path.Exists(value) && Path.GetExtension(value).ToLower() == ".nmoney") ? value : null;
    }

    /// <summary>
    /// Frees resources used by the MainWindowController object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the MainWindowController object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Localizer.Dispose();
            foreach (var controller in _openAccounts)
            {
                controller.Dispose();
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController(RecentAccountsChanged, Localizer);

    /// <summary>
    /// Creates a new DashboardViewController
    /// </summary>
    /// <returns>The DashboardViewController</returns>
    public DashboardViewController CreateDashboardViewController() => new DashboardViewController(_openAccounts, Localizer, Configuration.Current.GroupDefaultColor);

    /// <summary>
    /// Gets an AccountViewController for the most recent account
    /// </summary>
    /// <returns>The AccountViewController</returns>
    public AccountViewController GetMostRecentAccountViewController() => _openAccounts[_openAccounts.Count - 1];

    /// <summary>
    /// Creates an AccountViewController for the specified path
    /// </summary>
    /// <param name="path">The path of the open account</param>
    /// <returns>The AccountViewController or null if the account path is not open</returns>
    public AccountViewController? CreateAccountViewController(string path)
    {
        try
        {
            return _openAccounts[_openAccounts.FindIndex(x => x.AccountPath == path)];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a color for an account type
    /// </summary>
    /// <param name="accountType">The account type</param>
    /// <returns>The rgb color for the account type</returns>
    public string GetColorForAccountType(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Checking => Configuration.Current.AccountCheckingColor,
            AccountType.Savings => Configuration.Current.AccountSavingsColor,
            AccountType.Business => Configuration.Current.AccountBusinessColor,
            _ => Configuration.Current.AccountSavingsColor
        };
    }

    /// <summary>
    /// Gets whether or not an account with the given path is opened or not
    /// </summary>
    /// <param name="path">The path of the account to check</param>
    /// <returns>True if the account is open, else false</returns>
    public bool IsAccountOpen(string path) => _openAccounts.Any(x => x.AccountPath == path);

    /// <summary>
    /// Adds an account to the list of opened accounts
    /// </summary>
    /// <param name="path">The path of the account</param>
    /// <param name="showOpenedNotification">Whether or not to show a notification if an account is opened</param>
    /// <param name="password">A password for an account (if available)</param>
    /// <returns>True if account added, else false (account already added)</returns>
    public async Task<bool> AddAccountAsync(string path, bool showOpenedNotification = true, string? password = null)
    {
        if (Path.GetExtension(path).ToLower() != ".nmoney")
        {
            path += ".nmoney";
        }
        if (!_openAccounts.Any(x => x.AccountPath == path))
        {
            var controller = new AccountViewController(path, Localizer, NotificationSent, RecentAccountsChanged);
            controller.TransferSent += OnTransferSent;
            if (controller.AccountNeedsPassword && string.IsNullOrEmpty(password))
            {
                password = await AccountLoginAsync!(controller.AccountPath);
            }
            if (!controller.Login(password))
            {
                controller.Dispose();
                if (password != null)
                {
                    NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["InvalidPassword"], NotificationSeverity.Error));
                }
                return false;
            }
            _openAccounts.Add(controller);
            AccountAdded?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else
        {
            if (showOpenedNotification)
            {
                NotificationSent?.Invoke(this, new NotificationSentEventArgs(Localizer["AccountOpenedAlready"], NotificationSeverity.Warning));
            }
            return false;
        }
    }

    /// <summary>
    /// Closes the account with the provided index
    /// </summary>
    /// <param name="index">int</param>
    public void CloseAccount(int index)
    {
        _openAccounts[index].Dispose();
        _openAccounts.RemoveAt(index);
    }

    /// <summary>
    /// Occurs when a transfer is sent from an account
    /// </summary>
    /// <param name="transfer">The transfer sent</param>
    private async void OnTransferSent(object? sender, Transfer transfer)
    {
        await AddAccountAsync(transfer.DestinationAccountPath, false, transfer.DestinationAccountPassword);
        var controller = _openAccounts.Find(x => x.AccountPath == transfer.DestinationAccountPath)!;
        await controller.StartupAsync();
        await controller.ReceiveTransferAsync(transfer);
    }
}