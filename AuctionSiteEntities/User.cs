using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace WU.Entity {
    public class User : IUser{
        public int UserId { get; set; }

        [Required]
        [Index("UsernameIsUniqueInEachSite", 2, IsUnique = true)]
        [MinLength(DomainConstraints.MinUserName)]
        [MaxLength(DomainConstraints.MaxUserName)]
        public string Username { get; set; }

        [Required]
        [Index("UsernameIsUniqueInEachSite", 1, IsUnique = true)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Required]
        public string PasswordStored { get; set; }
        [NotMapped]
        [MinLength(DomainConstraints.MinUserPassword)]
        public string Password {
            get { return PasswordStored; }
            set
            {
                var passAndSalt = value + UserId.ToString();
                using (var hashSystem = System.Security.Cryptography.SHA256.Create())
                {
                    PasswordStored = hashSystem.ComputeHash(Encoding.ASCII.GetBytes(passAndSalt)).ToString();
                }
            }
        }

        string IUser.Username => Username;

        IEnumerable<IAuction> IUser.WonAuctions() {
            throw new NotImplementedException();
        }

        void IUser.Delete() {
            throw new NotImplementedException();
        }
    }
}
