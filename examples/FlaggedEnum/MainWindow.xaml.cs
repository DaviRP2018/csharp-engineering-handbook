using System.Diagnostics;
using System.Windows;

namespace FlaggedEnum;

[Flags]
public enum UserPermissions
{
    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    Execute = 1 << 2,
    Admin = 1 << 3,
    Delete = 1 << 4,
    Share = 1 << 5
}

public class User
{
    // Individual booleans for "Traditional" check
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanExecute { get; set; }
    public bool CanAdmin { get; set; }
    public bool CanDelete { get; set; }
    public bool CanShare { get; set; }

    // Flagged enum for "Modern" check
    public UserPermissions Permissions { get; set; }
}

public partial class MainWindow : Window
{
    private const int UserCount = 10_000_000;
    private readonly List<User> _users = new(UserCount);

    public MainWindow()
    {
        InitializeComponent();
        PrepareData();
    }

    private void PrepareData()
    {
        var random = new Random(42);
        for (var i = 0; i < UserCount; i++)
        {
            var p = (UserPermissions)random.Next(0, 64);
            _users.Add(new User
            {
                Permissions = p,
                CanRead = p.HasFlag(UserPermissions.Read),
                CanWrite = p.HasFlag(UserPermissions.Write),
                CanExecute = p.HasFlag(UserPermissions.Execute),
                CanAdmin = p.HasFlag(UserPermissions.Admin),
                CanDelete = p.HasFlag(UserPermissions.Delete),
                CanShare = p.HasFlag(UserPermissions.Share)
            });
        }
    }

    private void BtnRunTest_OnClick(object sender, RoutedEventArgs e)
    {
        // 1. Get required permissions from UI ===================================================
        var reqRead = ChkRead.IsChecked ?? false;
        var reqWrite = ChkWrite.IsChecked ?? false;
        var reqExec = ChkExecute.IsChecked ?? false;
        var reqAdmin = ChkAdmin.IsChecked ?? false;
        var reqDel = ChkDelete.IsChecked ?? false;
        var reqShare = ChkShare.IsChecked ?? false;

        var reqFlags = UserPermissions.None;
        if (reqRead) reqFlags |= UserPermissions.Read;
        if (reqWrite) reqFlags |= UserPermissions.Write;
        if (reqExec) reqFlags |= UserPermissions.Execute;
        if (reqAdmin) reqFlags |= UserPermissions.Admin;
        if (reqDel) reqFlags |= UserPermissions.Delete;
        if (reqShare) reqFlags |= UserPermissions.Share;

        // 2. Run Traditional Test ================================================================
        long traditionalMatches = 0;
        var sw = Stopwatch.StartNew();
        foreach (var user in _users)
        {
            // Traditional IFs
            if (reqRead && !user.CanRead) continue;
            if (reqWrite && !user.CanWrite) continue;
            if (reqExec && !user.CanExecute) continue;
            if (reqAdmin && !user.CanAdmin) continue;
            if (reqDel && !user.CanDelete) continue;
            if (reqShare && !user.CanShare) continue;

            traditionalMatches++;
        }

        sw.Stop();
        TxtTraditionalTime.Text = $"Time: {sw.Elapsed.TotalMilliseconds:F2}ms";
        TxtTraditionalResult.Text = $"Matches found: {traditionalMatches:N0}";

        // 3. Run Flagged Enum Test ===============================================================
        long flaggedMatches = 0;
        sw.Restart();
        foreach (var user in _users)
            // Flagged Enum bitwise check
            if ((user.Permissions & reqFlags) == reqFlags)
                flaggedMatches++;

        sw.Stop();
        TxtFlaggedTime.Text = $"Time: {sw.Elapsed.TotalMilliseconds:F2}ms";
        TxtFlaggedResult.Text = $"Matches found: {flaggedMatches:N0}";
    }
}
