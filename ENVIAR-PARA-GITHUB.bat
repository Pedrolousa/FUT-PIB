@echo off
chcp 65001 > nul
echo.
echo ============================================
echo   FUT PIB - ENVIAR ALTERACOES AO GITHUB
echo ============================================
echo.

git status
echo.
git add .
git commit -m "Preparar FUT PIB para Supabase e Render"
git branch -M main
git push -u origin main

echo.
echo Operacao concluida.
pause
