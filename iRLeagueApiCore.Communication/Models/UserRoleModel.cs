using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Schema to use for permission control over a single role for a user
    /// </summary>
    public class UserRoleModel
    {
        /// <summary>
        /// Name of the user to change permission
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Name of the role to give/revoke
        /// </summary>
        public string RoleName { get; set; }
    }
}
