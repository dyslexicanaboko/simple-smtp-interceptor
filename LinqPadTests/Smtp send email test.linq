<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Namespace>System.Net.Mail</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
</Query>

//Open with LinqPad https://www.linqpad.net/
void Main()
{
	//Send a single email
	//SingleSend("fake@email.com");

	//Send a single email null message - no attachments
	SingleSend(new Email { Message = null } );

	//Send a single email with three attachments
	//SingleSend(new Email { From = "textFiles@email.com", Attachments = FilesText });
	//SingleSend(new Email { From = "imageFiles@email.com", Attachments = FilesImage });
	//SingleSend(new Email { From = "largeFile@email.com", Attachments = FilesLarge } );

	//Send 100 emails in parallel
	//ParallelSend(100);
	
	//Send 100+ emails with different subject lines
	//SendSubjectBatchedEmails();
}

private static readonly string[] FilesText = new string[] 
{
	"TestEmailAttachment01.txt",
	"TestEmailAttachment02.txt",
	"TestEmailAttachment03.txt"
};

private static readonly string[] FilesImage = new string[]
{
	"TestEmailAttachment01.jpg",
	"TestEmailAttachment02.bmp",
	"TestEmailAttachment03.gif",
	"TestEmailAttachment04.png",
	"01-31-2020 13-45-13.jpg"
};

private static readonly string[] FilesLarge = new string[]
{
	"LargeGarbageFile.dat"
};

//Send many emails, some with similar subject lines to create a batch feel
//This is to exercise a variety in emails being sent
public void SendSubjectBatchedEmails(int emailsToSend = 100)
{
	var r = new Random();
	
	//Subject and number of emails to send
	var dictSubjects = new Dictionary<string, int>();
	
	//Using 10% of what is being sent in as the divisor seed
	var ds = Convert.ToInt32((double)emailsToSend * 0.1);
	
	//Get the number of subjects that cannot be greater 
	//than the number of emails to send
	var divisor = r.Next(ds);
	
	//Low possibility that the divisor could be zero
	if(divisor == 0) divisor = 1;
	
	var subjects = emailsToSend / divisor;
	
	double remainingEmails = emailsToSend;
	
	for (int i = 0; i < subjects; i++)
	{
		int distro;

		if (remainingEmails > 0)
		{
			//Get a random distribution
		    distro = Convert.ToInt32(Math.Ceiling(remainingEmails * r.NextDouble()));

			//Reduce the number of emails available to send from the distribution
			remainingEmails -= distro;
		}
		else
		{
			//If the distribution has gotten to or below zero
			//Then send one email for the remaining subjects
			distro = 1;
		}

		var subject = $@"Subject batch i{i}_r{r.Next()}_c{distro}";

		//Each subject will have X number of emails to send
		dictSubjects.Add(subject, distro);
	}

	Console.WriteLine($"Sending {emailsToSend} emails with {subjects} subjects.");
	
	dictSubjects.Dump();

	//Send each subject individually
	Parallel.ForEach(dictSubjects, kvp => {
		for (int i = 0; i < kvp.Value; i++)
		{
			SingleSend($"task_{i}@local.com", kvp.Key);
		}
	});
}

public void SingleSend(string addressTo, string subject = Email.StockSubject)
{
	var obj = new Email
	{
		To = addressTo,
		Subject = subject
	};
	
	SingleSend(obj);
}

// Boiler plate send method simplified to TO address and optional subject
public void SingleSend(Email email)
{
	using (var client = new SmtpClient())
	{
		using (var mail = new MailMessage(email.From, email.To))
		{
			client.Port = 25;
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.UseDefaultCredentials = false;
			client.Host = "localhost";
			
			mail.Subject = email.Subject;
			mail.Body = email.Message;
			mail.IsBodyHtml = true;

			string strAttachments = null;

			var hasAttachments = email.Attachments != null && email.Attachments.Any();

			if (hasAttachments)
			{
				var lst = GetAttachments(email.Attachments);

				var sb = new StringBuilder();

				lst.ForEach(x => { 
					mail.Attachments.Add(x);
					
					sb.Append("\t\t").Append(x.Name).AppendLine();
				});
				
				strAttachments = sb.ToString();
			}

			client.Send(mail);

			Console.WriteLine($"{email.To}, {email.Subject}, {mail.Body}");

			if (hasAttachments)
			{
				Console.WriteLine($"Attachments:\n{strAttachments}");
			}
		}
	}
}

// Send one email to get the idea and make sure your setup is working
public void SingleSend(int taskNumber)
{
	SingleSend($"task_{taskNumber}@local.com");
}

// Send many emails in parallel
public void ParallelSend(int numberOfTasks)
{
	Parallel.For(0, numberOfTasks, SingleSend);	
}

public List<Attachment> GetAttachments(string[] fileNames)
{
	var lst = new List<Attachment>(fileNames.Length);
	
	var path = Path.GetDirectoryName(Util.CurrentQueryPath);
	
	foreach (var file in fileNames)
	{
		var filePath = Path.Combine(path, file);
		
		string mimeType = MimeMapping.GetMimeMapping(file);
		
		var a = new Attachment(filePath, mimeType);
		
		lst.Add(a);
	}
	
	return lst;
}

public class Email
{
	public const string StockSubject = "This is a test email";
	
	public string From { get; set; } = @"linqPadTest@local.com";

	public string To { get; set; } = @"whatever@email.com";

	public string Subject { get; set; } = StockSubject;

	public string Message { get; set; } = @"<html><span>This is a test heading</span></html>";
	
	public string[] Attachments { get; set; }
}