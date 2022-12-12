﻿using NickvisionMoney.GNOME.Views;
using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;

namespace NickvisionMoney.GNOME;

/// <summary>
/// The Program
/// </summary>
public class Program
{
    private readonly Adw.Application _application;

    /// <summary>
    /// Main method
    /// </summary>
    /// <param name="args">string[]</param>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public static int Main(string[] args) => new Program().Run();

    /// <summary>
    /// Constructs a Program
    /// </summary>
    public Program()
    {
        Adw.Module.Initialize();
        _application = Adw.Application.New("org.nickvision.money", Gio.ApplicationFlags.FlagsNone);
        _application.OnActivate += OnActivate;
    }

    /// <summary>
    /// Runs the program
    /// </summary>
    /// <returns>Return code from Adw.Application.Run()</returns>
    public int Run() => _application.Run();

    /// <summary>
    /// Occurs when the application is activated
    /// </summary>
    /// <param name="sedner">Gio.Application</param>
    /// <param name="e">EventArgs</param>
    private void OnActivate(Gio.Application sedner, EventArgs e)
    {
        //Controller Setup
        var mainWindowController = new MainWindowController();
        mainWindowController.AppInfo.ID = "org.nickvision.money";
        mainWindowController.AppInfo.Name = "Nickvision Money";
        mainWindowController.AppInfo.ShortName = "Money";
        mainWindowController.AppInfo.Description = mainWindowController.Localizer["Description"];
        mainWindowController.AppInfo.Version = "2022.12.0-next";
        mainWindowController.AppInfo.Changelog = "<ul><li>Initial Release</li></ul>";
        mainWindowController.AppInfo.GitHubRepo = new Uri("https://github.com/nlogozzo/NickvisionMoney");
        mainWindowController.AppInfo.IssueTracker = new Uri("https://github.com/nlogozzo/NickvisionMoney/issues/new");
        mainWindowController.AppInfo.SupportUrl = new Uri("https://github.com/nlogozzo/NickvisionMoney/discussions");
        //Set Adw Theme
        _application.StyleManager!.ColorScheme = mainWindowController.Theme switch
        {
            Theme.System => Adw.ColorScheme.PreferLight,
            Theme.Light => Adw.ColorScheme.ForceLight,
            Theme.Dark => Adw.ColorScheme.ForceDark,
            _ => Adw.ColorScheme.PreferLight
        };
        //Main Window
        var mainWindow = new MainWindow(mainWindowController, _application);
        _application.AddWindow(mainWindow);
        mainWindow.Start();
    }
}
