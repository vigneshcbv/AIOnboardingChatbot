using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using ChatbotCustomerOnboarding.DataModel;
using LaYumba.Functional;

/* Sends Email once the Quote is confirmed/
   ///-----------------------------------------------------------------
   ///   Namespace:      <ChatbotCustomerOnboarding>
   ///   Class:          <EmailHelper>
   ///   Description:    <Sends email using SMTP>
   ///   Author:         <Vignesh Chandran balan>                    
 ///-----------------------------------------------------------------*/


namespace ChatbotCustomerOnboarding.BotHelpers
{
    using static F;

    public class EmailService
    {
        public static async Task<bool> SendEmail(string quoteCustomer, Attachment attachment)
        {
            try
            {
                string smtpAddress = "smtp.mail.yahoo.com";
                int portNumber = 587;
                bool enableSSL = true;

                string emailFrom = "projectswen2021@yahoo.com";
                string password = "gmsakxqsnihlkugb";
                string emailTo = CreateCustomer.Instance.EmailAddress.ToString();
                string subject = $"Your Personalized Renter's Insurance Quote";
                string body = $"{CreateCustomer.Instance.FirstName} {CreateCustomer.Instance.LastName}, here is your Personalized Renter's Insurance Quote: {Convert.ToDecimal(quoteCustomer).ToString("C2", CultureInfo.CurrentCulture)}\r\n\r\n";
                body += "Email: " + CreateCustomer.Instance.EmailAddress + "\n";
                body += "Message: \n" + "Demo: Onboarding Customer feature 2 Assignment" + "\n";
                //   string[] cc = { "PetrenkoO6197@UHCL.edu", "ObryantJ2383@UHCL.edu", "murukutlas3934@uhcl.edu" };

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFrom);
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.Attachments.Add(attachment);
                    //foreach (var person in cc)
                    //    mail.CC.Add(new MailAddress(person));
                    //mail.IsBodyHtml = true;
                    // Can set to false, if you are sending pure text.

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return false;

            }
        }

        public static async Task<bool> SendEmail(string quoteCustomer)
        {
            try
            {
                string smtpAddress = "smtp.mail.yahoo.com";
                int portNumber = 587;
                bool enableSSL = true;

                string emailFrom = "projectswen2021@yahoo.com";
                string password = "gmsakxqsnihlkugb";
                string emailTo = CreateCustomer.Instance.EmailAddress.ToString();
                string subject = $"Here is your Quote: {quoteCustomer} CustomerID: {CreateCustomer.Instance.CustomerId}";
                string body = "CustomerName: " + CreateCustomer.Instance.FirstName + " " + CreateCustomer.Instance.LastName + "\n";
                body += "Email: " + CreateCustomer.Instance.EmailAddress + "\n";
                body += "Message: \n" + "Demo: Onboarding Customer feature 2 Assignment" + "\n";
                //   string[] cc = { "PetrenkoO6197@UHCL.edu", "ObryantJ2383@UHCL.edu", "murukutlas3934@uhcl.edu" };

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFrom);
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    //foreach (var person in cc)
                    //    mail.CC.Add(new MailAddress(person));
                    //mail.IsBodyHtml = true;
                    // Can set to false, if you are sending pure text.

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFrom, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                    }
                }

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return false;

            }
        }
    }
}
