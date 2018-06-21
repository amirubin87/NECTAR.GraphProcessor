SET LogPath=%LogFileDirectory%%LogFileName%  
     
ECHO Current Role: %RoleName% >> "%LogPath%" 2>&1  
ECHO Current Role Instance: %InstanceId% >> "%LogPath%" 2>&1  
ECHO Current Directory: %CD% >> "%LogPath%" 2>&1  
     
ECHO We will first verify if startup has been executed before by checking %RoleRoot%\StartupComplete.txt. >> "%LogPath%" 2>&1  
     
IF EXIST "%RoleRoot%\StartupComplete.txt" (  
ECHO Startup has already run, skipping. >> "%LogPath%" 2>&1  
EXIT /B 0  
)  
    
Echo Installing Chocolatey >> "%LogPath%" 2>&1  
    
@powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'))" && SET PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin  >> "%LogPath%" 2>&1  
    
IF ERRORLEVEL EQU 0 (  
    
    Echo Installing Java runtime >> "%LogPath%" 2>&1  
    
    %ALLUSERSPROFILE%\chocolatey\bin\choco install javaruntime -y >> "%LogPath%" 2>&1  
    
    IF ERRORLEVEL EQU 0 (            
                ECHO Java installed. Startup completed. >> "%LogPath%" 2>&1  
                ECHO Startup completed. >> "%RoleRoot%\StartupComplete.txt" 2>&1  
                EXIT /B 0  
    ) ELSE (  
        ECHO An error occurred. The ERRORLEVEL = %ERRORLEVEL%. >> "%LogPath%" 2>&1  
        EXIT %ERRORLEVEL%  
    )  

	Echo Installing python runtime >> "%LogPath%" 2>&1  
    
    %ALLUSERSPROFILE%\chocolatey\bin\choco install python2 -y >> "%LogPath%" 2>&1  
    
    IF ERRORLEVEL EQU 0 (            
                ECHO python installed. Startup completed. >> "%LogPath%" 2>&1  
                ECHO Startup completed. >> "%RoleRoot%\StartupComplete.txt" 2>&1  
                EXIT /B 0  
    ) ELSE (  
        ECHO An error occurred. The ERRORLEVEL = %ERRORLEVEL%. >> "%LogPath%" 2>&1  
        EXIT %ERRORLEVEL%  
    )  
) ELSE (  
ECHO An error occurred while install chocolatey The ERRORLEVEL = %ERRORLEVEL%. >> "%LogPath%" 2>&1  
EXIT %ERRORLEVEL%  
)  