# PowerShell script to set up Docker with WSL, enable Swarm mode, and deploy using docker-compose

# Ensure the script is running as Administrator
function Check-Admin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-Host "This script must be run as Administrator. Exiting..." -ForegroundColor Red
        exit 1
    }
}

# Check if this is a continuation
$flagFile = "ContinueFlag.txt"
$isContinuation = Test-Path $flagFile
if ($isContinuation) {
    Write-Host "Continuing after reboot..."
    Remove-Item $flagFile
} else {
    Write-Host "Starting initial run..."
}

# Enable WSL if not enabled
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

# Download and install the latest Docker Desktop
function Install-Docker {
    if (!$isContinuation) {
        $dockerInstaller = "$env:TEMP\DockerDesktopInstaller.exe"
        Write-Host "Downloading the latest Docker Desktop installer..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri "https://desktop.docker.com/win/stable/Docker%20Desktop%20Installer.exe" -OutFile $dockerInstaller

        Write-Host "Installing Docker Desktop..." -ForegroundColor Yellow
        Start-Process -FilePath $dockerInstaller -ArgumentList "install --quiet" -Wait -NoNewWindow
        Write-Host "Docker installation complete. Please restart your system if necessary." -ForegroundColor Green

        # Create a flag to indicate continuation
        "Continue" | Out-File $flagFile

        # Schedule the script to run after reboot
        Register-ScheduledTask -TaskName "ContinueScript" -Trigger (New-ScheduledTaskTrigger -AtStartup) `
            -Action (New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"C:\Scripts\MyScript.ps1`"") `
            -Force

        # Trigger reboot
        Write-Host "Rebooting now..."
        Restart-Computer -Force
    }
}

# Enable Swarm mode
function Enable-Swarm {
    if ($isContinuation) {
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
}

# Deploy services using docker-compose
function Deploy-Compose {
    if ($isContinuation) {
        if (Test-Path "docker-compose.swarm.yml") {
            Write-Host "Deploying services using Docker Compose..." -ForegroundColor Yellow
            docker stack deploy -c docker-compose.yml -c docker-compose.swarm.yml powrintegration_compose
            Write-Host "Services deployed successfully." -ForegroundColor Green
        } else {
            Write-Host "docker-compose.yml or docker-compose.swarm.yml not found!" -ForegroundColor Red
        }
    }
}

# Run functions
Check-Admin
Enable-WSL
Install-Docker
Enable-Swarm
Deploy-Compose
