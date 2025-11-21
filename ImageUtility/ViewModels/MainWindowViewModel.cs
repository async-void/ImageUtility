using Avalonia;
using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageUtility.Features.CustomTheme;
using ImageUtility.Features.Theming;
using ImageUtility.Services;
using ImageUtility.Utilities;
using ImageUtility.Views;
using Microsoft.Extensions.Logging;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Enums;
using SukiUI.MessageBox;
using SukiUI.Models;
using SukiUI.Theme.Shadcn;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ImageUtility.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        public IAvaloniaReadOnlyList<ViewModelBase> Pages { get; }
        public PageNavigationService PageNavigationService { get; }

        public IAvaloniaReadOnlyList<SukiColorTheme> Themes { get; }

        public IAvaloniaReadOnlyList<SukiBackgroundStyle> BackgroundStyles { get; }

        public ISukiToastManager ToastManager { get; }
        public ISukiDialogManager DialogManager { get; }

        [ObservableProperty] private ThemeVariant _baseTheme;
        [ObservableProperty] private ViewModelBase? _activePage;
        [ObservableProperty] private bool _windowLocked;
        [ObservableProperty] private bool _titleBarVisible = true;
        [ObservableProperty] private SukiBackgroundStyle _backgroundStyle = SukiBackgroundStyle.GradientSoft;
        [ObservableProperty] private bool _animationsEnabled;
        [ObservableProperty] private string? _customShaderFile;
        [ObservableProperty] private string? _title;
        [ObservableProperty] private bool _transitionsEnabled;
        [ObservableProperty] private double _transitionTime;

        [ObservableProperty] private bool _showTitleBar = true;
        [ObservableProperty] private bool _showBottomBar = true;

        private readonly SukiTheme _theme;
       // private readonly ThemingViewModel _theming;

        public MainWindowViewModel(IEnumerable<ViewModelBase> demoPages, PageNavigationService pageNavigationService, 
            ISukiToastManager toastManager, ISukiDialogManager dialogManager, ILogger<MainWindowViewModel> logger)
        {
            ToastManager = toastManager;
            DialogManager = dialogManager;
            _logger = logger;
            Pages = new AvaloniaList<ViewModelBase>(demoPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
            PageNavigationService = pageNavigationService;
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            Title =$"Image Utility ver: {version?.Major}.{version?.Minor}.{version?.Build}";
            //_theming = (ThemingViewModel)Pages.First(x => x is ThemingViewModel);
            //_theming.BackgroundStyleChanged += style => BackgroundStyle = style;
            //_theming.BackgroundAnimationsChanged += enabled => AnimationsEnabled = enabled;
            //_theming.CustomBackgroundStyleChanged += shader => CustomShaderFile = shader;
            //_theming.BackgroundTransitionsChanged += enabled => TransitionsEnabled = enabled;

            BackgroundStyles = new AvaloniaList<SukiBackgroundStyle>(Enum.GetValues<SukiBackgroundStyle>());
            _theme = SukiTheme.GetInstance();

            // Subscribe to the navigation service (when a page navigation is requested)
            pageNavigationService.NavigationRequested += pageType =>
            {
                var page = Pages.FirstOrDefault(x => x.GetType() == pageType);
                if (page is null || ActivePage?.GetType() == pageType) return;
                ActivePage = page;
            };

            Themes = _theme.ColorThemes;
            BaseTheme = _theme.ActiveBaseTheme;

            // Subscribe to the base theme changed events
            _theme.OnBaseThemeChanged += variant =>
            {
                BaseTheme = variant;
                ToastManager.CreateSimpleInfoToast()
                    .WithTitle("Theme Changed")
                    .WithContent($"Theme has changed to {variant}.")
                    .Queue();
            };

            // Subscribe to the color theme changed events
            _theme.OnColorThemeChanged += theme => ToastManager.CreateSimpleInfoToast()
                .WithTitle("Color Changed")
                .WithContent($"Color has changed to {theme.DisplayName}.")
                .Queue();
            _logger.LogInformation("Page Navigation Service initialized.");
            _logger.LogInformation("Navigation Pages Loaded.");
            _logger.LogInformation("Theme Service initialized.");
           
        }

        [RelayCommand]
        private void ToggleAnimations()
        {
            AnimationsEnabled = !AnimationsEnabled;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle(AnimationsEnabled ? "Animation Enabled" : "Animation Disabled")
                .WithContent(AnimationsEnabled ? "Background animations are now enabled." : "Background animations are now disabled.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleTransitions()
        {
            TransitionsEnabled = !TransitionsEnabled;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle(TransitionsEnabled ? "Transitions Enabled" : "Transitions Disabled")
                .WithContent(TransitionsEnabled ? "Background transitions are now enabled." : "Background transitions are now disabled.")
                .Queue();
        }

        [RelayCommand]
        private async Task OnShowAbout()
        {
            var msgBox = new SukiMessageBoxHost
            {
                ActionButtonsPreset = SukiMessageBoxButtons.OK,
                ShowHeaderContentSeparator = true,
                IconPreset = SukiMessageBoxIcons.Information,
                Header = "About Image Utility",
                Content = "Image Utility is a simple application for basic image renaming, resizing & conversion tasks.\n\n" +
                          "see Help for more details\r\nDeveloped by Suncoast Software.\n\n" +
                          "© 2026 All rights reserved."
            };
            await SukiMessageBox.ShowDialog(msgBox);
        }

        [RelayCommand]
        private void ToggleBaseTheme() =>
            _theme.SwitchBaseTheme();

        public void ChangeTheme(SukiColorTheme theme) =>
            _theme.ChangeColorTheme(theme);

        [RelayCommand]
        private void ShadCnMode()
        {
            try
            {
                var msgBox = new SukiMessageBoxHost
                {
                    ActionButtonsPreset = SukiMessageBoxButtons.OK,
                    IconPreset = SukiMessageBoxIcons.Error,
                    Header = "Unsupported",
                    Content = "Shadcn theme is currently not supported in this version."
                };
                using var _ = SukiMessageBox.ShowDialog(msgBox);
                //Shadcn.Configure(Application.Current, Application.Current.ActualThemeVariant);
            }
            catch
            {
              _logger.LogError("Failed to apply Shadcn theme.");
            }
           
        }

        [RelayCommand]
        private void CreateCustomTheme()
        {
            DialogManager.CreateDialog()
                .WithViewModel(dialog => new CustomThemeDialogViewModel(_theme, dialog))
                .TryShow();
        }

        [RelayCommand]
        private void ToggleWindowLock()
        {
            WindowLocked = !WindowLocked;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle($"Window {(WindowLocked ? "Locked" : "Unlocked")}")
                .WithContent($"Window has been {(WindowLocked ? "locked" : "unlocked")}.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleTitleBackground()
        {
            ShowTitleBar = !ShowTitleBar;
            ShowBottomBar = !ShowBottomBar;
        }

        [RelayCommand]
        private void ToggleTitleBar()
        {
            TitleBarVisible = !TitleBarVisible;
            ToastManager.CreateSimpleInfoToast()
                .WithTitle($"Title Bar {(TitleBarVisible ? "Visible" : "Hidden")}")
                .WithContent($"Window title bar has been {(TitleBarVisible ? "shown" : "hidden")}.")
                .Queue();
        }

        [RelayCommand]
        private void ToggleRightToLeft() => _theme.IsRightToLeft = !_theme.IsRightToLeft;

        [RelayCommand]
        private static void OpenUrl(string url) => UrlUtilities.OpenUrl(url);

        //partial void OnBackgroundStyleChanged(SukiBackgroundStyle value) =>
        //    _theming.BackgroundStyle = value;

        //partial void OnAnimationsEnabledChanged(bool value) =>
        //    _theming.BackgroundAnimations = value;

        //partial void OnTransitionsEnabledChanged(bool value) =>
        //    _theming.BackgroundTransitions = value;
    }
}
