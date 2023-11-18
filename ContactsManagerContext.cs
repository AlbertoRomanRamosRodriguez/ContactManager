using System;
using System.Collections.Generic;
using System.Data.Entity;
using ContactManager.Models;
using System.Linq;
using System.Web;

namespace ContactManager
{
    public class ContactsManagerContext: DbContext
    {
        public DbSet<tUser> Users { get; set; }
        public DbSet<tContact> Contacts { get; set; }
    }
}