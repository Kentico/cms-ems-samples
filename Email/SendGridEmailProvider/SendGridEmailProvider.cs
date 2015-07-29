using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using SendGrid;

using CMS.EmailEngine;
using CMS.EventLog;
using CMS;
using CMS.Helpers;
using System.Text;
using System.Net.Mime;
using CMS.IO;

[assembly: RegisterCustomClass("SendGridEmailProvider", typeof(SendGridEmailProvider))]

public class CustomCompletedEventArgs : AsyncCompletedEventArgs
{
    public CustomCompletedEventArgs(Exception error, bool cancelled, object userState)
        : base(error, cancelled, userState)
    {
    }
}

public class SendGridEmailProvider : EmailProvider
{
    /// <summary>
    /// Synchronously sends an email through the SMTP server.
    /// </summary>
    /// <param name="siteName">Site name</param>
    /// <param name="message">Email message</param>
    /// <param name="smtpServer">SMTP server</param>
    protected override void SendEmailInternal(string siteName, MailMessage message, SMTPServerInfo smtpServer)
    {
        //Send the email via SendGrid
        SendSendGridEmail(message);
    }


    /// <summary>
    /// Asynchronously sends an email through the SMTP server.
    /// </summary>
    /// <param name="siteName">Site name</param>
    /// <param name="message">Email message</param>
    /// <param name="smtpServer">SMTP server</param>
    /// <param name="emailToken">Email token that represents the message being sent</param>   
    protected override void SendEmailAsyncInternal(string siteName, MailMessage message, SMTPServerInfo smtpServer, EmailToken emailToken)
    {
        //Send the email via SendGrid
        SendSendGridEmail(message);

        CustomCompletedEventArgs args = new CustomCompletedEventArgs(null, false, emailToken);
        OnSendCompleted(args);
    }

    /// <summary>
    /// Raises the SendCompleted event after the send is completed.
    /// </summary>
    /// <param name="e">Provides data for the async SendCompleted event</param>
    protected override void OnSendCompleted(AsyncCompletedEventArgs e)
    {
        base.OnSendCompleted(e);
    }

    /// <summary>
    /// This function will send the email via SendGrid.
    /// </summary>
    /// <param name="kMessage">MailMessage - Message object to send</param>
    protected void SendSendGridEmail(MailMessage kMessage)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Date:");
            sb.AppendLine();
            sb.Append(DateTime.Now.ToString());
            sb.AppendLine();

            // Create the email object first, then add the properties.
            var sgMessage = new SendGridMessage();

            // Add the message properties.
            sgMessage.From = new MailAddress(kMessage.From.ToString());
            sb.Append("From:");
            sb.AppendLine();
            sb.Append(sgMessage.From.Address);
            sb.AppendLine();

            // Add multiple addresses to the To field.
            sb.Append("To:");
            sb.AppendLine();
            foreach (MailAddress address in kMessage.To)
            {
                sgMessage.AddTo(address.Address);
                sb.Append(address.Address + ";");
            }
            sb.AppendLine();

            sgMessage.Subject = kMessage.Subject;
            sb.Append("Subject:");
            sb.AppendLine();
            sb.Append(sgMessage.Subject);
            sb.AppendLine();

            // HTML & plain-text
            if (kMessage.AlternateViews.Count > 0)
            {
                foreach (AlternateView view in kMessage.AlternateViews)
                {
                    // Position must be reset first 
                    if (view.ContentStream.CanSeek)
                    {
                        view.ContentStream.Position = 0;
                    }

                    using (StreamWrapper wrappedStream = StreamWrapper.New(view.ContentStream))
                    {
                        using (StreamReader reader = StreamReader.New(wrappedStream))
                        {
                            if (view.ContentType.MediaType == MediaTypeNames.Text.Html)
                            {
                                sgMessage.Html = reader.ReadToEnd();
                            }
                            else if (view.ContentType.MediaType == MediaTypeNames.Text.Plain)
                            {
                                sgMessage.Text = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            sb.Append("Body:");
            if (ValidationHelper.GetString(sgMessage.Html, "") != "")
            {
                sb.Append(sgMessage.Html);
            }
            else
            {
                sb.Append(sgMessage.Text);
            }
            sb.AppendLine();

            //Handle any attachments
            sb.Append("Attachments:");
            sb.AppendLine();
            foreach (Attachment attachment in kMessage.Attachments)
            {
                sgMessage.AddAttachment(attachment.ContentStream, attachment.Name);
                sb.Append(attachment.Name);
                sb.AppendLine();
            }

            //Enable click tracking
            sgMessage.EnableClickTracking(true);

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential("[SendGridLogin]", "[SendGridPassword]");

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            transportWeb.Deliver(sgMessage);

            //Log the email details to the Event Log
            EventLogProvider.LogInformation("SendSendGridEmail", "EMAIL SENT", ValidationHelper.GetString(sb, ""));
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("SendSendGridEmail", "EXCEPTION", ex);
        }
    }
}
