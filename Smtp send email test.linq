<Query Kind="Program">
  <Namespace>System.Net.Mail</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//Open with LinqPad https://www.linqpad.net/
void Main()
{
	//Send a single email
	SingleSend("fake@email.com");
	
	//Send 100 emails in parallel
	//ParallelSend(100);
}

// Define other methods and classes here
public void SingleSend(string addressTo)
{
	using (var client = new SmtpClient())
	{
		using (var mail = new MailMessage("linqPadTest@local.com", addressTo))
		{
			client.Port = 25;
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.UseDefaultCredentials = false;
			client.Host = "localhost";
			
			mail.Subject = "this is a test email.";
			mail.Body = "this is my test email body";

			client.Send(mail);
		}
	}
}

public void SingleSend(int taskNumber)
{
	SingleSend($"task_{taskNumber}@local.com");
}

public void ParallelSend(int numberOfTasks)
{
	Parallel.For(0, numberOfTasks, SingleSend);	
}