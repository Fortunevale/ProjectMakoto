echo "Resetting dev branch in 5 seconds.."
sleep 2
echo "Resetting dev branch in 3 seconds.."
sleep 1
echo "Resetting dev branch in 2 seconds.."
sleep 1
echo "Resetting dev branch in 1 seconds.."
sleep 1
echo "Resetting dev branch.."
echo "Pulling preview branch.."
git checkout preview
git pull origin preview
echo "Hard resetting dev branch.."
git checkout dev
git reset --hard origin/preview
echo "Pushing reset branch.."
git push --force
sleep 1
exit