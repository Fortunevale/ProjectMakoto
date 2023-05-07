echo "Resetting dev branch in 10 seconds, Ctrl+C to cancel."
sleep 5
echo "Resetting dev branch in 5 seconds, Ctrl+C to cancel."
sleep 2
echo "Resetting dev branch in 3 seconds, Ctrl+C to cancel."
sleep 1
echo "Resetting dev branch in 2 seconds, Ctrl+C to cancel."
sleep 1
echo "Resetting dev branch in 1 seconds, Ctrl+C to cancel."
sleep 1
echo "Resetting dev branch.."
echo "Pulling main branch.."
git checkout main
git pull origin main
echo "Hard resetting dev branch.."
git checkout dev
git reset --hard origin/main
echo "Pushing reset branch.."
git push --force
sleep 1
exit