using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class RoleDTO
    {
        public RoleDTO()
        {
            this.SelectedPermissions = new List<RolePermissionStatusesDTO>();

            this.Roles = new List<webpages_Roles>();
            this.Permissions = new List<webpages_Permissions>();
        }

        #region INFO
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        #endregion

        #region DATA
        [Required]
        public IList<RolePermissionStatusesDTO> SelectedPermissions { get; set; }
        #endregion

        #region HELPERS
        public IList<webpages_Roles> Roles { get; set; }
        public IList<webpages_Permissions> Permissions { get; set; }
        public IList<StatusCriterion> StatusCriterions { get; set; }
        #endregion

    }

    /// <summary>
    /// DTO class used in the Role permission form
    /// </summary>
    public class RolePermissionStatusesDTO
    {
        public int PermissionId { get; set; }
        private int? status_id;
        public int? StatusId {
            get
            {
                return status_id;
            }
            set {
                status_id = value == 0 ? new Nullable<int>() : value;
            }
        }
    }
}