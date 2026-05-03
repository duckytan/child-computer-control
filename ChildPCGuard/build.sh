#!/bin/bash
# Build script for ChildPCGuard
# Run on Windows with .NET 8 SDK installed

set -e

echo "Restoring NuGet packages..."
dotnet restore ChildPCGuard.sln

echo "Building solution..."
dotnet build ChildPCGuard.sln -c Release --no-restore

echo "Running tests..."
dotnet test tests/ChildPCGuard.Tests/ChildPCGuard.Tests.csproj -c Release --no-build --verbosity normal

echo "Publishing projects..."
dotnet publish src/ChildPCGuard.GuardService/ChildPCGuard.GuardService.csproj -c Release -o publish/GuardService --no-build
dotnet publish src/ChildPCGuard.Agent/ChildPCGuard.Agent.csproj -c Release -o publish/Agent --no-build
dotnet publish src/ChildPCGuard.LockOverlay/ChildPCGuard.LockOverlay.csproj -c Release -o publish/LockOverlay --no-build

echo "Build completed successfully!"
