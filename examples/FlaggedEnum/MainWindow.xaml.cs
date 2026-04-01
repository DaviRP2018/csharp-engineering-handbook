using System.Diagnostics;
using System.Windows;

namespace FlaggedEnum;

public enum NormalPermissions
{
    Read,
    Write,
    Execute,
    Admin,
    Delete,
    Share
}

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
    /*
     * MEMORY COMPARISON:
     *
     * 1. Traditional approach (List<NormalPermissions>):
     *    - List Object Header: 8-16 bytes
     *    - Pointer to the internal array: 8 bytes (on 64-bit)
     *    - Internal Array Object: Header (8-16 bytes) + Length (4 bytes) + Padding
     *    - Actual Data (Enums): Each int in the array is 4 bytes.
     *    - Capacity: A List often allocates more than it uses (e.g., capacity of 4 or 8).
     *    - ESTIMATE: ~40-80+ bytes per user just to store a few permissions.
     *
     * 2. Modern approach (FlaggedPermissions Enum):
     *    - A single 'int' (the underlying type of the Enum).
     *    - COST: Exactly 4 bytes per user.
     *
     * VISUAL REPRESENTATION IN MEMORY:
     *
     * [ List Approach ]                 [ Flagged Enum Approach ]
     * User Object                       User Object
     * +-----------------------+         +-----------------------+
     * | List Reference (8b)  |------>   | Flagged (4b) [001011] |
     * +-----------------------+         +-----------------------+
     *             |
     *             v
     *        List Object
     *        +-----------------------+
     *        | Array Ref (8b) |------> Internal Array [R, W, E, 0, 0...]
     *        +-----------------------+
     */

    // Traditional approach using a List of normal Enums
    public List<NormalPermissions> NormalPermissions { get; set; } = new();

    // Modern approach using a Flagged Enum
    public UserPermissions FlaggedPermissions { get; set; }
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
            var user = new User { FlaggedPermissions = p };

            if (p.HasFlag(UserPermissions.Read))
                user.NormalPermissions.Add(NormalPermissions.Read);
            if (p.HasFlag(UserPermissions.Write))
                user.NormalPermissions.Add(NormalPermissions.Write);
            if (p.HasFlag(UserPermissions.Execute))
                user.NormalPermissions.Add(NormalPermissions.Execute);
            if (p.HasFlag(UserPermissions.Admin))
                user.NormalPermissions.Add(NormalPermissions.Admin);
            if (p.HasFlag(UserPermissions.Delete))
                user.NormalPermissions.Add(NormalPermissions.Delete);
            if (p.HasFlag(UserPermissions.Share))
                user.NormalPermissions.Add(NormalPermissions.Share);

            _users.Add(user);
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

        // 2. Run Traditional Test (Checking multiple items in a List) ============================
        var reqNormal = new List<NormalPermissions>();
        if (reqRead) reqNormal.Add(NormalPermissions.Read);
        if (reqWrite) reqNormal.Add(NormalPermissions.Write);
        if (reqExec) reqNormal.Add(NormalPermissions.Execute);
        if (reqAdmin) reqNormal.Add(NormalPermissions.Admin);
        if (reqDel) reqNormal.Add(NormalPermissions.Delete);
        if (reqShare) reqNormal.Add(NormalPermissions.Share);

        long traditionalMatches = 0;
        var sw = Stopwatch.StartNew();
        foreach (var user in _users)
        {
            var isMatch = true;
            foreach (var req in reqNormal)
            {
                if (user.NormalPermissions.Contains(req)) continue;
                isMatch = false;
                break;
            }

            if (isMatch)
                traditionalMatches++;
        }

        sw.Stop();
        TxtTraditionalTime.Text = $"Time: {sw.Elapsed.TotalMilliseconds:F2}ms";
        TxtTraditionalResult.Text = $"Matches found: {traditionalMatches:N0}";

        // 3. Run Flagged Enum Test (Single Bitwise Operation) ====================================
        var reqFlags = UserPermissions.None;
        if (reqRead) reqFlags |= UserPermissions.Read;
        if (reqWrite) reqFlags |= UserPermissions.Write;
        if (reqExec) reqFlags |= UserPermissions.Execute;
        if (reqAdmin) reqFlags |= UserPermissions.Admin;
        if (reqDel) reqFlags |= UserPermissions.Delete;
        if (reqShare) reqFlags |= UserPermissions.Share;

        long flaggedMatches = 0;
        sw.Restart();
        foreach (var user in _users)
            // Single bitwise operation vs multiple List lookups
            if (user.FlaggedPermissions.HasFlag(reqFlags))
                flaggedMatches++;

        sw.Stop();
        TxtFlaggedTime.Text = $"Time: {sw.Elapsed.TotalMilliseconds:F2}ms";
        TxtFlaggedResult.Text = $"Matches found: {flaggedMatches:N0}";
    }
}
