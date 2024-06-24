@echo off
rem 執行npm build命令並在成功後進入./dist資料夾
npm run build && (
    cd ./dist

    rem 刪除已存在的server.zip（如果存在）
    if exist server.zip del server.zip

    rem 將除了server.zip以外的所有檔案打包成server.zip
    powershell Compress-Archive -Path * -CompressionLevel Optimal -DestinationPath server.zip -Force

    rem 切換回原本的目錄
    cd ..
)

rem 通知任務完成
echo Build and zip process completed.
pause
