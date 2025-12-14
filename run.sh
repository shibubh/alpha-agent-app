#!/bin/bash

# Agent Orchestration System - Run Script

echo "Building Agent Orchestration System..."
dotnet build AgentOrchestration.sln --configuration Release

if [ $? -eq 0 ]; then
    echo ""
    echo "Build successful! Starting the application..."
    echo ""
    dotnet run --project src/AgentOrchestration.CLI/AgentOrchestration.CLI.csproj --configuration Release
else
    echo ""
    echo "Build failed. Please check the error messages above."
    exit 1
fi
