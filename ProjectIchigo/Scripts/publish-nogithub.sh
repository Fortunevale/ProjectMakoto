cd ..
echo ""
echo Building..
echo ""
dotnet publish --configuration RELEASE --runtime linux-x64 --self-contained false --output "Project_Ichigo/"

echo ""
echo Getting latest git hash..
echo ""
git rev-parse --short HEAD > Project_Ichigo/LatestGitPush.cfg
git branch --show-current >> Project_Ichigo/LatestGitPush.cfg
echo $(date +%d.%m.%y) >> Project_Ichigo/LatestGitPush.cfg
echo $(date +%H:%M:%S,00) >> Project_Ichigo/LatestGitPush.cfg

echo Clearing current latestBotVersion..
ssh xorog@192.168.178.29 -p 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" "cd /home/xorog/Desktop && rm -rf ./latestProjectIchigo"

echo ""
echo Uploading..
echo ""
scp -P 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" -r "Project_Ichigo/" xorog@192.168.178.29:/home/xorog/Desktop/Project_Ichigo
ssh xorog@192.168.178.29 -p 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" "cd /home/xorog/Desktop && mv Project_Ichigo latestProjectIchigo"
sleep 1
rm -rf "Project_Ichigo/"

echo ""
echo Restarting bots..
echo ""

ssh xorog@192.168.178.29 -p 6969 -i "C:/Users/Xorog/Documents/Keys/fortunevale-key.openssh" "touch /home/xorog/Desktop/ProjectIchigo/updated"