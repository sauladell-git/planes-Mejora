using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using INET.Data;

namespace INET.Services.DTO
{
    public class UserDTO
    {
        public UserDTO()
        {
            this.SelectedFieldsIds = new List<int>();
            this.SelectedLevelsIds = new List<int>();
            this.SelectedProvincesIds = new List<int>();
            this.Roles = new List<webpages_Roles>();
            this.Provinces = new List<Province>();
            this.Fields = new List<Field>();
            this.Levels = new List<InstitutionLevel>();
            this.Users = new List<UserProfile>();
        }

        public int UserId { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public bool IsEnabled { get; set; }

        [Required]
        public IList<int> SelectedFieldsIds { get; set; }

        public IList<int> SelectedLevelsIds { get; set; }

        public IList<int> SelectedProvincesIds { get; set; }

        public string CurrentUser { get; set; }

        public List<webpages_Roles> Roles { get; set; }
        public List<Province> Provinces { get; set; }
        public List<Field> Fields { get; set; }
        public List<InstitutionLevel> Levels { get; set; }
        public List<UserProfile> Users { get; set; }
    }
}