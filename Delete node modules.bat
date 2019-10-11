ECHO OFF

ECHO You are going to delete all node modules - run this as Admin 
PAUSE
ECHO.
J:

CD "J:\Dev\GitHub\dyslexicanaboko\simple-smtp-interceptor\SimpleSmtpInterceptor.Web\ClientApp"
ECHO.
ECHO This can take a while so please be patient... It's like 300MB of tiny files.
ECHO.
ECHO If you see an error don't worry:
ECHO     1. Sometimes you have to run this again
ECHO     2. Sometimes you have to just delete the remaining folder(s) manually after running this script first
ECHO.
rmdir /S /Q node_modules
ECHO.
PAUSE