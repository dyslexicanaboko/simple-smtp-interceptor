<Query Kind="Program">
  <Namespace>System.Net.Mail</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//Open with LinqPad https://www.linqpad.net/
void Main()
{
	//Send a single email
	//SingleSend("fake@email.com");
	
	//Send 100 emails in parallel
	//ParallelSend(100);
	
	//Send 100+ emails with different subject lines
	SendSubjectBatchedEmails();
}

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
		var subject = $@"Subject batch i{i}_r{r.Next()}";

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

// Boiler plate send method simplified to TO address and optional subject
public void SingleSend(string addressTo, string subject = @"This is a test email")
{
	using (var client = new SmtpClient())
	{
		using (var mail = new MailMessage("linqPadTest@local.com", addressTo))
		{
			client.Port = 25;
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.UseDefaultCredentials = false;
			client.Host = "localhost";
			
			mail.Subject = subject;
			mail.Body = "This is my test email body";

			client.Send(mail);
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