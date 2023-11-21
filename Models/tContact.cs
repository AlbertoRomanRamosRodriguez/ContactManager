using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ContactManager.Models
{
    public class tContact
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(128)]
        public string FirstName { get; set; }

        [StringLength(128)]
        public string LastName { get; set; }

        [Required]
        [StringLength(128)]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        public Guid Owner { get; set; }

        //Constructors
        public tContact ()
        {

        }
        public tContact (Guid id, string firstName, string lastName, string email, DateTime dateOfBirth, string phone, Guid owner)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            DateOfBirth = dateOfBirth;
            Phone = phone;
            Owner = owner;
        }
    }

}