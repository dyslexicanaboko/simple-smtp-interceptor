# simple-smtp-interceptor
A very simplistic, bare minimum, SMTP interceptor for capturing SMTP traffic. Written in C# dot net core 2.1. Includes a very basic UI, console, tester and windows service using nssm.exe.

## Updates
- 10/05/2019 - Initial push to GitHub. Only handles email with no attachments.
- 10/14/2019 - I adjusted the stock UI to be a little more friendly to use. It's still hideous, but slightly more functional.
- 10/31/2019 - Added an error log for any emails that cannot be saved for any reason.
- 02/02/2020
  - Basic support for attachments. Attachments were not the main focus of this project, but I didn't realize how they impact the composition of an email during serialization. Therefore to minimize transmission errors, I have implemented support for this. Regardless of the number of attachments, they will be compressed into a zip file and stored in the database.
  - Updated UI to only show the HTML version of a message. Text option is still available but not selected by default. Included a column for attachments count.

## Why did you make another project for this when others exist?
I was really tired of trying to find an SMTP development friendly server that I could host for everyone in my department to use for testing. I know of a few, but they don't work properly or some are just over the top with configuration. I wanted something straight forward and when I can't find it, I just build it myself.

## Who is this project good for?
Microsoft developers or anyone willing to figure out how to set it up. This project is the bare minimum so that I could take it and go. I got it to the point where it would work to do just one thing and that is capture email.

### My UI is terrible
I still have to update the UI - however you can bypass the UI and just hit the database instead. Or build your own UI that's way better than mine. The UI is built using a stock Angular 5 template so it is already outdated. I will worry more about this later (11/17/2018).
- 02/09/2020 - I am getting pretty annoyed with Angular, so I am thinking about providing an alternate MVC interface. I'm still thinking about it.

# How do I get started?
I tried making this as simple as possible, which also means this project isn't feature packed. 

