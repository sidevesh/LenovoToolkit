﻿using IdeapadToolkitService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vanara.PInvoke;

namespace IdeapadToolkit.Services
{
    public class UEFISettingsService : IUEFISettingsService
    {
        private static readonly string Guid = "{D743491E-F484-4952-A87D-8D5DD189B70C}";
        private static readonly string ScopeName = "FBSWIF";
        private static readonly int ScopeAttribute = 7;

        private bool SetPrivilege(bool enable)
        {
            try
            {
                IntPtr zero = IntPtr.Zero;
                if (!Win32.OpenProcessToken(Win32.GetCurrentProcess(), 40U, ref zero))
                {
                    return false;
                }
                TokenPrivelege newState;
                newState.Count = 1;
                newState.Luid = 0L;
                newState.Attr = enable ? 2 : 0;
                if (!Win32.LookupPrivilegeValue((string)null, "SeSystemEnvironmentPrivilege", ref newState.Luid))
                {
                    return false;
                }
                if (!Win32.AdjustTokenPrivileges(zero, false, ref newState, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public int GetFlipToBootStatus()
        {
            if (!SetPrivilege(true)) return -1;
            int dataFromUefi = -1;
            try
            {
                LenovoFlipToBootSwInterface structure = new LenovoFlipToBootSwInterface()
                {
                    FlipToBootEn = 0,
                    Reserved1 = 0,
                    Reserved2 = 0,
                    Reserved3 = 0
                };
                
                var res = Win32.GetFirmwareEnvironmentVariableExW(ScopeName, Guid, ref structure, Marshal.SizeOf<LenovoFlipToBootSwInterface>(), IntPtr.Zero);
                if (res != 0)
                {
                    dataFromUefi = ((LenovoFlipToBootSwInterface)structure).FlipToBootEn;
                }
                else
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    dataFromUefi = lastWin32Error * -1;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _ = SetPrivilege(false);
            }
            return dataFromUefi;
        }

        public int SetFlipToBootStatus(bool newStatus)
        {
            if (!SetPrivilege(true)) return -1;
            int num1 = -1;
            try
            {
                LenovoFlipToBootSwInterface structure = new LenovoFlipToBootSwInterface()
                {
                    FlipToBootEn = newStatus ? (byte)1 : (byte)0,
                    Reserved1 = 0,
                    Reserved2 = 0,
                    Reserved3 = 0
                };

                var res = (Win32.SetFirmwareEnvironmentVariableExW(ScopeName, Guid, ref structure, Marshal.SizeOf<LenovoFlipToBootSwInterface>(), ScopeAttribute));
                if (res != 0) return 0;
            }
            catch (Exception ex)
            {
            }
            finally { _ = SetPrivilege(false); }
            return num1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LenovoFlipToBootSwInterface
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte FlipToBootEn;
            [MarshalAs(UnmanagedType.U1)]
            public byte Reserved1;
            [MarshalAs(UnmanagedType.U1)]
            public byte Reserved2;
            [MarshalAs(UnmanagedType.U1)]
            public byte Reserved3;
        }
    }
}
