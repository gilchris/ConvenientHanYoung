using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenientHanYoung
{
    class RegistryUtil
    {
        private const string APP_NAME = "ConvenientHanYoung";
        private const string START_WITH_REGISTRY_KEY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        private string execetablePath;

        public RegistryUtil(string execetablePath)
        {
            this.execetablePath = execetablePath;
            // 현재 프로그램이 실행된 위치와 레지스트리에 등록된 정보가 다르면 덮어씌우기 위해 다음과 같이 실행
            if (isRegisteredForStartProgram)
            {
                registerForStartProgram(true);
            }
        }

        public bool isRegisteredForStartProgram
        {
            get
            {
                bool r = false;
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(START_WITH_REGISTRY_KEY))
                {
                    r = (null != key.GetValue(APP_NAME));
                }
                return r;
            }
        }

        public void registerForStartProgram(bool isSet)
        {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(START_WITH_REGISTRY_KEY, true))
            {
                if (isSet)
                {
                    key.SetValue(APP_NAME, execetablePath);
                }
                else
                {
                    key.DeleteValue(APP_NAME, false);
                }
            }
        }
    }
}
