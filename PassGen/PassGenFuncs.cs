using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace PassGen
{
    public class PassGenFuncs
    {
        static char[] PASSWORDCHARACTERS = "ABCEFGHKLMNPQRSTUVWXYZ0987654321abdefghjmnqrtuwy".ToCharArray();
        static int PASSWORDCHARACTERSLEN = PASSWORDCHARACTERS.Length;
        const string REGPATH = @"SOFTWARE\PassGen";
        const string REGVALUECURRENTKEY = "CurrentKey";

        public static void ClipboardClear()
        {
            //ClipboardSetText("");
            Clipboard.Clear();
        }

        public static string ClipboardGetText()
        {
            return Clipboard.GetText();
        }

        public static void ClipboardSetText(string sText)
        {
            ClipboardClear();
            Clipboard.SetData("Text", sText);
        }

        public static string GeneratePassword(string Key, string Passphrase)
        {
            string Value = Key + "-" + Passphrase;
            var Hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(Value));
            string Password = "";
            for (int x = 0; x <= 19; x++)
            {
                int iChar = Hash[x] % PASSWORDCHARACTERSLEN;
                Password += PASSWORDCHARACTERS[iChar];
                if ((x + 1) % 4 == 0 && x < 19) { Password += "-"; }
            }
            Hash = null;
            return Password;
        }

        public static void KeyDelete(string sGUID)
        {
            RegistryKey oKey = RegistryOpen(true);
            oKey.DeleteValue(sGUID);
        }

        public static string KeyGetActive()
        {
            RegistryKey oKey = RegistryOpen(true);
            string sGUID = (string)oKey.GetValue(REGVALUECURRENTKEY, null);
            oKey.Close();
            return sGUID;
        }

        public static string KeyParseDate(string Key)
        {
            if (Key == null) { return null; }
            return Key.Substring(0, 10);
        }

        public static string KeyParseValue(string Key)
        {
            if (Key == null) { return null; }
            return Key.Substring(10, Key.Length - 10);
        }

        public static string KeyRead(string sGUID)
        {
            RegistryKey oKey = RegistryOpen();
            byte[] hProtectedData = (byte[])oKey.GetValue(sGUID, null);
            oKey.Close();
            if (hProtectedData != null)
            {
                byte[] hUnprotectedData = ProtectedData.Unprotect(hProtectedData, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(hUnprotectedData);
            }
            return null;
        }

        public static string KeyReadCurrent()
        {
            string sCurrentKey = KeyGetActive();
            if (sCurrentKey == null) { return null; }
            return KeyParseValue(KeyRead(sCurrentKey));
        }
        
        public static string KeySave(string sKeyValue, string sKeyDate, string sGUID = null)
        {
            if (sGUID == null)
            {
                sGUID = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
            }
            byte[] hProtectedData = ProtectedData.Protect((byte[]) Encoding.UTF8.GetBytes(sKeyDate + sKeyValue), null, DataProtectionScope.CurrentUser);
            RegistryKey oKey = RegistryOpen(true);
            oKey.SetValue(sGUID, hProtectedData, RegistryValueKind.Binary);
            oKey.Close();
            return sGUID;
        }

        public static void KeySetActive(string sKeyGUID)
        {
            RegistryKey oKey = RegistryOpen(true);
            oKey.SetValue(REGVALUECURRENTKEY, sKeyGUID, RegistryValueKind.String);
            oKey.Close();
        }

        public static RegistryKey RegistryOpen(bool bWriteEnabled = false)
        {
            RegistryKey oRegistry = Registry.CurrentUser;
            RegistryKey oSubKey = oRegistry.OpenSubKey(REGPATH, bWriteEnabled);
            if (oSubKey == null)
            {
                oSubKey = oRegistry.CreateSubKey(REGPATH, bWriteEnabled);
            }
            return oSubKey;
        }

        public static bool StringIsComplex(string String)
        {
            string Pattern = @"(?=^.{8,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[\p{P}])";
            //Regex rg = new Regex(Pattern);
            //return rg.IsMatch(String);
            return StringRegEx(Pattern, String);
        }

        public static bool StringIsValidGUID(string sGUID)
        {
            string Pattern = @"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
            return StringRegEx(Pattern, sGUID);
        }

        private static bool StringRegEx(string Pattern, string Test)
        {
            Regex rg = new Regex(Pattern);
            return rg.IsMatch(Test);
        }
    }
}