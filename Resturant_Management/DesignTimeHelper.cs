using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Resturant_Management
{
    /// <summary>
    /// Utility to accurately detect if code is executing within Visual Studio's 
    /// .NET 8 Windows Forms Out-of-Process Designer (DesignToolsServer.exe) or devenv.exe.
    /// Prevents crashes such as "Value cannot be null (WriteAsync)" caused by running
    /// runtime-only layout modifications, control reparenting, or database queries at design time.
    /// </summary>
    public static class DesignTimeHelper
    {
        private static bool? _isInDesignMode;

        public static bool IsInDesignMode(Control? control = null)
        {
            if (_isInDesignMode.HasValue)
                return _isInDesignMode.Value;

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                _isInDesignMode = true;
                return true;
            }

            if (control?.Site?.DesignMode == true)
            {
                _isInDesignMode = true;
                return true;
            }

            try
            {
                using var proc = Process.GetCurrentProcess();
                string name = proc.ProcessName;
                if (name.Equals("devenv", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("DesignToolsServer", StringComparison.OrdinalIgnoreCase))
                {
                    _isInDesignMode = true;
                    return true;
                }
            }
            catch
            {
            }

            _isInDesignMode = false;
            return false;
        }
    }
}
