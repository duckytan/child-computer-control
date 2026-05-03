@echo off
chcp 65001 >nul
echo ======================================
echo   ChildPCGuard 启动脚本
echo ======================================
echo.
echo 正在启动服务...
echo 按 Ctrl+C 可停止服务
echo ------------------------------------
cd /d "%~dp0"
start "" "ChildPCGuard.GuardService.exe" --console
