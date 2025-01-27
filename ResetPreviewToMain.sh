echo "Resetting preview branch in 5 seconds.."
sleep 2
echo "Resetting preview branch in 3 seconds.."
sleep 1
echo "Resetting preview branch in 2 seconds.."
sleep 1
echo "Resetting preview branch in 1 seconds.."
sleep 1
echo "Resetting preview branch.."
echo "Pulling main branch.."
git checkout main
git pull origin main
echo "Hard resetting preview branch.."
git checkout preview
git reset --hard origin/main
echo "Pushing reset branch.."
git push --force
sleep 1
exit