@echo off

cd .\templates && npm install && npm run build && cd ..\ && dotnet build -c RELEASE -v n /m:1