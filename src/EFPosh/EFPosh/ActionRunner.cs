using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SimpleImpersonation;

namespace EFPosh
{
    public class ActionRunner
    {
        private PoshCredential _poshCredential;
        public ActionRunner(PoshCredential poshCredential = null)
        {
            _poshCredential = poshCredential;
        }
        public T RunAction<T>(Func<T> action)
        {
            if(_poshCredential != null)
            {
                return Impersonation.RunAsUser<T>(_poshCredential.UserCredentials, _poshCredential.LogonType, action);
            }
            return action();
        }
        public void RunAction(Action action)
        {
            if (_poshCredential != null)
            {
                Impersonation.RunAsUser(_poshCredential.UserCredentials, _poshCredential.LogonType, action);
            }
            action();
        }
    }
    public class PoshCredential
    {
        public string UserName { get; set; }
        public string Domain { get; set; }
        public SecureString Pass { get; set; }
        public LogonType LogonType { get; set; } = LogonType.Network;
        public UserCredentials UserCredentials
        {
            get
            {
                if (String.IsNullOrEmpty(UserName))
                {
                    return null;
                }
                return new UserCredentials(Domain, UserName, Pass);
            }
        }
    }
}
