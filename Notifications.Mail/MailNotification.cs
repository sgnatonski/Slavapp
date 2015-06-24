using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Mail
{
    public abstract class MailNotification<T, TMsg> : INotification<T>
    {
        private readonly MailConfig mailConfig;
        private readonly INotificationReceiver notificationReceiver;
        private readonly Func<MailData, TMsg> messageCreator;

        public MailNotification(MailConfig mailConfig, INotificationReceiver notificationReceiver, Func<MailData, TMsg> messageCreator)
        {
            this.mailConfig = mailConfig;
            this.notificationReceiver = notificationReceiver;
            this.messageCreator = messageCreator;
        }

        protected abstract MailData PrepareMail(T data);

        public void Notify(T data)
        {
            var maildata = this.PrepareMail(data);
            if (maildata == null)
                return;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(this.mailConfig.From);

            if (maildata.Sender != null)
            {
                mail.ReplyToList.Add(maildata.Sender);
            }

            if (maildata.Recipients != null)
            {
                foreach (var r in maildata.Recipients)
                {
                    mail.To.Add(r);
                }
            }

            if (maildata.CC != null)
            {
                foreach (var r in maildata.CC)
                {
                    mail.CC.Add(r);
                }
            }

            if (maildata.BCC != null)
            {
                foreach (var r in maildata.BCC)
                {
                    mail.Bcc.Add(r);
                }
            }

            SmtpClient client = new SmtpClient(this.mailConfig.Host, this.mailConfig.Port);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(this.mailConfig.UserName, this.mailConfig.Password);
            mail.IsBodyHtml = true;
            mail.Subject = maildata.Title;
            mail.Body = maildata.Body;
            client.Send(mail);

            var recipients = Enumerable.Empty<MailAddress>().ToList();
            if (maildata.Recipients != null)
            {
                recipients.AddRange(maildata.Recipients);
            }
            if (maildata.CC != null)
            {
                recipients.AddRange(maildata.CC);
            }
            if (maildata.BCC != null)
            {
                recipients.AddRange(maildata.BCC);
            }

            var senderAddress = this.mailConfig.From;
            if (maildata.Sender != null)
            {
                senderAddress = maildata.Sender.Address;
            }

            this.notificationReceiver.Send(this.messageCreator(maildata));
        }
    }
}