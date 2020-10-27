@echo off
goto :init

:mocks
	start "Mocks" dotnet watch --project src\Mocks\Mocks.csproj run
	set "Started=true"
	goto :eof

:service
	start "Service" dotnet watch --project src\Service\Service.csproj run
	set "Started=true"
	goto :eof

:dashboard
	start "Dashboard" dotnet watch --project src\Dashboard\Dashboard.csproj run
	set "Started=true"
	goto :eof

:init
    set "__BAT_NAME=%~nx0"


:parse
    if "%~1"=="" goto :validate

	if /i "%~1"=="/?"            call :usage & goto :eof
	if /i "%~1"=="--help"        call :usage & goto :eof

	if /i "%~1"=="/a"            call :mocks &  call :service & call :dashboard & goto :eof
	if /i "%~1"=="--all"         call :mocks &  call :service & call :dashboard & goto :eof

	if /i "%~1"=="/m"            call :mocks & shift & goto :parse
	if /i "%~1"=="--mocks"       call :mocks & shift & goto :parse
	
	if /i "%~1"=="/s"            call :service & shift & goto :parse
	if /i "%~1"=="--service"     call :service & shift & goto :parse
	
	if /i "%~1"=="/d"            call :dashboard & shift & goto :parse
	if /i "%~1"=="--dashboard"   call :dashboard & shift & goto :parse

:validate
	if not defined Started call :usage & goto :eof
	goto :eof
	
:usage
    echo USAGE:
    echo   %__BAT_NAME% [flags]
    echo.
    echo.  /?, --help          shows this help
    echo.  /m, --mocks         run the Mocks project
    echo.  /s, --service       run the Service project
    echo.  /d, --dashboard     run the Dashboard project
    echo.  /a, --all           run all projects
    goto :eof
