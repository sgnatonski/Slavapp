using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Mail
{
    public class MailData
    {
        public MailAddress[] Recipients { get; set; }

        public MailAddress[] CC { get; set; }

        public MailAddress[] BCC { get; set; }

        public MailAddress Sender { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }
    }
}
