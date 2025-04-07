# Define the stack name and compose files
$stackName = "powrintegration_compose"
$composeFile1 = "docker-compose.yml"
$composeFile2 = "docker-compose.swarm.yml"

try {
    # Check if the stack exists
    $services = docker stack services $stackName --format "{{.Name}}" 2>$null
    if ($services) {
        Write-Output "Removing existing stack: $stackName"
        docker stack rm $stackName

        # Wait until the stack is fully removed
        $maxWaitSeconds = 60  # Maximum time to wait in seconds
        $waitIntervalSeconds = 30  # Interval between checks
        $elapsedSeconds = 0

        Write-Output "Waiting for stack '$stackName' to be fully removed..."
        do {
            Start-Sleep -Seconds $waitIntervalSeconds
            $elapsedSeconds += $waitIntervalSeconds
            $services = docker stack services $stackName --format "{{.Name}}" 2>$null
        } while ($services -and $elapsedSeconds -lt $maxWaitSeconds)

        if ($services) {
            Write-Output "Failed to remove stack '$stackName' within $maxWaitSeconds seconds. Aborting."
            exit 1
        }
        Write-Output "Stack '$stackName' successfully removed."
    }
    else {
        Write-Output "No existing stack '$stackName' found. Proceeding with deployment."
    }

    # Redeploy the stack using the specified compose files
    Write-Output "Deploying stack: $stackName"
    docker stack deploy -c $composeFile1 -c $composeFile2 $stackName

    # Verify deployment
    $deployedServices = docker stack services $stackName --format "{{.Name}}" 2>$null
    if ($deployedServices) {
        Write-Output "Stack '$stackName' has been successfully redeployed."
    }
    else {
        Write-Output "Deployment of stack '$stackName' failed. Please check the compose files and Docker Swarm status."
        exit 1
    }
}
catch {
    Write-Output "An error occurred during the process: $($_.Exception.Message)"
    exit 1
}