## Do you need this for local development? 
If you are looking for a local SMTP development server I strongly recommend using [smtp4dev](https://github.com/rnwood/smtp4dev) by [rnwood](https://github.com/rnwood) for the following reasons:
- It is an established project
- I have used it for years and I can tell you from personal experience it is very good
- It is easy to setup
- It is far more configurable than what I am providing

## Do you need this for your whole team?
I created this project so that my development team and QA engineers could have emails trapped locally without having to scrub email addresses. The benefits of this project is:
- It works with SQL Server (versus SQL Lite)
- It can be run on a server as a Windows Service
- It can be run locally as a console if needed
- You can view the captured emails from the included minimalistic website
- You can query the captured emails directly from the SQL Server
- Additionally you can develop against this however you'd like

## If you need something like this for your team then let's get started!
First let's discuss what each project is for so that it isn't mysterious or misunderstood. The project consists of the following components:
1. SimpleSmtpInterceptor.Console
1. SimpleSmtpInterceptor.Data
1. SimpleSmtpInterceptor.Lib
1. SimpleSmtpInterceptor.Web
1. Windows service
1. Smtp send email test.linq

### SimpleSmtpInterceptor.Console
A console application that can just be run directly in Visual Studio if you wanted to or you can compile it, copy the binaries somewhere and then run it using the following command from CMD:

`dotnet SimpleSmtpInterceptor.ConsoleApp.dll`

Remember this whole project was written in dot net core 2.1, so you need to make sure you have all of your updates.

This console application also acts as the server which will be explained later in the "Windows service" section below.

### SimpleSmtpInterceptor.Data
I used Fluent API (Code first) against Entity Framework Core (EF) in order to make the data layer which consists of a whole one table. The table is named `dbo.Email` and so there is a complimentary model named `Email`.

### SimpleSmtpInterceptor.Lib
This is the library that houses all of the Fake SMTP server code and speaks to the Data layer. One thing to know is that I picked up the core of this code from a CodeProject example here is my acknowledgement to that author:
> All credit for the basis of this code goes to the author "Al Forno" of this comment on Code-Project
> https://www.codeproject.com/Tips/286952/create-a-simple-smtp-server-in-csharp?msg=4363652#xx4363652xx
>
> I have modified this code for my needs and cleaned it up to my liking.

Improvements that I have made are:
1. Modernized the code
1. Changed the server from only accepting serial requests to using parallelism in order to handle multiple requests
1. Saves all incoming emails to the `dbo.Email` table as is

### SimpleSmtpInterceptor.Web
- Incredibly basic Angular 5 (yeah it's outdated because of the template that comes with VS 2017...) site that connects to the same data layer to display captured emails from the `dbo.Email` table.
- Button for purging all email

Just host this site in IIS and you are ready to go.
- No logins
- No inboxes
- No hassle
- Just captured email

### Windows service
This is where things get a little complicated, first of all this isn't actually a project. You will notice it is a bunch of batch files and an executable. Here is what those are for:
1. run.bat - Batch file for running the console application outside of visual studio
1. install.bat - Batch file for installing the SimpleSmtpInterceptor windows service using nssm.exe
1. uninstall.bat - Batch file for uninstalling the SimpleSmtpInterceptor windows service using nssm.exe
1. is running.bat - Batch file for checking if the SimpleSmtpInterceptor windows service is running using nssm.exe
1. nssm.exe - NSSM - the Non-Sucking Service Manager which I obtained here: [http://nssm.cc/download](http://nssm.cc/download)

#### Why are you using NSSM?
I am using NSSM because dot net core windows services aren't a thing. This still hasn't been solidified by Microsoft and honestly this was the simplest way of creating a windows service quickly with the least amount of hassle.

Just a word of warning - the version I have up on git is for Windows 10, the NSSM prelease build 2.2.4-101

#### Why didn't you use Top Shelf instead?
Because I used NSSM first and it worked - so why change what works?

#### How do I use this project as a windows service?
1. Publish the Console project using the following settings:
   1. Configuration   : Release | Any CPU
   1. Target Framework: netcoreapp2.1
   1. Deployment Mode : Self-contained //I have not had any luck with portable
   1. Target Runtime  : win-x64 //This is up to you
   1. Target location : bin\Release\netcoreapp2.1\publish\ //default value is fine
1. Copy the binaries in the publish folder to a installation location. No need to copy the "runtimes" folder.
1. Copy the contents of the `Windows service` folder to the same installation location
1. Edit the `run.bat` to point to where `SimpleSmtpInterceptor.ConsoleApp.dll` is located
1. Run the `install.bat` so that `nssm.exe` runs
   1. You will be prompted for administrative access more than likely
1. Point `nssm.exe` to the absolute path of `run.bat` for both the:
   1. Path
   1. Startup directory
1. Set a custom `log on` if necessary by clicking on the `log on` tab
   1. You want to do this if you are using integrated security for connecting to your SQL Server
   1. If you have problems running the server, make sure to check the windows Event Viewer for messages
   
#### How do I do \_____ with NSSM?
If you double click on nssm.exe it will tell you everything you can do. Additionally you can go to their website to read up on it [here](http://nssm.cc/commands).

### Smtp send email test.linq
This is test code I am providing so that you can test out the server with dummy emails. I wrote it with [LINQPad](https://www.linqpad.net/), but you can just as easily copy the code into VS to run it if you want. However it's easier to just use LINQPad in my opinion.

These are the available test methods:
1. SingleSend - use this for testing your setup otherwise you could run into some issues with a bunch of bounced emails on your local.
   1. This is incredibly annoying to deal with, especially if you have Norton AntiVirus installed which will tell you about every single email that has failed to send in a separate pop up window. You have been warned.
1. ParallelSend - after you have setup your console or windows service properly, you can use this for testing a lot of email at once.
1. SendSubjectBatchedEmails - send a multitude of emails with random subject lines to have some variety in what is being sent/received.

#### Upates to this script
I am constantly making updates to this script to test the project. So I cannot guarantee what state I will leave it in. Some methods may not work anymore depending on what I am doing. Eventually I will realize this and fix it.
