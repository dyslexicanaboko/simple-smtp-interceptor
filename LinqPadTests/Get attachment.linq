<Query Kind="Program">
  <Connection>
    <ID>eac9ecfc-eff2-4a24-81af-e3ace612cbda</ID>
    <Persist>true</Persist>
    <Server>.</Server>
    <IsProduction>true</IsProduction>
    <Database>SimpleSmtpInterceptor</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

void Main()
{
	var obj = Emails.Single(x => x.EmailId == 263);
	
	File.WriteAllBytes(@"J:\Dump\Blah.zip", obj.AttachmentArchive.ToArray());
}
