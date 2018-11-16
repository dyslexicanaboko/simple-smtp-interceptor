# simple-smtp-interceptor
A very simplistic, bare minimum, SMTP interceptor for capturing SMTP traffic written in C# dot net core 2.1. Includes a very basic UI, console, tester and windows service using nssm.exe

I was really tired of trying to find an SMTP development friendly server that I could host for everyone in my department to use for testing. I know of a few, but they don't work properly or some are just over the top with configuration. I wanted something straight forward and when I can't find it, I just build it myself.

This project is the bare minimum. I got it to the point where it would work to do just one thing and that is capture email.

I still have to update the UI - however you can bypass the UI and just hit the database instead. Or build your own UI that's way better than mine. The UI is built using Angular 4.
