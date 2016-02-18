// main controller for site

using System;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using WebRole1.Models;

namespace WebRole1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        // GET: home/contact
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        // POST: home/contact
        [HttpPost]
        public ActionResult Contact(ContactForm c)
        {
            if (ModelState.IsValid)
            {
                MailMessage msg = new MailMessage();
                SmtpClient client = new SmtpClient();
                //client.Host = "smtp.gmail.com";
                client.Host = "smtp-mail.outlook.com";
                client.UseDefaultCredentials = false;
                client.Port = 25;
                client.Timeout = 20000;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new System.Net.NetworkCredential("ward.keyth2@outlook.com", "");

                msg.From = new MailAddress("ward.keyth2@outlook.com");
                msg.To.Add("ward.keyth@gmail.com");
                msg.Subject = "PPR contact form";
                msg.Body = "From: " + c.FirstName + " Comment: " + c.Comment;
                client.Send(msg);
                msg.Dispose();
                // return View("Success");
                return View("Search");
                
            }
            return View(c);
        }

        // GET: home/search
        [HttpGet]
        public ActionResult Search()
        {
            return View();
        }

        // POST: home/search
        [HttpPost]
        public ActionResult Search(SearchDB searchFor)
        {
            return View(searchFor);
        }
    }
}