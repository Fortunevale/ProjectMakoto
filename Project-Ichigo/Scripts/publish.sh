cd ..
git checkout xorog
git add --all
git commit -m "Auto-Commit"
git push

git checkout main
git fetch
git pull
git push

echo ""
echo Building..
echo ""
dotnet publish --runtime linux-x64 --self-contained false --output "ProjectIchigo/"

echo ""
echo Getting latest git hash..
echo ""
git rev-parse --short HEAD > ProjectIchigo/LatestGitPush.cfg
git branch --show-current >> ProjectIchigo/LatestGitPush.cfg
echo %date% >> ProjectIchigo/LatestGitPush.cfg
echo %time% >> ProjectIchigo/LatestGitPush.cfg

echo Clearing current latestBotVersion..
ssh xorog@192.168.178.29 -p 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" "cd /home/xorog/Desktop && rm -rf ./latestProjectIchigo"

echo ""
echo Uploading..
echo ""
scp -P 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" -r "ProjectIchigo/" xorog@192.168.178.29:/home/xorog/Desktop/ProjectIchigo
ssh xorog@192.168.178.29 -p 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" "cd /home/xorog/Desktop && mv ProjectIchigo latestProjectIchigo"
rm -rf "ProjectIchigo/"

echo ""
echo Restarting bots..
echo ""

echo "" | tee updated
#scp -P 6969 -i "/mnt/01D7DB05B77BEAF0/Dokumente/Keys/fortunevale-key.openssh" "updated" xorog@192.168.178.29:/home/xorog/Desktop/Serverproject
rm updated

sleep 1

git checkout xorog