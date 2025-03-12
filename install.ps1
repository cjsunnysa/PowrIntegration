$flagFile = "ContinueFlag.txt"
$isContinuation = Test-Path $flagFile

function EnsureAdmin {
	$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-Host "This script must be run as Administrator. Exiting..." -ForegroundColor Red
        exit 1
    }
}

function Check-Continuation {
	if ($isContinuation) {
		Write-Host "Continuing after reboot..."
		Deregister-Continue
	} else {
		Write-Host "Starting initial run..."
	}
}

function Enable-WSL {
    $wslFeature = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux

    if ($wslFeature.State -ne "Enabled") {
        Write-Host "Enabling WSL..." -ForegroundColor Yellow
        Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart
        Write-Host "WSL enabled. A restart may be required." -ForegroundColor Green
    } else {
        Write-Host "WSL is already enabled." -ForegroundColor Green
    }
}

function Install-Docker {
	$dockerApp = Get-ItemProperty "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*" | Where-Object { $_.DisplayName -like "*Docker Desktop*" }
	
	if (-not $dockerApp) {
		$dockerInstaller = $PWD.Path + "\DockerDesktopInstaller.exe"
		$installationFileExists = Test-Path $dockerInstaller
		
		if (-not $installationFileExists) {
			Write-Host "Downloading the latest Docker Desktop installer..." -ForegroundColor Yellow
			Invoke-WebRequest -Uri "https://desktop.docker.com/win/stable/Docker%20Desktop%20Installer.exe" -OutFile $dockerInstaller
		}

		Write-Host "Installing Docker Desktop..." -ForegroundColor Yellow
		Start-Process -FilePath $dockerInstaller -ArgumentList "install --quiet --accept-license --backend=wsl-2 --always-run-service" -Wait -NoNewWindow
		Write-Host "Docker installation complete. Restarting your system." -ForegroundColor Green

		# Schedule the script to run after reboot
		Register-Continue
		
		# Trigger reboot
		Write-Host "Rebooting now..."
		Restart-Computer -Force
	}
}

function Register-Continue {
	# Create a flag to indicate continuation
	"Continue" | Out-File $flagFile
	
	# Get the path of the current script
	$scriptPath = $PSCommandPath
	$scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptPath)

	# Define the Startup folder path
	$startupFolder = [Environment]::GetFolderPath("Startup")
	$shortcutPath = Join-Path $startupFolder "$scriptName.lnk"

	# Check if the shortcut already exists, if not, create it
	if (-not (Test-Path $shortcutPath)) {
		# Create a shell object to make the shortcut
		$shell = New-Object -ComObject WScript.Shell
		$shortcut = $shell.CreateShortcut($shortcutPath)
		
		# Set the target to PowerShell running this script
		$shortcut.TargetPath = "powershell.exe"
		$shortcut.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`""
		$shortcut.WorkingDirectory = Split-Path $scriptPath -Parent
		$shortcut.Description = "Runs $scriptName as Administrator"
		
		# Optional: Set an icon (default PowerShell icon here)
		$shortcut.IconLocation = "%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
		
		# Save the shortcut
		$shortcut.Save()

		# Set the shortcut to run as administrator
		$bytes = [System.IO.File]::ReadAllBytes($shortcutPath)
		$bytes[0x15] = $bytes[0x15] -bor 0x20 # Set byte 21 (0x15) bit 6 to enable RunAsAdmin
		[System.IO.File]::WriteAllBytes($shortcutPath, $bytes)

		Write-Host "Shortcut created at $shortcutPath with Run as Administrator enabled."
	} else {
		Write-Host "Shortcut already exists at $shortcutPath."
	}
}

function Deregister-Continue {
	Remove-Item $flagFile
	# Get the path of the current script
	
	$scriptPath = $PSCommandPath
	$scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptPath)

	# Define the Startup folder path
	$startupFolder = [Environment]::GetFolderPath("Startup")
	$shortcutPath = Join-Path $startupFolder "$scriptName.lnk"

	# Check if the shortcut exists and remove it
	if (Test-Path $shortcutPath) {
		Remove-Item -Path $shortcutPath -Force
		Write-Host "Shortcut removed from $shortcutPath."
	} else {
		Write-Host "No shortcut found at $shortcutPath."
	}
}

function Enable-Swarm {
	Write-Host "Checking if Docker Swarm is enabled..." -ForegroundColor Yellow
	
	$swarmStatus = docker info | Select-String "Swarm: active"
	
	if (-not $swarmStatus) {
		Write-Host "Initializing Docker Swarm mode..." -ForegroundColor Yellow
		docker swarm init
		Write-Host "Docker Swarm mode enabled." -ForegroundColor Green
	} else {
		Write-Host "Docker Swarm mode is already active." -ForegroundColor Green
	}
}

function Deploy-Compose {
	$isStackDeployed = docker service ls --filter "name=powrintegration_compose_powrintegration-backoffice" --format '{{json .}}' | convertfrom-json | select-object -first 1
	
	if ($isStackDeployed) {
		Write-Host "Docker stack is already deployed and running." -ForegroundColor Green
		return
	}
	
	if (Test-Path "docker-compose.swarm.yml") {
		Write-Host "Deploying services using Docker Compose..." -ForegroundColor Yellow
		docker stack deploy -c docker-compose.yml -c docker-compose.swarm.yml powrintegration_compose
		Write-Host "Services deployed successfully." -ForegroundColor Green
	} else {
		Write-Host "docker-compose.yml or docker-compose.swarm.yml not found!" -ForegroundColor Red
	}
}

EnsureAdmin
Check-Continuation
Enable-WSL
Install-Docker
Enable-Swarm
Deploy-Compose