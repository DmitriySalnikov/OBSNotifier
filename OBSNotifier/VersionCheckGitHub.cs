using Newtonsoft.Json;
using OBSNotifier;
using System;
using System.Diagnostics;
using System.Net;
using System.Windows;

internal class VersionCheckerGitHub : IDisposable
{
    public Version SkipVersion = null;
    /// <summary>
    /// Emits after some MessageBoxes have been shown, and returned the result.
    /// </summary>
    public event EventHandler<ShowMessageBoxEventData> MessageBoxShown;
    /// <summary>
    /// Emits after the user has chosen to skip the version. Also this class will automatically update <see cref="SkipVersion"/>
    /// </summary>
    public event EventHandler<VersionSkipByUserData> VersionSkippedByUser;

    WebClient updateClient = null;
    bool _isSilentCheck = true;

    readonly Uri githubLink;
    readonly string appName;

    /// <summary>
    /// Init version checker with <paramref name="profile"/> name and <paramref name="repo"/> name.
    /// It will be formated like this: <code>$"https://api.github.com/repos/{profile}/{repo}/releases/latest"</code>
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="repo"></param>
    public VersionCheckerGitHub(string profile, string repo, string appName)
    {
        githubLink = new Uri($"https://api.github.com/repos/{profile}/{repo}/releases/latest");
        this.appName = appName;
    }

    public void CheckForUpdates(bool isSilentCheck = false)
    {
        // Skip if currently checking
        if (updateClient != null)
            return;

        updateClient = new WebClient();
        updateClient.DownloadStringCompleted += UpdateClient_DownloadStringCompleted;
        updateClient.Headers.Add("Content-Type", "application/json");
        updateClient.Headers.Add("User-Agent", appName);

        try
        {
            _isSilentCheck = isSilentCheck;
            updateClient.DownloadStringAsync(githubLink);
        }
        catch (Exception ex)
        {
            if (!_isSilentCheck)
                ShowMessageBox($"{Utils.Tr("message_box_version_check_failed_request")}\n{ex.Message}");
            ClearUpdateData();
        }
    }

    private void UpdateClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
    {
        if (e.Error is WebException webExp)
        {
            if (!_isSilentCheck)
                ShowMessageBox($"{Utils.Tr("message_box_version_check_failed_parse_info")}\n{webExp.Message}");

            ClearUpdateData();
            return;
        }

        try
        {
            dynamic resultObject = JsonConvert.DeserializeObject(e.Result);
            Version newVersion = new Version(resultObject.tag_name.Value);
            Version currentVersion = new Version(System.Windows.Forms.Application.ProductVersion);
            string updateUrl = resultObject.html_url.Value;

            // Skip if the new version matches the skip version, or don't skip if checking manually
            if (newVersion != SkipVersion || !_isSilentCheck)
            {
                // New release
                if (newVersion > currentVersion)
                {
                    var updateDialog = ShowMessageBox(Utils.TrFormat("message_box_version_check_new_version_available", System.Windows.Forms.Application.ProductVersion, newVersion), Utils.TrFormat("message_box_version_check_new_version_available_title", appName), MessageBoxButton.YesNoCancel);
                    if (updateDialog == MessageBoxResult.Yes)
                    {
                        // Open the download page
                        Process.Start(updateUrl);
                    }
                    else if (updateDialog == MessageBoxResult.No)
                    {
                        SkipVersion = newVersion;
                        VersionSkippedByUser?.Invoke(this, new VersionSkipByUserData(newVersion));
                    }
                }
                else
                {
                    // Don't show this on startup
                    if (!_isSilentCheck)
                    {
                        ShowMessageBox(Utils.TrFormat("message_box_version_check_latest_version", System.Windows.Forms.Application.ProductVersion));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Don't show this on startup
            if (!_isSilentCheck)
            {
                ShowMessageBox($"{Utils.Tr("message_box_version_check_failed_to_check")}\n{ex.Message}");
            }
        }

        ClearUpdateData();
    }

    void ClearUpdateData()
    {
        updateClient?.Dispose();
        updateClient = null;
        SkipVersion = null;

        MessageBoxShown = null;
        VersionSkippedByUser = null;
    }
    public void Dispose()
    {
        ClearUpdateData();
    }

    MessageBoxResult ShowMessageBox(string messageBoxText, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
    {
        MessageBoxResult res = MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
        MessageBoxShown?.Invoke(this, new ShowMessageBoxEventData(messageBoxText, caption, button, icon, defaultResult, options, res));
        return res;
    }

    public class ShowMessageBoxEventData : EventArgs
    {
        public string Message { get; set; }
        public string Caption { get; set; }
        public MessageBoxButton Button { get; set; }
        public MessageBoxImage Icon { get; set; }
        public MessageBoxResult DefaultResult { get; set; }
        public MessageBoxOptions Options { get; set; }
        public MessageBoxResult Result { get; set; }

        public ShowMessageBoxEventData(string message, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options, MessageBoxResult result)
        {
            Message = message;
            Caption = caption;
            Button = button;
            Icon = icon;
            DefaultResult = defaultResult;
            Options = options;
            Result = result;
        }
    }

    public class VersionSkipByUserData : EventArgs
    {
        public Version SkippedVersion;

        public VersionSkipByUserData(Version skippedVersion)
        {
            SkippedVersion = skippedVersion;
        }
    }
}
