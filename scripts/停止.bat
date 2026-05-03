@echo off
chcp 65001 >nul
echo ======================================
echo   ChildPCGuard 停止脚本
echo ======================================
echo.
echo 正在停止服务...
echo.
taskkill /F /IM ChildPCGuard.GuardService.exe 2>nul
taskkill /F /IM ChildPCGuard.Agent.exe 2>nul
taskkill /F /IM LockOverlay.exe 2>nul
echo 服务已停止
pause
