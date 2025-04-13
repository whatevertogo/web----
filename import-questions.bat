@echo off
echo Importing questions to database...
npm install axios --no-save
node dist/importQuestions.cjs
echo Import completed!
pause
