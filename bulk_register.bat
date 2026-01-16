@echo off
REM Bulk DNS registration using curl and external JSON file

curl.exe -X POST "https://127.0.0.1:443/dns/register/bulk" -H "Content-Type: application/json" --data-binary "@bulk.json"
