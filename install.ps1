function EnsureAdmin {
	$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-Host "This script must be run as Administrator. Exiting..." -ForegroundColor Red
        exit 1
    }
}

function Enable-WSL {
    $wslFeature = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux

    if ($wslFeature.State -ne "Enabled") {
        Write-Output "Enabling WSL..."
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

		Write-Output "Installing Docker Desktop..."
		Start-Process -FilePath $dockerInstaller -ArgumentList "install --quiet --accept-license --backend=wsl-2 --always-run-service" -Wait -NoNewWindow
		
		Write-Host "Docker installation complete. Restarting your system." -ForegroundColor Green
		Restart-Computer -Force
	}
}

function Ensure-Docker-Running {
	# Check if the Docker Desktop process is currently active.
	$dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue

	if ($dockerProcess) {
		# If the process is found, inform the user that Docker Desktop is already operational.
		Write-Host "Docker Desktop is currently running." -ForegroundColor Green
	} else {
		# If the process is not found, proceed to start Docker Desktop.
		Write-Output "Docker Desktop is not running. Attempting to start it now."
		
		# Define the typical installation path for Docker Desktop executable.
		$dockerPath = "C:\Program Files\Docker\Docker\Docker Desktop.exe"
		
		# Verify if the executable exists at the specified location.
		if (Test-Path -Path $dockerPath) {
			# Launch Docker Desktop using the Start-Process cmdlet.
			Start-Process -FilePath $dockerPath
			Write-Output "Docker Desktop has been initiated successfully."
			Write-Host "Waiting 10 seconds for start-up to complete." -ForegroundColor Yellow
			# Brief pause to allow the process status to update, if needed.
			Start-Sleep -Seconds 10
			Write-Host "Docker Desktop started." -ForegroundColor Green
		} else {
			# If the executable is not found, notify the user of the issue.
			Write-Host "Error: Docker Desktop executable not found at the expected path: $dockerPath. Please verify the installation." -ForegroundColor Red
		}
	}
}

function Is-Stack-Running {
	$stackList = docker stack ls --format "{{.Name}}"
	
	if ($stackList -contains "powrintegration_compose") {
		return $true
	}
	
	return $false
}

function Remove-Docker-Stack {
	docker stack rm powrintegration_compose
}

function Check-Required-Ports {
	Write-Output "Checking if Docker stack is running.."
	if (Is-Stack-Running) {
		Write-Host "Stack is already running. Removing stack.." -ForegroundColor Yellow
		Remove-Docker-Stack
		Start-Sleep -Seconds 5
		Write-Host "Docker stack removed." -ForegroundColor Green
	}
	else {
		Write-Host "Docker stack is not running." -ForegroundColor Green
	}
	
	# Define the list of ports to check
	$ports = @(15672, 9090, 4317, 3100, 14250, 3000)

	# Initialize an empty array to store results
	$results = @()

	# Loop through each port and test if it is in use
	foreach ($port in $ports) {
		try {
			# Test the connection to localhost on the specified port
			$test = Test-NetConnection -ComputerName "localhost" -Port $port -WarningAction SilentlyContinue
			
			# Determine if the port is in use based on the TcpTestSucceeded property
			$status = if ($test.TcpTestSucceeded) { "Yes" } else { "No" }
			
			# Create a custom object with the port number and its status
			$results += [PSCustomObject]@{
				Port   = $port
				Used   = $status
			}
		}
		catch {
			# Handle any errors that occur during the test
			$results += [PSCustomObject]@{
				Port   = $port
				Used   = "Error: $($_.Exception.Message)"
			}
		}
	}

	# Display the results in a formatted table
	$results | Format-Table -AutoSize

	# Optional: Summary of ports in use
	$portsInUse = $results | Where-Object { $_.Status -eq "Yes" }
	if ($portsInUse) {
		Write-Host "Summary: The following ports are currently in use:" -ForegroundColor Red
		$portsInUse | ForEach-Object { Write-Host " - Port $($_.Port)" -ForegroundColor Red }
		exit 1
	} else {
		Write-Host "None of the required ports are currently in use." -ForegroundColor Green
	}
}


function Enable-Swarm {
	Write-Output "Checking if Docker Swarm is enabled..."
	
	$swarmStatus = docker info | Select-String "Swarm: active"
	
	if (-not $swarmStatus) {
		Write-Output "Initializing Docker Swarm mode..."
		docker swarm init
		Write-Host "Docker Swarm mode enabled." -ForegroundColor Green
	} else {
		Write-Host "Docker Swarm mode is already active." -ForegroundColor Green
	}
}

function Deploy-Compose {
	$isStackDeployed = Is-Stack-Running
	
	if ($isStackDeployed) {
		Write-Host "Docker stack is already deployed and running." -ForegroundColor Green
		return
	}
	
	if (Test-Path "docker-compose.swarm.yml") {
		Write-Output "Deploying services using Docker Compose..."
		docker stack deploy -c docker-compose.yml -c docker-compose.swarm.yml powrintegration_compose
		Write-Host "Services deployed successfully." -ForegroundColor Green
	} else {
		Write-Host "docker-compose.yml or docker-compose.swarm.yml not found!" -ForegroundColor Red
	}
}

EnsureAdmin
Enable-WSL
Install-Docker
Ensure-Docker-Running
Check-Required-Ports
Enable-Swarm
Deploy-Compose