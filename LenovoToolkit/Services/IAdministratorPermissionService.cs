using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenovoToolkit.Services
{
    public interface IAdministratorPermissionService
    {
        public bool IsAdministrator { get; }
        public void RelaunchAsAdmin();
    }
}
