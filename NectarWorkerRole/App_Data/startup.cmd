IF "%IsEmulated%" == "true" goto :EOF  

SET LogPath="%LogFileDirectory%%LogFileName%"
ECHO Current Role: %RoleName% >> "%LogPath%" 2>&1  
ECHO Current Role: %RoleName%  >> "%LogFileDirectory%%LogFileName%"
ECHO Current Role: %RoleName%  >> "%PathToStartupStorage%%LogFileName%"

ECHO Current Role Instance: %InstanceId% >> "%LogPath%" 2>&1  
ECHO Current Directory: %CD% >> "%LogPath%" 2>&1 
ECHO Current Role: %RoleName% >> "%LogPath%" 2>&1  
ECHO Current Role Instance: %InstanceId% >> "%LogPath%" 2>&1  
ECHO Current Directory: %CD% >> "%LogPath%" 2>&1  
     
ECHO We will first verify if startup has been executed before by checking %PathToStartupStorage%\StartupComplete.txt. >> "%LogPath%" 2>&1  
     
IF EXIST "%LogFileDirectory%\StartupComplete.txt" (  
	ECHO Startup has already run, skipping. >> "%LogPath%" 2>&1  
EXIT /B 0  
)  
    
Echo Installing Chocolatey >> "%LogPath%" 2>&1  
    
@powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))"   >> "%LogPath%" 2>&1  
SET PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin  >> "%LogPath%" 2>&1  
   
Echo timeout 60 >> "%LogPath%" 2>&1 
timeout 60
Echo timeout 60 DONE >> "%LogPath%" 2>&1 
IF ERRORLEVEL EQU 0 (  
    
    ECHO Installing Java runtime >> "%LogPath%" 2>&1  
    
    %ALLUSERSPROFILE%\chocolatey\bin\choco install jre8 --version 8.0.144 -y >> "%LogPath%" 2>&1  
	
    IF ERRORLEVEL EQU 0 (            
                ECHO Java installed. Startup completed. >> "%LogPath%" 2>&1  
                
				Echo Installing python runtime >> "%LogPath%" 2>&1  
    
				%ALLUSERSPROFILE%\chocolatey\bin\choco install python2 --version 2.7.13 -y >> "%LogPath%" 2>&1  
    
				IF ERRORLEVEL EQU 0 (            
							ECHO python installed. Startup completed. >> "%LogPath%" 2>&1  
							timeout 20
							ECHO Startup completed. >> "%LogFileDirectory%\StartupComplete.txt" 2>&1  
							ECHO refreshenv >> "%LogPath%" 2>&1
    %ALLUSERSPROFILE%\chocolatey\redirects\RefreshEnv.cmd >> "%LogPath%" 2>&1  
							EXIT /B 0  
				) ELSE (  
					ECHO An error occurred in python. The ERRORLEVEL = %ERRORLEVEL%. >> "%LogPath%" 2>&1  
					EXIT %ERRORLEVEL%  
				)  			
				 
    ) ELSE (  
        ECHO An error occurred in java. The ERRORLEVEL = %ERRORLEVEL%. >> "%LogPath%" 2>&1  
        EXIT %ERRORLEVEL%  
    )  
	
) ELSE (  
	ECHO An error occurred while install chocolatey The ERRORLEVEL = %ERRORLEVEL%. >> "%LogPath%" 2>&1  
	EXIT %ERRORLEVEL%  
)  