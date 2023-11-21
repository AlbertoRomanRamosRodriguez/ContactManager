using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Text.RegularExpressions;

using ContactManager;
using ContactManager.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ContactManager.Controllers
{
    public class ContactsController : ApiController
    {

        private ContactsManagerContext db = new ContactsManagerContext();
        private JwtValidator jwtval = new JwtValidator("ContactManager", "www.gsichanllengeapi.com", "qwertyuiopasdfghjklzxcvbnm123456");

        public class ContactInputModel
        {
            public string FirstName;
            public string LastName;
            public string Email;
            public DateTime DateOfBirth;
            public string Phone;
            public int? age
            {
               

                get{
                    DateTime today = DateTime.Now;

                    int age = today.Year - DateOfBirth.Year;

                    if (DateOfBirth.Day < today.Day && DateOfBirth.Month < today.Month) { // Not the birthday yet
                        age--;
                    }

                    return age;

                }
            }

        }

        public class ResContact : tContact
        {
            public int age
            {


                get
                {
                    DateTime today = DateTime.Now;

                    int age = today.Year - DateOfBirth.Year;

                    if (DateOfBirth.Day < today.Day && DateOfBirth.Month < today.Month)
                    { // Not the birthday yet
                        age--;
                    }

                    return age;

                }
            }
        }

        public bool validateEmail (string email)
        {
            if (email == null)
            {
                return false;
            }

            string pattern = @"^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(email);
        }
        static tContact CopyContactInformation (ContactInputModel source, tContact destination)
        {
            destination.FirstName = source.FirstName;
            destination.LastName = source.LastName;
            destination.Email = source.Email;
            destination.DateOfBirth = source.DateOfBirth;
            destination.Phone = source.Phone;

            return destination;
        }

        // GET: api/Contacts
        [ResponseType(typeof(List<tContact>))]
        public async Task<IHttpActionResult> GetContacts ()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null)
            {
                return Unauthorized();
            }

            string token = authHeader.Parameter;
            JwtSecurityToken validatedToken = new JwtSecurityToken();
            jwtval.ValidateToken(token, out validatedToken);

            if (validatedToken == null)
            {
                return Unauthorized();
                
            }

            //var contacts = await db.Contacts.Select(c => new ResContact
            //{
            //    FirstName = c.FirstName,
            //    LastName = c.LastName,
            //    Email = c.Email,
            //    DateOfBirth = c.DateOfBirth,
            //    Phone = c.Phone
            //}).ToListAsync (); // Assuming db is a DbContext
            var contacts = await db.Contacts.ToListAsync();
            return Ok(contacts);

            }

        // GET: api/Contacts/5
        [ResponseType(typeof(tContact))]
        public async Task<IHttpActionResult> GettContact (Guid id)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null)
            {
                return Unauthorized();
            }
            string token = authHeader.Parameter;
            JwtSecurityToken validatedToken = new JwtSecurityToken();
            jwtval.ValidateToken(token, out validatedToken);

            if (validatedToken == null)
            {
                return Unauthorized();

            }

            tContact tContact = await db.Contacts.FindAsync(id);
            if (tContact == null)
            {
                return NotFound();
            }

            return Ok(tContact);
        }


        // PUT: api/Contacts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PuttContact (Guid id, [FromBody] ContactInputModel updContact)
        {
            var authHeader = Request.Headers.Authorization;
            string token = authHeader.Parameter;
            JwtSecurityToken validatedToken = new JwtSecurityToken();
            jwtval.ValidateToken(token, out validatedToken);

            if (validatedToken == null)
            {
                return Unauthorized();

            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            tContact targetContact = db.Contacts.Where(c => c.Id == id).FirstOrDefault();
            targetContact = CopyContactInformation(updContact, targetContact);

            db.Entry(targetContact).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tContactExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Contacts
        [ResponseType(typeof(tContact))]
        public async Task<IHttpActionResult> PosttContact([FromBody] ContactInputModel contact)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null)
            {
                return Unauthorized();
            }

            string token = authHeader.Parameter;
            JwtSecurityToken validatedToken = new JwtSecurityToken();
            jwtval.ValidateToken(token, out validatedToken);

            if (validatedToken == null)
            {
                return Unauthorized();

            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!validateEmail(contact.Email))
            {
                return BadRequest("Must provide a valid email address");
            }

            if(contact.age <= 18 || contact.age == null)
            {
                return BadRequest("The new contact must be at least 18 years old");
            }

            if(contact.FirstName == "" || contact.FirstName == null || contact.Phone == "" || contact.Phone == null)
            {
                return BadRequest("Field cannot be empty");
            }
            
            if(contact.LastName == "") // Handling the case lastname is an empty string
            {
                contact.LastName = null;
            }

            Guid newGuid = Guid.NewGuid();

            string username = validatedToken.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            Guid userGuid = db.Users.FirstOrDefault(u => u.Username == username).Id;
            
            tContact newContact = new tContact(newGuid,contact.FirstName, contact.LastName, contact.Email, contact.DateOfBirth, contact.Phone, userGuid);

            db.Contacts.Add(newContact);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = newContact.Id }, newContact);
        }

        // DELETE: api/Contacts/5
        [ResponseType(typeof(tContact))]
        public async Task<IHttpActionResult> DeletetContact(Guid id)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null)
            {
                return Unauthorized();
            }

            string token = authHeader.Parameter;
            JwtSecurityToken validatedToken = new JwtSecurityToken();
            jwtval.ValidateToken(token, out validatedToken);


            if (validatedToken != null) {

                string country = validatedToken.Claims.FirstOrDefault(c => c.Type == "Country").Value;
                List<string> roles = validatedToken.Claims.Where(c => c.Type == "Role").Select(c => c.Value).ToList();

                if (!roles.Contains("Administrator") || country != "CU")
                {
                    return Unauthorized();
                }

            } else 
            {
                return Unauthorized();

            }


            tContact tContact = await db.Contacts.FindAsync(id);
            if (tContact == null)
            {
                return NotFound();
            }

            db.Contacts.Remove(tContact);
            await db.SaveChangesAsync();

            return Ok(tContact);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool tContactExists(Guid id)
        {
            return db.Contacts.Count(e => e.Id == id) > 0;
        }
    }
}